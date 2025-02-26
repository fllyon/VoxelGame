using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class World : MonoBehaviour {

    public Material material;

    void Start() {
        Mesh mesh = new Mesh();
        mesh.vertices = BlockData.Vertices;
        mesh.uv = BlockData.UV;
        mesh.triangles = BlockData.Triangles;

        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        gameObject.GetComponent<MeshRenderer>().material = material;

    }
}
