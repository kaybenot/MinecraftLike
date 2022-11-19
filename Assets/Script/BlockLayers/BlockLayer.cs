using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BlockLayer : MonoBehaviour
{
    [SerializeField] private BlockLayer next;

    public bool Handle(Chunk chunk, Vector3Int position, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (TryGenerate(chunk, position, surfaceHeightNoise, mapSeedOffset))
            return true;
        if (next != null)
            return next.Handle(chunk, position, surfaceHeightNoise, mapSeedOffset);
        return false;
    }
    
    protected abstract bool TryGenerate(Chunk chunk, Vector3Int position, int surfaceHeightNoise, Vector2Int mapSeedOffset);
}
