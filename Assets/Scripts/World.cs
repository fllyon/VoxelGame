using System;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class World : MonoBehaviour {

    public Material material;
    Dictionary<int, Chunk> chunks;

    void Start() {

        int y = 0;
        for (int x = 0; x < 4; ++x) {
            for (int z = 0; z < 4; ++z) {

                GameObject curr_chunk = new GameObject("chunk (" + x + " " + y + " " + z + ")");
                curr_chunk.transform.position = new Vector3(x * Chunk.CHUNK_SIZE, y, z * Chunk.CHUNK_SIZE);

                curr_chunk.AddComponent<MeshFilter>();
                curr_chunk.AddComponent<MeshRenderer>();
                curr_chunk.AddComponent<Chunk>();
                curr_chunk.GetComponent<Chunk>().Init(new Vector3Int(x, y, z), material);

                curr_chunk.GetComponent<Chunk>();

                chunks[Tuple.Create(x, y, z).GetHashCode()] = curr_chunk.GetComponent<Chunk>();
            }
        }

    }
}
