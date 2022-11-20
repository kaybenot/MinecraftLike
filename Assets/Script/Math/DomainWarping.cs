using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DomainWarping : MonoBehaviour
{
    [SerializeField] private CustomNoiseSettings noiseDomainX, noiseDomainY;
    [SerializeField] private int amplitudeX = 20, amplitudeY = 20;

    public float GenerateDomainNoise(int x, int z, CustomNoiseSettings defaultNoiseSettings)
    {
        Vector2 domainOffset = GenerateDomainOffset(x, z);
        return CustomNoise.OctavePerlin(x + domainOffset.x, z + domainOffset.y, defaultNoiseSettings);
    }

    public Vector2 GenerateDomainOffset(int x, int z)
    {
        float noiseX = CustomNoise.OctavePerlin(x, z, noiseDomainX) * amplitudeX;
        float noiseY = CustomNoise.OctavePerlin(x, z, noiseDomainY) * amplitudeY;
        return new Vector2(noiseX, noiseY);
    }

    public Vector2Int GenerateDomainOffsetInt(int x, int z)
    {
        return Vector2Int.RoundToInt(GenerateDomainOffset(x, z));
    }
}
