using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private int _sceneIndexToLoad = -1;
    private string _sceneNameToLoad = "";
    private Scene _sceneRefToLoad;

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
        _sceneIndexToLoad = sceneIndex;
        HandleLoad( onLoadedAction);
    }

    /// <summary> Queues scene to load and start loading screen </summary>
    public void LoadScene(string sceneName, Action onLoadedAction = null)
    {
        _sceneNameToLoad = sceneName;
        HandleLoad( onLoadedAction);
    }

    /// <summary> Queues scene to load and start loading screen </summary>
    public void LoadScene(Scene sceneToLoad, Action onLoadedAction = null)
    {
        _sceneRefToLoad = sceneToLoad;
        HandleLoad( onLoadedAction);
    }

    void HandleLoad(Action onLoadedAction = null){
        loadingAnimator.SetTrigger("LoadScene");
        nextAction = onLoadedAction;
    }

    /// <summary> Starts loading scene to load asynchronously </summary>
    public void StartLoadingScene()
    {
        if(_sceneIndexToLoad >= 0)
        {
            SceneManager.LoadSceneAsync(_sceneIndexToLoad);
        } 
        else if (_sceneNameToLoad != "")
        {
            SceneManager.LoadSceneAsync(_sceneNameToLoad);
        }
        else if (_sceneRefToLoad != null)
        {
            SceneManager.LoadSceneAsync(_sceneRefToLoad.name);
        }
        else
        {
            Logger.Error("No scene index or name was given to load!", this);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        loadingAnimator.SetTrigger("Unload");
        _sceneIndexToLoad = -1;
        _sceneNameToLoad = "";

        if (nextAction != null)
        {
            nextAction.Invoke();
            nextAction = null;
        }
    }
}
