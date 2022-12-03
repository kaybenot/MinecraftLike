using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceLayer : BlockLayer
{
    [SerializeField] private BlockType surfaceBlockType;
    
    protected override bool TryGenerate(Chunk chunk, Vector3Int position, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (position.y == surfaceHeightNoise)
        {
            chunk.SetBlock(position, surfaceBlockType, true);
            return true;
        }

        return false;
    }
}
