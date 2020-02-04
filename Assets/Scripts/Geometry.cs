using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Geometry {
    public enum Face {
        Top = 0,
        Bottom,
        Left,
        Right,
        Front,
        Back,
    }

    public static Vector3Int Offset(this Face f) {
        switch (f) {
            case Face.Top:
                return new Vector3Int(0, 1, 0);
            case Face.Bottom:
                return new Vector3Int(0, -1, 0);
            case Face.Left:
                return new Vector3Int(-1, 0, 0);
            case Face.Right:
                return new Vector3Int(1, 0, 0);
            case Face.Front:
                return new Vector3Int(0, 0, 1);
            case Face.Back:
                return new Vector3Int(0, 0, -1);
            default:
                // TODO: Figure out how to error here
                return new Vector3Int(0, 0, 0);
        }
    }

    public static readonly Vector3[] vertices = {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),
    };

    public static readonly int[,] triangles = {
        {3, 7, 2, 2, 7, 6}, // Top Face
		{1, 5, 0, 0, 5, 4}, // Bottom Face
		{4, 7, 0, 0, 7, 3}, // Left Face
		{1, 2, 5, 5, 2, 6}, // Right Face
		{5, 6, 4, 4, 6, 7}, // Front Face
        {0, 3, 1, 1, 3, 2}, // Back Face
    };

    public static Vector2[] uvs = {
        new Vector2 (0.0f, 0.0f),
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 0.0f),
        new Vector2 (1.0f, 0.0f),
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 1.0f),
    };
}