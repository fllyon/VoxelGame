using UnityEngine;

public class World : MonoBehaviour {

    public Chunk chunk { get; private set; }
    
    void Start() {

        for (int x = 0; x < 4; ++x)
        {
            for (int z = 0; z < 4; ++z)
            {
                for (int y = 0; y < 2; ++y)
                {
                    GameObject chunk_object = new GameObject("Chunk", typeof(MeshFilter), typeof(MeshRenderer));
                    chunk_object.transform.position = new Vector3(x*32, y*32, z*32);
                    chunk = chunk_object.AddComponent<Chunk>();
                }
            }
        }
        
    }
}
