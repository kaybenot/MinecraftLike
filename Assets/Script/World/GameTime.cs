using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTime : MonoBehaviour
{
    [Header("Time settings")]
    [SerializeField, Min(0f)] private float timeScale = 20f;

    [Header("Time related objects")]
    [SerializeField] private GameObject sun;
    [SerializeField, Range(0f, 1f)] private float ambientIntensityNight = 0.15f;

    public static int Second => (int) time % 60;
    public static int Minute => (int) time / 60 % 60;
    public static int Hour => (int) time / 3600 % 24;
    public static int Day => (int) time / 86400;

    private static float time;
    private Color ambientColor;
    private float baseIntensity;
    private Light sunLight;

    private void Awake()
    {
        ambientColor = RenderSettings.ambientLight;
    }

    private void Start()
    {
        sunLight = sun.GetComponent<Light>();
        baseIntensity = sunLight.intensity;
        GameManager.World.WorldGenerator.OnWorldCreated += () => SetTime(0, 0, 12, 0);
    }

    private void FixedUpdate()
    {
        if (!GameManager.World.IsWorldCreated)
            return;
        
        time += Time.fixedDeltaTime * timeScale;
        updateSun();
        
        // Ambient lerp
        float rotation01 = sun.transform.localRotation.x;
        float t, sunT;
        if (rotation01 >= 0f && rotation01 <= 0.1f && sun.activeSelf)
        {
            t = CustomNoise.RemapValue(rotation01, 0f, 0.1f, ambientIntensityNight, 1f);
            sunT = CustomNoise.RemapValue(rotation01, 0f, 0.1f, 0f, 1f);
            RenderSettings.ambientLight = ambientColor * t;
            sunLight.intensity = baseIntensity * sunT;
        }
        else if (rotation01 >= 0.9f && rotation01 <= 1f && sun.activeSelf)
        {
            t = CustomNoise.RemapValue(rotation01, 0.9f, 1f, 1f, ambientIntensityNight);
            sunT = CustomNoise.RemapValue(rotation01, 0.9f, 1f, 1f, 0f);
            RenderSettings.ambientLight = ambientColor * t;
            sunLight.intensity = baseIntensity * sunT;
        }
    }

    public static void SetTime(int sec, int min, int h, int day)
    {
        time = sec + 60 * (min + 60 * (h + 24 * day));
    }

    private void updateSun()
    {
        sun.transform.localRotation = Quaternion.Euler(-90f + 360f * ((time % 86400f) / 86400f),
            0f,
            0f);
        if(sun.transform.localEulerAngles.x < 0f || sun.transform.localEulerAngles.x > 180f)
            sun.SetActive(false);
        else if(!sun.activeSelf)
            sun.SetActive(true);
    }
}
