using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BlockLayer : MonoBehaviour
{
    public bool Generate(Chunk chunk, Vector3Int location, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        return false;
    }
}
