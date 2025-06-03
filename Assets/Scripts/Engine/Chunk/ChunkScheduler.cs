using System.Diagnostics;
using System.Linq;
using Priority_Queue;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class ChunkScheduler {

    public ChunkManager chunk_manager;

    private Data.BlockData block_data;
    private NativeArray<int3> verts;
    private NativeArray<int3> dirs;
    private NativeArray<VertexAttributeDescriptor> vertex_attributes;

    private int generate_batch_size = 4;
    private FastPriorityQueue<ChunkNode> generate_queue;
    private NativeArray<int3> generate_jobs;
    private NativeParallelHashMap<int3, ChunkData> generate_output;
    private JobHandle generate_handle = default;
    
    private int render_batch_size = 4;
    private FastPriorityQueue<ChunkNode> render_queue;
    private NativeArray<int3> render_jobs; 
    private ChunkAccessor render_accessor;
    private Mesh.MeshDataArray render_output;
    private JobHandle render_handle = default;
    bool render_output_allocated = false;

    public ChunkScheduler(Data.BlockData _block_data) {

        block_data = _block_data;
        
        verts = new NativeArray<int3>(24, Allocator.Persistent);
        verts[0] = new int3(0, 1, 0); verts[1] = new int3(0, 1, 1); verts[2] = new int3(1, 1, 1); verts[3] = new int3(1, 1, 0);
        verts[4] = new int3(0, 0, 1); verts[5] = new int3(1, 0, 1); verts[6] = new int3(1, 1, 1); verts[7] = new int3(0, 1, 1);
        verts[8] = new int3(1, 0, 1); verts[9] = new int3(1, 0, 0); verts[10] = new int3(1, 1, 0); verts[11] = new int3(1, 1, 1);
        verts[12] = new int3(1, 0, 0); verts[13] = new int3(0, 0, 0); verts[14] = new int3(0, 1, 0); verts[15] = new int3(1, 1, 0);
        verts[16] = new int3(0, 0, 0); verts[17] = new int3(0, 0, 1); verts[18] = new int3(0, 1, 1); verts[19] = new int3(0, 1, 0);
        verts[20] = new int3(0, 0, 0); verts[21] = new int3(1, 0, 0); verts[22] = new int3(1, 0, 1); verts[23] = new int3(0, 0, 1);

        dirs = new NativeArray<int3>(6, Allocator.Persistent);
        dirs[0] = new int3(0, 1, 0);
        dirs[1] = new int3(0, 0, 1);
        dirs[2] = new int3(1, 0, 0);
        dirs[3] = new int3(0, 0, -1);
        dirs[4] = new int3(-1, 0, 0);
        dirs[5] = new int3(0, -1, 0);

        vertex_attributes = new NativeArray<VertexAttributeDescriptor>(3, Allocator.Persistent);
        vertex_attributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
        vertex_attributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);
        vertex_attributes[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2);

        generate_queue = new FastPriorityQueue<ChunkNode>(WorldSettings.LOAD_CONTAINER_SIZE);
        generate_jobs = new NativeArray<int3>(generate_batch_size, Allocator.Persistent);
        generate_output = new NativeParallelHashMap<int3, ChunkData>(WorldSettings.LOAD_CONTAINER_SIZE, Allocator.Persistent);
        generate_handle = default;

        render_queue = new FastPriorityQueue<ChunkNode>(WorldSettings.RENDER_CONTAINER_SIZE);
        render_jobs = new NativeArray<int3>(render_batch_size, Allocator.Persistent);
        render_accessor = default;
        render_output = Mesh.AllocateWritableMeshData(0);
        render_output_allocated = true;
        render_handle = default;
    }

    // ============================================================= //
    //                         Utility Types                         //
    // ============================================================= //

    private class ChunkNode : FastPriorityQueueNode {
        public int3 chunk_pos;
        public ChunkNode(int3 _chunk_pos) { chunk_pos = _chunk_pos; }
    }
    
    // ============================================================= //
    //                        Public Methods                         //
    // ============================================================= //

    public void Update() { CompleteJobs(); }

    public void LateUpdate() { ScheduleJobs(); }

    public void ReplaceGenerateQueue(NativeList<int3> chunks) {
        generate_queue.Clear();
        QueueChunksForGeneration(chunks);
    }

    public void QueueChunksForGeneration(NativeList<int3> chunks) {
        foreach (int3 chunk_coord in chunks) {
            generate_queue.Enqueue(new ChunkNode(chunk_coord), Player.ChunkDistanceFromPlayer(chunk_coord));
        }
    }

    public void ReplaceRenderQueue(NativeList<int3> chunks) {
        render_queue.Clear();
        QueueChunksForRendering(chunks);
    }

    public void QueueChunksForRendering(NativeList<int3> chunks) {
        foreach (int3 chunk_coord in chunks) {
            render_queue.Enqueue(new ChunkNode(chunk_coord), Player.ChunkDistanceFromPlayer(chunk_coord));
        }
    }

    public void Dispose() {

        generate_handle.Complete();
        render_handle.Complete();

        verts.Dispose();
        dirs.Dispose();
        vertex_attributes.Dispose();

        if (generate_jobs.IsCreated) { generate_jobs.Dispose(); }
        foreach (var pair in generate_output) { pair.Value.Dispose(); }
        generate_output.Dispose();

        if (render_jobs.IsCreated) { render_jobs.Dispose(); }
        if (render_accessor.IsCreated) { render_accessor.Dispose(); }
        if (render_output_allocated) { render_output.Dispose(); }

    }

    // ============================================================= //
    //                       Utility Functions                       //
    // ============================================================= //

    private void CompleteJobs() {

        generate_handle.Complete();
        chunk_manager.AddGeneratedChunks(generate_output);
        if (generate_jobs.IsCreated) generate_jobs.Dispose();
        generate_output.Clear();

        render_handle.Complete();
        if (render_accessor.IsCreated) { render_accessor.Dispose(); }
        if (render_jobs.IsCreated) { chunk_manager.AddRenderedChunks(render_jobs, render_output); }
        render_output_allocated = false;
        if (render_jobs.IsCreated) { render_jobs.Dispose(); }

    }

    private void ScheduleJobs() {

        if (generate_queue.Count != 0) {

            int generate_job_count = math.min(generate_queue.Count, generate_batch_size);
            generate_jobs = new NativeArray<int3>(generate_job_count, Allocator.Persistent);
            for (int idx = 0; idx < generate_job_count; ++idx) {
                generate_jobs[idx] = generate_queue.Dequeue().chunk_pos;
            }
            ChunkJob generate_job = new ChunkJob {
                jobs = generate_jobs,
                output = generate_output.AsParallelWriter()
            };
            generate_handle = generate_job.Schedule(generate_jobs.Length, generate_batch_size);
        }

        if (render_queue.Count != 0) {

            int render_job_count = math.min(render_queue.Count, generate_batch_size);
            render_jobs = new NativeArray<int3>(render_job_count, Allocator.Persistent);
            for (int idx = 0; idx < render_job_count; ++idx) {
                render_jobs[idx] = render_queue.Dequeue().chunk_pos;
            }

            render_output = Mesh.AllocateWritableMeshData(render_jobs.Length);
            render_output_allocated = true;

            render_accessor = chunk_manager.GetAccessor(render_jobs);
            RenderJob render_job = new RenderJob {
                jobs = render_jobs,
                accessor = render_accessor,
                verts = verts,
                dirs = dirs,
                vertex_attributes = vertex_attributes,
                block_data = block_data,
                output = render_output
            };

            render_handle = render_job.Schedule(render_jobs.Length, render_batch_size);
        }

    }
    
}