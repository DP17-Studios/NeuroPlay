using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Represents a single cell in the memory grid
/// </summary>
public class GridCell : MonoBehaviour
{
    [Header("Visual States")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color correctSelectionColor = Color.green;
    [SerializeField] private Color incorrectSelectionColor = Color.red;
    
    [Header("Animation")]
    [SerializeField] private float highlightAnimationDuration = 0.3f;
    [SerializeField] private float selectionAnimationDuration = 0.5f;
    
    // References
    private Image cellImage;
    private Button cellButton;
    
    // Cell data
    private int cellIndex;
    
    // Events
    public event Action<int> OnCellClicked;
    
    private void Awake()
    {
        cellImage = GetComponent<Image>();
        cellButton = GetComponent<Button>();
        
        if (cellImage == null)
            cellImage = gameObject.AddComponent<Image>();
            
        if (cellButton == null)
            cellButton = gameObject.AddComponent<Button>();
            
        // Set default color
        cellImage.color = defaultColor;
        
        // Set up button click
        cellButton.onClick.AddListener(() => {
            OnCellClicked?.Invoke(cellIndex);
        });
    }
    
    public void SetIndex(int index)
    {
        cellIndex = index;
    }
    
    public int GetIndex()
    {
        return cellIndex;
    }
    
    public void Highlight(bool highlight)
    {
        StopAllCoroutines();
        
        if (highlight)
        {
            StartCoroutine(AnimateColor(highlightColor, highlightAnimationDuration));
        }
        else
        {
            StartCoroutine(AnimateColor(defaultColor, highlightAnimationDuration));
        }
    }
    
    public void Select(bool correct)
    {
        StopAllCoroutines();
        
        Color targetColor = correct ? correctSelectionColor : incorrectSelectionColor;
        StartCoroutine(AnimateColorWithBounce(targetColor, selectionAnimationDuration));
    }
    
    public void Reset()
    {
        StopAllCoroutines();
        cellImage.color = defaultColor;
        transform.localScale = Vector3.one;
    }
    
    private IEnumerator AnimateColor(Color targetColor, float duration)
    {
        Color startColor = cellImage.color;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            cellImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        cellImage.color = targetColor;
    }
    
    private IEnumerator AnimateColorWithBounce(Color targetColor, float duration)
    {
        Color startColor = cellImage.color;
        Vector3 startScale = transform.localScale;
        Vector3 bounceScale = startScale * 1.2f;
        float elapsedTime = 0f;
        
        // First half: scale up and change color
        while (elapsedTime < duration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / (duration / 2));
            cellImage.color = Color.Lerp(startColor, targetColor, t);
            transform.localScale = Vector3.Lerp(startScale, bounceScale, t);
            yield return null;
        }
        
        // Second half: scale back down
        elapsedTime = 0f;
        while (elapsedTime < duration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / (duration / 2));
            transform.localScale = Vector3.Lerp(bounceScale, startScale, t);
            yield return null;
        }
        
        transform.localScale = startScale;
    }
}