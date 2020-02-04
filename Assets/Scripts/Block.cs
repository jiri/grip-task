using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockPrototype {
    public string name;
    public bool isSolid;

    [Header("Textures")]
    public int topTexture;
    public int bottomTexture;
    public int leftTexture;
    public int rightTexture;
    public int frontTexture;
    public int backTexture;

    public int GetTextureID(Geometry.Face face) {
        switch (face) {
            case Geometry.Face.Top:
                return topTexture;
            case Geometry.Face.Bottom:
                return bottomTexture;
            case Geometry.Face.Left:
                return leftTexture;
            case Geometry.Face.Right:
                return rightTexture;
            case Geometry.Face.Front:
                return frontTexture;
            case Geometry.Face.Back:
                return backTexture;
            default:
                Debug.LogError($"Unknown enum value {face}");
                return 0;
        }
    }
}
