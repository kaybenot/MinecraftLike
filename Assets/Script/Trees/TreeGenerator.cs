using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGenerator : MonoBehaviour
{
    public CustomNoiseSettings treeNoiseSettings;
    public DomainWarping DomainWarping;

    public TreeData GenerateTreeData(Chunk chunk, Vector2Int mapSeedOffset)
    {
        treeNoiseSettings.WorldOffset = mapSeedOffset;
        TreeData treeData = new TreeData();
        float[,] noiseData = generateTreeNoise(chunk, treeNoiseSettings);
        treeData.TreePositions =
            DataProcessing.FindLocalMaxima(noiseData, chunk.WorldPosition.x, chunk.WorldPosition.z);
        return treeData;
    }

    private float[,] generateTreeNoise(Chunk chunk, CustomNoiseSettings noiseSettings)
    {
        float[,] noiseMax = new float[chunk.Size, chunk.Size];
        int xMax = chunk.WorldPosition.x + chunk.Size;
        int xMin = chunk.WorldPosition.x;
        int zMax = chunk.WorldPosition.z + chunk.Size;
        int zMin = chunk.WorldPosition.z;
        int xIndex = 0, zIndex = 0;
        for (int x = xMin; x < xMax; x++)
        {
            for (int z = zMin; z < zMax; z++)
            {
                noiseMax[xIndex, zIndex] = DomainWarping.GenerateDomainNoise(x, z, treeNoiseSettings);
                zIndex++;
            }

            xIndex++;
            zIndex = 0;
        }

        return noiseMax;
    }
}
