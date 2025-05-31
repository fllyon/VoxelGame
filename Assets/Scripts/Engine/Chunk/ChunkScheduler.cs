using Priority_Queue;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class ChunkScheduler {

    public ChunkManager chunk_manager;

    private Data.BlockData block_data;

    private FastPriorityQueue<ChunkNode> generate_queue;
    private NativeArray<int3> generate_jobs;
    private NativeParallelHashMap<int3, ChunkData> generate_output;
    private JobHandle generate_handle = default;
    
    private FastPriorityQueue<ChunkNode> render_queue;
    private NativeArray<int3> render_jobs; 
    private Mesh.MeshDataArray render_output;
    private JobHandle render_handle = default;

    public ChunkScheduler(Data.BlockData _block_data) {
        block_data = _block_data;

        int generate_max = WorldSettings.LOAD_DISTANCE.Cubed();
        generate_queue = new FastPriorityQueue<ChunkNode>(generate_max);
        generate_jobs = new NativeArray<int3>(1, Allocator.Persistent);
        generate_output = new NativeParallelHashMap<int3, ChunkData>(generate_max, Allocator.Persistent);
        generate_handle = default;

        int render_max = WorldSettings.RENDER_DISTANCE.Cubed();
        render_queue = new FastPriorityQueue<ChunkNode>(render_max);
        render_jobs = new NativeArray<int3>(1, Allocator.Persistent);
        render_output = default;
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

    public void QueueChunksForGeneration(NativeList<int3> chunks) {
        foreach (int3 chunk_pos in chunks) {
            generate_queue.Enqueue(new ChunkNode(chunk_pos), Player.ChunkDistanceFromPlayer(chunk_pos));
        }
    }

    public void QueueChunksForRendering(NativeList<int3> chunks) {
        foreach (int3 chunk_pos in chunks) {
            render_queue.Enqueue(new ChunkNode(chunk_pos), Player.ChunkDistanceFromPlayer(chunk_pos));
        }
    }

    public void Dispose() {}

    // ============================================================= //
    //                       Utility Functions                       //
    // ============================================================= //

    private void CompleteJobs() {

        generate_handle.Complete();
        chunk_manager.AddGeneratedChunks(generate_output);
        generate_output.Clear();

        render_handle.Complete();
        chunk_manager.AddRenderedChunks(render_jobs, render_output);

    }

    public void ScheduleJobs() {

        if (generate_queue.Count != 0) {
            generate_jobs[0] = generate_queue.Dequeue().chunk_pos;
            ChunkJob generate_job = new ChunkJob {
                jobs = generate_jobs,
                output = generate_output.AsParallelWriter()
            };
            generate_handle = generate_job.Schedule(generate_jobs.Length, 1);
        }

        if (render_queue.Count != 0) {
            // TODO: Make and schedule a render job
        }

    }
    
}