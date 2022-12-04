using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private RectTransform fillRT;
    [SerializeField] private TMP_Text text;
    [SerializeField] private float margin = 5f;

    public float CurrentProgress { get; private set; } = 0f;
    
    private float width;

    void Start()
    {
        RectTransform rt = GetComponent<RectTransform>();
        width = rt.rect.width - 2 * margin;
    }

    public void SetDescription(string desc)
    {
        text.text = desc;
    }

    public void SetProgress(float percent)
    {
        fillRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * percent);
        CurrentProgress = percent;
    }

    public void Coroutine(IEnumerator routine)
    {
        StartCoroutine(routine);
    }
}
