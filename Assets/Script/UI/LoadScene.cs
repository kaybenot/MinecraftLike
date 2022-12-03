using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    [SerializeField] private int SceneIndex = 1;

    public void Load()
    {
        SceneManager.LoadScene(SceneIndex);
    }
}
