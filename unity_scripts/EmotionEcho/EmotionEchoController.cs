using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Main controller for the EmotionEcho game - Emotional Health and Depression Monitoring
/// </summary>
public class EmotionEchoController : MonoBehaviour
{
    [Header("Game Configuration")]
    [SerializeField] private float sessionDuration = 600f; // 10 minutes
    [SerializeField] private int minResponsesRequired = 5;
    [SerializeField] private float minResponseInterval = 30f; // Minimum time between responses
    
    [Header("References")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private VoiceRecognitionManager voiceRecognitionManager;
    [SerializeField] private EmotionAnalyzer emotionAnalyzer;
    [SerializeField] private StoryManager storyManager;
    [SerializeField] private UIManager uiManager;
    
    // Game state
    private bool gameRunning = false;
    private float timeRemaining;
    private int responsesGiven = 0;
    private float lastResponseTime = 0f;
    
    // Emotion tracking
    private List<string> playerResponses = new List<string>();
    private List<EmotionData> detectedEmotions = new List<EmotionData>();
    private float overallMoodScore = 50f; // 0-100, where 0 is very negative, 100 is very positive
    private float emotionalVariability = 0f;
    
    // API client reference
    private NeuroplaysApiClient apiClient;
    
    // Struct to store emotion data
    [Serializable]
    public struct EmotionData
    {
        public string emotion; // joy, sadness, anger, fear, neutral, etc.
        public float intensity; // 0-1 scale
        public float timestamp; // When the emotion was detected
        public string triggerText; // What dialogue triggered this emotion
        public string responseText; // What the player said
    }
    
    private void Awake()
    {
        // Initialize components if not set in inspector
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();
            
        if (voiceRecognitionManager == null)
            voiceRecognitionManager = FindObjectOfType<VoiceRecognitionManager>();
            
        if (emotionAnalyzer == null)
            emotionAnalyzer = FindObjectOfType<EmotionAnalyzer>();
            
        if (storyManager == null)
            storyManager = FindObjectOfType<StoryManager>();
            
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
    }
    
    private void Start()
    {
        apiClient = NeuroplaysApiClient.Instance;
        
        // Get game configuration from the server
        StartCoroutine(apiClient.GetGameConfig((success, config) => {
            if (success && config != null && config.ContainsKey("merged_config"))
            {
                Dictionary<string, object> gameConfig = config["merged_config"] as Dictionary<string, object>;
                ApplyGameConfig(gameConfig);
            }
            
            // Start the game session
            StartCoroutine(apiClient.StartSession((sessionSuccess, sessionId) => {
                if (sessionSuccess)
                {
                    StartGame();
                }
                else
                {
                    Debug.LogError("Failed to start game session. Playing in offline mode.");
                    StartGame();
                }
            }));
        }));
        
        // Set up event listeners
        voiceRecognitionManager.OnSpeechRecognized += HandleSpeechRecognized;
        dialogueManager.OnDialoguePresented += HandleDialoguePresented;
        storyManager.OnStoryNodeChanged += HandleStoryNodeChanged;
    }
    
    private void OnDestroy()
    {
        // Clean up event listeners
        voiceRecognitionManager.OnSpeechRecognized -= HandleSpeechRecognized;
        dialogueManager.OnDialoguePresented -= HandleDialoguePresented;
        storyManager.OnStoryNodeChanged -= HandleStoryNodeChanged;
    }
    
    private void ApplyGameConfig(Dictionary<string, object> config)
    {
        if (config.ContainsKey("session_duration"))
            sessionDuration = Convert.ToSingle(config["session_duration"]);
            
        if (config.ContainsKey("min_responses_required"))
            minResponsesRequired = Convert.ToInt32(config["min_responses_required"]);
            
        if (config.ContainsKey("min_response_interval"))
            minResponseInterval = Convert.ToSingle(config["min_response_interval"]);
            
        // Pass relevant config to other managers
        if (dialogueManager != null)
            dialogueManager.ApplyConfig(config);
            
        if (storyManager != null)
            storyManager.ApplyConfig(config);
            
        Debug.Log("Applied game configuration from server");
    }
    
    private void StartGame()
    {
        gameRunning = true;
        timeRemaining = sessionDuration;
        responsesGiven = 0;
        lastResponseTime = -minResponseInterval; // Allow immediate first response
        playerResponses.Clear();
        detectedEmotions.Clear();
        overallMoodScore = 50f;
        emotionalVariability = 0f;
        
        // Start the story
        storyManager.StartStory();
        
        // Start UI
        uiManager.UpdateTimeRemaining(timeRemaining);
        uiManager.UpdateResponsesCount(responsesGiven, minResponsesRequired);
        uiManager.UpdateMoodIndicator(overallMoodScore);
        
        Debug.Log("EmotionEcho game started");
    }
    
    private void Update()
    {
        if (!gameRunning)
            return;
            
        // Update game timer
        timeRemaining -= Time.deltaTime;
        uiManager.UpdateTimeRemaining(timeRemaining);
        
        // Check for game over conditions
        if (timeRemaining <= 0 || (responsesGiven >= minResponsesRequired && storyManager.IsAtEndNode()))
        {
            EndGame(true);
        }
    }
    
    private void HandleSpeechRecognized(string recognizedText)
    {
        if (!gameRunning)
            return;
            
        // Check if enough time has passed since last response
        if (Time.time - lastResponseTime < minResponseInterval)
        {
            Debug.Log("Response too soon after previous one");
            return;
        }
        
        // Record the response
        playerResponses.Add(recognizedText);
        responsesGiven++;
        lastResponseTime = Time.time;
        
        // Update UI
        uiManager.UpdateResponsesCount(responsesGiven, minResponsesRequired);
        
        // Analyze emotion in the response
        StartCoroutine(AnalyzeEmotion(recognizedText, dialogueManager.GetCurrentDialogue()));
        
        // Advance the dialogue
        dialogueManager.PlayerResponded(recognizedText);
    }
    
    private void HandleDialoguePresented(string dialogueText)
    {
        // Start listening for player response
        voiceRecognitionManager.StartListening();
        
        // Upload periodic data
        if (responsesGiven > 0 && responsesGiven % 5 == 0)
        {
            UploadGameMetrics();
        }
    }
    
    private void HandleStoryNodeChanged(string nodeId, string nodeType)
    {
        // If we reach an emotional branch point, use the current mood to influence the path
        if (nodeType == "emotional_branch")
        {
            string emotionalPath = DetermineEmotionalPath();
            storyManager.ChooseEmotionalPath(emotionalPath);
        }
    }
    
    private IEnumerator AnalyzeEmotion(string responseText, string triggerText)
    {
        // Local analysis first
        emotionAnalyzer.AnalyzeText(responseText, (emotion, intensity) => {
            // Create emotion data entry
            EmotionData emotionData = new EmotionData
            {
                emotion = emotion,
                intensity = intensity,
                timestamp = Time.time,
                triggerText = triggerText,
                responseText = responseText
            };
            
            // Add to our list
            detectedEmotions.Add(emotionData);
            
            // Update mood score based on emotion
            UpdateMoodScore(emotion, intensity);
            
            // Update UI
            uiManager.UpdateMoodIndicator(overallMoodScore);
            uiManager.ShowEmotionFeedback(emotion, intensity);
        });
        
        yield return null;
    }
    
    private void UpdateMoodScore(string emotion, float intensity)
    {
        // Simple mapping of emotions to mood score changes
        float moodChange = 0f;
        
        switch (emotion.ToLower())
        {
            case "joy":
            case "happiness":
            case "excitement":
                moodChange = 10f * intensity;
                break;
                
            case "contentment":
            case "calm":
                moodChange = 5f * intensity;
                break;
                
            case "neutral":
                moodChange = 0f;
                break;
                
            case "anxiety":
            case "fear":
                moodChange = -5f * intensity;
                break;
                
            case "sadness":
            case "grief":
                moodChange = -8f * intensity;
                break;
                
            case "anger":
            case "disgust":
                moodChange = -10f * intensity;
                break;
                
            default:
                moodChange = 0f;
                break;
        }
        
        // Apply the change
        float previousMood = overallMoodScore;
        overallMoodScore = Mathf.Clamp(overallMoodScore + moodChange, 0f, 100f);
        
        // Track emotional variability (how much the mood fluctuates)
        emotionalVariability += Mathf.Abs(overallMoodScore - previousMood);
    }
    
    private string DetermineEmotionalPath()
    {
        // Determine which emotional path to take based on current mood
        if (overallMoodScore >= 70f)
            return "positive";
        else if (overallMoodScore <= 30f)
            return "negative";
        else
            return "neutral";
    }
    
    private void EndGame(bool completed)
    {
        gameRunning = false;
        
        // Upload final metrics
        UploadGameMetrics();
        
        // End the session
        StartCoroutine(apiClient.EndSession(Mathf.RoundToInt(overallMoodScore), completed, (success, message) => {
            if (success)
            {
                Debug.Log("Game session ended successfully");
            }
            else
            {
                Debug.LogError($"Error ending game session: {message}");
            }
        }));
        
        // Show results
        uiManager.ShowResults(overallMoodScore, emotionalVariability, detectedEmotions);
        
        Debug.Log($"Game over! Final mood score: {overallMoodScore}");
    }
    
    private void UploadGameMetrics()
    {
        // Calculate emotional metrics
        float emotionDiversity = CalculateEmotionDiversity();
        Dictionary<string, int> emotionCounts = CountEmotions();
        
        // Calculate mood shift (how much the mood changed from start to end)
        float moodShift = overallMoodScore - 50f; // Starting mood was 50
        
        // Prepare data for upload
        apiClient.AddSessionData("mood_score", overallMoodScore);
        apiClient.AddSessionData("emotional_variability", emotionalVariability);
        apiClient.AddSessionData("emotion_diversity", emotionDiversity);
        apiClient.AddSessionData("mood_shift", moodShift);
        apiClient.AddSessionData("emotion_counts", emotionCounts);
        apiClient.AddSessionData("player_responses", playerResponses);
        apiClient.AddSessionData("detected_emotions", detectedEmotions);
        apiClient.AddSessionData("responses_given", responsesGiven);
        
        // Upload data
        StartCoroutine(apiClient.UploadSessionData());
    }
    
    private float CalculateEmotionDiversity()
    {
        // Count unique emotions detected
        HashSet<string> uniqueEmotions = new HashSet<string>();
        foreach (EmotionData emotion in detectedEmotions)
        {
            uniqueEmotions.Add(emotion.emotion);
        }
        
        // Calculate diversity score (0-1)
        float maxPossibleEmotions = 8f; // Assuming 8 basic emotions
        return Mathf.Min(1f, uniqueEmotions.Count / maxPossibleEmotions);
    }
    
    private Dictionary<string, int> CountEmotions()
    {
        // Count occurrences of each emotion
        Dictionary<string, int> counts = new Dictionary<string, int>();
        
        foreach (EmotionData emotion in detectedEmotions)
        {
            if (counts.ContainsKey(emotion.emotion))
                counts[emotion.emotion]++;
            else
                counts[emotion.emotion] = 1;
        }
        
        return counts;
    }
}