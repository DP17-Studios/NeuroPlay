using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the F1-style light sequence for the ReactionLights game
/// </summary>
public class LightSequenceManager : MonoBehaviour
{
    [Header("Sequence Configuration")]
    [SerializeField] private float lightOnInterval = 0.5f;
    [SerializeField] private float minRedLightDuration = 5f;
    [SerializeField] private float maxRedLightDuration = 10f;
    [SerializeField] private int numberOfLights = 5;
    
    // Event callbacks
    public event Action OnSequenceStarted;
    public event Action OnRedLightsComplete;
    public event Action OnGreenLightsOn;
    
    // External references
    private ReactionUIManager uiManager;
    private ReactionLightsController gameController;
    
    // Sequence state
    private bool isSequenceRunning = false;
    private Coroutine currentSequence;
    
    private void Awake()
    {
        gameController = GetComponent<ReactionLightsController>();
        uiManager = GetComponent<ReactionUIManager>();
        
        if (gameController == null)
        {
            Debug.LogError("LightSequenceManager requires a ReactionLightsController component on the same GameObject");
        }
        
        if (uiManager == null)
        {
            Debug.LogError("LightSequenceManager requires a ReactionUIManager component on the same GameObject");
        }
    }
    
    /// <summary>
    /// Starts the F1-style light sequence
    /// </summary>
    public void StartLightSequence()
    {
        if (isSequenceRunning)
        {
            StopSequence();
        }
        
        currentSequence = StartCoroutine(RunSequence());
    }
    
    /// <summary>
    /// Stops the current light sequence
    /// </summary>
    public void StopSequence()
    {
        if (currentSequence != null)
        {
            StopCoroutine(currentSequence);
            currentSequence = null;
        }
        
        isSequenceRunning = false;
        
        // Turn off all lights
        for (int i = 0; i < numberOfLights; i++)
        {
            SetLightState(i, true, false);  // Turn off red lights
            SetLightState(i, false, false); // Turn off green lights
        }
    }
    
    /// <summary>
    /// Runs the complete light sequence
    /// </summary>
    private IEnumerator RunSequence()
    {
        isSequenceRunning = true;
        
        // Notify sequence started
        if (OnSequenceStarted != null)
        {
            OnSequenceStarted();
        }
        
        // Turn on red lights one by one
        for (int i = 0; i < numberOfLights; i++)
        {
            SetLightState(i, true, true); // Turn on red light
            yield return new WaitForSeconds(lightOnInterval);
        }
        
        // Notify red lights are all on
        if (OnRedLightsComplete != null)
        {
            OnRedLightsComplete();
        }
        
        // Random wait before going green
        float waitTime = UnityEngine.Random.Range(minRedLightDuration, maxRedLightDuration);
        yield return new WaitForSeconds(waitTime);
        
        // Turn all red lights off and green lights on
        for (int i = 0; i < numberOfLights; i++)
        {
            SetLightState(i, true, false);  // Turn off red light
            SetLightState(i, false, true);  // Turn on green light
        }
        
        // Notify green lights are on
        if (OnGreenLightsOn != null)
        {
            OnGreenLightsOn();
        }
        
        isSequenceRunning = false;
    }
    
    /// <summary>
    /// Sets the state of a specific light
    /// </summary>
    private void SetLightState(int index, bool isRed, bool isActive)
    {
        if (uiManager != null)
        {
            uiManager.UpdateLightState(index, isRed, isActive);
        }
    }
    
    /// <summary>
    /// Generates a random wait time within the configured range
    /// </summary>
    public float GetRandomWaitTime()
    {
        return UnityEngine.Random.Range(minRedLightDuration, maxRedLightDuration);
    }
}