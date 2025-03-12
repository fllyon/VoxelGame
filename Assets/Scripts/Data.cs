using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UIElements;


[System.Serializable]
public class Data {

    public static List<BlockType> blockTypes = new List<BlockType>();


    public Data() {
        InitializeBlockTypes();
    }

    private void InitializeBlockTypes() {
        string jsonData = Resources.Load<TextAsset>("BlockData").text;
        blockTypes = JsonUtility.FromJson<BlockTypeWrapper>(jsonData).blockTypes;
    }

}