using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Controls character animations, poses, and behaviors in the SocialScope game
/// </summary>
public class CharacterController : MonoBehaviour
{
    [Header("Character Model")]
    [SerializeField] private GameObject characterModel;
    [SerializeField] private Animator animator;
    
    [Header("Environment")]
    [SerializeField] private GameObject[] environmentPrefabs;
    [SerializeField] private Transform environmentContainer;
    
    [Header("Animation Parameters")]
    [SerializeField] private float blendSpeed = 5.0f;
    [SerializeField] private float idleVariationInterval = 10.0f;
    
    // Character state
    private string currentPose = "neutral";
    private GameObject currentEnvironment;
    private float idleTimer = 0f;
    
    // Mapping of pose names to animation parameters
    private Dictionary<string, string> poseToAnimParam = new Dictionary<string, string>();
    
    // Environment name to prefab mapping
    private Dictionary<string, GameObject> environmentMapping = new Dictionary<string, GameObject>();
    
    private void Awake()
    {
        // Initialize pose mappings
        InitializePoseMappings();
        
        // Initialize environment mappings
        InitializeEnvironmentMappings();
    }
    
    private void Start()
    {
        // Set default pose
        SetPose("neutral");
    }
    
    private void Update()
    {
        // Add subtle idle variations
        UpdateIdleVariations();
    }
    
    private void InitializePoseMappings()
    {
        // Map pose names to animation parameters
        poseToAnimParam["neutral"] = "Neutral";
        poseToAnimParam["sitting"] = "Sitting";
        poseToAnimParam["standing"] = "Standing";
        poseToAnimParam["waving"] = "Waving";
        poseToAnimParam["pointing"] = "Pointing";
        poseToAnimParam["arms_crossed"] = "ArmsCrossed";
        poseToAnimParam["leaning_forward"] = "LeaningForward";
        poseToAnimParam["gesturing_to_chair"] = "GesturingToChair";
        poseToAnimParam["handshake"] = "Handshake";
        poseToAnimParam["thinking"] = "Thinking";
    }
    
    private void InitializeEnvironmentMappings()
    {
        // Map environment names to prefabs
        if (environmentPrefabs != null && environmentPrefabs.Length > 0)
        {
            for (int i = 0; i < environmentPrefabs.Length; i++)
            {
                if (environmentPrefabs[i] != null)
                {
                    environmentMapping[environmentPrefabs[i].name.ToLower()] = environmentPrefabs[i];
                }
            }
        }
    }
    
    public void SetPose(string poseName)
    {
        if (string.IsNullOrEmpty(poseName) || animator == null)
            return;
            
        currentPose = poseName.ToLower();
        
        // Reset all pose parameters
        foreach (var pose in poseToAnimParam.Values)
        {
            animator.SetBool(pose, false);
        }
        
        // Set the requested pose
        if (poseToAnimParam.ContainsKey(currentPose))
        {
            animator.SetBool(poseToAnimParam[currentPose], true);
            
            // Trigger transition
            animator.SetTrigger("ChangePose");
        }
        else
        {
            Debug.LogWarning($"Unknown pose: {poseName}");
            
            // Default to neutral
            animator.SetBool("Neutral", true);
            animator.SetTrigger("ChangePose");
        }
        
        Debug.Log($"Character pose set to: {poseName}");
    }
    
    public void SetEnvironment(string environmentName)
    {
        if (string.IsNullOrEmpty(environmentName))
            return;
            
        // Clear current environment
        if (currentEnvironment != null)
        {
            Destroy(currentEnvironment);
            currentEnvironment = null;
        }
        
        // Try to find exact match
        string envNameLower = environmentName.ToLower();
        GameObject prefabToUse = null;
        
        if (environmentMapping.ContainsKey(envNameLower))
        {
            prefabToUse = environmentMapping[envNameLower];
        }
        else
        {
            // Try to find partial match
            foreach (var mapping in environmentMapping)
            {
                if (envNameLower.Contains(mapping.Key) || mapping.Key.Contains(envNameLower))
                {
                    prefabToUse = mapping.Value;
                    break;
                }
            }
            
            // If still no match, use first available environment
            if (prefabToUse == null && environmentPrefabs.Length > 0)
            {
                prefabToUse = environmentPrefabs[0];
            }
        }
        
        // Instantiate new environment
        if (prefabToUse != null && environmentContainer != null)
        {
            currentEnvironment = Instantiate(prefabToUse, environmentContainer);
        }
        
        Debug.Log($"Environment set to: {environmentName}");
    }
    
    private void UpdateIdleVariations()
    {
        // Add subtle variations to idle animations to make character seem more lifelike
        idleTimer += Time.deltaTime;
        
        if (idleTimer >= idleVariationInterval)
        {
            idleTimer = 0f;
            
            // Trigger a random idle variation if in neutral pose
            if (currentPose == "neutral" && animator != null)
            {
                int variation = UnityEngine.Random.Range(1, 4); // 1-3 different variations
                animator.SetInteger("IdleVariation", variation);
                animator.SetTrigger("IdleVariationTrigger");
            }
        }
    }
    
    public void PlayGesture(string gestureName)
    {
        if (string.IsNullOrEmpty(gestureName) || animator == null)
            return;
            
        // Play one-time gesture animation
        animator.SetTrigger(gestureName);
        
        Debug.Log($"Playing gesture: {gestureName}");
    }
    
    public void LookAt(Vector3 targetPosition)
    {
        if (characterModel == null)
            return;
            
        // Make character look at a specific point
        Vector3 directionToTarget = targetPosition - characterModel.transform.position;
        directionToTarget.y = 0; // Keep character upright
        
        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            characterModel.transform.rotation = Quaternion.Slerp(
                characterModel.transform.rotation,
                targetRotation,
                Time.deltaTime * blendSpeed
            );
        }
    }
    
    public void MoveToPosition(Vector3 targetPosition, Action onArrival = null)
    {
        // In a real implementation, this would handle pathfinding and movement
        // For now, we'll just teleport the character
        if (characterModel != null)
        {
            characterModel.transform.position = targetPosition;
            onArrival?.Invoke();
        }
    }
}