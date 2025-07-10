using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Controller for the NeuroSprint game - ADHD and attention training
/// </summary>
public class NeuroSprintController : MonoBehaviour
{
    [Header("Game Configuration")]
    [SerializeField] private float gameSpeed = 5f;
    [SerializeField] private float obstacleSpawnRate = 2f;
    [SerializeField] private float distractionSpawnRate = 3f;
    [SerializeField] private float gameTimeSeconds = 120f;
    
    [Header("References")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject distractionPrefab;
    [SerializeField] private Transform obstacleSpawnPoint;
    
    // Game state
    private bool gameRunning = false;
    private float timeRemaining;
    private int score = 0;
    private int obstaclesAvoided = 0;
    private int obstaclesHit = 0;
    private int distractionsIgnored = 0;
    private int distractionsTriggered = 0;
    
    // Metrics for ADHD analysis
    private List<float> reactionTimes = new List<float>();
    private List<float> attentionScores = new List<float>();
    private float lastObstacleTime;
    private float attentionScore = 100f;
    
    // API client reference
    private NeuroplaysApiClient apiClient;
    
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
    }
    
    private void ApplyGameConfig(Dictionary<string, object> config)
    {
        if (config.ContainsKey("game_speed"))
            gameSpeed = System.Convert.ToSingle(config["game_speed"]);
            
        if (config.ContainsKey("obstacle_spawn_rate"))
            obstacleSpawnRate = System.Convert.ToSingle(config["obstacle_spawn_rate"]);
            
        if (config.ContainsKey("distraction_spawn_rate"))
            distractionSpawnRate = System.Convert.ToSingle(config["distraction_spawn_rate"]);
            
        if (config.ContainsKey("game_time_seconds"))
            gameTimeSeconds = System.Convert.ToSingle(config["game_time_seconds"]);
            
        Debug.Log("Applied game configuration from server");
    }
    
    private void StartGame()
    {
        gameRunning = true;
        timeRemaining = gameTimeSeconds;
        score = 0;
        obstaclesAvoided = 0;
        obstaclesHit = 0;
        distractionsIgnored = 0;
        distractionsTriggered = 0;
        reactionTimes.Clear();
        attentionScores.Clear();
        attentionScore = 100f;
        
        // Start spawning obstacles and distractions
        StartCoroutine(SpawnObstacles());
        StartCoroutine(SpawnDistractions());
        
        Debug.Log("NeuroSprint game started");
    }
    
    private void Update()
    {
        if (!gameRunning)
            return;
            
        // Update game timer
        timeRemaining -= Time.deltaTime;
        
        // Check for game over
        if (timeRemaining <= 0)
        {
            EndGame(true);
        }
        
        // Decay attention score slightly over time (natural attention loss)
        attentionScore = Mathf.Max(0, attentionScore - (Time.deltaTime * 0.5f));
        
        // Record attention score periodically
        if (Time.frameCount % 300 == 0) // Every ~5 seconds at 60fps
        {
            attentionScores.Add(attentionScore);
        }
    }
    
    private IEnumerator SpawnObstacles()
    {
        while (gameRunning)
        {
            yield return new WaitForSeconds(Random.Range(obstacleSpawnRate * 0.7f, obstacleSpawnRate * 1.3f));
            
            if (!gameRunning)
                yield break;
                
            // Spawn obstacle
            GameObject obstacle = Instantiate(obstaclePrefab, obstacleSpawnPoint.position, Quaternion.identity);
            
            // Record time for reaction time measurement
            lastObstacleTime = Time.time;
        }
    }
    
    private IEnumerator SpawnDistractions()
    {
        while (gameRunning)
        {
            yield return new WaitForSeconds(Random.Range(distractionSpawnRate * 0.7f, distractionSpawnRate * 1.3f));
            
            if (!gameRunning)
                yield break;
                
            // Spawn distraction
            Vector3 spawnPos = obstacleSpawnPoint.position + new Vector3(Random.Range(-5f, 5f), Random.Range(-3f, 3f), 0);
            GameObject distraction = Instantiate(distractionPrefab, spawnPos, Quaternion.identity);
        }
    }
    
    // Called when player successfully avoids an obstacle
    public void OnObstacleAvoided()
    {
        obstaclesAvoided++;
        score += 10;
        
        // Calculate reaction time (time between obstacle spawn and successful avoidance)
        float reactionTime = Time.time - lastObstacleTime;
        reactionTimes.Add(reactionTime);
        
        // Increase attention score for good performance
        attentionScore = Mathf.Min(100f, attentionScore + 2f);
        
        // Upload periodic data
        if (obstaclesAvoided % 10 == 0)
        {
            UploadGameMetrics();
        }
    }
    
    // Called when player hits an obstacle
    public void OnObstacleHit()
    {
        obstaclesHit++;
        score = Mathf.Max(0, score - 5);
        
        // Decrease attention score for poor performance
        attentionScore = Mathf.Max(0, attentionScore - 10f);
    }
    
    // Called when player ignores a distraction
    public void OnDistractionIgnored()
    {
        distractionsIgnored++;
        score += 5;
        
        // Increase attention score for ignoring distractions
        attentionScore = Mathf.Min(100f, attentionScore + 1f);
    }
    
    // Called when player is distracted
    public void OnDistractionTriggered()
    {
        distractionsTriggered++;
        score = Mathf.Max(0, score - 2);
        
        // Decrease attention score for being distracted
        attentionScore = Mathf.Max(0, attentionScore - 5f);
    }
    
    private void EndGame(bool completed)
    {
        gameRunning = false;
        
        // Upload final metrics
        UploadGameMetrics();
        
        // End the session
        StartCoroutine(apiClient.EndSession(score, completed, (success, message) => {
            if (success)
            {
                Debug.Log("Game session ended successfully");
            }
            else
            {
                Debug.LogError($"Error ending game session: {message}");
            }
        }));
        
        Debug.Log($"Game over! Final score: {score}");
    }
    
    private void UploadGameMetrics()
    {
        // Calculate average reaction time
        float avgReactionTime = reactionTimes.Count > 0 ? reactionTimes.Average() : 0;
        
        // Calculate attention consistency (standard deviation of attention scores)
        float attentionConsistency = 0;
        if (attentionScores.Count > 1)
        {
            float mean = attentionScores.Average();
            float sumSquaredDiff = attentionScores.Sum(score => Mathf.Pow(score - mean, 2));
            attentionConsistency = Mathf.Sqrt(sumSquaredDiff / (attentionScores.Count - 1));
        }
        
        // Prepare data for upload
        apiClient.AddSessionData("score", score);
        apiClient.AddSessionData("obstacles_avoided", obstaclesAvoided);
        apiClient.AddSessionData("obstacles_hit", obstaclesHit);
        apiClient.AddSessionData("distractions_ignored", distractionsIgnored);
        apiClient.AddSessionData("distractions_triggered", distractionsTriggered);
        apiClient.AddSessionData("reaction_times", reactionTimes);
        apiClient.AddSessionData("avg_reaction_time", avgReactionTime);
        apiClient.AddSessionData("attention_score", attentionScore);
        apiClient.AddSessionData("attention_consistency", attentionConsistency);
        
        // Upload data
        StartCoroutine(apiClient.UploadSessionData());
    }
}