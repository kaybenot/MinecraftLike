using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugInfo : MonoBehaviour
{
    [Header("Debug text objects")]
    [SerializeField] private TMP_Text worldPosText;
    [SerializeField] private TMP_Text chunkPosText;
    
    private void FixedUpdate()
    {
        worldPosText.text = GameManager.Player.BlockPosition.ToString();
        chunkPosText.text = GameManager.Player.ChunkPosition.ToString();
    }
}
