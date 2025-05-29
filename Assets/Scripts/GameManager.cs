using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] GameObject startMenuUI;
    [SerializeField] Button playButton;
    [SerializeField] TextMeshProUGUI TimeLeft;
    [SerializeField] TextMeshProUGUI playerState;
    [SerializeField] TextMeshProUGUI countdownText;
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] Button playAgainButton;
    [SerializeField] Image GirlFace;

    public TextMeshProUGUI PlayerState => playerState;


    public bool GameStarted { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Time.timeScale = 0f;  // Pause the game at start
        GameStarted = false;

        playButton.onClick.AddListener(() => StartCoroutine(StartGameCountdown()));
        playAgainButton.onClick.AddListener(RestartGame); // Hook restart
    }

    IEnumerator StartGameCountdown()
    {
        startMenuUI.SetActive(false);
        countdownText.gameObject.SetActive(true);

        // Countdown: 3, 2, 1, Go!
        string[] countdown = { "3", "2", "1", "Go!" };
        foreach (string step in countdown)
        {
            countdownText.text = step;
            yield return new WaitForSecondsRealtime(1f);
        }

        countdownText.gameObject.SetActive(false);
        StartGame();  // Proceed to start game
    }

    void StartGame()
    {
        GameStarted = true;
        Time.timeScale = 1f;  // Resume the game

        // Enable PlayerState and TimeLeft text
        PlayerState.gameObject.SetActive(true);
        TimeLeft.gameObject.SetActive(true);
        GirlFace.gameObject.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PlayerDied()
    {
        resultText.gameObject.SetActive(true);
        playAgainButton.gameObject.SetActive(true);
    }

    public void PlayerWon()
    {
        resultText.text = "You survived!";
        resultText.gameObject.SetActive(true);
        playAgainButton.gameObject.SetActive(true);
    }
}