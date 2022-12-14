using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenu : MonoBehaviour
{
    [SerializeField] private int sceneIndex = 0;
    
    public void Load()
    {
        Save.SaveWorld(GameState.SaveSlot);
        SceneManager.LoadSceneAsync(sceneIndex);
    }
}
