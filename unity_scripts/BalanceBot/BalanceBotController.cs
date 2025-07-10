using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main controller for the BalanceBot game - Motor Coordination Trainer
/// </summary>
public class BalanceBotController : MonoBehaviour
{
    [Header("Game Configuration")]
    [SerializeField] private float sessionDuration = 180f; // 3 minutes
    [SerializeField] private float difficultyIncreaseInterval = 30f; // Increase difficulty every 30 seconds
    [SerializeField] private int maxDifficultyLevel = 5;
    [SerializeField] private bool adaptiveDifficulty = true;
    
    [Header("Balance Platform")]
    [SerializeField] private Transform balancePlatform;
    [SerializeField] private float platformTiltSpeed = 2.0f;
    [SerializeField] private float maxTiltAngle = 15.0f;
    [SerializeField] private float autoTiltAmount = 0.5f; // How much the platform tilts automatically
    [SerializeField] private float autoTiltSpeed = 0.2f; // Speed of automatic tilting
    
    [Header("Robot")]
    [SerializeField] private Transform robotModel;
    [SerializeField] private float robotFallThreshold = 30.0f; // Angle at which robot falls
    [SerializeField] private float robotRecoveryTime = 2.0f; // Time to recover after falling
    
    [Header("Obstacles")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private float obstacleSpawnInterval = 5.0f;
    [SerializeField] private float obstacleSpeed = 2.0f;
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private TextMeshProUGUI stabilityText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Slider stabilityMeter;
    
    [Header("Audio")]
    [SerializeField] private AudioClip balanceSound;
    [SerializeField] private AudioClip fallSound;
    [SerializeField] private AudioClip obstacleHitSound;
    [SerializeField] private AudioClip levelUpSound;
    [SerializeField] private AudioClip gameOverSound;
    
    // Game state
    private bool gameRunning = false;
    private float timeRemaining;
    private int score = 0;
    private int currentDifficultyLevel = 1;
    private float timeSinceLastDifficultyIncrease = 0f;
    private float timeSinceLastObstacleSpawn = 0f;
    private bool robotFallen = false;
    private float robotRecoveryTimer = 0f;
    
    // Input tracking
    private Vector2 currentTilt = Vector2.zero;
    private Vector2 targetTilt = Vector2.zero;
    private Vector2 autoTilt = Vector2.zero;
    
    // Performance metrics
    private float totalStabilityTime = 0f;
    private int fallCount = 0;
    private int obstaclesAvoided = 0;
    private int obstaclesHit = 0;
    private List<float> reactionTimes = new List<float>();
    private float lastImbalanceTime = 0f;
    private float currentStability = 100f;
    private List<PerformanceSnapshot> performanceHistory = new List<PerformanceSnapshot>();
    
    // Sensors and input
    private bool usingMobileControls = false;
    private Vector3 initialAcceleration;
    private bool calibrated = false;
    
    // API client
    private NeuroplaysApiClient apiClient;
    
    // Audio source
    private AudioSource audioSource;
    
    // Performance data structure
    [Serializable]
    private class PerformanceSnapshot
    {
        public float timeStamp;
        public float stability;
        public int difficultyLevel;
        public Vector2 platformTilt;
        public bool robotFallen;
        public float reactionTime;
    }
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        gameOverPanel.SetActive(false);
        
        // Check if we're on mobile
        usingMobileControls = Application.platform == RuntimePlatform.Android || 
                             Application.platform == RuntimePlatform.IPhonePlayer;
    }
    
    private void Start()
    {
        apiClient = NeuroplaysApiClient.Instance;
        
        // Get game configuration from server
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
        if (config.ContainsKey("session_duration"))
            sessionDuration = Convert.ToSingle(config["session_duration"]);
            
        if (config.ContainsKey("difficulty_increase_interval"))
            difficultyIncreaseInterval = Convert.ToSingle(config["difficulty_increase_interval"]);
            
        if (config.ContainsKey("max_difficulty_level"))
            maxDifficultyLevel = Convert.ToInt32(config["max_difficulty_level"]);
            
        if (config.ContainsKey("adaptive_difficulty"))
            adaptiveDifficulty = Convert.ToBoolean(config["adaptive_difficulty"]);
            
        if (config.ContainsKey("platform_tilt_speed"))
            platformTiltSpeed = Convert.ToSingle(config["platform_tilt_speed"]);
            
        if (config.ContainsKey("max_tilt_angle"))
            maxTiltAngle = Convert.ToSingle(config["max_tilt_angle"]);
            
        if (config.ContainsKey("obstacle_spawn_interval"))
            obstacleSpawnInterval = Convert.ToSingle(config["obstacle_spawn_interval"]);
            
        if (config.ContainsKey("obstacle_speed"))
            obstacleSpeed = Convert.ToSingle(config["obstacle_speed"]);
            
        Debug.Log("Applied game configuration from server");
    }
    
    private void StartGame()
    {
        gameRunning = true;
        timeRemaining = sessionDuration;
        score = 0;
        currentDifficultyLevel = 1;
        timeSinceLastDifficultyIncrease = 0f;
        timeSinceLastObstacleSpawn = 0f;
        robotFallen = false;
        robotRecoveryTimer = 0f;
        currentTilt = Vector2.zero;
        targetTilt = Vector2.zero;
        autoTilt = Vector2.zero;
        totalStabilityTime = 0f;
        fallCount = 0;
        obstaclesAvoided = 0;
        obstaclesHit = 0;
        reactionTimes.Clear();
        lastImbalanceTime = 0f;
        currentStability = 100f;
        performanceHistory.Clear();
        
        // Reset platform and robot
        if (balancePlatform != null)
            balancePlatform.rotation = Quaternion.identity;
            
        if (robotModel != null)
            robotModel.localRotation = Quaternion.identity;
            
        // Clear any existing obstacles
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Obstacle"))
            {
                Destroy(child.gameObject);
            }
        }
        
        // Calibrate accelerometer if using mobile
        if (usingMobileControls)
        {
            StartCoroutine(CalibrateAccelerometer());
        }
        
        // Update UI
        UpdateUI();
        
        Debug.Log("BalanceBot game started");
    }
    
    private IEnumerator CalibrateAccelerometer()
    {
        // Wait for a moment to get stable readings
        yield return new WaitForSeconds(1.0f);
        
        // Take initial reading as baseline
        initialAcceleration = Input.acceleration;
        calibrated = true;
        
        Debug.Log($"Accelerometer calibrated. Initial: {initialAcceleration}");
    }
    
    private void Update()
    {
        if (!gameRunning)
            return;
            
        // Update game timer
        timeRemaining -= Time.deltaTime;
        UpdateUI();
        
        // Check for game over
        if (timeRemaining <= 0)
        {
            EndGame();
            return;
        }
        
        // Handle input
        HandleInput();
        
        // Update platform tilt
        UpdatePlatformTilt();
        
        // Check robot balance
        CheckRobotBalance();
        
        // Handle difficulty progression
        UpdateDifficulty();
        
        // Spawn obstacles
        UpdateObstacles();
        
        // Take performance snapshot every second
        if (Time.frameCount % 60 == 0)
        {
            TakePerformanceSnapshot();
        }
        
        // Upload data periodically
        if (Time.frameCount % 300 == 0) // Every 5 seconds at 60fps
        {
            UploadGameMetrics(false);
        }
    }
    
    private void HandleInput()
    {
        if (robotFallen)
            return;
            
        if (usingMobileControls && calibrated)
        {
            // Mobile tilt controls using accelerometer
            Vector3 acceleration = Input.acceleration - initialAcceleration;
            targetTilt.x = Mathf.Clamp(acceleration.x * 2.0f, -1.0f, 1.0f);
            targetTilt.y = Mathf.Clamp(acceleration.y * 2.0f, -1.0f, 1.0f);
        }
        else
        {
            // Keyboard/mouse controls
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            
            targetTilt.x = Mathf.Clamp(horizontalInput, -1.0f, 1.0f);
            targetTilt.y = Mathf.Clamp(verticalInput, -1.0f, 1.0f);
        }
    }
    
    private void UpdatePlatformTilt()
    {
        if (balancePlatform == null)
            return;
            
        // Update auto-tilt (difficulty-based random tilting)
        float autoTiltFactor = autoTiltAmount * (currentDifficultyLevel / (float)maxDifficultyLevel);
        autoTilt.x += (UnityEngine.Random.value * 2 - 1) * autoTiltSpeed * autoTiltFactor * Time.deltaTime;
        autoTilt.y += (UnityEngine.Random.value * 2 - 1) * autoTiltSpeed * autoTiltFactor * Time.deltaTime;
        
        // Clamp auto-tilt
        autoTilt.x = Mathf.Clamp(autoTilt.x, -autoTiltFactor, autoTiltFactor);
        autoTilt.y = Mathf.Clamp(autoTilt.y, -autoTiltFactor, autoTiltFactor);
        
        // Combine player input with auto-tilt
        Vector2 combinedTilt = targetTilt + autoTilt;
        
        // Smoothly interpolate current tilt toward target
        currentTilt = Vector2.Lerp(currentTilt, combinedTilt, platformTiltSpeed * Time.deltaTime);
        
        // Apply tilt to platform
        float xAngle = currentTilt.y * maxTiltAngle; // Forward/backward tilt (around X axis)
        float zAngle = -currentTilt.x * maxTiltAngle; // Left/right tilt (around Z axis)
        
        balancePlatform.rotation = Quaternion.Euler(xAngle, 0, zAngle);
        
        // Update stability based on tilt
        float tiltMagnitude = currentTilt.magnitude;
        float stabilityFactor = 1.0f - (tiltMagnitude / 1.0f); // 1.0 is max possible tilt magnitude
        currentStability = Mathf.Lerp(currentStability, stabilityFactor * 100f, Time.deltaTime * 2f);
        
        // Check for imbalance
        if (tiltMagnitude > 0.7f && !robotFallen)
        {
            if (lastImbalanceTime == 0f)
            {
                // Start tracking reaction time
                lastImbalanceTime = Time.time;
            }
        }
        else if (tiltMagnitude < 0.3f && lastImbalanceTime > 0f)
        {
            // Calculate reaction time
            float reactionTime = Time.time - lastImbalanceTime;
            reactionTimes.Add(reactionTime);
            lastImbalanceTime = 0f;
            
            // Add points for quick recovery
            if (reactionTime < 1.0f)
            {
                AddPoints(5);
            }
        }
    }
    
    private void CheckRobotBalance()
    {
        if (robotModel == null)
            return;
            
        if (robotFallen)
        {
            // Handle recovery
            robotRecoveryTimer -= Time.deltaTime;
            
            if (robotRecoveryTimer <= 0)
            {
                // Reset robot
                robotFallen = false;
                robotModel.localRotation = Quaternion.identity;
            }
            
            return;
        }
        
        // Calculate robot tilt based on platform tilt
        float xAngle = currentTilt.y * (maxTiltAngle * 1.2f); // Exaggerate for visual effect
        float zAngle = -currentTilt.x * (maxTiltAngle * 1.2f);
        
        // Apply some lag to robot movement for more realistic physics feel
        Quaternion targetRotation = Quaternion.Euler(xAngle, 0, zAngle);
        robotModel.localRotation = Quaternion.Slerp(robotModel.localRotation, targetRotation, Time.deltaTime * 5f);
        
        // Check if robot has fallen
        float currentTiltAngle = Quaternion.Angle(Quaternion.identity, robotModel.localRotation);
        
        if (currentTiltAngle > robotFallThreshold)
        {
            RobotFall();
        }
        else if (currentTiltAngle < 5.0f)
        {
            // Robot is stable, add stability time
            totalStabilityTime += Time.deltaTime;
            
            // Add points for maintaining balance
            if (Time.frameCount % 60 == 0) // Once per second at 60fps
            {
                AddPoints(1);
            }
        }
    }
    
    private void RobotFall()
    {
        if (robotFallen)
            return;
            
        robotFallen = true;
        fallCount++;
        robotRecoveryTimer = robotRecoveryTime;
        
        // Visual effect - exaggerate the fall
        float fallDirectionX = currentTilt.x > 0 ? 1 : -1;
        float fallDirectionZ = currentTilt.y > 0 ? 1 : -1;
        robotModel.localRotation = Quaternion.Euler(fallDirectionZ * 60f, 0, fallDirectionX * 60f);
        
        // Play fall sound
        audioSource.PlayOneShot(fallSound);
        
        // Deduct points
        AddPoints(-10);
        
        Debug.Log("Robot has fallen!");
    }
    
    private void UpdateDifficulty()
    {
        timeSinceLastDifficultyIncrease += Time.deltaTime;
        
        // Increase difficulty at regular intervals
        if (timeSinceLastDifficultyIncrease >= difficultyIncreaseInterval && currentDifficultyLevel < maxDifficultyLevel)
        {
            currentDifficultyLevel++;
            timeSinceLastDifficultyIncrease = 0f;
            
            // Play level up sound
            audioSource.PlayOneShot(levelUpSound);
            
            // Adjust difficulty parameters
            AdjustDifficultyParameters();
            
            Debug.Log($"Difficulty increased to level {currentDifficultyLevel}");
        }
        
        // Adaptive difficulty adjustment
        if (adaptiveDifficulty && Time.frameCount % 300 == 0) // Every 5 seconds at 60fps
        {
            AdaptDifficultyBasedOnPerformance();
        }
    }
    
    private void AdjustDifficultyParameters()
    {
        // Increase auto-tilt amount
        autoTiltAmount = Mathf.Min(1.0f, 0.5f + (currentDifficultyLevel * 0.1f));
        
        // Increase obstacle speed
        obstacleSpeed = 2.0f + (currentDifficultyLevel * 0.5f);
        
        // Decrease obstacle spawn interval
        obstacleSpawnInterval = Mathf.Max(2.0f, 5.0f - (currentDifficultyLevel * 0.5f));
    }
    
    private void AdaptDifficultyBasedOnPerformance()
    {
        // If player is doing very well, make it slightly harder
        if (currentStability > 90f && fallCount == 0)
        {
            autoTiltAmount = Mathf.Min(1.0f, autoTiltAmount + 0.05f);
        }
        // If player is struggling, make it slightly easier
        else if (currentStability < 50f || fallCount > 5)
        {
            autoTiltAmount = Mathf.Max(0.2f, autoTiltAmount - 0.05f);
        }
    }
    
    private void UpdateObstacles()
    {
        timeSinceLastObstacleSpawn += Time.deltaTime;
        
        // Spawn new obstacles
        if (timeSinceLastObstacleSpawn >= obstacleSpawnInterval)
        {
            SpawnObstacle();
            timeSinceLastObstacleSpawn = 0f;
        }
        
        // Update existing obstacles
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Obstacle"))
            {
                // Move obstacle
                child.Translate(Vector3.forward * obstacleSpeed * Time.deltaTime);
                
                // Check for collision with robot
                if (Vector3.Distance(child.position, robotModel.position) < 1.0f)
                {
                    // Collision detected
                    HandleObstacleCollision(child.gameObject);
                }
                
                // Remove if too far
                if (child.position.z > 10.0f)
                {
                    Destroy(child.gameObject);
                    obstaclesAvoided++;
                    AddPoints(5);
                }
            }
        }
    }
    
    private void SpawnObstacle()
    {
        if (obstaclePrefabs.Length == 0)
            return;
            
        // Select random obstacle prefab
        GameObject prefab = obstaclePrefabs[UnityEngine.Random.Range(0, obstaclePrefabs.Length)];
        
        // Calculate random position on platform
        float x = UnityEngine.Random.Range(-2.0f, 2.0f);
        float z = -10.0f; // Start behind the platform
        
        // Instantiate obstacle
        GameObject obstacle = Instantiate(prefab, new Vector3(x, 0.5f, z), Quaternion.identity, transform);
        obstacle.tag = "Obstacle";
        
        Debug.Log("Spawned obstacle");
    }
    
    private void HandleObstacleCollision(GameObject obstacle)
    {
        // Play hit sound
        audioSource.PlayOneShot(obstacleHitSound);
        
        // Visual effect
        obstacle.GetComponent<Renderer>().material.color = Color.red;
        
        // Deduct points
        AddPoints(-5);
        
        // Track collision
        obstaclesHit++;
        
        // Remove obstacle
        Destroy(obstacle, 0.2f);
        
        Debug.Log("Collision with obstacle");
    }
    
    private void AddPoints(int points)
    {
        score += points;
        score = Mathf.Max(0, score); // Ensure score doesn't go below zero
    }
    
    private void TakePerformanceSnapshot()
    {
        // Calculate average reaction time
        float avgReactionTime = 0f;
        if (reactionTimes.Count > 0)
        {
            float sum = 0f;
            foreach (float time in reactionTimes)
            {
                sum += time;
            }
            avgReactionTime = sum / reactionTimes.Count;
        }
        
        // Create snapshot
        PerformanceSnapshot snapshot = new PerformanceSnapshot
        {
            timeStamp = sessionDuration - timeRemaining,
            stability = currentStability,
            difficultyLevel = currentDifficultyLevel,
            platformTilt = currentTilt,
            robotFallen = robotFallen,
            reactionTime = avgReactionTime
        };
        
        performanceHistory.Add(snapshot);
    }
    
    private void EndGame()
    {
        gameRunning = false;
        
        // Upload final metrics
        UploadGameMetrics(true);
        
        // End the session
        StartCoroutine(apiClient.EndSession(score, true, (success, message) => {
            if (success)
            {
                Debug.Log("Game session ended successfully");
            }
            else
            {
                Debug.LogError($"Error ending game session: {message}");
            }
        }));
        
        // Show game over panel
        gameOverPanel.SetActive(true);
        TextMeshProUGUI finalScoreText = gameOverPanel.transform.Find("FinalScoreText")?.GetComponent<TextMeshProUGUI>();
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {score}\nStability: {currentStability:F1}%\nFalls: {fallCount}";
        }
        
        // Play game over sound
        audioSource.PlayOneShot(gameOverSound);
        
        Debug.Log($"Game over! Final score: {score}");
    }
    
    private void UploadGameMetrics(bool isFinal)
    {
        // Calculate motor control metrics
        float stabilityPercentage = totalStabilityTime / (sessionDuration - timeRemaining) * 100f;
        float obstacleAvoidanceRate = obstaclesAvoided > 0 ? 
            (float)obstaclesAvoided / (obstaclesAvoided + obstaclesHit) * 100f : 0f;
        
        // Calculate average reaction time
        float avgReactionTime = 0f;
        if (reactionTimes.Count > 0)
        {
            float sum = 0f;
            foreach (float time in reactionTimes)
            {
                sum += time;
            }
            avgReactionTime = sum / reactionTimes.Count;
        }
        
        // Prepare data for upload
        apiClient.AddSessionData("current_score", score);
        apiClient.AddSessionData("difficulty_level", currentDifficultyLevel);
        apiClient.AddSessionData("stability_percentage", stabilityPercentage);
        apiClient.AddSessionData("fall_count", fallCount);
        apiClient.AddSessionData("obstacles_avoided", obstaclesAvoided);
        apiClient.AddSessionData("obstacles_hit", obstaclesHit);
        apiClient.AddSessionData("obstacle_avoidance_rate", obstacleAvoidanceRate);
        apiClient.AddSessionData("average_reaction_time", avgReactionTime);
        apiClient.AddSessionData("current_stability", currentStability);
        
        // Add full performance history if this is the final upload
        if (isFinal)
        {
            apiClient.AddSessionData("performance_history", performanceHistory);
            
            // Add motor control assessment
            Dictionary<string, object> motorAssessment = new Dictionary<string, object>
            {
                { "balance_control", CalculateBalanceControlScore() },
                { "precision", CalculatePrecisionScore() },
                { "reaction_time", CalculateReactionTimeScore() },
                { "adaptation", CalculateAdaptationScore() },
                { "overall_motor_control", CalculateOverallMotorControlScore() }
            };
            
            apiClient.AddSessionData("motor_assessment", motorAssessment);
        }
        
        // Upload data
        StartCoroutine(apiClient.UploadSessionData());
    }
    
    private float CalculateBalanceControlScore()
    {
        // Balance control score based on stability time and fall count
        float stabilityFactor = totalStabilityTime / (sessionDuration - timeRemaining);
        float fallPenalty = Mathf.Max(0f, 1f - (fallCount * 0.1f));
        
        return Mathf.Clamp01(stabilityFactor * fallPenalty) * 100f;
    }
    
    private float CalculatePrecisionScore()
    {
        // Precision score based on how well the player maintained the robot at center
        float tiltVariance = 0f;
        int snapshotCount = performanceHistory.Count;
        
        if (snapshotCount > 0)
        {
            float sumSquaredTilt = 0f;
            foreach (PerformanceSnapshot snapshot in performanceHistory)
            {
                sumSquaredTilt += snapshot.platformTilt.sqrMagnitude;
            }
            tiltVariance = sumSquaredTilt / snapshotCount;
        }
        
        return Mathf.Clamp01(1f - tiltVariance) * 100f;
    }
    
    private float CalculateReactionTimeScore()
    {
        // Reaction time score - lower is better
        if (reactionTimes.Count == 0)
            return 50f; // Default score if no reaction times recorded
            
        float avgReactionTime = 0f;
        foreach (float time in reactionTimes)
        {
            avgReactionTime += time;
        }
        avgReactionTime /= reactionTimes.Count;
        
        // Scale: 0.5s or less is perfect, 2s or more is poor
        return Mathf.Clamp01((2f - Mathf.Min(2f, avgReactionTime)) / 1.5f) * 100f;
    }
    
    private float CalculateAdaptationScore()
    {
        // Adaptation score based on performance at higher difficulty levels
        if (performanceHistory.Count < 10)
            return 50f; // Default score if not enough data
            
        // Compare stability in first third vs last third of the game
        int firstThird = performanceHistory.Count / 3;
        int lastThird = performanceHistory.Count - firstThird;
        
        float earlyStability = 0f;
        float lateStability = 0f;
        
        for (int i = 0; i < firstThird; i++)
        {
            earlyStability += performanceHistory[i].stability;
        }
        earlyStability /= firstThird;
        
        for (int i = lastThird; i < performanceHistory.Count; i++)
        {
            lateStability += performanceHistory[i].stability;
        }
        lateStability /= (performanceHistory.Count - lastThird);
        
        // If late stability is at least 80% of early stability despite increased difficulty,
        // that's good adaptation
        return Mathf.Clamp01(lateStability / Mathf.Max(1f, earlyStability)) * 100f;
    }
    
    private float CalculateOverallMotorControlScore()
    {
        // Weighted average of all scores
        float balanceScore = CalculateBalanceControlScore();
        float precisionScore = CalculatePrecisionScore();
        float reactionScore = CalculateReactionTimeScore();
        float adaptationScore = CalculateAdaptationScore();
        
        return (balanceScore * 0.4f + precisionScore * 0.25f + reactionScore * 0.2f + adaptationScore * 0.15f);
    }
    
    private void UpdateUI()
    {
        // Update timer
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = $"Time: {minutes:00}:{seconds:00}";
        
        // Update score
        scoreText.text = $"Score: {score}";
        
        // Update difficulty
        difficultyText.text = $"Level: {currentDifficultyLevel}";
        
        // Update stability meter
        stabilityMeter.value = currentStability / 100f;
        stabilityText.text = $"Stability: {currentStability:F1}%";
    }
    
    public void RestartGame()
    {
        gameOverPanel.SetActive(false);
        StartGame();
    }
}