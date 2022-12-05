using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenu : MonoBehaviour
{
    [SerializeField] private int sceneIndex = 0;
    
    public void Load()
    {
        Save.SaveWorld(0);
        SceneManager.LoadSceneAsync(sceneIndex);
    }
}
