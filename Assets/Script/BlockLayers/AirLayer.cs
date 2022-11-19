using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirLayer : BlockLayer
{
    protected override bool TryGenerate(Chunk chunk, Vector3Int position, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (position.y > surfaceHeightNoise)
        {
            chunk.SetBlock(position, BlockType.Air);
            return true;
        }

        return false;
    }
}
