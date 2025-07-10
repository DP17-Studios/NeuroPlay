using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages the user interface for the NeuroSprint game
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Main Menu")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI highScoreText;
    
    [Header("Gameplay UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Slider attentionMeter;
    [SerializeField] private Button pauseButton;
    
    [Header("Pause Menu")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    
    [Header("Game Over")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI attentionScoreText;
    [SerializeField] private TextMeshProUGUI reactionTimeText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button exitButton;
    
    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Button tutorialNextButton;
    [SerializeField] private Button tutorialSkipButton;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private Sprite[] tutorialSprites;
    
    // References
    private NeuroSprintController gameController;
    private ScoreManager scoreManager;
    
    // Tutorial state
    private int currentTutorialStep = 0;
    private string[] tutorialSteps = new string[]
    {
        "Welcome to NeuroSprint! This game will test your attention and reaction time.",
        "Use the arrow keys or WASD to move. Up/W to jump, Down/S to slide.",
        "Avoid obstacles to earn points. The longer you survive, the higher your score!",
        "Watch out for distractions! Ignore them to keep your attention score high.",
        "Your attention score affects your performance. Stay focused!",
        "Ready to play? Let's go!"
    };
    
    private void Awake()
    {
        // Find references
        gameController = FindObjectOfType<NeuroSprintController>();
        scoreManager = FindObjectOfType<ScoreManager>();
        
        // Set up button listeners
        SetupButtonListeners();
    }
    
    private void Start()
    {
        // Show main menu initially
        ShowMainMenu();
        
        // Update high score
        UpdateHighScore();
    }
    
    private void SetupButtonListeners()
    {
        // Main Menu
        if (startButton != null) startButton.onClick.AddListener(OnStartButtonClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitButtonClicked);
        
        // Gameplay
        if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseButtonClicked);
        
        // Pause Menu
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeButtonClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartButtonClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        
        // Game Over
        if (playAgainButton != null) playAgainButton.onClick.AddListener(OnPlayAgainButtonClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitButtonClicked);
        
        // Tutorial
        if (tutorialNextButton != null) tutorialNextButton.onClick.AddListener(OnTutorialNextButtonClicked);
        if (tutorialSkipButton != null) tutorialSkipButton.onClick.AddListener(OnTutorialSkipButtonClicked);
    }
    
    #region Panel Management
    
    public void ShowMainMenu()
    {
        SetActivePanel(mainMenuPanel);
    }
    
    public void ShowGameplayPanel()
    {
        SetActivePanel(gameplayPanel);
    }
    
    public void ShowPausePanel()
    {
        SetActivePanel(pausePanel);
        Time.timeScale = 0f; // Pause the game
    }
    
    public void ShowGameOverPanel()
    {
        SetActivePanel(gameOverPanel);
        
        // Update final score info
        if (finalScoreText != null && scoreManager != null)
        {
            finalScoreText.text = $"Final Score: {scoreManager.GetCurrentScore()}";
        }
        
        if (attentionScoreText != null && scoreManager != null)
        {
            attentionScoreText.text = $"Attention Score: {Mathf.RoundToInt(scoreManager.GetAttentionScore())}%";
        }
        
        // Reaction time would come from the game controller in a real implementation
        if (reactionTimeText != null)
        {
            reactionTimeText.text = "Average Reaction Time: 0.75s";
        }
    }
    
    public void ShowTutorial()
    {
        SetActivePanel(tutorialPanel);
        currentTutorialStep = 0;
        UpdateTutorialUI();
    }
    
    private void SetActivePanel(GameObject panel)
    {
        // Deactivate all panels
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        
        // Activate the specified panel
        if (panel != null) panel.SetActive(true);
    }
    
    #endregion
    
    #region Button Handlers
    
    private void OnStartButtonClicked()
    {
        // Show tutorial first
        ShowTutorial();
    }
    
    private void OnSettingsButtonClicked()
    {
        // Show settings panel (not implemented in this example)
        Debug.Log("Settings button clicked");
    }
    
    private void OnQuitButtonClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    private void OnPauseButtonClicked()
    {
        ShowPausePanel();
    }
    
    private void OnResumeButtonClicked()
    {
        ShowGameplayPanel();
        Time.timeScale = 1f; // Resume the game
    }
    
    private void OnRestartButtonClicked()
    {
        Time.timeScale = 1f; // Ensure normal time scale
        // Restart the game
        if (gameController != null)
        {
            // In a real implementation, you would call a restart method on the game controller
            Debug.Log("Restart game");
        }
        ShowGameplayPanel();
    }
    
    private void OnMainMenuButtonClicked()
    {
        Time.timeScale = 1f; // Ensure normal time scale
        ShowMainMenu();
    }
    
    private void OnPlayAgainButtonClicked()
    {
        // Restart the game
        if (gameController != null)
        {
            // In a real implementation, you would call a restart method on the game controller
            Debug.Log("Play again");
        }
        ShowGameplayPanel();
    }
    
    private void OnExitButtonClicked()
    {
        ShowMainMenu();
    }
    
    private void OnTutorialNextButtonClicked()
    {
        currentTutorialStep++;
        
        if (currentTutorialStep >= tutorialSteps.Length)
        {
            // Tutorial complete, start the game
            StartGame();
        }
        else
        {
            UpdateTutorialUI();
        }
    }
    
    private void OnTutorialSkipButtonClicked()
    {
        // Skip tutorial and start game
        StartGame();
    }
    
    #endregion
    
    private void StartGame()
    {
        ShowGameplayPanel();
        
        // Start the game
        if (gameController != null)
        {
            // In a real implementation, you would call a start method on the game controller
            Debug.Log("Start game");
        }
    }
    
    private void UpdateTutorialUI()
    {
        if (tutorialText != null && currentTutorialStep < tutorialSteps.Length)
        {
            tutorialText.text = tutorialSteps[currentTutorialStep];
        }
        
        if (tutorialImage != null && tutorialSprites != null && tutorialSprites.Length > 0)
        {
            int imageIndex = Mathf.Min(currentTutorialStep, tutorialSprites.Length - 1);
            tutorialImage.sprite = tutorialSprites[imageIndex];
        }
    }
    
    private void UpdateHighScore()
    {
        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("NeuroSprint_HighScore", 0);
            highScoreText.text = $"High Score: {highScore}";
        }
    }
    
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
    
    public void UpdateTime(float timeRemaining)
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timeText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }
    
    public void UpdateAttentionMeter(float attentionValue)
    {
        if (attentionMeter != null)
        {
            attentionMeter.value = attentionValue / 100f;
        }
    }
}