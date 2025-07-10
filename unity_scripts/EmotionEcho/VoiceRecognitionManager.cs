using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Windows.Speech;
using System.Linq;

/// <summary>
/// Handles speech input and processing for the EmotionEcho game
/// </summary>
public class VoiceRecognitionManager : MonoBehaviour
{
    [Header("Voice Recognition Settings")]
    [SerializeField] private float confidenceThreshold = 0.5f;
    [SerializeField] private float maxRecordingTime = 10f;
    [SerializeField] private bool useMockRecognition = false; // For testing without microphone
    [SerializeField] private string[] mockResponses;
    
    [Header("Audio Feedback")]
    [SerializeField] private AudioClip startListeningSound;
    [SerializeField] private AudioClip stopListeningSound;
    [SerializeField] private AudioClip recognitionSuccessSound;
    
    // Events
    public event Action<string> OnSpeechRecognized;
    public event Action OnListeningStarted;
    public event Action OnListeningStopped;
    
    // Speech recognition components
    private DictationRecognizer dictationRecognizer;
    private AudioSource audioSource;
    private bool isListening = false;
    private float listeningTimer = 0f;
    private string currentRecognizedText = "";
    
    // Mock recognition for testing
    private System.Random random = new System.Random();
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        // Initialize mock responses if needed
        if (useMockRecognition && (mockResponses == null || mockResponses.Length == 0))
        {
            mockResponses = new string[]
            {
                "I feel happy about that.",
                "That makes me a bit sad.",
                "I'm not sure how to respond to that.",
                "I'm feeling anxious just thinking about it.",
                "That's really interesting to hear.",
                "I've never thought about it that way before.",
                "I strongly disagree with that perspective.",
                "That reminds me of something from my childhood."
            };
        }
    }
    
    private void Start()
    {
        if (!useMockRecognition)
        {
            InitializeDictationRecognizer();
        }
    }
    
    private void OnDestroy()
    {
        if (dictationRecognizer != null)
        {
            if (dictationRecognizer.Status == SpeechSystemStatus.Running)
            {
                dictationRecognizer.Stop();
            }
            dictationRecognizer.Dispose();
        }
    }
    
    private void Update()
    {
        // Check for listening timeout
        if (isListening)
        {
            listeningTimer -= Time.deltaTime;
            
            if (listeningTimer <= 0)
            {
                StopListening();
            }
        }
    }
    
    private void InitializeDictationRecognizer()
    {
        dictationRecognizer = new DictationRecognizer();
        
        dictationRecognizer.DictationResult += (text, confidence) => {
            // Only accept results above confidence threshold
            if (confidence >= confidenceThreshold)
            {
                currentRecognizedText += text + " ";
            }
        };
        
        dictationRecognizer.DictationComplete += (completionCause) => {
            if (completionCause != DictationCompletionCause.Complete)
            {
                Debug.LogWarningFormat("Dictation completed unsuccessfully: {0}.", completionCause);
            }
            
            // Process the final recognized text
            ProcessRecognizedSpeech();
            
            isListening = false;
            OnListeningStopped?.Invoke();
        };
        
        dictationRecognizer.DictationError += (error, hresult) => {
            Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
            isListening = false;
            OnListeningStopped?.Invoke();
        };
    }
    
    public void StartListening()
    {
        if (isListening)
            return;
            
        isListening = true;
        listeningTimer = maxRecordingTime;
        currentRecognizedText = "";
        
        // Play audio feedback
        if (startListeningSound != null)
        {
            audioSource.PlayOneShot(startListeningSound);
        }
        
        OnListeningStarted?.Invoke();
        
        if (useMockRecognition)
        {
            // Use mock recognition after a delay
            StartCoroutine(MockRecognitionRoutine());
        }
        else
        {
            // Start the actual dictation recognizer
            if (dictationRecognizer != null && dictationRecognizer.Status != SpeechSystemStatus.Running)
            {
                dictationRecognizer.Start();
            }
        }
    }
    
    public void StopListening()
    {
        if (!isListening)
            return;
            
        // Play audio feedback
        if (stopListeningSound != null)
        {
            audioSource.PlayOneShot(stopListeningSound);
        }
        
        if (useMockRecognition)
        {
            // Just set the flag, coroutine will handle the rest
            isListening = false;
        }
        else
        {
            // Stop the actual dictation recognizer
            if (dictationRecognizer != null && dictationRecognizer.Status == SpeechSystemStatus.Running)
            {
                dictationRecognizer.Stop();
            }
        }
        
        OnListeningStopped?.Invoke();
    }
    
    private void ProcessRecognizedSpeech()
    {
        // Clean up the text
        string processedText = currentRecognizedText.Trim();
        
        if (!string.IsNullOrEmpty(processedText))
        {
            // Play success sound
            if (recognitionSuccessSound != null)
            {
                audioSource.PlayOneShot(recognitionSuccessSound);
            }
            
            // Notify listeners
            OnSpeechRecognized?.Invoke(processedText);
        }
    }
    
    private IEnumerator MockRecognitionRoutine()
    {
        // Wait for a random time to simulate thinking/speaking
        float waitTime = UnityEngine.Random.Range(1.5f, 4.0f);
        yield return new WaitForSeconds(waitTime);
        
        // If still listening, generate a mock response
        if (isListening)
        {
            int responseIndex = random.Next(0, mockResponses.Length);
            currentRecognizedText = mockResponses[responseIndex];
            
            // Process the mock response
            ProcessRecognizedSpeech();
            
            // Stop listening
            isListening = false;
            OnListeningStopped?.Invoke();
        }
    }
    
    // For external control of mock responses (e.g., for testing specific emotional responses)
    public void SetMockResponses(string[] responses)
    {
        if (useMockRecognition)
        {
            mockResponses = responses;
        }
    }
}