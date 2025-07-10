using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

/// <summary>
/// API client for communicating with the Neuroplay Django backend
/// </summary>
public class NeuroplaysApiClient : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string apiBaseUrl = "http://localhost:8000/";
    [SerializeField] private string authToken = "";
    
    [Header("Session Information")]
    [SerializeField] private int playerId;
    [SerializeField] private string gameName;
    [SerializeField] private string difficultyLevel = "medium";
    
    private int currentSessionId = -1;
    private Dictionary<string, object> sessionData = new Dictionary<string, object>();
    
    // Singleton instance
    private static NeuroplaysApiClient _instance;
    public static NeuroplaysApiClient Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<NeuroplaysApiClient>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("NeuroplaysApiClient");
                    _instance = go.AddComponent<NeuroplaysApiClient>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// Initialize the API client with configuration
    /// </summary>
    public void Initialize(string apiUrl, string token, int player, string game, string difficulty = "medium")
    {
        apiBaseUrl = apiUrl;
        authToken = token;
        playerId = player;
        gameName = game;
        difficultyLevel = difficulty;
        
        Debug.Log($"Neuroplay API Client initialized for game: {gameName}, player: {playerId}");
    }
    
    /// <summary>
    /// Start a new game session
    /// </summary>
    public IEnumerator StartSession(Action<bool, string> callback)
    {
        string url = $"{apiBaseUrl}games/start-session/";
        
        Dictionary<string, object> requestData = new Dictionary<string, object>
        {
            { "player_id", playerId },
            { "game_name", gameName },
            { "difficulty_level", difficultyLevel }
        };
        
        string jsonData = JsonConvert.SerializeObject(requestData);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Token {authToken}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Dictionary<string, object> response = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
                currentSessionId = Convert.ToInt32(response["id"]);
                Debug.Log($"Session started with ID: {currentSessionId}");
                callback(true, currentSessionId.ToString());
            }
            else
            {
                Debug.LogError($"Error starting session: {request.error}");
                callback(false, request.error);
            }
        }
    }
    
    /// <summary>
    /// End the current game session
    /// </summary>
    public IEnumerator EndSession(int score, bool completed, Action<bool, string> callback)
    {
        if (currentSessionId < 0)
        {
            Debug.LogError("No active session to end");
            callback(false, "No active session");
            yield break;
        }
        
        string url = $"{apiBaseUrl}games/end-session/{currentSessionId}/";
        
        Dictionary<string, object> requestData = new Dictionary<string, object>
        {
            { "score", score },
            { "completed", completed }
        };
        
        string jsonData = JsonConvert.SerializeObject(requestData);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Token {authToken}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Session {currentSessionId} ended successfully");
                
                // Upload any accumulated session data
                if (sessionData.Count > 0)
                {
                    yield return UploadSessionData(null);
                }
                
                currentSessionId = -1;
                sessionData.Clear();
                callback(true, "Session ended");
            }
            else
            {
                Debug.LogError($"Error ending session: {request.error}");
                callback(false, request.error);
            }
        }
    }
    
    /// <summary>
    /// Add data to the current session
    /// </summary>
    public void AddSessionData(string key, object value)
    {
        if (currentSessionId < 0)
        {
            Debug.LogWarning("No active session, data will be cached until session starts");
        }
        
        sessionData[key] = value;
    }
    
    /// <summary>
    /// Upload the current session data to the server
    /// </summary>
    public IEnumerator UploadSessionData(Action<bool, string> callback = null)
    {
        if (currentSessionId < 0)
        {
            Debug.LogError("No active session for data upload");
            if (callback != null) callback(false, "No active session");
            yield break;
        }
        
        if (sessionData.Count == 0)
        {
            Debug.LogWarning("No session data to upload");
            if (callback != null) callback(true, "No data to upload");
            yield break;
        }
        
        string url = $"{apiBaseUrl}games/upload-data/{currentSessionId}/";
        
        Dictionary<string, object> requestData = new Dictionary<string, object>
        {
            { "session_data", sessionData }
        };
        
        string jsonData = JsonConvert.SerializeObject(requestData);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Token {authToken}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Session data uploaded successfully");
                sessionData.Clear();
                if (callback != null) callback(true, "Data uploaded");
            }
            else
            {
                Debug.LogError($"Error uploading session data: {request.error}");
                if (callback != null) callback(false, request.error);
            }
        }
    }
    
    /// <summary>
    /// Get game configuration from the server
    /// </summary>
    public IEnumerator GetGameConfig(Action<bool, Dictionary<string, object>> callback)
    {
        string url = $"{apiBaseUrl}games/config/{gameName}/?player_id={playerId}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Token {authToken}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Dictionary<string, object> config = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
                Debug.Log("Game configuration retrieved successfully");
                callback(true, config);
            }
            else
            {
                Debug.LogError($"Error getting game config: {request.error}");
                callback(false, null);
            }
        }
    }
}