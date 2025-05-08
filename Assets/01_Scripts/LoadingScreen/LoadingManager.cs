using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    /// <summary> Loads scene with given index </summary>
    public void LoadScene(int sceneIndex)
    {
        LoadSceneStatic(sceneIndex);
    }

    /// <summary> Loads scene with given index </summary>
    public void LoadScene(string sceneName)
    {
        LoadSceneStatic(sceneName);
    }

    /// <summary> Loads scene with given index </summary>
    public void LoadScene(Scene sceneRef)
    {
        LoadSceneStatic(sceneRef);
    }


    public static void LoadSceneStatic(int sceneIndex, Action nextAction = null)
    {
        if (sceneIndex < 0) return;

        SceneLoader sceneLoader = SceneLoader.Instance;
        if (!sceneLoader)
        {
            return;
        }

        sceneLoader.LoadScene(sceneIndex, nextAction);
    }

    public static void LoadSceneStatic(Scene sceneToLoad, Action nextAction = null)
    {
        if (sceneToLoad == null) return;

        SceneLoader sceneLoader = SceneLoader.Instance;
        if (!sceneLoader)
        {
            return;
        }

        sceneLoader.LoadScene(sceneToLoad, nextAction);
    }

    public static void LoadSceneStatic(string sceneName, Action nextAction = null)
    {
        if (sceneName == "") return;

        SceneLoader sceneLoader = SceneLoader.Instance;
        if (!sceneLoader)
        {
            return;
        }

        sceneLoader.LoadScene(sceneName, nextAction);
    }
}
