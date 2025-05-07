using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private int _sceneToLoad;
    [SerializeField] private Animator loadingAnimator;
    public static SceneLoader Instance;

    private Action nextAction;

    void Awake()
    {
        // If there's a loading screen already
        // Destroy this one
        if (Instance != null)
        { 
            Destroy(this.gameObject);
        }
        else
        {
            // Make this object always loaded
            DontDestroyOnLoad(this);
            Instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    /// <summary> Queues scene to load and start loading screen </summary>
    public void LoadScene(int sceneIndex, Action onLoadedAction = null)
    {
        _sceneToLoad = sceneIndex;
        loadingAnimator.SetTrigger("LoadScene");
        nextAction = onLoadedAction;
    }

    /// <summary> Starts loading scene to load asynchronously </summary>
    public void StartLoadingScene()
    {
        SceneManager.LoadSceneAsync(_sceneToLoad);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        loadingAnimator.SetTrigger("Unload");

        if (nextAction != null)
        {
            nextAction.Invoke();
            nextAction = null;
        }
    }
}
