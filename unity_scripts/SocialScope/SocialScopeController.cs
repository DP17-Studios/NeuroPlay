using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Main controller for the SocialScope game - Autism Interaction Simulator
/// </summary>
public class SocialScopeController : MonoBehaviour
{
    [Header("Game Configuration")]
    [SerializeField] private float sessionDuration = 600f; // 10 minutes
    [SerializeField] private int scenariosPerSession = 5;
    [SerializeField] private float timePerScenario = 120f; // 2 minutes
    
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scenarioText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject scenarioPanel;
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private Button[] choiceButtons;
    
    [Header("Character")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private FacialExpressionController facialController;
    [SerializeField] private DialogueController dialogueController;
    
    [Header("Audio")]
    [SerializeField] private AudioClip correctChoiceSound;
    [SerializeField] private AudioClip incorrectChoiceSound;
    [SerializeField] private AudioClip scenarioStartSound;
    [SerializeField] private AudioClip gameOverSound;
    
    // Game state
    private bool gameRunning = false;
    private float timeRemaining;
    private int currentScenario = 0;
    private float scenarioTimeRemaining;
    private int score = 0;
    private bool waitingForChoice = false;
    
    // Scenario data
    private List<SocialScenario> scenarios = new List<SocialScenario>();
    private SocialScenario currentScenarioData;
    private int currentInteractionStep = 0;
    
    // Performance tracking
    private List<ScenarioResult> scenarioResults = new List<ScenarioResult>();
    private Dictionary<string, int> socialSkillScores = new Dictionary<string, int>();
    
    // API client
    private NeuroplaysApiClient apiClient;
    
    // Audio source
    private AudioSource audioSource;
    
    [Serializable]
    public class SocialScenario
    {
        public string id;
        public string title;
        public string description;
        public string setting;
        public string socialSkillFocus;
        public List<SocialInteraction> interactions;
        public string conclusion;
    }
    
    [Serializable]
    public class SocialInteraction
    {
        public string characterDialogue;
        public string characterEmotion;
        public string characterBody;
        public List<InteractionChoice> choices;
        public string socialCue;
    }
    
    [Serializable]
    public class InteractionChoice
    {
        public string text;
        public int appropriatenessScore; // 1-5 scale
        public string feedback;
        public string resultingEmotion;
    }
    
    [Serializable]
    public class ScenarioResult
    {
        public string scenarioId;
        public string socialSkillFocus;
        public int totalScore;
        public int maxPossibleScore;
        public float completionTime;
        public List<InteractionResult> interactions;
    }
    
    [Serializable]
    public class InteractionResult
    {
        public int interactionIndex;
        public int choiceIndex;
        public int appropriatenessScore;
        public float responseTime;
        public string socialCueIdentified; // Whether the player identified the social cue
    }
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        // Initialize UI
        gameOverPanel.SetActive(false);
        feedbackPanel.SetActive(false);
        scenarioPanel.SetActive(false);
        choicesPanel.SetActive(false);
        
        // Initialize social skill scores
        socialSkillScores["nonverbal_cues"] = 0;
        socialSkillScores["conversation"] = 0;
        socialSkillScores["empathy"] = 0;
        socialSkillScores["conflict_resolution"] = 0;
        socialSkillScores["social_norms"] = 0;
    }
    
    private void Start()
    {
        apiClient = NeuroplaysApiClient.Instance;
        
        // Get game configuration and scenarios from server
        StartCoroutine(apiClient.GetGameConfig((success, config) => {
            if (success && config != null && config.ContainsKey("merged_config"))
            {
                Dictionary<string, object> gameConfig = config["merged_config"] as Dictionary<string, object>;
                ApplyGameConfig(gameConfig);
                
                // Get scenarios
                StartCoroutine(GetScenarios((scenariosSuccess) => {
                    if (scenariosSuccess)
                    {
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
                    }
                    else
                    {
                        Debug.LogError("Failed to get scenarios. Using mock scenarios.");
                        CreateMockScenarios();
                        StartGame();
                    }
                }));
            }
            else
            {
                Debug.LogError("Failed to get game configuration. Using defaults.");
                CreateMockScenarios();
                
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
            }
        }));
    }
    
    private void ApplyGameConfig(Dictionary<string, object> config)
    {
        if (config.ContainsKey("session_duration"))
            sessionDuration = Convert.ToSingle(config["session_duration"]);
            
        if (config.ContainsKey("scenarios_per_session"))
            scenariosPerSession = Convert.ToInt32(config["scenarios_per_session"]);
            
        if (config.ContainsKey("time_per_scenario"))
            timePerScenario = Convert.ToSingle(config["time_per_scenario"]);
            
        Debug.Log("Applied game configuration from server");
    }
    
    private IEnumerator GetScenarios(Action<bool> callback)
    {
        // In a real implementation, this would fetch scenarios from the server
        // For now, we'll create mock scenarios
        CreateMockScenarios();
        
        yield return new WaitForSeconds(0.5f);
        
        callback(true);
    }
    
    private void CreateMockScenarios()
    {
        // Create mock scenarios for testing
        scenarios.Clear();
        
        // Scenario 1: Cafe Conversation
        SocialScenario scenario1 = new SocialScenario
        {
            id = "cafe_conversation",
            title = "Cafe Conversation",
            description = "You're meeting a friend at a cafe. Practice maintaining a conversation and reading social cues.",
            setting = "Busy cafe with ambient noise",
            socialSkillFocus = "conversation",
            interactions = new List<SocialInteraction>(),
            conclusion = "The conversation ends as your friend needs to leave for an appointment."
        };
        
        // Add interactions for scenario 1
        SocialInteraction interaction1_1 = new SocialInteraction
        {
            characterDialogue = "Hey there! Sorry I'm late. The traffic was terrible today.",
            characterEmotion = "apologetic",
            characterBody = "waving",
            choices = new List<InteractionChoice>(),
            socialCue = "Your friend seems stressed about being late."
        };
        
        interaction1_1.choices.Add(new InteractionChoice
        {
            text = "No problem! I just got here myself.",
            appropriatenessScore = 5,
            feedback = "Great response! You acknowledged their concern and made them feel comfortable.",
            resultingEmotion = "relieved"
        });
        
        interaction1_1.choices.Add(new InteractionChoice
        {
            text = "You're always late. It's really annoying.",
            appropriatenessScore = 1,
            feedback = "This response is confrontational and might make your friend feel worse.",
            resultingEmotion = "hurt"
        });
        
        interaction1_1.choices.Add(new InteractionChoice
        {
            text = "I've been waiting for 20 minutes, but it's fine.",
            appropriatenessScore = 2,
            feedback = "This passive-aggressive response might make your friend feel guilty.",
            resultingEmotion = "guilty"
        });
        
        scenario1.interactions.Add(interaction1_1);
        
        // Add more interactions for scenario 1...
        SocialInteraction interaction1_2 = new SocialInteraction
        {
            characterDialogue = "So, how has your week been? You mentioned you had a big project at work?",
            characterEmotion = "interested",
            characterBody = "leaning_forward",
            choices = new List<InteractionChoice>(),
            socialCue = "Your friend is showing interest in your life."
        };
        
        interaction1_2.choices.Add(new InteractionChoice
        {
            text = "It's been really busy! The project is challenging but going well. How about you?",
            appropriatenessScore = 5,
            feedback = "Excellent! You shared information and showed reciprocal interest.",
            resultingEmotion = "engaged"
        });
        
        interaction1_2.choices.Add(new InteractionChoice
        {
            text = "Yeah, it's fine I guess. Whatever.",
            appropriatenessScore = 2,
            feedback = "This response doesn't continue the conversation and seems dismissive.",
            resultingEmotion = "confused"
        });
        
        interaction1_2.choices.Add(new InteractionChoice
        {
            text = "Let me tell you EVERYTHING about my project! It started when my boss...",
            appropriatenessScore = 3,
            feedback = "While sharing is good, this response might lead to dominating the conversation.",
            resultingEmotion = "overwhelmed"
        });
        
        scenario1.interactions.Add(interaction1_2);
        
        // Add scenario 1 to the list
        scenarios.Add(scenario1);
        
        // Scenario 2: Job Interview
        SocialScenario scenario2 = new SocialScenario
        {
            id = "job_interview",
            title = "Job Interview",
            description = "You're attending a job interview. Practice professional communication and reading the interviewer's cues.",
            setting = "Corporate office, interview room",
            socialSkillFocus = "nonverbal_cues",
            interactions = new List<SocialInteraction>(),
            conclusion = "The interview concludes with the interviewer saying they'll be in touch soon."
        };
        
        // Add interactions for scenario 2
        SocialInteraction interaction2_1 = new SocialInteraction
        {
            characterDialogue = "Good morning. Thank you for coming in today. Please, have a seat.",
            characterEmotion = "professional",
            characterBody = "gesturing_to_chair",
            choices = new List<InteractionChoice>(),
            socialCue = "The interviewer is formal but welcoming."
        };
        
        interaction2_1.choices.Add(new InteractionChoice
        {
            text = "Good morning. Thank you for the opportunity to interview for this position.",
            appropriatenessScore = 5,
            feedback = "Perfect! You matched the interviewer's professional tone.",
            resultingEmotion = "approving"
        });
        
        interaction2_1.choices.Add(new InteractionChoice
        {
            text = "Hey! What's up? This office is really cool!",
            appropriatenessScore = 1,
            feedback = "This is too casual for a job interview setting.",
            resultingEmotion = "concerned"
        });
        
        interaction2_1.choices.Add(new InteractionChoice
        {
            text = "Morning. [Sit down without saying anything else]",
            appropriatenessScore = 3,
            feedback = "This response is a bit too brief and misses an opportunity to make a good impression.",
            resultingEmotion = "neutral"
        });
        
        scenario2.interactions.Add(interaction2_1);
        
        // Add more scenarios...
        scenarios.Add(scenario2);
        
        // Create at least 5 scenarios
        SocialScenario scenario3 = new SocialScenario
        {
            id = "conflict_resolution",
            title = "Resolving a Misunderstanding",
            description = "Your friend thinks you said something hurtful about them to another friend. Navigate this conflict.",
            setting = "Park bench, quiet afternoon",
            socialSkillFocus = "conflict_resolution",
            interactions = new List<SocialInteraction>(),
            conclusion = "You and your friend reach an understanding and feel closer after resolving the issue."
        };
        
        // Add interactions for scenario 3
        scenarios.Add(scenario3);
        
        SocialScenario scenario4 = new SocialScenario
        {
            id = "group_activity",
            title = "Group Project Planning",
            description = "You're working with classmates on a group project. Practice collaboration and sharing ideas.",
            setting = "School library, study room",
            socialSkillFocus = "social_norms",
            interactions = new List<SocialInteraction>(),
            conclusion = "The group agrees on a plan and divides tasks for the project."
        };
        
        // Add interactions for scenario 4
        scenarios.Add(scenario4);
        
        SocialScenario scenario5 = new SocialScenario
        {
            id = "emotional_support",
            title = "Supporting a Friend",
            description = "Your friend is going through a difficult time. Practice showing empathy and providing support.",
            setting = "Friend's home, living room",
            socialSkillFocus = "empathy",
            interactions = new List<SocialInteraction>(),
            conclusion = "Your friend feels better after talking with you and appreciates your support."
        };
        
        // Add interactions for scenario 5
        scenarios.Add(scenario5);
        
        Debug.Log($"Created {scenarios.Count} mock scenarios");
    }
    
    private void StartGame()
    {
        gameRunning = true;
        timeRemaining = sessionDuration;
        currentScenario = 0;
        score = 0;
        scenarioResults.Clear();
        
        // Reset social skill scores
        foreach (string skill in socialSkillScores.Keys)
        {
            socialSkillScores[skill] = 0;
        }
        
        // Start first scenario
        StartNextScenario();
        
        Debug.Log("SocialScope game started");
    }
    
    private void StartNextScenario()
    {
        if (currentScenario >= scenariosPerSession || currentScenario >= scenarios.Count)
        {
            EndGame();
            return;
        }
        
        // Get current scenario
        currentScenarioData = scenarios[currentScenario];
        currentInteractionStep = 0;
        scenarioTimeRemaining = timePerScenario;
        
        // Show scenario introduction
        scenarioPanel.SetActive(true);
        choicesPanel.SetActive(false);
        
        TextMeshProUGUI titleText = scenarioPanel.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descriptionText = scenarioPanel.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
        
        if (titleText != null)
            titleText.text = currentScenarioData.title;
            
        if (descriptionText != null)
            descriptionText.text = currentScenarioData.description;
            
        // Play scenario start sound
        audioSource.PlayOneShot(scenarioStartSound);
        
        // Set up character
        if (characterController != null)
            characterController.SetEnvironment(currentScenarioData.setting);
            
        // Start scenario after delay
        StartCoroutine(StartScenarioAfterDelay(3.0f));
        
        Debug.Log($"Starting scenario {currentScenario + 1}: {currentScenarioData.title}");
    }
    
    private IEnumerator StartScenarioAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        scenarioPanel.SetActive(false);
        
        // Start first interaction
        StartNextInteraction();
    }
    
    private void StartNextInteraction()
    {
        if (currentInteractionStep >= currentScenarioData.interactions.Count)
        {
            // End of scenario
            EndScenario();
            return;
        }
        
        // Get current interaction
        SocialInteraction interaction = currentScenarioData.interactions[currentInteractionStep];
        
        // Display character dialogue and emotion
        if (dialogueController != null)
            dialogueController.SpeakDialogue(interaction.characterDialogue);
            
        if (facialController != null)
            facialController.SetExpression(interaction.characterEmotion);
            
        if (characterController != null)
            characterController.SetPose(interaction.characterBody);
            
        // Display social cue (if appropriate for difficulty level)
        instructionText.text = $"Social Cue: {interaction.socialCue}";
        
        // Show choices
        DisplayChoices(interaction.choices);
        
        waitingForChoice = true;
        
        Debug.Log($"Started interaction {currentInteractionStep + 1}");
    }
    
    private void DisplayChoices(List<InteractionChoice> choices)
    {
        choicesPanel.SetActive(true);
        
        // Set up choice buttons
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < choices.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);
                TextMeshProUGUI buttonText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = choices[i].text;
                
                int choiceIndex = i; // Capture for lambda
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => MakeChoice(choiceIndex));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }
    
    private void MakeChoice(int choiceIndex)
    {
        if (!waitingForChoice)
            return;
            
        waitingForChoice = false;
        choicesPanel.SetActive(false);
        
        // Get current interaction and choice
        SocialInteraction interaction = currentScenarioData.interactions[currentInteractionStep];
        InteractionChoice choice = interaction.choices[choiceIndex];
        
        // Record result
        InteractionResult result = new InteractionResult
        {
            interactionIndex = currentInteractionStep,
            choiceIndex = choiceIndex,
            appropriatenessScore = choice.appropriatenessScore,
            responseTime = timePerScenario - scenarioTimeRemaining,
            socialCueIdentified = "yes" // Assume player read the cue
        };
        
        // Ensure the current scenario has a results entry
        if (currentScenario >= scenarioResults.Count)
        {
            scenarioResults.Add(new ScenarioResult
            {
                scenarioId = currentScenarioData.id,
                socialSkillFocus = currentScenarioData.socialSkillFocus,
                totalScore = 0,
                maxPossibleScore = currentScenarioData.interactions.Count * 5, // 5 is max score per interaction
                completionTime = 0,
                interactions = new List<InteractionResult>()
            });
        }
        
        scenarioResults[currentScenario].interactions.Add(result);
        
        // Update score
        int pointsEarned = choice.appropriatenessScore;
        score += pointsEarned;
        scenarioResults[currentScenario].totalScore += pointsEarned;
        
        // Update social skill score
        if (socialSkillScores.ContainsKey(currentScenarioData.socialSkillFocus))
        {
            socialSkillScores[currentScenarioData.socialSkillFocus] += pointsEarned;
        }
        
        // Show feedback
        ShowFeedback(choice);
        
        // Play sound based on choice quality
        if (choice.appropriatenessScore >= 4)
        {
            audioSource.PlayOneShot(correctChoiceSound);
        }
        else if (choice.appropriatenessScore <= 2)
        {
            audioSource.PlayOneShot(incorrectChoiceSound);
        }
        
        Debug.Log($"Player chose option {choiceIndex + 1} with score {choice.appropriatenessScore}");
        
        // Update character reaction
        if (facialController != null)
            facialController.SetExpression(choice.resultingEmotion);
    }
    
    private void ShowFeedback(InteractionChoice choice)
    {
        feedbackPanel.SetActive(true);
        feedbackText.text = choice.feedback;
        
        // Show feedback for a few seconds, then continue
        StartCoroutine(ContinueAfterFeedback(3.0f));
    }
    
    private IEnumerator ContinueAfterFeedback(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        feedbackPanel.SetActive(false);
        
        // Move to next interaction
        currentInteractionStep++;
        StartNextInteraction();
    }
    
    private void EndScenario()
    {
        // Calculate completion time
        scenarioResults[currentScenario].completionTime = timePerScenario - scenarioTimeRemaining;
        
        // Show scenario conclusion
        scenarioPanel.SetActive(true);
        
        TextMeshProUGUI titleText = scenarioPanel.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descriptionText = scenarioPanel.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
        
        if (titleText != null)
            titleText.text = "Scenario Complete";
            
        if (descriptionText != null)
            descriptionText.text = currentScenarioData.conclusion;
            
        // Upload scenario data
        UploadScenarioData(currentScenario);
        
        // Move to next scenario after delay
        StartCoroutine(NextScenarioDelay());
        
        Debug.Log($"Completed scenario {currentScenario + 1}");
    }
    
    private IEnumerator NextScenarioDelay()
    {
        yield return new WaitForSeconds(3.0f);
        
        scenarioPanel.SetActive(false);
        
        // Move to next scenario
        currentScenario++;
        StartNextScenario();
    }
    
    private void EndGame()
    {
        gameRunning = false;
        
        // Upload final metrics
        UploadGameMetrics();
        
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
            finalScoreText.text = $"Final Score: {score}\nScenarios Completed: {currentScenario}";
        }
        
        // Show social skill breakdown
        TextMeshProUGUI skillsText = gameOverPanel.transform.Find("SkillsText")?.GetComponent<TextMeshProUGUI>();
        if (skillsText != null)
        {
            string skillsBreakdown = "Social Skills:\n";
            foreach (var skill in socialSkillScores)
            {
                string skillName = skill.Key.Replace("_", " ");
                skillName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(skillName);
                skillsBreakdown += $"{skillName}: {skill.Value}\n";
            }
            skillsText.text = skillsBreakdown;
        }
        
        // Play game over sound
        audioSource.PlayOneShot(gameOverSound);
        
        Debug.Log($"Game over! Final score: {score}");
    }
    
    private void UploadScenarioData(int scenarioIndex)
    {
        if (scenarioIndex >= scenarioResults.Count)
            return;
            
        ScenarioResult result = scenarioResults[scenarioIndex];
        
        apiClient.AddSessionData($"scenario_{scenarioIndex + 1}", result);
        StartCoroutine(apiClient.UploadSessionData());
    }
    
    private void UploadGameMetrics()
    {
        // Calculate social intelligence metrics
        Dictionary<string, object> socialMetrics = new Dictionary<string, object>();
        
        // Calculate overall social intelligence score (0-100)
        int totalScore = 0;
        int maxPossibleScore = 0;
        
        foreach (ScenarioResult result in scenarioResults)
        {
            totalScore += result.totalScore;
            maxPossibleScore += result.maxPossibleScore;
        }
        
        float socialIntelligenceScore = maxPossibleScore > 0 ? 
            (float)totalScore / maxPossibleScore * 100f : 0f;
            
        socialMetrics["social_intelligence_score"] = socialIntelligenceScore;
        
        // Calculate skill-specific scores
        foreach (var skill in socialSkillScores)
        {
            // Normalize to 0-100 scale
            int maxSkillScore = 0;
            foreach (ScenarioResult result in scenarioResults)
            {
                if (result.socialSkillFocus == skill.Key)
                {
                    maxSkillScore += result.maxPossibleScore;
                }
            }
            
            float normalizedScore = maxSkillScore > 0 ?
                (float)skill.Value / maxSkillScore * 100f : 0f;
                
            socialMetrics[$"{skill.Key}_score"] = normalizedScore;
        }
        
        // Calculate response time metrics
        float avgResponseTime = 0f;
        int responseCount = 0;
        
        foreach (ScenarioResult result in scenarioResults)
        {
            foreach (InteractionResult interaction in result.interactions)
            {
                avgResponseTime += interaction.responseTime;
                responseCount++;
            }
        }
        
        if (responseCount > 0)
        {
            avgResponseTime /= responseCount;
        }
        
        socialMetrics["average_response_time"] = avgResponseTime;
        
        // Prepare data for upload
        apiClient.AddSessionData("final_score", score);
        apiClient.AddSessionData("scenarios_completed", currentScenario);
        apiClient.AddSessionData("social_metrics", socialMetrics);
        apiClient.AddSessionData("scenario_results", scenarioResults);
        apiClient.AddSessionData("social_skill_scores", socialSkillScores);
        
        // Upload data
        StartCoroutine(apiClient.UploadSessionData());
    }
    
    private void Update()
    {
        if (!gameRunning)
            return;
            
        // Update game timer
        timeRemaining -= Time.deltaTime;
        
        // Update scenario timer if in a scenario
        if (currentScenario < scenariosPerSession && currentScenario < scenarios.Count)
        {
            scenarioTimeRemaining -= Time.deltaTime;
        }
        
        // Update UI
        UpdateUI();
        
        // Check for time-based game over
        if (timeRemaining <= 0)
        {
            EndGame();
        }
    }
    
    private void UpdateUI()
    {
        // Update timer
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = $"Time: {minutes:00}:{seconds:00}";
        
        // Update score
        scoreText.text = $"Score: {score}";
        
        // Update scenario text
        if (currentScenario < scenariosPerSession && currentScenario < scenarios.Count)
        {
            scenarioText.text = $"Scenario {currentScenario + 1}/{scenariosPerSession}: {currentScenarioData.title}";
        }
    }
    
    public void RestartGame()
    {
        gameOverPanel.SetActive(false);
        StartGame();
    }
}