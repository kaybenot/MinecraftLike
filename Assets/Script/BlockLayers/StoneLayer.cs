using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneLayer : BlockLayer
{
    [SerializeField, Range(0, 1)] public float stoneThreshold = 0.5f;
    [SerializeField] private CustomNoiseSettings stoneNoiseSettings;
    [SerializeField] private DomainWarping DomainWarping;

    protected override bool TryGenerate(Chunk chunk, Vector3Int position, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (chunk.WorldPosition.y > surfaceHeightNoise)
            return false;

        stoneNoiseSettings.WorldOffset = mapSeedOffset;
        float stoneNoise = DomainWarping.GenerateDomainNoise(position.x + chunk.WorldPosition.x, position.z + chunk.WorldPosition.z, stoneNoiseSettings);

        int endPosition = surfaceHeightNoise;
        if (chunk.WorldPosition.y < 0)
            endPosition = chunk.WorldPosition.y + chunk.Height;

        if (stoneNoise > stoneThreshold)
        {
            for (int i = chunk.WorldPosition.y; i <= endPosition; i++)
            {
                Vector3Int pos = new Vector3Int(position.x, i, position.z);
                chunk.SetBlock(pos, BlockType.Stone);
            }

            return true;
        }

        return false;
    }
}
