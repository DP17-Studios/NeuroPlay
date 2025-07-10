using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// Analyzes text and voice for emotional content in the EmotionEcho game
/// </summary>
public class EmotionAnalyzer : MonoBehaviour
{
    [Header("Emotion Analysis Settings")]
    [SerializeField] private bool useLocalAnalysis = true;
    [SerializeField] private TextAsset emotionKeywordsJson;
    [SerializeField] private float intensityThreshold = 0.3f;
    
    [Header("Voice Analysis")]
    [SerializeField] private bool analyzeVoiceFeatures = true;
    [SerializeField] private float pitchWeight = 0.4f;
    [SerializeField] private float volumeWeight = 0.3f;
    [SerializeField] private float speedWeight = 0.3f;
    
    // Callback delegate for emotion analysis results
    public delegate void EmotionAnalysisCallback(string emotion, float intensity);
    
    // Dictionary of emotion keywords for local analysis
    private Dictionary<string, List<string>> emotionKeywords = new Dictionary<string, List<string>>();
    
    // API client for server-side analysis
    private NeuroplaysApiClient apiClient;
    
    // Voice feature tracking
    private float baselinePitch = 1.0f;
    private float baselineVolume = 0.5f;
    private float baselineSpeechRate = 1.0f;
    
    private void Awake()
    {
        // Load emotion keywords if available
        if (emotionKeywordsJson != null)
        {
            LoadEmotionKeywords();
        }
        else if (useLocalAnalysis)
        {
            // Fallback to basic emotions if no JSON provided
            InitializeBasicEmotions();
        }
    }
    
    private void Start()
    {
        apiClient = NeuroplaysApiClient.Instance;
    }
    
    private void LoadEmotionKeywords()
    {
        try
        {
            // Parse JSON to get emotion keywords
            EmotionKeywordData data = JsonUtility.FromJson<EmotionKeywordData>(emotionKeywordsJson.text);
            
            if (data != null && data.emotions != null)
            {
                foreach (EmotionCategory category in data.emotions)
                {
                    emotionKeywords[category.name] = new List<string>(category.keywords);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading emotion keywords: {e.Message}");
            InitializeBasicEmotions();
        }
    }
    
    private void InitializeBasicEmotions()
    {
        // Basic emotion keywords as fallback
        emotionKeywords = new Dictionary<string, List<string>>
        {
            { "joy", new List<string> { "happy", "joy", "glad", "delighted", "pleased", "excited", "wonderful" } },
            { "sadness", new List<string> { "sad", "unhappy", "depressed", "down", "miserable", "upset", "grief" } },
            { "anger", new List<string> { "angry", "mad", "furious", "irritated", "annoyed", "frustrated" } },
            { "fear", new List<string> { "afraid", "scared", "frightened", "terrified", "anxious", "worried" } },
            { "surprise", new List<string> { "surprised", "shocked", "amazed", "astonished", "unexpected" } },
            { "disgust", new List<string> { "disgusted", "revolted", "repulsed", "gross", "yuck" } },
            { "neutral", new List<string> { "ok", "fine", "neutral", "average", "normal" } }
        };
    }
    
    public void AnalyzeText(string text, EmotionAnalysisCallback callback)
    {
        if (string.IsNullOrEmpty(text))
        {
            callback?.Invoke("neutral", 0.5f);
            return;
        }
        
        if (useLocalAnalysis)
        {
            // Perform local keyword-based analysis
            PerformLocalAnalysis(text, callback);
        }
        else
        {
            // Use server-side analysis
            StartCoroutine(PerformServerAnalysis(text, callback));
        }
    }
    
    public void AnalyzeVoice(AudioClip voiceClip, EmotionAnalysisCallback callback)
    {
        if (!analyzeVoiceFeatures || voiceClip == null)
        {
            callback?.Invoke("neutral", 0.5f);
            return;
        }
        
        // Extract voice features
        float pitch = ExtractPitch(voiceClip);
        float volume = ExtractVolume(voiceClip);
        float speechRate = ExtractSpeechRate(voiceClip);
        
        // Analyze voice features
        AnalyzeVoiceFeatures(pitch, volume, speechRate, callback);
    }
    
    private void PerformLocalAnalysis(string text, EmotionAnalysisCallback callback)
    {
        // Normalize and tokenize text
        string normalizedText = text.ToLower();
        string[] words = normalizedText.Split(new char[] { ' ', '.', ',', '!', '?', ';', ':', '-', '\n', '\r' }, 
                                             StringSplitOptions.RemoveEmptyEntries);
        
        // Count emotion keywords
        Dictionary<string, int> emotionCounts = new Dictionary<string, int>();
        Dictionary<string, float> emotionScores = new Dictionary<string, float>();
        
        foreach (var emotion in emotionKeywords.Keys)
        {
            emotionCounts[emotion] = 0;
            emotionScores[emotion] = 0f;
            
            foreach (string word in words)
            {
                if (emotionKeywords[emotion].Contains(word))
                {
                    emotionCounts[emotion]++;
                }
            }
            
            // Calculate score based on frequency and word count
            if (words.Length > 0)
            {
                emotionScores[emotion] = (float)emotionCounts[emotion] / words.Length;
            }
        }
        
        // Find dominant emotion
        string dominantEmotion = "neutral";
        float highestScore = intensityThreshold; // Minimum threshold to detect an emotion
        
        foreach (var emotion in emotionScores.Keys)
        {
            if (emotionScores[emotion] > highestScore)
            {
                highestScore = emotionScores[emotion];
                dominantEmotion = emotion;
            }
        }
        
        // Normalize intensity to 0-1 range
        float intensity = Mathf.Min(1.0f, highestScore * 2.0f); // Scale up for better differentiation
        
        // Return result
        callback?.Invoke(dominantEmotion, intensity);
    }
    
    private IEnumerator PerformServerAnalysis(string text, EmotionAnalysisCallback callback)
    {
        // Prepare request data
        Dictionary<string, object> requestData = new Dictionary<string, object>
        {
            { "text", text }
        };
        
        // This would be a real API call in a production implementation
        // For now, we'll simulate a server response after a delay
        yield return new WaitForSeconds(0.5f);
        
        // Simulate server response
        string[] possibleEmotions = { "joy", "sadness", "anger", "fear", "surprise", "disgust", "neutral" };
        string randomEmotion = possibleEmotions[UnityEngine.Random.Range(0, possibleEmotions.Length)];
        float randomIntensity = UnityEngine.Random.Range(0.3f, 0.9f);
        
        // Return result
        callback?.Invoke(randomEmotion, randomIntensity);
    }
    
    private float ExtractPitch(AudioClip clip)
    {
        // In a real implementation, this would analyze the audio data
        // For now, return a random value
        return UnityEngine.Random.Range(0.7f, 1.3f) * baselinePitch;
    }
    
    private float ExtractVolume(AudioClip clip)
    {
        // In a real implementation, this would calculate RMS amplitude
        // For now, return a random value
        return UnityEngine.Random.Range(0.3f, 0.8f);
    }
    
    private float ExtractSpeechRate(AudioClip clip)
    {
        // In a real implementation, this would count syllables per second
        // For now, return a random value
        return UnityEngine.Random.Range(0.7f, 1.3f) * baselineSpeechRate;
    }
    
    private void AnalyzeVoiceFeatures(float pitch, float volume, float speechRate, EmotionAnalysisCallback callback)
    {
        // Normalize relative to baseline
        float relativePitch = pitch / baselinePitch;
        float relativeVolume = volume / baselineVolume;
        float relativeSpeechRate = speechRate / baselineSpeechRate;
        
        // Calculate emotional features
        bool highPitch = relativePitch > 1.2f;
        bool lowPitch = relativePitch < 0.8f;
        bool loudVolume = relativeVolume > 1.2f;
        bool softVolume = relativeVolume < 0.8f;
        bool fastSpeech = relativeSpeechRate > 1.2f;
        bool slowSpeech = relativeSpeechRate < 0.8f;
        
        // Simple rule-based emotion detection
        string emotion = "neutral";
        float intensity = 0.5f;
        
        if (highPitch && loudVolume && fastSpeech)
        {
            emotion = "joy";
            intensity = 0.8f;
        }
        else if (highPitch && loudVolume)
        {
            emotion = "anger";
            intensity = 0.7f;
        }
        else if (lowPitch && slowSpeech)
        {
            emotion = "sadness";
            intensity = 0.6f;
        }
        else if (highPitch && fastSpeech)
        {
            emotion = "fear";
            intensity = 0.7f;
        }
        else if (highPitch)
        {
            emotion = "surprise";
            intensity = 0.6f;
        }
        else if (lowPitch && loudVolume)
        {
            emotion = "disgust";
            intensity = 0.6f;
        }
        
        // Return result
        callback?.Invoke(emotion, intensity);
    }
    
    // Calibrate baseline voice features for a specific user
    public void CalibrateVoiceBaseline(AudioClip neutralSpeech)
    {
        if (neutralSpeech != null)
        {
            baselinePitch = ExtractPitch(neutralSpeech);
            baselineVolume = ExtractVolume(neutralSpeech);
            baselineSpeechRate = ExtractSpeechRate(neutralSpeech);
        }
    }
}

// Classes for JSON deserialization
[Serializable]
public class EmotionKeywordData
{
    public EmotionCategory[] emotions;
}

[Serializable]
public class EmotionCategory
{
    public string name;
    public string[] keywords;
}