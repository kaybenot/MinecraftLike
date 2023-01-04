using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeLayer : BlockLayer
{
    [SerializeField] private float terrainHeightLimit = 25;
    
    protected override bool TryGenerate(Chunk chunk, Vector3Int position, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (chunk.WorldPosition.y < 0)
            return false;
        if (surfaceHeightNoise < terrainHeightLimit &&
            chunk.ChunkGenerator.TreeData.TreePositions.Contains(new Vector2Int(chunk.WorldPosition.x + position.x, chunk.WorldPosition.z + position.z)))
        {
            Vector3Int localPos = new Vector3Int(position.x, surfaceHeightNoise, position.z);
            BlockType type = chunk.GetBlock(localPos).BlockType;
            if (type == BlockType.Grass)
            {
                chunk.SetBlock(localPos, BlockType.Dirt, true);
                for (int i = 1; i < 5; i++)
                {
                    localPos.y = surfaceHeightNoise + i;
                    chunk.SetBlock(localPos, BlockType.Log, true);
                }

                foreach (var pos in treeLeavesPositions)
                    chunk.ChunkGenerator.TreeData.TreeLeavesSolid.Add(new Vector3Int(position.x + pos.x, surfaceHeightNoise + 5 + pos.y, position.z + pos.z));
            }
        }

        return false;
    }

    private static List<Vector3Int> treeLeavesPositions = new List<Vector3Int>()
    {
        new Vector3Int(-2, 0, -2),
        new Vector3Int(-2, 0, -1),
        new Vector3Int(-2, 0, 0),
        new Vector3Int(-2, 0, 1),
        new Vector3Int(-2, 0, 2),
        new Vector3Int(-1, 0, -2),
        new Vector3Int(-1, 0, -1),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(-1, 0, 1),
        new Vector3Int(-1, 0, 2),
        new Vector3Int(0, 0, -2),
        new Vector3Int(0, 0, -1),
        new Vector3Int(0, 0, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, 2),
        new Vector3Int(1, 0, -2),
        new Vector3Int(1, 0, -1),
        new Vector3Int(1, 0, 0),
        new Vector3Int(1, 0, 1),
        new Vector3Int(1, 0, 2),
        new Vector3Int(2, 0, -2),
        new Vector3Int(2, 0, -1),
        new Vector3Int(2, 0, 0),
        new Vector3Int(2, 0, 1),
        new Vector3Int(2, 0, 2),
        new Vector3Int(-1, 1, -1),
        new Vector3Int(-1, 1, -0),
        new Vector3Int(-1, 1, 1),
        new Vector3Int(0, 1, -1),
        new Vector3Int(0, 1, -0),
        new Vector3Int(0, 1, 1),
        new Vector3Int(1, 1, -1),
        new Vector3Int(1, 1, 0),
        new Vector3Int(1, 1, 1),
        new Vector3Int(0, 2, 0),
    };
}
