using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BiomeGenerator : MonoBehaviour
{
    [SerializeField] private DomainWarping biomeWarping;
    [SerializeField] private CustomNoiseSettings biomeNoiseSettings;
    [SerializeField] private List<BiomeData> biomeDatas;

    public List<Vector3Int> BiomeCenters { get; private set; }

    private List<float> biomeNoise = new List<float>();
    

    public void GenerateBiomePoints(Vector2Int mapSeedOffset)
    {
        BiomeCenters = calculateBiomeCenters();
        for (int i = 0; i < BiomeCenters.Count; i++)
        {
            Vector2Int domainWarpingOffset = biomeWarping.GenerateDomainOffsetInt(BiomeCenters[i].x, BiomeCenters[i].z);
            BiomeCenters[i] += new Vector3Int(domainWarpingOffset.x, 0, domainWarpingOffset.y);
        }

        biomeNoise = calculateBiomeNoise(mapSeedOffset);
    }

    private List<float> calculateBiomeNoise(Vector2Int mapSeedOffset)
    {
        biomeNoiseSettings.WorldOffset = mapSeedOffset;
        return BiomeCenters.Select(center => CustomNoise.OctavePerlin(center.x, center.y, biomeNoiseSettings)).ToList();
    }
    
    private List<Vector3Int> calculateBiomeCenters()
    {
        Vector3Int playerPos = Vector3Int.zero; // TODO: Player position; GameManager.Player.BlockPosition;
        int renderDistance = GameManager.World.RenderDistance;
        int mapSize = GameManager.World.ChunkSize;
        int biomeLength = renderDistance * mapSize;

        Vector3Int origin = new Vector3Int(Mathf.RoundToInt(playerPos.x / (float) biomeLength) * biomeLength, 0,
            Mathf.RoundToInt(playerPos.z / (float) biomeLength) * biomeLength);
        HashSet<Vector3Int> biomeCenters = new HashSet<Vector3Int>();
        biomeCenters.Add(origin);
        foreach (var dir in Extensions.Vector2IntDirections())
        {
            Vector3Int biomePoint1 = new Vector3Int(origin.x + dir.x * biomeLength, 0, origin.z + dir.y * biomeLength);
            Vector3Int biomePoint2 =
                new Vector3Int(origin.x + dir.x * biomeLength, 0, origin.z + dir.y * 2 * biomeLength);
            Vector3Int biomePoint3 =
                new Vector3Int(origin.x + dir.x * 2 * biomeLength, 0, origin.z + dir.y * biomeLength);
            Vector3Int biomePoint4 =
                new Vector3Int(origin.x + dir.x * 2 * biomeLength, 0, origin.z + dir.y * 2 * biomeLength);
            biomeCenters.Add(biomePoint1);
            biomeCenters.Add(biomePoint2);
            biomeCenters.Add(biomePoint3);
            biomeCenters.Add(biomePoint4);
        }

        return new List<Vector3Int>(biomeCenters);
    }
}
