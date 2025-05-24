using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour {

    public MeshFilter mesh_filter;

    void Awake() {
        mesh_filter = GetComponent<MeshFilter>();
    }
}
