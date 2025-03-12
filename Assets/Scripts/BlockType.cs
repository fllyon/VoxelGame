using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BlockType {

    public uint id;
    public string name;
    public int[] faces;
    public bool isTransparent;
    
}

[Serializable]
public class BlockTypeWrapper {
    public List<BlockType> blockTypes;
}