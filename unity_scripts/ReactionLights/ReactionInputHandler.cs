using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handles user input for the ReactionLights game
/// </summary>
public class ReactionInputHandler : MonoBehaviour, IPointerDownHandler
{
    [Header("Input Configuration")]
    [SerializeField] private bool allowTouchInput = true;
    [SerializeField] private bool allowMouseInput = true;
    [SerializeField] private bool allowKeyboardInput = true;
    [SerializeField] private KeyCode reactKey = KeyCode.Space;
    
    // Event callbacks
    public event Action OnUserReaction;
    
    // External references
    private ReactionLightsController gameController;
    
    // Input state
    private bool isInputEnabled = true;
    
    private void Awake()
    {
        gameController = GetComponentInParent<ReactionLightsController>();
        
        if (gameController == null)
        {
            Debug.LogError("ReactionInputHandler requires a ReactionLightsController component in parent hierarchy");
        }
    }
    
    private void Update()
    {
        if (!isInputEnabled) return;
        
        // Handle keyboard input
        if (allowKeyboardInput && Input.GetKeyDown(reactKey))
        {
            TriggerReaction();
        }
        
        // Handle mouse input
        if (allowMouseInput && Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            TriggerReaction();
        }
        
        // Touch input is handled by the IPointerDownHandler interface
    }
    
    /// <summary>
    /// Handles pointer down events (touch/mouse)
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isInputEnabled || !allowTouchInput) return;
        
        TriggerReaction();
    }
    
    /// <summary>
    /// Triggers the reaction event
    /// </summary>
    private void TriggerReaction()
    {
        if (OnUserReaction != null)
        {
            OnUserReaction();
        }
    }
    
    /// <summary>
    /// Enables or disables input processing
    /// </summary>
    public void SetInputEnabled(bool enabled)
    {
        isInputEnabled = enabled;
    }
    
    /// <summary>
    /// Checks if the pointer is over a UI element
    /// </summary>
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}