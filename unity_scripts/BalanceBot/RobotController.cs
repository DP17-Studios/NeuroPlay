using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the visual representation of the robot in the BalanceBot game
/// </summary>
public class RobotController : MonoBehaviour
{
    [Header("Robot Parts")]
    [SerializeField] private Transform robotBody;
    [SerializeField] private Transform robotHead;
    [SerializeField] private Transform leftArm;
    [SerializeField] private Transform rightArm;
    [SerializeField] private Transform leftLeg;
    [SerializeField] private Transform rightLeg;
    
    [Header("Balance Animation")]
    [SerializeField] private float armBalanceSpeed = 5.0f;
    [SerializeField] private float maxArmAngle = 60.0f;
    [SerializeField] private float bodySwayAmount = 5.0f;
    [SerializeField] private float headTiltAmount = 10.0f;
    [SerializeField] private float legAdjustAmount = 5.0f;
    
    [Header("Fall Animation")]
    [SerializeField] private float fallAnimationSpeed = 2.0f;
    [SerializeField] private AnimationCurve fallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    // References
    private BalanceBotController gameController;
    private Transform platformTransform;
    
    // Animation state
    private bool isFalling = false;
    private float fallProgress = 0f;
    private Vector3 fallDirection = Vector3.zero;
    private Quaternion[] initialRotations;
    private Vector3[] initialPositions;
    
    private void Awake()
    {
        gameController = FindObjectOfType<BalanceBotController>();
        
        // Store initial transforms
        initialRotations = new Quaternion[6];
        initialPositions = new Vector3[6];
        
        Transform[] parts = { robotBody, robotHead, leftArm, rightArm, leftLeg, rightLeg };
        
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] != null)
            {
                initialRotations[i] = parts[i].localRotation;
                initialPositions[i] = parts[i].localPosition;
            }
        }
    }
    
    private void Start()
    {
        // Find platform
        platformTransform = transform.parent;
    }
    
    private void Update()
    {
        if (platformTransform == null)
            return;
            
        if (isFalling)
        {
            UpdateFallAnimation();
        }
        else
        {
            UpdateBalanceAnimation();
        }
    }
    
    private void UpdateBalanceAnimation()
    {
        // Get platform tilt angles
        float xTilt = platformTransform.rotation.eulerAngles.x;
        if (xTilt > 180) xTilt -= 360;
        
        float zTilt = platformTransform.rotation.eulerAngles.z;
        if (zTilt > 180) zTilt -= 360;
        
        // Normalize tilt to -1 to 1 range
        float normalizedXTilt = xTilt / 15.0f; // Assuming max tilt is 15 degrees
        float normalizedZTilt = zTilt / 15.0f;
        
        // Animate robot parts to counterbalance
        AnimateCounterbalance(normalizedXTilt, normalizedZTilt);
    }
    
    private void AnimateCounterbalance(float xTilt, float zTilt)
    {
        // Body sway - subtle counter-movement
        if (robotBody != null)
        {
            Quaternion targetRotation = Quaternion.Euler(
                -xTilt * bodySwayAmount * 0.5f,
                0,
                -zTilt * bodySwayAmount
            );
            
            robotBody.localRotation = Quaternion.Slerp(
                robotBody.localRotation,
                initialRotations[0] * targetRotation,
                Time.deltaTime * armBalanceSpeed
            );
        }
        
        // Head tilt - looks in direction of tilt
        if (robotHead != null)
        {
            Quaternion targetRotation = Quaternion.Euler(
                xTilt * headTiltAmount,
                0,
                zTilt * headTiltAmount
            );
            
            robotHead.localRotation = Quaternion.Slerp(
                robotHead.localRotation,
                initialRotations[1] * targetRotation,
                Time.deltaTime * armBalanceSpeed
            );
        }
        
        // Arms - extend opposite to tilt direction
        if (leftArm != null)
        {
            Quaternion targetRotation = Quaternion.Euler(
                0,
                0,
                -zTilt * maxArmAngle
            );
            
            leftArm.localRotation = Quaternion.Slerp(
                leftArm.localRotation,
                initialRotations[2] * targetRotation,
                Time.deltaTime * armBalanceSpeed
            );
        }
        
        if (rightArm != null)
        {
            Quaternion targetRotation = Quaternion.Euler(
                0,
                0,
                -zTilt * maxArmAngle
            );
            
            rightArm.localRotation = Quaternion.Slerp(
                rightArm.localRotation,
                initialRotations[3] * targetRotation,
                Time.deltaTime * armBalanceSpeed
            );
        }
        
        // Legs - subtle adjustments for balance
        if (leftLeg != null)
        {
            Quaternion targetRotation = Quaternion.Euler(
                -xTilt * legAdjustAmount,
                0,
                zTilt * legAdjustAmount
            );
            
            leftLeg.localRotation = Quaternion.Slerp(
                leftLeg.localRotation,
                initialRotations[4] * targetRotation,
                Time.deltaTime * armBalanceSpeed
            );
        }
        
        if (rightLeg != null)
        {
            Quaternion targetRotation = Quaternion.Euler(
                -xTilt * legAdjustAmount,
                0,
                zTilt * legAdjustAmount
            );
            
            rightLeg.localRotation = Quaternion.Slerp(
                rightLeg.localRotation,
                initialRotations[5] * targetRotation,
                Time.deltaTime * armBalanceSpeed
            );
        }
    }
    
    public void StartFall(Vector3 direction)
    {
        if (isFalling)
            return;
            
        isFalling = true;
        fallProgress = 0f;
        fallDirection = direction.normalized;
        
        // Store current transforms as starting point for fall
        Transform[] parts = { robotBody, robotHead, leftArm, rightArm, leftLeg, rightLeg };
        
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] != null)
            {
                initialRotations[i] = parts[i].localRotation;
                initialPositions[i] = parts[i].localPosition;
            }
        }
    }
    
    private void UpdateFallAnimation()
    {
        // Progress the fall animation
        fallProgress += Time.deltaTime * fallAnimationSpeed;
        
        if (fallProgress >= 1.0f)
        {
            fallProgress = 1.0f;
        }
        
        float curveValue = fallCurve.Evaluate(fallProgress);
        
        // Apply fall animation to each part
        if (robotBody != null)
        {
            robotBody.localRotation = Quaternion.Slerp(
                initialRotations[0],
                Quaternion.Euler(fallDirection.x * 60f, 0, fallDirection.z * 60f),
                curveValue
            );
        }
        
        if (robotHead != null)
        {
            robotHead.localRotation = Quaternion.Slerp(
                initialRotations[1],
                Quaternion.Euler(fallDirection.x * 40f, 0, fallDirection.z * 40f),
                curveValue
            );
        }
        
        // Arms flail outward
        if (leftArm != null)
        {
            leftArm.localRotation = Quaternion.Slerp(
                initialRotations[2],
                Quaternion.Euler(0, 0, 80f),
                curveValue
            );
        }
        
        if (rightArm != null)
        {
            rightArm.localRotation = Quaternion.Slerp(
                initialRotations[3],
                Quaternion.Euler(0, 0, -80f),
                curveValue
            );
        }
        
        // Legs bend
        if (leftLeg != null)
        {
            leftLeg.localRotation = Quaternion.Slerp(
                initialRotations[4],
                Quaternion.Euler(30f, 0, 0),
                curveValue
            );
        }
        
        if (rightLeg != null)
        {
            rightLeg.localRotation = Quaternion.Slerp(
                initialRotations[5],
                Quaternion.Euler(-30f, 0, 0),
                curveValue
            );
        }
    }
    
    public void ResetRobot()
    {
        isFalling = false;
        fallProgress = 0f;
        
        // Reset all parts to initial state
        Transform[] parts = { robotBody, robotHead, leftArm, rightArm, leftLeg, rightLeg };
        
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] != null)
            {
                parts[i].localRotation = initialRotations[i];
                parts[i].localPosition = initialPositions[i];
            }
        }
    }
}