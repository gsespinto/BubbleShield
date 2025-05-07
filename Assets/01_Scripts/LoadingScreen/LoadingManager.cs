using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    /// <summary> Loads scene with given index </summary>
    public void LoadScene(int sceneIndex)
    {
        SceneLoader sceneLoader = SceneLoader.Instance;
        if(!sceneLoader)
        {
            return;
        }

        sceneLoader.LoadScene(sceneIndex);
    }

    public static void LoadSceneStatic(int sceneIndex, Action nextAction = null)
    {
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

        sceneLoader.LoadScene(sceneToLoad.buildIndex, nextAction);
    }

    public static void LoadSceneStatic(string scenePath, Action nextAction = null)
    {
        Scene sceneToLoad = SceneManager.GetSceneByPath(scenePath);
        if (sceneToLoad == null) return;

        SceneLoader sceneLoader = SceneLoader.Instance;
        if (!sceneLoader)
        {
            return;
        }

        sceneLoader.LoadScene(sceneToLoad.buildIndex, nextAction);
    }
}
