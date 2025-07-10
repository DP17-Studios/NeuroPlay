using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the player character in the NeuroSprint game
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float slideSpeed = 5f;
    [SerializeField] private float laneChangeSpeed = 5f;
    [SerializeField] private float gravity = 20f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    
    [Header("Audio")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip slideSound;
    [SerializeField] private AudioClip hitSound;
    
    // State variables
    private bool isGrounded = false;
    private bool isJumping = false;
    private bool isSliding = false;
    private float verticalVelocity = 0f;
    private int currentLane = 1; // 0 = left, 1 = center, 2 = right
    private float[] lanePositions = { -2.5f, 0f, 2.5f }; // X positions of lanes
    
    // Components
    private AudioSource audioSource;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Vector2 originalColliderSize;
    private Vector2 slideColliderSize;
    
    // References
    private NeuroSprintController gameController;
    
    private void Awake()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
        
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        if (boxCollider == null) boxCollider = gameObject.AddComponent<BoxCollider2D>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        // Store original collider size for sliding
        originalColliderSize = boxCollider.size;
        slideColliderSize = new Vector2(originalColliderSize.x, originalColliderSize.y * 0.5f);
        
        // Find game controller
        gameController = FindObjectOfType<NeuroSprintController>();
    }
    
    private void Start()
    {
        // Configure rigidbody
        rb.gravityScale = 0; // We'll handle gravity manually
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Initialize position
        transform.position = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
    }
    
    private void Update()
    {
        // Check if grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Handle input
        HandleInput();
        
        // Apply gravity
        ApplyGravity();
        
        // Move to target lane
        MoveLane();
        
        // Update animations
        UpdateAnimations();
    }
    
    private void HandleInput()
    {
        // Jump
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) && isGrounded && !isSliding)
        {
            Jump();
        }
        
        // Slide
        if ((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) && isGrounded && !isJumping)
        {
            StartSlide();
        }
        else if ((Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S)) && isSliding)
        {
            EndSlide();
        }
        
        // Lane change
        if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) && currentLane > 0)
        {
            currentLane--;
        }
        else if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) && currentLane < 2)
        {
            currentLane++;
        }
    }
    
    private void Jump()
    {
        isJumping = true;
        verticalVelocity = jumpForce;
        
        // Play sound
        if (jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }
    
    private void StartSlide()
    {
        if (!isSliding)
        {
            isSliding = true;
            
            // Adjust collider for sliding
            boxCollider.size = slideColliderSize;
            boxCollider.offset = new Vector2(0, -originalColliderSize.y * 0.25f);
            
            // Play sound
            if (slideSound != null)
            {
                audioSource.PlayOneShot(slideSound);
            }
            
            // Start slide timer
            StartCoroutine(SlideTimer());
        }
    }
    
    private void EndSlide()
    {
        isSliding = false;
        
        // Restore collider
        boxCollider.size = originalColliderSize;
        boxCollider.offset = Vector2.zero;
    }
    
    private IEnumerator SlideTimer()
    {
        // Auto-end slide after 1 second
        yield return new WaitForSeconds(1f);
        EndSlide();
    }
    
    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            // Apply gravity
            verticalVelocity -= gravity * Time.deltaTime;
        }
        else if (verticalVelocity < 0)
        {
            // Reset vertical velocity when landing
            verticalVelocity = 0;
            isJumping = false;
        }
        
        // Apply vertical movement
        rb.velocity = new Vector2(rb.velocity.x, verticalVelocity);
    }
    
    private void MoveLane()
    {
        // Calculate target X position
        float targetX = lanePositions[currentLane];
        
        // Move towards target lane
        float newX = Mathf.Lerp(transform.position.x, targetX, laneChangeSpeed * Time.deltaTime);
        
        // Update position
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
    
    private void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsJumping", isJumping);
            animator.SetBool("IsSliding", isSliding);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle"))
        {
            // Play hit sound
            if (hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
            
            // Visual feedback (e.g., flash red)
            StartCoroutine(FlashDamage());
        }
    }
    
    private IEnumerator FlashDamage()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.color;
            renderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            renderer.color = originalColor;
        }
    }
}