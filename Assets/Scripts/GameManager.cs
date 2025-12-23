using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

// Mengatur skor, nyawa, UI, dan flow game (game over, restart, back to menu)
public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager Instance;

    // Flag game over
    private bool isGameOver = false;

    // Data score & nyawa
    [Header("Score & Lives")]
    public int score = 0;
    public int lives = 3;

    // High Score
    [Header("High Score")]
    public int highScore = 0;
    public TextMeshProUGUI highScoreTextInGame;

    // Referensi UI
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public Image[] hearts;
    public Sprite heartFull;
    public Sprite heartEmpty;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;

    private void Awake()
    {
        // memastikan hanya ada satu GameManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;

        // Ambil high score dari PlayerPrefs
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        // pastikan referensi UI terisi saat masuk scene gameplay
        AutoAssignUIIfNeeded();

        // sinkronkan tampilan UI dengan data awal
        UpdateScoreUI();
        UpdateHeartsUI();
        UpdateHighScoreUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        isGameOver = false;
    }

    // Mencari elemen UI otomatis jika belum terisi
    private void AutoAssignUIIfNeeded()
    {
        // ScoreText
        if (scoreText == null)
        {
            GameObject scoreObj = GameObject.FindGameObjectWithTag("ScoreText");
            if (scoreObj != null)
                scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
        }

        // Hati (nyawa)
        if (hearts == null || hearts.Length == 0)
        {
            Image[] allImages = GameObject.FindObjectsOfType<Image>();
            hearts = new Image[3];

            foreach (var img in allImages)
            {
                if (img.name == "Heart1") hearts[0] = img;
                else if (img.name == "Heart2") hearts[1] = img;
                else if (img.name == "Heart3") hearts[2] = img;
            }
        }

        // Panel Game Over
        if (gameOverPanel == null)
        {
            GameObject panelObj = GameObject.FindGameObjectWithTag("GameOverPanel");
            if (panelObj != null)
                gameOverPanel = panelObj;
        }
    }

    // Mengecek status game over
    public bool IsGameOver()
    {
        return isGameOver;
    }

    // ---------------- SCORE API ----------------

    // +score (jellyfish ketangkep)
    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();

        // SFX — tangkapan / event skor
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxCatch);
    }

    // -score (jellyfish lolos)
    public void MissJellyfish(int amount)
    {
        score -= amount;
        if (score < 0) score = 0;   // kalau mau bisa minus, hapus baris ini

        UpdateScoreUI();

        // SFX sama seperti kena player (sesuai permintaan)
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxHit);
    }

    // ---------------- LIVES API ----------------

    // Mengurangi nyawa + SFX KENA
    public void TakeLife(int amount)
    {
        lives -= amount;
        if (lives < 0) lives = 0;

        UpdateHeartsUI();

        // SFX — kena jellyfish (sama dengan jellyfish lolos)
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxHit);

        if (lives <= 0)
        {
            GameOver();
        }
    }

    // ---------------- UI UPDATE ----------------

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + score;
    }

    private void UpdateHeartsUI()
    {
        if (hearts == null || hearts.Length == 0)
            return;

        for (int i = 0; i < hearts.Length; i++)
            hearts[i].sprite = (i < lives) ? heartFull : heartEmpty;
    }

    private void UpdateHighScoreUI()
    {
        if (highScoreTextInGame != null)
            highScoreTextInGame.text = "HIGHSCORE: " + highScore;
    }

    // ---------------- GAME OVER ----------------

    private void GameOver()
    {
        Debug.Log("Game Over");

        // Hentikan BGM agar tidak bunyi terus
        if (AudioManager.Instance != null && AudioManager.Instance.bgmSource != null)
            AudioManager.Instance.bgmSource.Stop();

        // SFX — game over
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxGameOver);

        // simpan high score baru jika lebih tinggi
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
            UpdateHighScoreUI();
        }

        // tampilkan panel Game Over
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        isGameOver = true;
        Time.timeScale = 0f;
    }

    // ---------------- RESTART ----------------

    public void RestartGame()
    {
        // SFX — tekan tombol
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxButton);

        score = 0;
        lives = 3;

        Time.timeScale = 1f;
        isGameOver = false;

        SceneManager.LoadScene("SampleScene");
    }
}
