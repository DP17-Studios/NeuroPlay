using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages obstacle spawning and behavior for the NeuroSprint game
/// </summary>
public class ObstacleManager : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float minSpawnInterval = 1.5f;
    [SerializeField] private float maxSpawnInterval = 3.0f;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform despawnPoint;
    
    [Header("Difficulty Settings")]
    [SerializeField] private float speedIncreaseRate = 0.1f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float spawnIntervalDecreaseRate = 0.05f;
    [SerializeField] private float minSpawnIntervalLimit = 0.5f;
    
    // Runtime variables
    private float currentSpeed;
    private float currentMinSpawnInterval;
    private float currentMaxSpawnInterval;
    private bool isSpawning = false;
    private int obstaclesSpawned = 0;
    private int obstaclesPassed = 0;
    
    // Reference to the game controller
    private NeuroSprintController gameController;
    
    private void Awake()
    {
        gameController = FindObjectOfType<NeuroSprintController>();
        if (gameController == null)
        {
            Debug.LogError("ObstacleManager: NeuroSprintController not found in scene!");
        }
    }
    
    public void StartSpawning()
    {
        // Initialize runtime variables
        currentSpeed = baseSpeed;
        currentMinSpawnInterval = minSpawnInterval;
        currentMaxSpawnInterval = maxSpawnInterval;
        obstaclesSpawned = 0;
        obstaclesPassed = 0;
        
        // Start spawning coroutine
        isSpawning = true;
        StartCoroutine(SpawnObstacles());
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }
    
    private IEnumerator SpawnObstacles()
    {
        while (isSpawning)
        {
            // Wait for a random interval
            float interval = Random.Range(currentMinSpawnInterval, currentMaxSpawnInterval);
            yield return new WaitForSeconds(interval);
            
            if (!isSpawning) break;
            
            // Spawn a random obstacle
            SpawnObstacle();
            
            // Increase difficulty gradually
            IncreaseDifficulty();
        }
    }
    
    private void SpawnObstacle()
    {
        // Select a random obstacle prefab
        int index = Random.Range(0, obstaclePrefabs.Length);
        GameObject obstaclePrefab = obstaclePrefabs[index];
        
        // Instantiate the obstacle
        GameObject obstacle = Instantiate(obstaclePrefab, spawnPoint.position, Quaternion.identity);
        
        // Set obstacle properties
        Obstacle obstacleComponent = obstacle.GetComponent<Obstacle>();
        if (obstacleComponent != null)
        {
            obstacleComponent.Initialize(currentSpeed, gameController, this);
        }
        else
        {
            // If no Obstacle component, add a basic mover script
            StartCoroutine(MoveObstacle(obstacle));
        }
        
        obstaclesSpawned++;
    }
    
    private IEnumerator MoveObstacle(GameObject obstacle)
    {
        // Move the obstacle until it reaches the despawn point
        while (obstacle != null && obstacle.transform.position.x > despawnPoint.position.x)
        {
            obstacle.transform.position += Vector3.left * currentSpeed * Time.deltaTime;
            yield return null;
        }
        
        // Destroy the obstacle when it passes the despawn point
        if (obstacle != null)
        {
            Destroy(obstacle);
            obstaclesPassed++;
        }
    }
    
    private void IncreaseDifficulty()
    {
        // Increase speed gradually
        currentSpeed = Mathf.Min(maxSpeed, currentSpeed + speedIncreaseRate);
        
        // Decrease spawn interval gradually
        currentMinSpawnInterval = Mathf.Max(minSpawnIntervalLimit, currentMinSpawnInterval - spawnIntervalDecreaseRate);
        currentMaxSpawnInterval = Mathf.Max(currentMinSpawnInterval + 0.5f, currentMaxSpawnInterval - spawnIntervalDecreaseRate);
    }
    
    public void ObstacleAvoided()
    {
        obstaclesPassed++;
    }
    
    // For debugging and analytics
    public int GetObstaclesSpawned()
    {
        return obstaclesSpawned;
    }
    
    public int GetObstaclesPassed()
    {
        return obstaclesPassed;
    }
    
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
}

/// <summary>
/// Basic obstacle behavior component
/// </summary>
public class Obstacle : MonoBehaviour
{
    private float speed;
    private NeuroSprintController gameController;
    private ObstacleManager obstacleManager;
    private bool isAvoidable = true;
    
    public void Initialize(float speed, NeuroSprintController controller, ObstacleManager manager)
    {
        this.speed = speed;
        this.gameController = controller;
        this.obstacleManager = manager;
    }
    
    private void Update()
    {
        // Move obstacle from right to left
        transform.position += Vector3.left * speed * Time.deltaTime;
        
        // Destroy if off-screen
        if (transform.position.x < -15f)
        {
            if (isAvoidable)
            {
                // Player successfully avoided this obstacle
                gameController.OnObstacleAvoided();
                obstacleManager.ObstacleAvoided();
                isAvoidable = false;
            }
            
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isAvoidable)
        {
            // Player hit this obstacle
            gameController.OnObstacleHit();
            isAvoidable = false;
            
            // Visual feedback
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }
}