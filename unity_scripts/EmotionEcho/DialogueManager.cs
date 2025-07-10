using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Manages dialogue flow and branching for the EmotionEcho game
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float pauseBetweenLines = 1.5f;
    [SerializeField] private AudioClip typingSoundEffect;
    [SerializeField] private float typingSoundInterval = 0.1f;
    
    [Header("Response Settings")]
    [SerializeField] private float responseTimeLimit = 15f;
    [SerializeField] private bool autoAdvanceOnTimeout = true;
    
    // Events
    public event Action<string> OnDialoguePresented;
    public event Action<string, string> OnPlayerResponseProcessed; // response, sentiment
    
    // References
    private StoryManager storyManager;
    private AudioSource audioSource;
    
    // Current dialogue state
    private string currentDialogue;
    private bool isWaitingForResponse = false;
    private float responseTimer = 0f;
    
    // Dialogue history
    private List<DialogueEntry> dialogueHistory = new List<DialogueEntry>();
    
    [Serializable]
    public class DialogueEntry
    {
        public string speakerName;
        public string dialogueText;
        public string playerResponse;
        public float timestamp;
    }
    
    private void Awake()
    {
        storyManager = FindObjectOfType<StoryManager>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    public void ApplyConfig(Dictionary<string, object> config)
    {
        if (config.ContainsKey("typing_speed"))
            typingSpeed = Convert.ToSingle(config["typing_speed"]);
            
        if (config.ContainsKey("pause_between_lines"))
            pauseBetweenLines = Convert.ToSingle(config["pause_between_lines"]);
            
        if (config.ContainsKey("response_time_limit"))
            responseTimeLimit = Convert.ToSingle(config["response_time_limit"]);
            
        if (config.ContainsKey("auto_advance_on_timeout"))
            autoAdvanceOnTimeout = Convert.ToBoolean(config["auto_advance_on_timeout"]);
    }
    
    private void Update()
    {
        // Check for response timeout
        if (isWaitingForResponse)
        {
            responseTimer -= Time.deltaTime;
            
            if (responseTimer <= 0 && autoAdvanceOnTimeout)
            {
                // Auto-advance if no response given
                HandleResponseTimeout();
            }
        }
    }
    
    public void PresentDialogue(string speakerName, string dialogueText)
    {
        // Stop waiting for previous response
        isWaitingForResponse = false;
        
        // Store the current dialogue
        currentDialogue = dialogueText;
        
        // Add to history
        DialogueEntry entry = new DialogueEntry
        {
            speakerName = speakerName,
            dialogueText = dialogueText,
            playerResponse = "",
            timestamp = Time.time
        };
        dialogueHistory.Add(entry);
        
        // Start typing effect
        StartCoroutine(TypeDialogue(speakerName, dialogueText));
    }
    
    private IEnumerator TypeDialogue(string speakerName, string dialogueText)
    {
        // Notify UI to prepare for new dialogue
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.SetSpeakerName(speakerName);
            uiManager.ClearDialogueText();
        }
        
        // Type each character with delay
        float lastSoundTime = -typingSoundInterval;
        
        for (int i = 0; i < dialogueText.Length; i++)
        {
            // Add the next character
            if (uiManager != null)
            {
                uiManager.AppendDialogueText(dialogueText[i].ToString());
            }
            
            // Play typing sound at intervals
            if (typingSoundEffect != null && Time.time - lastSoundTime >= typingSoundInterval)
            {
                audioSource.PlayOneShot(typingSoundEffect, 0.5f);
                lastSoundTime = Time.time;
            }
            
            // Wait for the typing delay
            yield return new WaitForSeconds(typingSpeed);
        }
        
        // Wait for a pause after the dialogue
        yield return new WaitForSeconds(pauseBetweenLines);
        
        // Now wait for player response
        WaitForResponse();
    }
    
    private void WaitForResponse()
    {
        isWaitingForResponse = true;
        responseTimer = responseTimeLimit;
        
        // Notify that dialogue is fully presented and ready for response
        OnDialoguePresented?.Invoke(currentDialogue);
        
        // Update UI to show response prompt
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowResponsePrompt(responseTimeLimit);
        }
    }
    
    public void PlayerResponded(string response)
    {
        if (!isWaitingForResponse)
            return;
            
        isWaitingForResponse = false;
        
        // Update the last dialogue entry with the response
        if (dialogueHistory.Count > 0)
        {
            DialogueEntry lastEntry = dialogueHistory[dialogueHistory.Count - 1];
            lastEntry.playerResponse = response;
            dialogueHistory[dialogueHistory.Count - 1] = lastEntry;
        }
        
        // Update UI
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowPlayerResponse(response);
            uiManager.HideResponsePrompt();
        }
        
        // Process the response
        ProcessResponse(response);
    }
    
    private void HandleResponseTimeout()
    {
        isWaitingForResponse = false;
        
        // Update UI
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.HideResponsePrompt();
        }
        
        // Move to next dialogue with a default/timeout response
        ProcessResponse("[No response]");
    }
    
    private void ProcessResponse(string response)
    {
        // Determine sentiment (this would be done by EmotionAnalyzer in a real implementation)
        string simpleSentiment = "neutral";
        
        // Notify about the processed response
        OnPlayerResponseProcessed?.Invoke(response, simpleSentiment);
        
        // Get the next dialogue from the story manager
        storyManager.AdvanceStory(response, simpleSentiment);
    }
    
    public string GetCurrentDialogue()
    {
        return currentDialogue;
    }
    
    public List<DialogueEntry> GetDialogueHistory()
    {
        return dialogueHistory;
    }
}