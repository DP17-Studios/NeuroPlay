using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main controller for the ReactionLights game
/// </summary>
public class ReactionLightsController : MonoBehaviour
{
    [Header("Game Configuration")]
    [SerializeField] private int attemptsPerSession = 3;
    [SerializeField] private float minRedLightDuration = 5f;
    [SerializeField] private float maxRedLightDuration = 10f;
    [SerializeField] private int numberOfLights = 5;
    
    [Header("UI References")]
    [SerializeField] private GameObject[] redLights;
    [SerializeField] private GameObject[] greenLights;
    [SerializeField] private Text instructionText;
    [SerializeField] private Text resultText;
    [SerializeField] private Text attemptCounterText;
    [SerializeField] private Text averageReactionTimeText;
    [SerializeField] private Button startButton;
    [SerializeField] private Image touchPanel;
    
    // Game state
    private enum GameState { Ready, RedLights, GreenLights, Results, Complete }
    private GameState currentState = GameState.Ready;
    private int currentAttempt = 0;
    private float reactionStartTime = 0f;
    private bool isTouchable = false;
    private List<float> reactionTimes = new List<float>();
    private List<bool> falseStarts = new List<bool>();
    
    // API integration
    private NeuroplaysApiClient apiClient;
    
    private void Awake()
    {
        apiClient = NeuroplaysApiClient.Instance;
    }
    
    private void Start()
    {
        // Initialize UI
        UpdateUI();
        
        // Set up touch panel event
        touchPanel.raycastTarget = true;
        Button touchButton = touchPanel.GetComponent<Button>();
        if (touchButton != null)
        {
            touchButton.onClick.AddListener(HandleScreenTap);
        }
        else
        {
            Debug.LogError("TouchPanel needs a Button component for input detection");
        }
        
        // Start button setup
        startButton.onClick.AddListener(StartGame);
        
        // Start API session
        StartCoroutine(apiClient.StartSession((success, message) => {
            if (success)
            {
                Debug.Log("ReactionLights session started: " + message);
            }
            else
            {
                Debug.LogError("Failed to start session: " + message);
            }
        }));
    }
    
    /// <summary>
    /// Starts a new game session
    /// </summary>
    public void StartGame()
    {
        currentAttempt = 0;
        reactionTimes.Clear();
        falseStarts.Clear();
        
        startButton.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
        averageReactionTimeText.gameObject.SetActive(false);
        
        StartNextAttempt();
    }
    
    /// <summary>
    /// Starts the next attempt in the sequence
    /// </summary>
    private void StartNextAttempt()
    {
        currentAttempt++;
        
        if (currentAttempt > attemptsPerSession)
        {
            CompleteSession();
            return;
        }
        
        // Update UI
        attemptCounterText.text = $"Attempt {currentAttempt} of {attemptsPerSession}";
        instructionText.text = "Wait for green lights!";
        
        // Reset lights
        SetAllLightsActive(redLights, false);
        SetAllLightsActive(greenLights, false);
        
        // Start the light sequence
        StartCoroutine(RunLightSequence());
    }
    
    /// <summary>
    /// Handles the F1-style light sequence
    /// </summary>
    private IEnumerator RunLightSequence()
    {
        currentState = GameState.RedLights;
        isTouchable = true; // Allow touch to detect false starts
        
        // Turn on red lights one by one
        for (int i = 0; i < numberOfLights; i++)
        {
            redLights[i].SetActive(true);
            yield return new WaitForSeconds(0.5f);
        }
        
        // Random wait time
        float waitTime = UnityEngine.Random.Range(minRedLightDuration, maxRedLightDuration);
        yield return new WaitForSeconds(waitTime);
        
        // Check if a false start occurred
        if (currentState == GameState.RedLights)
        {
            // Switch to green lights
            SetAllLightsActive(redLights, false);
            SetAllLightsActive(greenLights, true);
            
            currentState = GameState.GreenLights;
            reactionStartTime = Time.time;
            
            // Auto-fail after 5 seconds if no tap
            StartCoroutine(AutoFailAfterDelay(5f));
        }
    }
    
    /// <summary>
    /// Auto-fails the current attempt if no response within the given time
    /// </summary>
    private IEnumerator AutoFailAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (currentState == GameState.GreenLights)
        {
            // Add a very slow reaction time
            reactionTimes.Add(delay);
            falseStarts.Add(false);
            
            // Show result
            ShowAttemptResult($"Too slow! {delay:F2}s");
        }
    }
    
    /// <summary>
    /// Handles screen tap events
    /// </summary>
    public void HandleScreenTap()
    {
        if (!isTouchable) return;
        
        switch (currentState)
        {
            case GameState.RedLights:
                // False start!
                HandleFalseStart();
                break;
                
            case GameState.GreenLights:
                // Valid reaction
                float reactionTime = Time.time - reactionStartTime;
                reactionTimes.Add(reactionTime);
                falseStarts.Add(false);
                
                // Show result
                ShowAttemptResult($"Reaction time: {reactionTime:F3}s");
                break;
                
            default:
                // Ignore taps in other states
                break;
        }
    }
    
    /// <summary>
    /// Handles a false start
    /// </summary>
    private void HandleFalseStart()
    {
        currentState = GameState.Results;
        isTouchable = false;
        
        // Stop any running sequences
        StopAllCoroutines();
        
        // Record false start
        reactionTimes.Add(-1f); // Use -1 to indicate false start
        falseStarts.Add(true);
        
        // Show result
        ShowAttemptResult("False start! Disqualified.");
    }
    
    /// <summary>
    /// Shows the result of the current attempt
    /// </summary>
    private void ShowAttemptResult(string result)
    {
        currentState = GameState.Results;
        isTouchable = false;
        
        // Update UI
        instructionText.text = "Tap to continue";
        resultText.text = result;
        resultText.gameObject.SetActive(true);
        
        // Upload data for this attempt
        UploadAttemptData();
        
        // Wait for tap to continue
        StartCoroutine(WaitForTapToContinue());
    }
    
    /// <summary>
    /// Waits for user tap to continue to next attempt
    /// </summary>
    private IEnumerator WaitForTapToContinue()
    {
        yield return new WaitForSeconds(0.5f); // Prevent accidental taps
        
        while (true)
        {
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                StartNextAttempt();
                break;
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Completes the game session and shows final results
    /// </summary>
    private void CompleteSession()
    {
        currentState = GameState.Complete;
        
        // Calculate average reaction time (excluding false starts)
        float totalTime = 0f;
        int validAttempts = 0;
        
        for (int i = 0; i < reactionTimes.Count; i++)
        {
            if (!falseStarts[i] && reactionTimes[i] > 0)
            {
                totalTime += reactionTimes[i];
                validAttempts++;
            }
        }
        
        float averageTime = validAttempts > 0 ? totalTime / validAttempts : 0f;
        
        // Update UI
        instructionText.text = "Session Complete";
        resultText.text = $"Valid attempts: {validAttempts} of {attemptsPerSession}";
        resultText.gameObject.SetActive(true);
        
        if (validAttempts > 0)
        {
            averageReactionTimeText.text = $"Average reaction time: {averageTime:F3}s";
            averageReactionTimeText.gameObject.SetActive(true);
        }
        
        startButton.gameObject.SetActive(true);
        
        // Upload final session data and end session
        Dictionary<string, object> sessionSummary = new Dictionary<string, object>
        {
            { "average_reaction_time", averageTime },
            { "valid_attempts", validAttempts },
            { "total_attempts", attemptsPerSession },
            { "false_starts", falseStarts.FindAll(fs => fs).Count }
        };
        
        apiClient.AddSessionData("session_summary", sessionSummary);
        
        StartCoroutine(apiClient.UploadSessionData());
        StartCoroutine(apiClient.EndSession(Mathf.RoundToInt(validAttempts * 100 / attemptsPerSession), true, (success, message) => {
            if (success)
            {
                Debug.Log("Session ended successfully: " + message);
            }
            else
            {
                Debug.LogError("Failed to end session: " + message);
            }
        }));
    }
    
    /// <summary>
    /// Uploads data for the current attempt
    /// </summary>
    private void UploadAttemptData()
    {
        int index = currentAttempt - 1;
        if (index < 0 || index >= reactionTimes.Count) return;
        
        Dictionary<string, object> attemptData = new Dictionary<string, object>
        {
            { "attempt_number", currentAttempt },
            { "reaction_time", reactionTimes[index] },
            { "is_false_start", falseStarts[index] },
            { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };
        
        apiClient.AddSessionData($"attempt_{currentAttempt}", attemptData);
        StartCoroutine(apiClient.UploadSessionData());
    }
    
    /// <summary>
    /// Updates all UI elements based on current game state
    /// </summary>
    private void UpdateUI()
    {
        // Hide all lights initially
        SetAllLightsActive(redLights, false);
        SetAllLightsActive(greenLights, false);
        
        // Set initial text
        instructionText.text = "Press Start to begin";
        attemptCounterText.text = "";
        resultText.gameObject.SetActive(false);
        averageReactionTimeText.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Helper method to set all lights in an array active or inactive
    /// </summary>
    private void SetAllLightsActive(GameObject[] lights, bool active)
    {
        foreach (GameObject light in lights)
        {
            if (light != null)
            {
                light.SetActive(active);
            }
        }
    }
}