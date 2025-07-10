using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls facial expressions and emotions for characters in the SocialScope game
/// </summary>
public class FacialExpressionController : MonoBehaviour
{
    [Header("Face Components")]
    [SerializeField] private SkinnedMeshRenderer faceMesh;
    [SerializeField] private Transform eyesParent;
    [SerializeField] private Transform mouthParent;
    
    [Header("Expression Settings")]
    [SerializeField] private float blendSpeed = 3.0f;
    [SerializeField] private float eyeBlinkInterval = 5.0f;
    [SerializeField] private float eyeBlinkDuration = 0.15f;
    
    // Blend shape indices for different expressions
    private Dictionary<string, int> expressionIndices = new Dictionary<string, int>();
    
    // Current expression state
    private string currentExpression = "neutral";
    private float[] targetBlendWeights;
    private float[] currentBlendWeights;
    private float blinkTimer = 0f;
    private bool isBlinking = false;
    
    // Expression definitions
    [System.Serializable]
    public class ExpressionDefinition
    {
        public string name;
        public float[] blendShapeWeights;
        public Vector3 eyeRotation;
        public Vector3 mouthPosition;
    }
    
    [SerializeField] private List<ExpressionDefinition> expressions = new List<ExpressionDefinition>();
    
    private void Awake()
    {
        // Initialize blend weights arrays
        if (faceMesh != null)
        {
            int blendShapeCount = faceMesh.sharedMesh.blendShapeCount;
            targetBlendWeights = new float[blendShapeCount];
            currentBlendWeights = new float[blendShapeCount];
            
            // Map expression names to blend shape indices
            for (int i = 0; i < blendShapeCount; i++)
            {
                string shapeName = faceMesh.sharedMesh.GetBlendShapeName(i).ToLower();
                expressionIndices[shapeName] = i;
            }
        }
        
        // Initialize with neutral expression
        SetExpression("neutral");
    }
    
    private void Update()
    {
        // Blend between expressions
        UpdateBlendShapes();
        
        // Handle eye blinking
        UpdateEyeBlink();
    }
    
    public void SetExpression(string expressionName)
    {
        if (string.IsNullOrEmpty(expressionName) || faceMesh == null)
            return;
            
        expressionName = expressionName.ToLower();
        currentExpression = expressionName;
        
        // Find the expression definition
        ExpressionDefinition expressionDef = null;
        
        foreach (var expr in expressions)
        {
            if (expr.name.ToLower() == expressionName)
            {
                expressionDef = expr;
                break;
            }
        }
        
        // If we don't have a definition for this expression, try to find a similar one
        if (expressionDef == null)
        {
            foreach (var expr in expressions)
            {
                if (expr.name.ToLower().Contains(expressionName) || 
                    expressionName.Contains(expr.name.ToLower()))
                {
                    expressionDef = expr;
                    break;
                }
            }
            
            // If still no match, default to neutral
            if (expressionDef == null)
            {
                foreach (var expr in expressions)
                {
                    if (expr.name.ToLower() == "neutral")
                    {
                        expressionDef = expr;
                        break;
                    }
                }
            }
        }
        
        // Apply the expression
        if (expressionDef != null)
        {
            // Set target blend weights
            for (int i = 0; i < targetBlendWeights.Length && i < expressionDef.blendShapeWeights.Length; i++)
            {
                targetBlendWeights[i] = expressionDef.blendShapeWeights[i];
            }
            
            // Set eye rotation
            if (eyesParent != null)
            {
                StartCoroutine(RotateTo(eyesParent, expressionDef.eyeRotation));
            }
            
            // Set mouth position
            if (mouthParent != null)
            {
                StartCoroutine(MoveTo(mouthParent, expressionDef.mouthPosition));
            }
        }
        else
        {
            Debug.LogWarning($"No expression definition found for: {expressionName}");
        }
        
        Debug.Log($"Setting expression to: {expressionName}");
    }
    
    private void UpdateBlendShapes()
    {
        if (faceMesh == null)
            return;
            
        bool updated = false;
        
        // Smoothly blend to target weights
        for (int i = 0; i < currentBlendWeights.Length; i++)
        {
            float target = targetBlendWeights[i];
            
            // Special case for blinking
            if (isBlinking && (i == expressionIndices.GetValueOrDefault("blink", -1) || 
                               i == expressionIndices.GetValueOrDefault("eye_close", -1)))
            {
                target = 100f;
            }
            
            if (Mathf.Abs(currentBlendWeights[i] - target) > 0.01f)
            {
                currentBlendWeights[i] = Mathf.Lerp(currentBlendWeights[i], target, Time.deltaTime * blendSpeed);
                faceMesh.SetBlendShapeWeight(i, currentBlendWeights[i]);
                updated = true;
            }
        }
        
        // If we're done updating, snap to exact values
        if (!updated)
        {
            for (int i = 0; i < currentBlendWeights.Length; i++)
            {
                if (!isBlinking || (i != expressionIndices.GetValueOrDefault("blink", -1) && 
                                   i != expressionIndices.GetValueOrDefault("eye_close", -1)))
                {
                    currentBlendWeights[i] = targetBlendWeights[i];
                    faceMesh.SetBlendShapeWeight(i, currentBlendWeights[i]);
                }
            }
        }
    }
    
    private void UpdateEyeBlink()
    {
        // Handle periodic eye blinking
        blinkTimer += Time.deltaTime;
        
        if (!isBlinking && blinkTimer >= eyeBlinkInterval)
        {
            // Start a blink
            isBlinking = true;
            blinkTimer = 0f;
            
            // Schedule end of blink
            StartCoroutine(EndBlink());
        }
    }
    
    private IEnumerator EndBlink()
    {
        yield return new WaitForSeconds(eyeBlinkDuration);
        isBlinking = false;
    }
    
    private IEnumerator RotateTo(Transform target, Vector3 rotation)
    {
        Quaternion startRotation = target.localRotation;
        Quaternion endRotation = Quaternion.Euler(rotation);
        float duration = 1.0f / blendSpeed;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }
        
        target.localRotation = endRotation;
    }
    
    private IEnumerator MoveTo(Transform target, Vector3 position)
    {
        Vector3 startPosition = target.localPosition;
        float duration = 1.0f / blendSpeed;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.localPosition = Vector3.Lerp(startPosition, position, t);
            yield return null;
        }
        
        target.localPosition = position;
    }
    
    // Helper method to create a custom expression by combining others
    public void BlendExpressions(string expression1, string expression2, float blend)
    {
        if (faceMesh == null)
            return;
            
        // Find the two expressions
        ExpressionDefinition expr1 = null;
        ExpressionDefinition expr2 = null;
        
        foreach (var expr in expressions)
        {
            if (expr.name.ToLower() == expression1.ToLower())
                expr1 = expr;
            else if (expr.name.ToLower() == expression2.ToLower())
                expr2 = expr;
        }
        
        if (expr1 == null || expr2 == null)
        {
            Debug.LogWarning($"Could not find expressions to blend: {expression1}, {expression2}");
            return;
        }
        
        // Blend between the two expressions
        for (int i = 0; i < targetBlendWeights.Length; i++)
        {
            if (i < expr1.blendShapeWeights.Length && i < expr2.blendShapeWeights.Length)
            {
                targetBlendWeights[i] = Mathf.Lerp(expr1.blendShapeWeights[i], expr2.blendShapeWeights[i], blend);
            }
        }
        
        // Blend eye rotation
        if (eyesParent != null)
        {
            Vector3 blendedRotation = Vector3.Lerp(expr1.eyeRotation, expr2.eyeRotation, blend);
            StartCoroutine(RotateTo(eyesParent, blendedRotation));
        }
        
        // Blend mouth position
        if (mouthParent != null)
        {
            Vector3 blendedPosition = Vector3.Lerp(expr1.mouthPosition, expr2.mouthPosition, blend);
            StartCoroutine(MoveTo(mouthParent, blendedPosition));
        }
        
        currentExpression = $"blend_{expression1}_{expression2}_{blend:F2}";
        Debug.Log($"Blending expressions: {expression1} and {expression2} with t={blend:F2}");
    }
}