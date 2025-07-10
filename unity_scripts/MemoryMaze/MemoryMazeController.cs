using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main controller for the MemoryMaze game - Dementia Cognitive Tracker
/// </summary>
public class MemoryMazeController : MonoBehaviour
{
    [Header("Game Configuration")]
    [SerializeField] private int initialGridSize = 3;
    [SerializeField] private int maxGridSize = 8;
    [SerializeField] private float memorizationTime = 5.0f;
    [SerializeField] private float recallTime = 10.0f;
    [SerializeField] private int maxLevels = 10;
    [SerializeField] private bool adaptiveDifficulty = true;
    
    [Header("Game Elements")]
    [SerializeField] private GameObject gridCellPrefab;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Audio")]
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip incorrectSound;
    [SerializeField] private AudioClip levelCompleteSound;
    [SerializeField] private AudioClip gameOverSound;
    
    // Game state
    private int currentLevel = 1;
    private int currentGridSize;
    private int score = 0;
    private bool isMemorizationPhase = false;
    private bool isRecallPhase = false;
    private float phaseTimer = 0f;
    private List<int> patternIndices = new List<int>();
    private List<int> playerSelections = new List<int>();
    private List<GameObject> gridCells = new List<GameObject>();
    
    // Performance tracking
    private int totalCorrectSelections = 0;
    private int totalIncorrectSelections = 0;
    private float averageRecallTime = 0f;
    private List<float> levelCompletionTimes = new List<float>();
    private float levelStartTime = 0f;
    private int consecutiveCorrectLevels = 0;
    private int consecutiveIncorrectLevels = 0;
    
    // Memory patterns by type
    private enum PatternType { Sequential, Spatial, Random }
    private PatternType currentPatternType = PatternType.Sequential;
    
    // Audio source
    private AudioSource audioSource;
    
    // API client
    private NeuroplaysApiClient apiClient;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        gameOverPanel.SetActive(false);
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
        if (config.ContainsKey("initial_grid_size"))
            initialGridSize = Convert.ToInt32(config["initial_grid_size"]);
            
        if (config.ContainsKey("max_grid_size"))
            maxGridSize = Convert.ToInt32(config["max_grid_size"]);
            
        if (config.ContainsKey("memorization_time"))
            memorizationTime = Convert.ToSingle(config["memorization_time"]);
            
        if (config.ContainsKey("recall_time"))
            recallTime = Convert.ToSingle(config["recall_time"]);
            
        if (config.ContainsKey("max_levels"))
            maxLevels = Convert.ToInt32(config["max_levels"]);
            
        if (config.ContainsKey("adaptive_difficulty"))
            adaptiveDifficulty = Convert.ToBoolean(config["adaptive_difficulty"]);
            
        Debug.Log("Applied game configuration from server");
    }
    
    private void StartGame()
    {
        currentLevel = 1;
        score = 0;
        currentGridSize = initialGridSize;
        totalCorrectSelections = 0;
        totalIncorrectSelections = 0;
        averageRecallTime = 0f;
        levelCompletionTimes.Clear();
        consecutiveCorrectLevels = 0;
        consecutiveIncorrectLevels = 0;
        
        UpdateUI();
        StartLevel();
        
        Debug.Log("MemoryMaze game started");
    }
    
    private void StartLevel()
    {
        // Clear previous grid
        ClearGrid();
        
        // Create new grid
        CreateGrid(currentGridSize);
        
        // Determine pattern type for this level
        DeterminePatternType();
        
        // Generate pattern
        GeneratePattern();
        
        // Start memorization phase
        StartMemorizationPhase();
        
        // Record level start time
        levelStartTime = Time.time;
        
        Debug.Log($"Level {currentLevel} started with grid size {currentGridSize} and pattern type {currentPatternType}");
    }
    
    private void ClearGrid()
    {
        foreach (GameObject cell in gridCells)
        {
            Destroy(cell);
        }
        
        gridCells.Clear();
        patternIndices.Clear();
        playerSelections.Clear();
    }
    
    private void CreateGrid(int size)
    {
        // Calculate grid layout
        float cellSize = Mathf.Min(gridContainer.rect.width, gridContainer.rect.height) / size;
        
        // Create cells
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                GameObject cell = Instantiate(gridCellPrefab, gridContainer);
                RectTransform rectTransform = cell.GetComponent<RectTransform>();
                
                // Position cell
                rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
                rectTransform.anchoredPosition = new Vector2(
                    x * cellSize - (size * cellSize / 2) + (cellSize / 2),
                    y * cellSize - (size * cellSize / 2) + (cellSize / 2)
                );
                
                // Set up cell
                int cellIndex = y * size + x;
                GridCell gridCell = cell.GetComponent<GridCell>();
                if (gridCell != null)
                {
                    gridCell.SetIndex(cellIndex);
                    gridCell.OnCellClicked += OnGridCellClicked;
                }
                
                gridCells.Add(cell);
            }
        }
    }
    
    private void DeterminePatternType()
    {
        // Rotate through pattern types as levels progress
        switch (currentLevel % 3)
        {
            case 0:
                currentPatternType = PatternType.Random;
                break;
            case 1:
                currentPatternType = PatternType.Sequential;
                break;
            case 2:
                currentPatternType = PatternType.Spatial;
                break;
        }
    }
    
    private void GeneratePattern()
    {
        int patternLength = Mathf.Min(currentLevel + 2, currentGridSize * currentGridSize / 2);
        
        switch (currentPatternType)
        {
            case PatternType.Sequential:
                GenerateSequentialPattern(patternLength);
                break;
            case PatternType.Spatial:
                GenerateSpatialPattern(patternLength);
                break;
            case PatternType.Random:
                GenerateRandomPattern(patternLength);
                break;
        }
    }
    
    private void GenerateSequentialPattern(int length)
    {
        // Sequential pattern - cells in a row, column, or diagonal
        int startIndex = UnityEngine.Random.Range(0, gridCells.Count);
        int direction = UnityEngine.Random.Range(0, 3); // 0: horizontal, 1: vertical, 2: diagonal
        
        for (int i = 0; i < length; i++)
        {
            int index = -1;
            
            switch (direction)
            {
                case 0: // Horizontal
                    index = (startIndex / currentGridSize) * currentGridSize + ((startIndex + i) % currentGridSize);
                    break;
                case 1: // Vertical
                    index = ((startIndex + i * currentGridSize) % (currentGridSize * currentGridSize));
                    break;
                case 2: // Diagonal
                    if (startIndex % (currentGridSize + 1) == 0) // Main diagonal
                        index = (startIndex + i * (currentGridSize + 1)) % (currentGridSize * currentGridSize);
                    else // Anti-diagonal
                        index = (startIndex + i * (currentGridSize - 1)) % (currentGridSize * currentGridSize);
                    break;
            }
            
            if (index >= 0 && index < gridCells.Count && !patternIndices.Contains(index))
                patternIndices.Add(index);
        }
    }
    
    private void GenerateSpatialPattern(int length)
    {
        // Spatial pattern - cells form a shape (L, square, etc.)
        int centerIndex = UnityEngine.Random.Range(0, gridCells.Count);
        patternIndices.Add(centerIndex);
        
        // Add surrounding cells based on a shape pattern
        int shapeType = UnityEngine.Random.Range(0, 3); // 0: L-shape, 1: square, 2: cross
        
        switch (shapeType)
        {
            case 0: // L-shape
                AddCellIfValid(centerIndex - currentGridSize); // Up
                AddCellIfValid(centerIndex - currentGridSize - 1); // Up-left
                AddCellIfValid(centerIndex - 1); // Left
                break;
                
            case 1: // Square
                AddCellIfValid(centerIndex + 1); // Right
                AddCellIfValid(centerIndex - currentGridSize); // Up
                AddCellIfValid(centerIndex - currentGridSize + 1); // Up-right
                break;
                
            case 2: // Cross
                AddCellIfValid(centerIndex + 1); // Right
                AddCellIfValid(centerIndex - 1); // Left
                AddCellIfValid(centerIndex - currentGridSize); // Up
                AddCellIfValid(centerIndex + currentGridSize); // Down
                break;
        }
        
        // If we need more cells, add random adjacent ones
        while (patternIndices.Count < length && patternIndices.Count < gridCells.Count / 2)
        {
            int lastIndex = patternIndices[patternIndices.Count - 1];
            List<int> adjacentIndices = new List<int>
            {
                lastIndex - currentGridSize, // Up
                lastIndex + currentGridSize, // Down
                lastIndex - 1, // Left
                lastIndex + 1  // Right
            };
            
            // Shuffle adjacent indices
            for (int i = 0; i < adjacentIndices.Count; i++)
            {
                int temp = adjacentIndices[i];
                int randomIndex = UnityEngine.Random.Range(i, adjacentIndices.Count);
                adjacentIndices[i] = adjacentIndices[randomIndex];
                adjacentIndices[randomIndex] = temp;
            }
            
            // Try to add each adjacent index
            bool added = false;
            foreach (int index in adjacentIndices)
            {
                if (AddCellIfValid(index))
                {
                    added = true;
                    break;
                }
            }
            
            // If we couldn't add any adjacent cells, break out
            if (!added)
                break;
        }
    }
    
    private void GenerateRandomPattern(int length)
    {
        // Random pattern - randomly distributed cells
        List<int> availableIndices = new List<int>();
        
        for (int i = 0; i < gridCells.Count; i++)
        {
            availableIndices.Add(i);
        }
        
        // Shuffle available indices
        for (int i = 0; i < availableIndices.Count; i++)
        {
            int temp = availableIndices[i];
            int randomIndex = UnityEngine.Random.Range(i, availableIndices.Count);
            availableIndices[i] = availableIndices[randomIndex];
            availableIndices[randomIndex] = temp;
        }
        
        // Select the first 'length' indices
        for (int i = 0; i < length && i < availableIndices.Count; i++)
        {
            patternIndices.Add(availableIndices[i]);
        }
    }
    
    private bool AddCellIfValid(int index)
    {
        if (index >= 0 && index < gridCells.Count && !patternIndices.Contains(index))
        {
            patternIndices.Add(index);
            return true;
        }
        return false;
    }
    
    private void StartMemorizationPhase()
    {
        isMemorizationPhase = true;
        isRecallPhase = false;
        phaseTimer = memorizationTime;
        
        // Show pattern
        foreach (int index in patternIndices)
        {
            GridCell cell = gridCells[index].GetComponent<GridCell>();
            if (cell != null)
            {
                cell.Highlight(true);
            }
        }
        
        instructionText.text = "Memorize the pattern!";
        
        Debug.Log("Memorization phase started");
    }
    
    private void StartRecallPhase()
    {
        isMemorizationPhase = false;
        isRecallPhase = true;
        phaseTimer = recallTime;
        
        // Hide pattern
        foreach (int index in patternIndices)
        {
            GridCell cell = gridCells[index].GetComponent<GridCell>();
            if (cell != null)
            {
                cell.Highlight(false);
            }
        }
        
        instructionText.text = "Recall the pattern!";
        
        Debug.Log("Recall phase started");
    }
    
    private void OnGridCellClicked(int index)
    {
        if (!isRecallPhase)
            return;
            
        GridCell cell = gridCells[index].GetComponent<GridCell>();
        
        // Check if cell is already selected
        if (playerSelections.Contains(index))
            return;
            
        // Add to player selections
        playerSelections.Add(index);
        
        // Check if selection is correct
        bool isCorrect = patternIndices.Contains(index);
        
        // Visual feedback
        cell.Select(isCorrect);
        
        // Audio feedback
        if (isCorrect)
        {
            audioSource.PlayOneShot(correctSound);
            totalCorrectSelections++;
        }
        else
        {
            audioSource.PlayOneShot(incorrectSound);
            totalIncorrectSelections++;
        }
        
        // Check if all pattern cells have been selected
        bool allCorrectSelected = true;
        foreach (int patternIndex in patternIndices)
        {
            if (!playerSelections.Contains(patternIndex))
            {
                allCorrectSelected = false;
                break;
            }
        }
        
        // If all correct cells selected, end level
        if (allCorrectSelected)
        {
            EndLevel(true);
        }
        // If max selections reached (pattern length + 2 allowed errors)
        else if (playerSelections.Count >= patternIndices.Count + 2)
        {
            EndLevel(false);
        }
    }
    
    private void EndLevel(bool success)
    {
        isRecallPhase = false;
        
        // Calculate level metrics
        float levelTime = Time.time - levelStartTime;
        levelCompletionTimes.Add(levelTime);
        
        // Calculate average recall time
        float totalTime = 0f;
        foreach (float time in levelCompletionTimes)
        {
            totalTime += time;
        }
        averageRecallTime = totalTime / levelCompletionTimes.Count;
        
        // Update consecutive counters
        if (success)
        {
            consecutiveCorrectLevels++;
            consecutiveIncorrectLevels = 0;
            
            // Add score
            int levelScore = patternIndices.Count * 10;
            score += levelScore;
            
            // Play success sound
            audioSource.PlayOneShot(levelCompleteSound);
            
            instructionText.text = "Level Complete!";
        }
        else
        {
            consecutiveCorrectLevels = 0;
            consecutiveIncorrectLevels++;
            
            // Play failure sound
            audioSource.PlayOneShot(incorrectSound);
            
            instructionText.text = "Try Again!";
        }
        
        // Upload level data
        UploadLevelData(success, levelTime);
        
        // Wait before starting next level
        StartCoroutine(NextLevelDelay(success));
        
        Debug.Log($"Level {currentLevel} ended. Success: {success}, Time: {levelTime:F2}s");
    }
    
    private IEnumerator NextLevelDelay(bool success)
    {
        // Show correct pattern
        foreach (int index in patternIndices)
        {
            GridCell cell = gridCells[index].GetComponent<GridCell>();
            if (cell != null)
            {
                cell.Highlight(true);
            }
        }
        
        yield return new WaitForSeconds(2.0f);
        
        if (currentLevel >= maxLevels)
        {
            EndGame(true);
        }
        else if (success)
        {
            // Advance to next level
            currentLevel++;
            
            // Adjust difficulty if needed
            if (adaptiveDifficulty)
            {
                AdjustDifficulty();
            }
            
            UpdateUI();
            StartLevel();
        }
        else
        {
            // Retry same level
            UpdateUI();
            StartLevel();
        }
    }
    
    private void AdjustDifficulty()
    {
        // Increase grid size after every 3 levels
        if (currentLevel % 3 == 0 && currentGridSize < maxGridSize)
        {
            currentGridSize++;
        }
        
        // Adjust memorization time based on performance
        if (consecutiveCorrectLevels >= 2)
        {
            // Make it harder - reduce memorization time
            memorizationTime = Mathf.Max(2.0f, memorizationTime - 0.5f);
        }
        else if (consecutiveIncorrectLevels >= 2)
        {
            // Make it easier - increase memorization time
            memorizationTime = Mathf.Min(10.0f, memorizationTime + 1.0f);
        }
        
        Debug.Log($"Adjusted difficulty: Grid size = {currentGridSize}, Memorization time = {memorizationTime:F1}s");
    }
    
    private void EndGame(bool completed)
    {
        // Calculate final score and metrics
        int finalScore = score;
        float accuracy = (totalCorrectSelections > 0) ? 
            (float)totalCorrectSelections / (totalCorrectSelections + totalIncorrectSelections) : 0f;
        
        // Upload final game data
        UploadGameMetrics(completed, finalScore, accuracy);
        
        // End the session
        StartCoroutine(apiClient.EndSession(finalScore, completed, (success, message) => {
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
            finalScoreText.text = $"Final Score: {finalScore}\nAccuracy: {accuracy:P0}\nLevels Completed: {currentLevel}";
        }
        
        // Play game over sound
        audioSource.PlayOneShot(gameOverSound);
        
        Debug.Log($"Game over! Final score: {finalScore}, Accuracy: {accuracy:P0}");
    }
    
    private void UploadLevelData(bool success, float levelTime)
    {
        Dictionary<string, object> levelData = new Dictionary<string, object>
        {
            { "level", currentLevel },
            { "grid_size", currentGridSize },
            { "pattern_type", currentPatternType.ToString() },
            { "pattern_length", patternIndices.Count },
            { "success", success },
            { "completion_time", levelTime },
            { "correct_selections", totalCorrectSelections },
            { "incorrect_selections", totalIncorrectSelections }
        };
        
        apiClient.AddSessionData($"level_{currentLevel}", levelData);
        StartCoroutine(apiClient.UploadSessionData());
    }
    
    private void UploadGameMetrics(bool completed, int finalScore, float accuracy)
    {
        // Calculate cognitive metrics
        float memoryCapacity = CalculateMemoryCapacity();
        float patternRecognition = CalculatePatternRecognition();
        float spatialMemory = CalculateSpatialMemory();
        float workingMemory = CalculateWorkingMemory();
        
        // Prepare data for upload
        apiClient.AddSessionData("final_score", finalScore);
        apiClient.AddSessionData("levels_completed", currentLevel);
        apiClient.AddSessionData("accuracy", accuracy);
        apiClient.AddSessionData("average_recall_time", averageRecallTime);
        apiClient.AddSessionData("memory_capacity", memoryCapacity);
        apiClient.AddSessionData("pattern_recognition", patternRecognition);
        apiClient.AddSessionData("spatial_memory", spatialMemory);
        apiClient.AddSessionData("working_memory", workingMemory);
        
        // Upload data
        StartCoroutine(apiClient.UploadSessionData());
    }
    
    private float CalculateMemoryCapacity()
    {
        // Calculate based on max pattern length successfully recalled
        int maxSuccessfulPatternLength = 0;
        
        for (int i = 0; i < currentLevel; i++)
        {
            int patternLength = Mathf.Min(i + 3, maxGridSize * maxGridSize / 2);
            if (i < levelCompletionTimes.Count)
            {
                maxSuccessfulPatternLength = patternLength;
            }
        }
        
        // Normalize to 0-1 range
        return Mathf.Clamp01((float)maxSuccessfulPatternLength / (maxGridSize * maxGridSize / 2));
    }
    
    private float CalculatePatternRecognition()
    {
        // Calculate based on performance on random patterns vs. sequential patterns
        // This would require tracking success rates by pattern type
        // For now, return a simple approximation based on overall accuracy
        return Mathf.Clamp01((float)totalCorrectSelections / (totalCorrectSelections + totalIncorrectSelections));
    }
    
    private float CalculateSpatialMemory()
    {
        // Calculate based on performance on spatial pattern types
        // This would require tracking success rates by pattern type
        // For now, return a simple approximation based on grid size mastery
        return Mathf.Clamp01((float)currentGridSize / maxGridSize);
    }
    
    private float CalculateWorkingMemory()
    {
        // Calculate based on recall speed and accuracy
        // Faster recall with high accuracy indicates better working memory
        float speedFactor = Mathf.Clamp01(1.0f - (averageRecallTime / (recallTime * 2)));
        float accuracyFactor = Mathf.Clamp01((float)totalCorrectSelections / (totalCorrectSelections + totalIncorrectSelections));
        
        return (speedFactor + accuracyFactor) / 2.0f;
    }
    
    private void Update()
    {
        // Update timer
        if (isMemorizationPhase || isRecallPhase)
        {
            phaseTimer -= Time.deltaTime;
            timerText.text = $"Time: {phaseTimer:F1}s";
            
            if (phaseTimer <= 0)
            {
                if (isMemorizationPhase)
                {
                    StartRecallPhase();
                }
                else if (isRecallPhase)
                {
                    EndLevel(false);
                }
            }
        }
    }
    
    private void UpdateUI()
    {
        levelText.text = $"Level: {currentLevel}";
        scoreText.text = $"Score: {score}";
    }
    
    public void RestartGame()
    {
        gameOverPanel.SetActive(false);
        StartGame();
    }
}