using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BlockType {

    public uint id;
    public string name;

    public int top;
    public int front;
    public int left;
    public int back;
    public int right;
    public int bottom;

    public bool isTransparent;
    
}

[Serializable]
public class BlockTypeWrapper {
    public List<BlockType> blockTypes;
}