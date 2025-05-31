using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public struct Data {

    public static int NUM_BLOCKS;

    // ============================================================= //
    //                           Json Data                           //
    // ============================================================= //

    [Serializable]
    public struct JsonDataList {
        public List<JsonData> blockTypes;
    }

    [Serializable]
    public struct JsonData {
        public int id;
        public string name;
        public int[] faces;
        public bool isTransparent;
    }

    // ============================================================= //
    //                           Block Data                          //
    // ============================================================= //

    [Serializable]
    public struct BlockData {
        public NativeArray<Block> block_data;
        public NativeArray<int> face_data;

        public BlockData(int num_blocks) { 
            block_data = new NativeArray<Block>(num_blocks, Allocator.Persistent);
            face_data = new NativeArray<int>(num_blocks * 6, Allocator.Persistent);
        }

        public void Dispose() {
            if (block_data.IsCreated) { block_data.Dispose(); }
            if (face_data.IsCreated) { face_data.Dispose(); }
        }
    }

    [Serializable]
    public struct Block {
        public FixedString32Bytes name;
        public bool is_transparent;

        public Block(string _name, bool _is_transparent) {
            name = _name;
            is_transparent = _is_transparent;
        }
    }
    
    // ============================================================= //
    //                       Utility Functions                       //
    // ============================================================= //

    public static BlockData LoadData() {

        string jsonData = Resources.Load<TextAsset>("BlockData").text;
        JsonDataList input = JsonUtility.FromJson<JsonDataList>(jsonData);

        NUM_BLOCKS = input.blockTypes.Count;

        BlockData output = new BlockData(NUM_BLOCKS);
        for (int id = 0; id < input.blockTypes.Count; ++id) {
            JsonData json_data = input.blockTypes[id];

            output.block_data[id] = new Block(json_data.name, json_data.isTransparent);

            int face_idx = id * 6;
            for (int face = 0; face < 6; ++face) {
                output.face_data[face_idx + face] = json_data.faces[face];
            }
            
        }

        return output;

    }

}