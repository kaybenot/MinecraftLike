using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndergroundLayer : BlockLayer
{
    [SerializeField] private BlockType undergroundBlockType;
    
    protected override bool TryGenerate(Chunk chunk, Vector3Int position, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (position.y < surfaceHeightNoise)
        {
            chunk.SetBlock(position, undergroundBlockType, true);
            return true;
        }

        return false;
    }
}
