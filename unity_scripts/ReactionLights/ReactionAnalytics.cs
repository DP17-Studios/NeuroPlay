using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles analytics for the ReactionLights game
/// </summary>
public class ReactionAnalytics : MonoBehaviour
{
    [Serializable]
    public class ReactionData
    {
        public int attemptNumber;
        public float reactionTime; // -1 indicates false start
        public bool isFalseStart;
        public DateTime timestamp;
        
        public ReactionData(int attempt, float time, bool falseStart)
        {
            attemptNumber = attempt;
            reactionTime = time;
            isFalseStart = falseStart;
            timestamp = DateTime.Now;
        }
    }
    
    private List<ReactionData> sessionData = new List<ReactionData>();
    private NeuroplaysApiClient apiClient;
    
    // Historical data across sessions
    private List<float> historicalReactionTimes = new List<float>();
    private int totalFalseStarts = 0;
    private int totalAttempts = 0;
    
    private void Awake()
    {
        apiClient = NeuroplaysApiClient.Instance;
    }
    
    /// <summary>
    /// Logs a reaction time for the current attempt
    /// </summary>
    public void LogReactionTime(int attemptNumber, float reactionTime, bool isFalseStart)
    {
        ReactionData data = new ReactionData(attemptNumber, reactionTime, isFalseStart);
        sessionData.Add(data);
        
        // Add to historical data
        if (!isFalseStart && reactionTime > 0)
        {
            historicalReactionTimes.Add(reactionTime);
        }
        
        if (isFalseStart)
        {
            totalFalseStarts++;
        }
        
        totalAttempts++;
        
        // Send to backend
        Dictionary<string, object> attemptData = new Dictionary<string, object>
        {
            { "attempt_number", attemptNumber },
            { "reaction_time_ms", isFalseStart ? -1 : reactionTime * 1000 }, // Convert to milliseconds
            { "is_false_start", isFalseStart },
            { "timestamp", data.timestamp.ToString("yyyy-MM-dd HH:mm:ss") }
        };
        
        apiClient.AddSessionData($"attempt_{attemptNumber}", attemptData);
    }
    
    /// <summary>
    /// Calculates the average reaction time for the current session
    /// </summary>
    public float CalculateSessionAverage()
    {
        float totalTime = 0f;
        int validCount = 0;
        
        foreach (ReactionData data in sessionData)
        {
            if (!data.isFalseStart && data.reactionTime > 0)
            {
                totalTime += data.reactionTime;
                validCount++;
            }
        }
        
        return validCount > 0 ? totalTime / validCount : 0f;
    }
    
    /// <summary>
    /// Calculates the average reaction time across all historical sessions
    /// </summary>
    public float CalculateHistoricalAverage()
    {
        if (historicalReactionTimes.Count == 0) return 0f;
        
        float totalTime = 0f;
        foreach (float time in historicalReactionTimes)
        {
            totalTime += time;
        }
        
        return totalTime / historicalReactionTimes.Count;
    }
    
    /// <summary>
    /// Calculates the percentage of false starts
    /// </summary>
    public float CalculateFalseStartPercentage()
    {
        if (totalAttempts == 0) return 0f;
        return (float)totalFalseStarts / totalAttempts * 100f;
    }
    
    /// <summary>
    /// Calculates the best (fastest) reaction time
    /// </summary>
    public float GetBestReactionTime()
    {
        if (historicalReactionTimes.Count == 0) return 0f;
        
        float best = float.MaxValue;
        foreach (float time in historicalReactionTimes)
        {
            if (time < best && time > 0)
            {
                best = time;
            }
        }
        
        return best != float.MaxValue ? best : 0f;
    }
    
    /// <summary>
    /// Uploads complete session analytics to the backend
    /// </summary>
    public void UploadSessionSummary()
    {
        float sessionAvg = CalculateSessionAverage();
        float historicalAvg = CalculateHistoricalAverage();
        float falseStartPercent = CalculateFalseStartPercentage();
        float bestTime = GetBestReactionTime();
        
        Dictionary<string, object> summary = new Dictionary<string, object>
        {
            { "session_average_ms", sessionAvg * 1000 }, // Convert to milliseconds
            { "historical_average_ms", historicalAvg * 1000 },
            { "false_start_percentage", falseStartPercent },
            { "best_reaction_time_ms", bestTime * 1000 },
            { "total_attempts", totalAttempts },
            { "valid_attempts", historicalReactionTimes.Count },
            { "false_starts", totalFalseStarts },
            { "session_timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };
        
        apiClient.AddSessionData("session_analytics", summary);
        StartCoroutine(apiClient.UploadSessionData());
        
        // Reset session data
        sessionData.Clear();
    }
}