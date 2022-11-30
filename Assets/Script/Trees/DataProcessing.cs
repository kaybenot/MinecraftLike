using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataProcessing
{
    public static List<Vector2Int> Directions = new List<Vector2Int>()
    {
        Vector2Int.up,
        Vector2Int.up + Vector2Int.right,
        Vector2Int.right,
        Vector2Int.right + Vector2Int.down,
        Vector2Int.down,
        Vector2Int.down + Vector2Int.left,
        Vector2Int.left,
        Vector2Int.left + Vector2Int.up
    };
    
    public static List<Vector2Int> FindLocalMaxima(float[,] noiseData, int xMin, int zMin)
    {
        List<Vector2Int> maximas = new List<Vector2Int>();
        for (int x = 0; x < noiseData.GetLength(0); x++)
        {
            for (int z = 0; z < noiseData.GetLength(1); z++)
            {
                float noiseVal = noiseData[x, z];
                if(checkNeighbours(noiseData, x, z, (neigbourNoise) => neigbourNoise < noiseVal))
                    maximas.Add(new Vector2Int(xMin + x, zMin + z));
            }
        }

        return maximas;
    }

    private static bool checkNeighbours(float[,] dataMatrix, int x, int y, Func<float, bool> successCondition)
    {
        foreach (var dir in Directions)
        {
            var neighbour = new Vector2Int(x + dir.x, y + dir.y);
            if (neighbour.x < 0 || neighbour.x >= dataMatrix.GetLength(0) || neighbour.y < 0 || neighbour.y >= dataMatrix.GetLength(1))
                continue;
            if (!successCondition(dataMatrix[neighbour.x, neighbour.y]))
                return false;
        }
        return true;
    }
}
