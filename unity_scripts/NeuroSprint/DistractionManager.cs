using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls visual and audio distractions in the NeuroSprint game
/// </summary>
public class DistractionManager : MonoBehaviour
{
    [Header("Visual Distraction Settings")]
    [SerializeField] private GameObject[] visualDistractionPrefabs;
    [SerializeField] private float visualDistractionDuration = 2f;
    [SerializeField] private float minVisualSpawnInterval = 4f;
    [SerializeField] private float maxVisualSpawnInterval = 8f;
    [SerializeField] private Transform[] visualSpawnPoints;


    [Header("Audio Distraction Settings")]
    [SerializeField] private AudioClip[] audioDistractionClips;
    [SerializeField] private float minAudioSpawnInterval = 5f;
    [SerializeField] private float maxAudioSpawnInterval = 10f;
    [SerializeField] private float audioVolume = 0.5f;


    [Header("Difficulty Settings")]
    [SerializeField] private float spawnIntervalDecreaseRate = 0.1f;
    [SerializeField] private float minSpawnIntervalLimit = 2f;
    [SerializeField] private int maxSimultaneousDistractions = 3;

    // Runtime variables

    private float currentMinVisualInterval;
    private float currentMaxVisualInterval;
    private float currentMinAudioInterval;
    private float currentMaxAudioInterval;
    private bool isSpawning = false;
    private int activeDistractions = 0;
    private int distractionsTriggered = 0;
    private int distractionsIgnored = 0;

    // Audio source for playing distraction sounds

    private AudioSource audioSource;

    // Reference to the game controller

    private NeuroSprintController gameController;


    private void Awake()
    {
        gameController = FindObjectOfType<NeuroSprintController>();
        if (gameController == null)
        {
            Debug.LogError("DistractionManager: NeuroSprintController not found in scene!");
        }

        // Create audio source for distractions

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = audioVolume;
    }


    public void StartDistractions()
    {
        // Initialize runtime variables
        currentMinVisualInterval = minVisualSpawnInterval;
        currentMaxVisualInterval = maxVisualSpawnInterval;
        currentMinAudioInterval = minAudioSpawnInterval;
        currentMaxAudioInterval = maxAudioSpawnInterval;
        activeDistractions = 0;
        distractionsTriggered = 0;
        distractionsIgnored = 0;

        // Start spawning coroutines
        isSpawning = true;
        StartCoroutine(SpawnVisualDistractions());
        StartCoroutine(PlayAudioDistractions());
    }


    public void StopDistractions()
    {
        isSpawning = false;
        StopAllCoroutines();

        // Clean up any active distractions

        foreach (Distraction distraction in FindObjectsOfType<Distraction>())
        {
            Destroy(distraction.gameObject);
        }
    }


    private IEnumerator SpawnVisualDistractions()
    {
        while (isSpawning)
        {
            // Wait for a random interval
            float interval = Random.Range(currentMinVisualInterval, currentMaxVisualInterval);
            yield return new WaitForSeconds(interval);


            if (!isSpawning) break;

            // Only spawn if we're under the maximum simultaneous distractions

            if (activeDistractions < maxSimultaneousDistractions)
            {
                SpawnVisualDistraction();

                // Increase difficulty gradually

                IncreaseDifficulty();
            }
        }
    }


    private IEnumerator PlayAudioDistractions()
    {
        while (isSpawning)
        {
            // Wait for a random interval
            float interval = Random.Range(currentMinAudioInterval, currentMaxAudioInterval);
            yield return new WaitForSeconds(interval);


            if (!isSpawning) break;


            PlayAudioDistraction();
        }
    }


    private void SpawnVisualDistraction()
    {
        // Select a random distraction prefab
        int prefabIndex = Random.Range(0, visualDistractionPrefabs.Length);
        GameObject distractionPrefab = visualDistractionPrefabs[prefabIndex];

        // Select a random spawn point

        int spawnIndex = Random.Range(0, visualSpawnPoints.Length);
        Transform spawnPoint = visualSpawnPoints[spawnIndex];

        // Instantiate the distraction

        GameObject distraction = Instantiate(distractionPrefab, spawnPoint.position, Quaternion.identity);

        // Set distraction properties

        Distraction distractionComponent = distraction.GetComponent<Distraction>();
        if (distractionComponent == null)
        {
            distractionComponent = distraction.AddComponent<Distraction>();
        }


        distractionComponent.Initialize(visualDistractionDuration, gameController, this);


        activeDistractions++;
    }


    private void PlayAudioDistraction()
    {
        // Select a random audio clip
        int clipIndex = Random.Range(0, audioDistractionClips.Length);
        AudioClip clip = audioDistractionClips[clipIndex];

        // Play the audio

        audioSource.PlayOneShot(clip);

        // Create a coroutine to check if the player gets distracted

        StartCoroutine(CheckAudioDistraction(clip.length));
    }


    private IEnumerator CheckAudioDistraction(float duration)
    {
        // Wait for the audio to finish
        yield return new WaitForSeconds(duration);

        // Randomly determine if the player was distracted
        // In a real implementation, this would be based on player behavior

        bool wasDistracted = Random.value < 0.3f;


        if (wasDistracted)
        {
            DistractionTriggered();
        }
        else
        {
            DistractionIgnored();
        }
    }


    private void IncreaseDifficulty()
    {
        // Decrease spawn intervals gradually
        currentMinVisualInterval = Mathf.Max(minSpawnIntervalLimit, currentMinVisualInterval - spawnIntervalDecreaseRate);
        currentMaxVisualInterval = Mathf.Max(currentMinVisualInterval + 2f, currentMaxVisualInterval - spawnIntervalDecreaseRate);


        currentMinAudioInterval = Mathf.Max(minSpawnIntervalLimit, currentMinAudioInterval - spawnIntervalDecreaseRate);
        currentMaxAudioInterval = Mathf.Max(currentMinAudioInterval + 2f, currentMaxAudioInterval - spawnIntervalDecreaseRate);
    }


    public void DistractionRemoved()
    {
        activeDistractions = Mathf.Max(0, activeDistractions - 1);
    }


    public void DistractionTriggered()
    {
        distractionsTriggered++;
        gameController.OnDistractionTriggered();
    }


    public void DistractionIgnored()
    {
        distractionsIgnored++;
        gameController.OnDistractionIgnored();
    }

    // For debugging and analytics

    public int GetDistractionTriggered()
    {
        return distractionsTriggered;
    }


    public int GetDistractionIgnored()
    {
        return distractionsIgnored;
    }
}

/// <summary>
/// Basic distraction behavior component
/// </summary>
public class Distraction : MonoBehaviour
{
    private float duration;
    private float startTime;
    private bool isActive = true;
    private NeuroSprintController gameController;
    private DistractionManager distractionManager;

    // Visual effects

    private Vector3 originalScale;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;


    public void Initialize(float duration, NeuroSprintController controller, DistractionManager manager)
    {
        this.duration = duration;
        this.gameController = controller;
        this.distractionManager = manager;
        this.startTime = Time.time;

        // Cache components

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }


        originalScale = transform.localScale;

        // Add a collider if not present

        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
        }
    }


    private void Update()
    {
        if (!isActive) return;

        // Visual pulsing effect

        float pulseAmount = 0.2f * Mathf.Sin(Time.time * 5f) + 1f;
        transform.localScale = originalScale * pulseAmount;


        if (spriteRenderer != null)
        {
            float alpha = 0.5f * Mathf.Sin(Time.time * 3f) + 0.5f;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }

        // Check if distraction duration has expired

        if (Time.time - startTime > duration)
        {
            // Player successfully ignored this distraction
            distractionManager.DistractionIgnored();
            RemoveDistraction();
        }
    }


    private void OnMouseDown()
    {
        if (isActive)
        {
            // Player was distracted and clicked on this distraction
            distractionManager.DistractionTriggered();
            RemoveDistraction();
        }
    }


    private void RemoveDistraction()
    {
        isActive = false;
        distractionManager.DistractionRemoved();

        // Visual feedback before destroying

        StartCoroutine(FadeOut());
    }


    private IEnumerator FadeOut()
    {
        float fadeTime = 0.5f;
        float startTime = Time.time;


        while (Time.time - startTime < fadeTime)
        {
            float t = (Time.time - startTime) / fadeTime;

            // Scale down

            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);

            // Fade out

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                spriteRenderer.color = new Color(color.r, color.g, color.b, 1 - t);
            }


            yield return null;
        }


        Destroy(gameObject);
    }
}