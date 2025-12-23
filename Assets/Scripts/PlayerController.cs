using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// Mengatur pergerakan player, spawn/destroy jaring, dan efek glow saat kena jellyfish
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 30f;        
    public float moveRangeX = 200f;    

    [Header("Net Spawn Settings")]
    public GameObject netPrefab;         
    public Transform netSpawnPoint;        
    public Key catchKey = Key.Space;     

    [Header("Hit Colliders")]
    public Collider2D headCollider;      
    public Collider2D netCollider;    

    [Header("Shock Glow Settings")]
    public SpriteRenderer playerRenderer;  
    public float shockGlowDuration = 0.5f;
    public Color shockGlowColor = Color.yellow;
    [Range(0f, 1f)]
    public float glowIntensity = 0.3f; 

    private Animator animator;          
    private float horizontalInput;       

    // shader
    private Material playerMaterial;    
    private Coroutine glowCoroutine;      

    // net yang sedang aktif
    private GameObject currentNet;       

    void Start()
    {
        animator = GetComponent<Animator>();

        if (playerRenderer == null)
            playerRenderer = GetComponentInChildren<SpriteRenderer>();

        // paksa scale selalu positif
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x),
            Mathf.Abs(transform.localScale.y),
            transform.localScale.z
        );

        // setup material glow
        if (playerRenderer != null)
        {
            playerMaterial = playerRenderer.material;
            playerMaterial.SetColor("_GlowColor", shockGlowColor);
            playerMaterial.SetFloat("_GlowAmount", 0f);
        }
    }

    void Update()
    {
        horizontalInput = 0f;

        // blok semua input kalau game over
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver())
        {
            horizontalInput = 0f;
            return;
        }

        if (Keyboard.current != null)
        {
            // baca input arah kanan/kiri
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
                horizontalInput = 1f;
            else if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
                horizontalInput = -1f;

            // SPACE down -> spawn net prefab
            if (Keyboard.current[catchKey].wasPressedThisFrame)
            {
                SpawnNet();
                animator.SetBool("IsCatching", true);
            }

            // SPACE up -> destroy net
            if (Keyboard.current[catchKey].wasReleasedThisFrame)
            {
                DestroyNet();
                animator.SetBool("IsCatching", false);
            }

            // pastikan net ikut bergerak di titik spawn
            if (currentNet != null && netSpawnPoint != null)
            {
                currentNet.transform.position = netSpawnPoint.position;
            }
        }

        // update parameter animasi jalan
        animator.SetBool("isWalking", horizontalInput != 0);
    }

    void FixedUpdate()
    {
        // gerakkan player secara fisik (smooth) dengan batas kiri/kanan
        float newX = transform.position.x + (horizontalInput * moveSpeed * Time.fixedDeltaTime);
        newX = Mathf.Clamp(newX, -moveRangeX, moveRangeX);
        transform.position = new Vector3(newX, transform.position.y, 0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // hanya peduli collider dengan tag Jellyfish
        if (!other.CompareTag("Jellyfish")) return;

        // kalau kepala yang kena jellyfish → player tersetrum
        if (headCollider != null && other.IsTouching(headCollider))
        {
            animator.SetBool("IsCatching", false);
            DestroyNet();

            animator.SetTrigger("Shock");
            ActivateShockGlow();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TakeLife(1);
            }

            Destroy(other.gameObject);             
        }
        else if (netCollider != null && other.IsTouching(netCollider))
        {
            // kena jaring, tidak shock (logika ditangani di NetController)
        }
    }

    // Net spawn/destroy
    void SpawnNet()
    {
        if (netPrefab == null) return;
        if (currentNet != null) return; 

        Vector3 spawnPos = transform.position;
        if (netSpawnPoint != null)
            spawnPos = netSpawnPoint.position;

        currentNet = Instantiate(netPrefab, spawnPos, Quaternion.identity);
    }

    // menghapus jaring yang sedang aktif
    void DestroyNet()
    {
        if (currentNet != null)
        {
            Destroy(currentNet);
            currentNet = null;
        }
    }

    // Shock glow
    void ActivateShockGlow()
    {
        if (playerMaterial == null) return;

        if (glowCoroutine != null)
            StopCoroutine(glowCoroutine);

        glowCoroutine = StartCoroutine(ShockGlowEffect());
    }

    // coroutine untuk menyalakan glow sebentar lalu mematikannya
    IEnumerator ShockGlowEffect()
    {
        playerMaterial.SetFloat("_GlowAmount", glowIntensity);
        yield return new WaitForSeconds(shockGlowDuration);
        playerMaterial.SetFloat("_GlowAmount", 0f);
    }
}
