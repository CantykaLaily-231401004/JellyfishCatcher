using UnityEngine;
using System.Collections;

// Net menangkap jellyfish dan efek glow shader
public class NetController : MonoBehaviour
{
    [Header("Glow Settings")]
    public float glowAmountOnCatch = 1.5f;   
    public float glowFadeTime = 1.0f;        

    private Material netMat;               
    private int glowID;                    
    private Coroutine glowRoutine;      

    void Awake()
    {
        var sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null)
        {
            netMat = sprite.material;
            glowID = Shader.PropertyToID("_GlowStrength");
            netMat.SetFloat(glowID, 0f);    
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Jellyfish")) return;

        JellyfishAI jellyfish = other.GetComponent<JellyfishAI>();
        if (jellyfish != null)
        {
            jellyfish.GetCaught(transform);
            GameManager.Instance.AddScore(10);

            // TAMBAHAN: SCALE EFFECT SAAT TANGKAP
            StartCoroutine(ScalePulse());

            TriggerGlow();
        }
    }

    // Coroutine untuk efek scale (membesar sedikit lalu kembali)
    IEnumerator ScalePulse()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;  // 20% lebih besar

        float duration = 0.15f;
        float elapsed = 0f;

        // Fase 1: Membesar
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;

        // Fase 2: Kembali ke ukuran normal
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale; 
    }

    void TriggerGlow()
    {
        if (netMat == null) return;

        if (glowRoutine != null)
            StopCoroutine(glowRoutine);

        glowRoutine = StartCoroutine(GlowFlash());
    }

    System.Collections.IEnumerator GlowFlash()
    {
        if (netMat == null) yield break;

        float t = 0f;

        // jalankan selama glowFadeTime detik
        while (t < glowFadeTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / glowFadeTime);  
        
            float tri = 1f - Mathf.Abs(2f * k - 1f);
            float value = tri * glowAmountOnCatch;

            netMat.SetFloat(glowID, value);
            yield return null;
        }

        netMat.SetFloat(glowID, 0f); 
    }
}