using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Custom Noise Settings", menuName = "Engine/Custom Noise Settings")]
public class CustomNoiseSettings : ScriptableObject
{
    public float NoiseZoom;
    public int Octaves;
    public Vector2Int Offset;
    public Vector2Int WorldOffset;
    public float Persistance;
    public float RedistributionModifier;
    public float Exponent;
}

public static class CustomNoise
{
    /// <summary>
    /// Remaps value to specific range.
    /// </summary>
    /// <param name="value">Current value</param>
    /// <param name="currMin">Min from</param>
    /// <param name="currMax">Max from</param>
    /// <param name="outMin">Min to</param>
    /// <param name="outMax">Max To</param>
    /// <returns>Remapped value</returns>
    public static float RemapValue(float value, float currMin, float currMax, float outMin, float outMax)
    {
        return outMin + (value - currMin) * (outMax - outMin) / (currMax - currMin);
    }
    
    /// <summary>
    /// Remaps value to 0-1 range.
    /// </summary>
    /// <returns>Remapped value</returns>
    public static float RemapValue01(float value, float outMin, float outMax)
    {
        return outMin + value * (outMax - outMin);
    }

    /// <summary>
    /// Remaps value to 0-1 range.
    /// </summary>
    /// <returns>Floored int value</returns>
    public static int RemapValue01Int(float value, float outMin, float outMax)
    {
        return (int) RemapValue01(value, outMin, outMax);
    }

    /// <summary>
    /// Increases intensity of irregularities in range around 1-2.
    /// Bigger or lower values can behave in opposite way.
    /// Can excess 0-1 range!
    /// </summary>
    /// <param name="noise">Noise value</param>
    /// <param name="settings">Noise settings</param>
    /// <returns>Redistributed value</returns>
    public static float Redistribution(float noise, CustomNoiseSettings settings)
    {
        return Mathf.Pow(noise * settings.RedistributionModifier, settings.Exponent);
    }
    
    /// <summary>
    /// Octave-Perlin algorithm increasing details.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="settings"></param>
    /// <returns>0-1 noise value</returns>
    public static float OctavePerlin(float x, float z, CustomNoiseSettings settings)
    {
        x *= settings.NoiseZoom;
        z *= settings.NoiseZoom;
        x += settings.NoiseZoom;
        z += settings.NoiseZoom;

        float total = 0f;
        float frequency = 1f;
        float amplitude = 1f;
        float amplitudeSum = 0f;
        for (int i = 0; i < settings.Octaves; i++)
        {
            total += Mathf.PerlinNoise((settings.Offset.x + settings.WorldOffset.x + x) * frequency,
                (settings.Offset.y + settings.WorldOffset.y + z) * frequency) * amplitude;

            amplitudeSum += amplitude;
            amplitude *= settings.Persistance;
            frequency *= 2;
        }

        return total / amplitudeSum;
    }
}
