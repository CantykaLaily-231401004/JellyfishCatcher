using UnityEngine;

public class JellyfishAI : MonoBehaviour
{
    [Header("Fall Settings")]
    public float fallSpeed = 5f;

    [Header("Destroy Settings")]
    public float destroyYPosition = -20f;

    [Header("Hit Flash Shader")]
    public Renderer jellyRenderer;
    public float hitDuration = 0.2f;

    private JellyfishSpawner spawner;
    private bool isCaught = false;

    private Material jellyMat;
    private int hitAmountID;

    private float horizontalDrift;
    private float rotateSpeed;

    void Awake()
    {
        hitAmountID = Shader.PropertyToID("_HitAmount");
    }

    void Start()
    {
        spawner = FindObjectOfType<JellyfishSpawner>();

        if (jellyRenderer == null)
            jellyRenderer = GetComponentInChildren<Renderer>();

        if (jellyRenderer != null)
        {
            jellyMat = jellyRenderer.material;
            jellyMat.SetFloat(hitAmountID, 0f);
        }

        // RANDOM BALANCING
        fallSpeed = Random.Range(2f, 6f);
        horizontalDrift = Random.Range(0.1f, 0.4f);
        rotateSpeed = Random.Range(2f, 6f);
    }

    void Update()
    {
        if (isCaught) return;

        // FALL SPEED BASED ON SCORE
        fallSpeed += GameManager.Instance.score * 0.001f;

        // jatuh
        transform.position -= new Vector3(0, fallSpeed * Time.deltaTime, 0);

        // drift horizontal dinamis
        float drift = Mathf.Sin(Time.time * rotateSpeed) * horizontalDrift * Time.deltaTime;
        transform.position += new Vector3(drift, 0, 0);

        // rotasi
        float angle = Mathf.Sin(Time.time * rotateSpeed) * 5f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // kalau hilang di bawah
        if (transform.position.y < destroyYPosition)
        {
            DestroyJellyfish();
        }
    }

    public void GetCaught(Transform netTransform)
    {
        if (isCaught) return;

        isCaught = true;

        transform.position = netTransform.position;
        transform.localScale *= 0.7f;

        jellyMat?.SetFloat(hitAmountID, 1f);

        Invoke(nameof(EndHitAndDestroy), hitDuration);
    }

    void EndHitAndDestroy()
    {
        jellyMat?.SetFloat(hitAmountID, 0f);
        DestroyJellyfish();
    }

    void DestroyJellyfish()
    {
        // jellyfish LOLOS (tidak tertangkap)
        if (!isCaught && GameManager.Instance != null)
        {
            GameManager.Instance.MissJellyfish(1);   // -1 skor
        }

        // beritahu spawner
        spawner?.OnJellyfishDestroyed();

        Destroy(gameObject);
    }


    void OnDestroy()
    {
        if (jellyMat != null)
            Destroy(jellyMat);
    }
}