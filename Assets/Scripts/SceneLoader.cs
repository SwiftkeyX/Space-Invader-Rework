using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Single gateway for all scene transitions. Additively loads/unloads the
/// MainMenu / GameLogic / HUD scenes on top of the persistent Bootstrap scene.
/// No other system may call SceneManager.LoadScene; no DontDestroyOnLoad.
/// See SceneLoader.md GDD.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public const string MainMenu = "MainMenu";
    public const string GameLogic = "GameLogic";
    public const string Hud = "HUD";

    public enum BootTarget { None, MainMenu, Gameplay }

    [Tooltip("What Bootstrap loads on start. MainMenu for the real flow; Gameplay to jump straight into a run.")]
    [SerializeField] private BootTarget bootTarget = BootTarget.MainMenu;

    /// <summary>Raised after a requested scene set finishes loading. Payload: a label of the active set.</summary>
    public event Action<string> OnSceneLoaded;

    private bool _busy;

    public bool IsBusy => _busy;

    private void Start()
    {
        switch (bootTarget)
        {
            case BootTarget.MainMenu: LoadMainMenu(); break;
            case BootTarget.Gameplay: LoadGameplay(); break;
        }
    }

    /// <summary>Show the main menu (unloads gameplay scenes).</summary>
    public void LoadMainMenu()
    {
        RequestTransition("MainMenu", new[] { MainMenu }, new[] { GameLogic, Hud });
    }

    /// <summary>Start gameplay: load GameLogic + HUD together (unloads the menu).</summary>
    public void LoadGameplay()
    {
        RequestTransition("Gameplay", new[] { GameLogic, Hud }, new[] { MainMenu });
    }

    /// <summary>Restart gameplay: unload then reload GameLogic + HUD fresh.</summary>
    public void ReloadGameplay()
    {
        RequestTransition("Gameplay", new[] { GameLogic, Hud }, new[] { GameLogic, Hud });
    }

    /// <summary>Tear down gameplay scenes (e.g. back to menu).</summary>
    public void UnloadGameplay()
    {
        RequestTransition("None", Array.Empty<string>(), new[] { GameLogic, Hud });
    }

    private void RequestTransition(string label, string[] toLoad, string[] toUnload)
    {
        if (_busy)
        {
            Debug.LogWarning($"[SceneLoader] Transition '{label}' requested while busy; ignored.");
            return;
        }
        StartCoroutine(TransitionRoutine(label, toLoad, toUnload));
    }

    private IEnumerator TransitionRoutine(string label, string[] toLoad, string[] toUnload)
    {
        _busy = true;

        // Unload the old set first (Bootstrap is never in these lists, so it is never unloaded).
        foreach (var scene in toUnload)
        {
            if (IsLoaded(scene))
                yield return SceneManager.UnloadSceneAsync(scene);
        }

        // Additively load the target set.
        foreach (var scene in toLoad)
        {
            if (!IsLoaded(scene))
                yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        }

        _busy = false;
        OnSceneLoaded?.Invoke(label);
    }

    private static bool IsLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.name == sceneName && s.isLoaded) return true;
        }
        return false;
    }
}
