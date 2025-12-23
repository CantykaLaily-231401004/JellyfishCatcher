using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    public TextMeshProUGUI highscoreText;

    private void Start()
    {
        // Ambil HighScore dari PlayerPrefs
        int savedHighScore = PlayerPrefs.GetInt("HighScore", 0);

        // Tampilkan di UI
        if (highscoreText != null)
            highscoreText.text = "HIGHSCORE: " + savedHighScore;
    }

    // Dipanggil tombol START
    public void StartGame()
    {
        StartCoroutine(StartGameWithSound());
    }

    private IEnumerator StartGameWithSound()
    {
        // Mainkan suara tombol
        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxButton);

        // Tunggu sedikit agar SFX terdengar
        yield return new WaitForSeconds(0.1f);

        // Pindah ke game
        SceneManager.LoadScene("SampleScene");
    }
}