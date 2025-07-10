using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Controls dialogue display and speech for characters in the SocialScope game
/// </summary>
public class DialogueController : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private Image speakerPortrait;
    
    [Header("Dialogue Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float punctuationPause = 0.2f;
    [SerializeField] private AudioClip typingSound;
    [SerializeField] private float typingSoundInterval = 0.1f;
    
    [Header("Character Settings")]
    [SerializeField] private string defaultSpeakerName = "Character";
    [SerializeField] private Sprite defaultPortrait;
    
    // Events
    public event Action<string> OnDialogueStarted;
    public event Action<string> OnDialogueCompleted;
    public event Action OnDialogueCancelled;
    
    // Dialogue state
    private string currentDialogue;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private AudioSource audioSource;
    
    // Character references
    private FacialExpressionController facialController;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        facialController = GetComponent<FacialExpressionController>();
        
        // Hide dialogue panel initially
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
    
    public void SpeakDialogue(string dialogue, string speakerName = "", Sprite portrait = null)
    {
        if (string.IsNullOrEmpty(dialogue))
            return;
            
        // Cancel any ongoing dialogue
        if (isTyping && typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            OnDialogueCancelled?.Invoke();
        }
        
        // Set up dialogue
        currentDialogue = dialogue;
        
        // Show dialogue panel
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
            
        // Set speaker name
        if (speakerNameText != null)
        {
            speakerNameText.text = string.IsNullOrEmpty(speakerName) ? defaultSpeakerName : speakerName;
        }
        
        // Set portrait
        if (speakerPortrait != null)
        {
            speakerPortrait.sprite = portrait != null ? portrait : defaultPortrait;
            speakerPortrait.gameObject.SetActive(speakerPortrait.sprite != null);
        }
        
        // Clear text
        if (dialogueText != null)
            dialogueText.text = "";
            
        // Start typing effect
        typingCoroutine = StartCoroutine(TypeDialogue(dialogue));
        
        // Notify listeners
        OnDialogueStarted?.Invoke(dialogue);
        
        Debug.Log($"Speaking dialogue: {dialogue}");
    }
    
    private IEnumerator TypeDialogue(string dialogue)
    {
        isTyping = true;
        
        // Start with empty text
        if (dialogueText != null)
            dialogueText.text = "";
            
        float lastSoundTime = -typingSoundInterval;
        
        // Type each character with delay
        for (int i = 0; i < dialogue.Length; i++)
        {
            // Add the next character
            if (dialogueText != null)
                dialogueText.text += dialogue[i];
                
            // Play typing sound at intervals
            if (typingSound != null && Time.time - lastSoundTime >= typingSoundInterval)
            {
                audioSource.PlayOneShot(typingSound, 0.5f);
                lastSoundTime = Time.time;
            }
            
            // Sync lip movement with text (in a real implementation, this would be more sophisticated)
            if (facialController != null && i % 5 == 0)
            {
                // Simple talking animation
                if (UnityEngine.Random.value > 0.5f)
                    facialController.SetExpression("talking_open");
                else
                    facialController.SetExpression("talking_closed");
            }
            
            // Add extra pause for punctuation
            if (i < dialogue.Length - 1 && IsPunctuation(dialogue[i]))
            {
                yield return new WaitForSeconds(punctuationPause);
            }
            else
            {
                yield return new WaitForSeconds(typingSpeed);
            }
        }
        
        // Reset to neutral expression when done talking
        if (facialController != null)
        {
            facialController.SetExpression("neutral");
        }
        
        isTyping = false;
        
        // Notify listeners
        OnDialogueCompleted?.Invoke(dialogue);
        
        // Wait for a moment before hiding the dialogue panel
        yield return new WaitForSeconds(1.0f);
        
        // Hide dialogue panel
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
    
    private bool IsPunctuation(char c)
    {
        return c == '.' || c == ',' || c == '!' || c == '?' || c == ';' || c == ':';
    }
    
    public void SkipTyping()
    {
        if (!isTyping)
            return;
            
        // Stop typing coroutine
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // Show full text immediately
        if (dialogueText != null)
            dialogueText.text = currentDialogue;
            
        isTyping = false;
        
        // Notify listeners
        OnDialogueCompleted?.Invoke(currentDialogue);
    }
    
    public bool IsDialogueActive()
    {
        return isTyping || (dialoguePanel != null && dialoguePanel.activeSelf);
    }
    
    public void HideDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        if (isTyping && typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;
            OnDialogueCancelled?.Invoke();
        }
    }
    
    // For more complex dialogue with branching
    public void SpeakDialogueWithOptions(string dialogue, string speakerName, Sprite portrait, 
                                        List<string> options, Action<int> onOptionSelected)
    {
        // First speak the dialogue
        SpeakDialogue(dialogue, speakerName, portrait);
        
        // Then show options (in a real implementation, this would be more sophisticated)
        StartCoroutine(ShowOptionsAfterDialogue(options, onOptionSelected));
    }
    
    private IEnumerator ShowOptionsAfterDialogue(List<string> options, Action<int> onOptionSelected)
    {
        // Wait until dialogue is complete
        while (isTyping)
        {
            yield return null;
        }
        
        // Wait a moment before showing options
        yield return new WaitForSeconds(0.5f);
        
        // In a real implementation, this would create and display option buttons
        Debug.Log("Showing dialogue options: " + string.Join(", ", options));
        
        // For now, we'll just simulate option selection
        int selectedOption = UnityEngine.Random.Range(0, options.Count);
        onOptionSelected?.Invoke(selectedOption);
    }
}