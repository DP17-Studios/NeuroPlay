using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages UI elements for the ReactionLights game
/// </summary>
public class ReactionUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private RectTransform lightsContainer;
    [SerializeField] private GameObject lightPrefab;
    [SerializeField] private Text instructionText;
    [SerializeField] private Text resultText;
    [SerializeField] private Text attemptCounterText;
    [SerializeField] private Text averageReactionTimeText;
    [SerializeField] private Button startButton;
    [SerializeField] private Image touchPanel;
    
    [Header("UI Configuration")]
    [SerializeField] private Color redLightColor = Color.red;
    [SerializeField] private Color greenLightColor = Color.green;
    [SerializeField] private int numberOfLights = 5;
    [SerializeField] private float lightSpacing = 100f;
    
    // Generated lights
    private List<GameObject> redLightObjects = new List<GameObject>();
    private List<GameObject> greenLightObjects = new List<GameObject>();
    
    // Reference to main controller
    private ReactionLightsController gameController;
    
    private void Awake()
    {
        gameController = GetComponent<ReactionLightsController>();
        
        if (gameController == null)
        {
            Debug.LogError("ReactionUIManager requires a ReactionLightsController component on the same GameObject");
        }
    }
    
    private void Start()
    {
        // Create lights dynamically
        CreateLights();
        
        // Set up UI references in the game controller
        SetupControllerReferences();
    }
    
    /// <summary>
    /// Creates the F1-style light objects dynamically
    /// </summary>
    private void CreateLights()
    {
        if (lightPrefab == null || lightsContainer == null)
        {
            Debug.LogError("Light prefab or container is missing");
            return;
        }
        
        // Calculate starting position for center alignment
        float startX = -(lightSpacing * (numberOfLights - 1)) / 2f;
        
        for (int i = 0; i < numberOfLights; i++)
        {
            // Create red light
            GameObject redLight = Instantiate(lightPrefab, lightsContainer);
            RectTransform redRect = redLight.GetComponent<RectTransform>();
            redRect.anchoredPosition = new Vector2(startX + (i * lightSpacing), 50f);
            
            Image redImage = redLight.GetComponent<Image>();
            if (redImage != null)
            {
                redImage.color = redLightColor;
            }
            
            redLight.name = $"RedLight_{i}";
            redLight.SetActive(false);
            redLightObjects.Add(redLight);
            
            // Create green light
            GameObject greenLight = Instantiate(lightPrefab, lightsContainer);
            RectTransform greenRect = greenLight.GetComponent<RectTransform>();
            greenRect.anchoredPosition = new Vector2(startX + (i * lightSpacing), 50f);
            
            Image greenImage = greenLight.GetComponent<Image>();
            if (greenImage != null)
            {
                greenImage.color = greenLightColor;
            }
            
            greenLight.name = $"GreenLight_{i}";
            greenLight.SetActive(false);
            greenLightObjects.Add(greenLight);
        }
    }
    
    /// <summary>
    /// Sets up UI references in the game controller
    /// </summary>
    private void SetupControllerReferences()
    {
        // Use reflection to set serialized fields in the controller
        // This is a bit of a hack but allows for proper separation of concerns
        
        System.Reflection.FieldInfo redLightsField = typeof(ReactionLightsController).GetField("redLights", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        System.Reflection.FieldInfo greenLightsField = typeof(ReactionLightsController).GetField("greenLights", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        System.Reflection.FieldInfo instructionTextField = typeof(ReactionLightsController).GetField("instructionText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        System.Reflection.FieldInfo resultTextField = typeof(ReactionLightsController).GetField("resultText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        System.Reflection.FieldInfo attemptCounterTextField = typeof(ReactionLightsController).GetField("attemptCounterText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        System.Reflection.FieldInfo averageReactionTimeTextField = typeof(ReactionLightsController).GetField("averageReactionTimeText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        System.Reflection.FieldInfo startButtonField = typeof(ReactionLightsController).GetField("startButton", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        System.Reflection.FieldInfo touchPanelField = typeof(ReactionLightsController).GetField("touchPanel", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (redLightsField != null) redLightsField.SetValue(gameController, redLightObjects.ToArray());
        if (greenLightsField != null) greenLightsField.SetValue(gameController, greenLightObjects.ToArray());
        if (instructionTextField != null) instructionTextField.SetValue(gameController, instructionText);
        if (resultTextField != null) resultTextField.SetValue(gameController, resultText);
        if (attemptCounterTextField != null) attemptCounterTextField.SetValue(gameController, attemptCounterText);
        if (averageReactionTimeTextField != null) averageReactionTimeTextField.SetValue(gameController, averageReactionTimeText);
        if (startButtonField != null) startButtonField.SetValue(gameController, startButton);
        if (touchPanelField != null) touchPanelField.SetValue(gameController, touchPanel);
    }
    
    /// <summary>
    /// Update the visual state of a light
    /// </summary>
    public void UpdateLightState(int index, bool isRed, bool isActive)
    {
        if (isRed && index >= 0 && index < redLightObjects.Count)
        {
            redLightObjects[index].SetActive(isActive);
        }
        else if (!isRed && index >= 0 && index < greenLightObjects.Count)
        {
            greenLightObjects[index].SetActive(isActive);
        }
    }
}