using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DomainWarping", menuName = "Engine/DomainWarping")]
public class DomainWarping : ScriptableObject
{
    /// <summary>
    /// Noise used for creating offsets.
    /// </summary>
    public CustomNoiseSettings NoiseDomainX, NoiseDomainY;
    /// <summary>
    /// Amplitudes creating warping effect.
    /// Increasing amplitude increases warp effect.
    /// </summary>
    public int AmplitudeX = 20, AmplitudeY = 20;

    /// <summary>
    /// Applies domain warping to noise.
    /// </summary>
    /// <param name="noiseSettings">Noise to warp</param>
    /// <returns>Warped noise</returns>
    public float GenerateDomainNoise(int x, int z, CustomNoiseSettings noiseSettings)
    {
        Vector2 domainOffset = GenerateDomainOffset(x, z);
        return CustomNoise.OctavePerlin(x + domainOffset.x, z + domainOffset.y, noiseSettings);
    }

    /// <summary>
    /// Creates offset used to warp noise.
    /// </summary>
    /// <returns>Domain offset</returns>
    public Vector2 GenerateDomainOffset(int x, int z)
    {
        float noiseX = CustomNoise.OctavePerlin(x, z, NoiseDomainX) * AmplitudeX;
        float noiseY = CustomNoise.OctavePerlin(x, z, NoiseDomainY) * AmplitudeY;
        return new Vector2(noiseX, noiseY);
    }
    
    public Vector2Int GenerateDomainOffsetInt(int x, int z)
    {
        return Vector2Int.RoundToInt(GenerateDomainOffset(x, z));
    }
}
