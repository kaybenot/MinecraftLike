using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterLayer : BlockLayer
{
    [SerializeField] private int waterLayer = 1;
    
    protected override bool TryGenerate(Chunk chunk, Vector3Int position, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (position.y > surfaceHeightNoise && position.y <= waterLayer)
        {
            chunk.SetBlock(position, BlockType.Water);
            if (position.y == surfaceHeightNoise + 1)
            {
                position.y = surfaceHeightNoise;
                chunk.SetBlock(position, BlockType.Sand);
            }
            return true;
        }
        return false;
    }
}
