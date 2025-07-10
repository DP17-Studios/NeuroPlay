using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages score calculation and display for the NeuroSprint game
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI attentionScoreText;
    [SerializeField] private TextMeshProUGUI timeRemainingText;
    [SerializeField] private Slider attentionMeter;
    
    [Header("Score Settings")]
    [SerializeField] private int pointsPerObstacleAvoided = 10;
    [SerializeField] private int pointsPerDistractionIgnored = 5;
    [SerializeField] private int penaltyPerObstacleHit = 5;
    [SerializeField] private int penaltyPerDistractionTriggered = 2;
    [SerializeField] private float distanceScoreMultiplier = 0.1f;
    
    // Runtime variables
    private int currentScore = 0;
    private int highScore = 0;
    private float attentionScore = 100f;
    private float gameTime = 0f;
    private float distanceTraveled = 0f;
    
    // References
    private NeuroSprintController gameController;
    
    private void Awake()
    {
        // Find game controller
        gameController = FindObjectOfType<NeuroSprintController>();
        
        // Load high score from player prefs
        highScore = PlayerPrefs.GetInt("NeuroSprint_HighScore", 0);
    }
    
    private void Start()
    {
        // Initialize UI
        UpdateScoreUI();
        UpdateHighScoreUI();
        UpdateAttentionUI();
    }
    
    public void StartGame(float gameDuration)
    {
        // Reset variables
        currentScore = 0;
        attentionScore = 100f;
        gameTime = gameDuration;
        distanceTraveled = 0f;
        
        // Update UI
        UpdateScoreUI();
        UpdateAttentionUI();
        UpdateTimeUI();
    }
    
    private void Update()
    {
        // Update game time
        if (gameTime > 0)
        {
            gameTime -= Time.deltaTime;
            UpdateTimeUI();
        }
        
        // Accumulate distance traveled (simplified)
        distanceTraveled += Time.deltaTime;
    }
    
    public void AddObstacleAvoided()
    {
        // Add points for avoiding an obstacle
        AddScore(pointsPerObstacleAvoided);
        
        // Increase attention score
        ModifyAttentionScore(2f);
    }
    
    public void AddObstacleHit()
    {
        // Subtract points for hitting an obstacle
        AddScore(-penaltyPerObstacleHit);
        
        // Decrease attention score
        ModifyAttentionScore(-10f);
    }
    
    public void AddDistractionIgnored()
    {
        // Add points for ignoring a distraction
        AddScore(pointsPerDistractionIgnored);
        
        // Increase attention score
        ModifyAttentionScore(1f);
    }
    
    public void AddDistractionTriggered()
    {
        // Subtract points for being distracted
        AddScore(-penaltyPerDistractionTriggered);
        
        // Decrease attention score
        ModifyAttentionScore(-5f);
    }
    
    public void AddDistanceScore()
    {
        // Add points based on distance traveled
        int distancePoints = Mathf.FloorToInt(distanceTraveled * distanceScoreMultiplier);
        AddScore(distancePoints);
    }
    
    private void AddScore(int points)
    {
        currentScore = Mathf.Max(0, currentScore + points);
        UpdateScoreUI();
        
        // Check for high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("NeuroSprint_HighScore", highScore);
            UpdateHighScoreUI();
        }
    }
    
    private void ModifyAttentionScore(float amount)
    {
        attentionScore = Mathf.Clamp(attentionScore + amount, 0f, 100f);
        UpdateAttentionUI();
    }
    
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }
    
    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {highScore}";
        }
    }
    
    private void UpdateAttentionUI()
    {
        if (attentionScoreText != null)
        {
            attentionScoreText.text = $"Attention: {Mathf.RoundToInt(attentionScore)}%";
        }
        
        if (attentionMeter != null)
        {
            attentionMeter.value = attentionScore / 100f;
            
            // Change color based on attention level
            if (attentionMeter.fillRect != null)
            {
                Image fillImage = attentionMeter.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    if (attentionScore > 75f)
                    {
                        fillImage.color = Color.green;
                    }
                    else if (attentionScore > 40f)
                    {
                        fillImage.color = Color.yellow;
                    }
                    else
                    {
                        fillImage.color = Color.red;
                    }
                }
            }
        }
    }
    
    private void UpdateTimeUI()
    {
        if (timeRemainingText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60);
            int seconds = Mathf.FloorToInt(gameTime % 60);
            timeRemainingText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }
    
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    public float GetAttentionScore()
    {
        return attentionScore;
    }
    
    public float GetRemainingTime()
    {
        return gameTime;
    }
}