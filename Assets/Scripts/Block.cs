using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Block
{
    Air,
    Dirt,
    Stone,
    Wood,
}

public static class BlockMethods
{
    public static bool IsTransparent(this Block block)
    {
        return block == Block.Air;
    }
}