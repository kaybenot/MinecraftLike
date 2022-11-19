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
    public static float RemapValue(float value, float currMin, float currMax, float outMin, float outMax)
    {
        return outMin + (value - currMin) * (outMax - outMin) / (currMax - currMin);
    }
    
    public static float RemapValue01(float value, float outMin, float outMax)
    {
        return outMin + value * (outMax - outMin);
    }

    public static int RemapValue01Int(float value, float outMin, float outMax)
    {
        return (int) RemapValue01(value, outMin, outMax);
    }

    public static float Redistribution(float noise, CustomNoiseSettings settings)
    {
        return Mathf.Pow(noise * settings.RedistributionModifier, settings.Exponent);
    }
    
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
