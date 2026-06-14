using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Presentation layer only — no gameplay logic. Renders HUD (score, lives, level),
/// the between-level power-up pick screen, and the game-over/victory end screen.
/// Lives in the HUD scene (loaded additively with GameLogic). Subscribes to contract
/// events and relays UI intent (restart, power-up pick) back as calls/events.
/// See UIManager.md GDD.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class UIManager : MonoBehaviour
{
    public event Action OnRestartRequested;

    // VisualElement handles — set once on bind
    private VisualElement _hud;
    private Label _livesLabel;
    private Label _levelLabel;
    private Label _scoreLabel;
    private Label _levelBanner;

    private VisualElement _pickScreen;
    private VisualElement _cardsContainer;

    private VisualElement _endScreen;
    private Label _resultLabel;
    private Label _finalScoreLabel;
    private Button _restartButton;

    private UIDocument _doc;
    private ScoreSystem _scoreSystem;
    private PowerUpSystem _powerUpSystem;
    private bool _bound;
    private bool _restartPending;

    // Juice: track previous lives to detect decrease
    private int _prevLives = -1;

    private void Awake()
    {
        _doc = GetComponent<UIDocument>();
    }

    private void Start()
    {
        // One-time cross-scene lookups (GameLogic loads before HUD — safe to Find here).
        _scoreSystem   = FindFirstObjectByType<ScoreSystem>();
        _powerUpSystem = FindFirstObjectByType<PowerUpSystem>();

        // Subscribe immediately so no events are missed during the bind wait.
        SubscribeToEvents();

        // UIDocument creates rootVisualElement synchronously but populates UXML children
        // asynchronously — wait until the tree is ready before querying elements.
        StartCoroutine(BindWhenReady());
    }

    private IEnumerator BindWhenReady()
    {
        while (_doc == null || _doc.rootVisualElement == null || _doc.rootVisualElement.childCount == 0)
            yield return null;
        BindUI();
        ReflectCurrentState();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    // -------------------------------------------------------------------------
    // Binding
    // -------------------------------------------------------------------------

    private void BindUI()
    {
        if (_doc == null || _doc.rootVisualElement == null) return;
        var root = _doc.rootVisualElement;

        _hud        = root.Q("hud");
        _livesLabel = root.Q<Label>("lives-label");

        _levelLabel  = root.Q<Label>("level-label");
        _scoreLabel  = root.Q<Label>("score-label");
        _levelBanner = root.Q<Label>("level-banner");

        _pickScreen     = root.Q("pick-screen");
        _cardsContainer = root.Q("cards-container");

        _endScreen        = root.Q("end-screen");
        _resultLabel      = root.Q<Label>("result-label");
        _finalScoreLabel  = root.Q<Label>("final-score-label");
        _restartButton    = root.Q<Button>("restart-button");

        if (_restartButton != null)
            _restartButton.clicked += HandleRestartClicked;

        _bound = true;
        SetPanel(_hud, true);
        SetPanel(_pickScreen, false);
        SetPanel(_endScreen, false);

        // Banner starts hidden
        if (_levelBanner != null)
            _levelBanner.style.display = DisplayStyle.None;
    }

    // -------------------------------------------------------------------------
    // Event subscriptions
    // -------------------------------------------------------------------------

    private void SubscribeToEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRunStarted   += HandleRunStarted;
            GameManager.Instance.OnRunEnded     += HandleRunEnded;
            GameManager.Instance.OnLivesChanged += HandleLivesChanged;
            GameManager.Instance.OnLevelChanged += HandleLevelChanged;
        }
        if (_scoreSystem   != null) _scoreSystem.OnScoreChanged     += HandleScoreChanged;
        if (_powerUpSystem != null) _powerUpSystem.OnPowerUpOffered += HandlePowerUpOffered;
    }

    private void UnsubscribeFromEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRunStarted   -= HandleRunStarted;
            GameManager.Instance.OnRunEnded     -= HandleRunEnded;
            GameManager.Instance.OnLivesChanged -= HandleLivesChanged;
            GameManager.Instance.OnLevelChanged -= HandleLevelChanged;
        }
        if (_scoreSystem   != null) _scoreSystem.OnScoreChanged     -= HandleScoreChanged;
        if (_powerUpSystem != null) _powerUpSystem.OnPowerUpOffered -= HandlePowerUpOffered;
    }

    // -------------------------------------------------------------------------
    // State reflection (handles events that fired before HUD loaded)
    // -------------------------------------------------------------------------

    private void ReflectCurrentState()
    {
        if (GameManager.Instance == null) return;
        var gm = GameManager.Instance;

        HandleLivesChanged(gm.Lives);
        HandleLevelChanged(gm.CurrentLevelIndex);
        if (_scoreSystem != null) HandleScoreChanged(_scoreSystem.Score);

        switch (gm.State)
        {
            case GameState.Running:
                ShowHUD();
                break;
            case GameState.Lost:
            case GameState.Won:
                ShowEndScreen(gm.State, _scoreSystem != null ? _scoreSystem.Score : 0);
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Event handlers
    // -------------------------------------------------------------------------

    private void HandleRunStarted()   { Time.timeScale = 1f; ShowHUD(); }
    private void HandleRunEnded(GameState result) => ShowEndScreen(result, _scoreSystem != null ? _scoreSystem.Score : 0);

    private void HandleLivesChanged(int lives)
    {
        SetText(_livesLabel, LivesString(lives));

        // Juice: flash red when lives decrease
        if (_bound && _livesLabel != null && _prevLives > 0 && lives < _prevLives)
            StartCoroutine(FlashElement(_livesLabel, new Color(1f, 0.2f, 0.2f), 0.3f));

        _prevLives = lives;
    }

    private void HandleLevelChanged(int level)
    {
        SetText(_levelLabel, $"LEVEL {level}");

        // Juice: show level banner briefly during gameplay
        if (_bound && _levelBanner != null &&
            GameManager.Instance != null && GameManager.Instance.State == GameState.Running)
        {
            _levelBanner.text = $"LEVEL {level}";
            StartCoroutine(ShowBanner(_levelBanner, 1.5f));
        }
    }

    private void HandleScoreChanged(int score)
    {
        SetText(_scoreLabel, score.ToString("N0"));

        // Juice: flash gold when score increases
        if (_bound && _scoreLabel != null)
            StartCoroutine(FlashElement(_scoreLabel, new Color(1f, 0.9f, 0.2f), 0.15f));
    }

    private void HandlePowerUpOffered(PowerUpData[] offer)
    {
        if (!_bound || _cardsContainer == null) return;
        _cardsContainer.Clear();
        foreach (var data in offer)
            _cardsContainer.Add(BuildCard(data));
        SetPanel(_pickScreen, true);
        Time.timeScale = 0f;
    }

    private void HandleRestartClicked()
    {
        if (_restartPending) return;
        _restartPending = true;
        OnRestartRequested?.Invoke(); // GameManager subscribes and calls RequestRestart
    }

    // -------------------------------------------------------------------------
    // Juice animations
    // -------------------------------------------------------------------------

    /// <summary>Briefly tints a VisualElement to flashColor then restores USS default.</summary>
    private IEnumerator FlashElement(VisualElement el, Color flashColor, float duration)
    {
        el.style.color = new StyleColor(flashColor);
        yield return new WaitForSecondsRealtime(duration);
        el.style.color = StyleKeyword.Null; // restore to USS-defined value
    }

    /// <summary>Shows the level banner for displayDuration seconds then hides it.</summary>
    private IEnumerator ShowBanner(Label banner, float displayDuration)
    {
        banner.style.display = DisplayStyle.Flex;
        yield return new WaitForSecondsRealtime(displayDuration);
        banner.style.display = DisplayStyle.None;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void ShowHUD()
    {
        if (!_bound) return;
        SetPanel(_pickScreen, false);
        SetPanel(_endScreen, false);
    }

    private void ShowEndScreen(GameState result, int finalScore)
    {
        if (!_bound) return;
        SetPanel(_pickScreen, false);
        SetPanel(_endScreen, true);
        _restartPending = false;

        if (_resultLabel != null)
            _resultLabel.text = result == GameState.Won ? "YOU WIN!" : "GAME OVER";
        if (_finalScoreLabel != null)
            _finalScoreLabel.text = $"Score: {finalScore:N0}";
    }

    private VisualElement BuildCard(PowerUpData data)
    {
        var card = new VisualElement();
        card.AddToClassList("powerup-card");

        var name = new Label(data.displayName);
        name.AddToClassList("card-name");

        var desc = new Label(data.description);
        desc.AddToClassList("card-desc");

        card.Add(name);
        card.Add(desc);

        // Capture for lambda closure
        var captured = data;
        card.RegisterCallback<ClickEvent>(_ => OnCardSelected(captured));

        return card;
    }

    private void OnCardSelected(PowerUpData chosen)
    {
        Time.timeScale = 1f;
        SetPanel(_pickScreen, false);
        _powerUpSystem?.SelectPowerUp(chosen);
    }

    private static void SetPanel(VisualElement panel, bool visible)
    {
        if (panel == null) return;
        panel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private static void SetText(Label label, string text)
    {
        if (label != null) label.text = text;
    }

    private static string LivesString(int lives)
    {
        if (lives <= 0) return "—";
        return new string('♥', Mathf.Clamp(lives, 0, 8));
    }
}
