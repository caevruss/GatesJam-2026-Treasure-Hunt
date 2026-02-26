using UnityEngine;
using TMPro;

public class TinderUIFlow : MonoBehaviour
{
    [SerializeField] private TinderRoundManager round;
    [SerializeField] private TinderInputBridge input;

    [Header("Panels")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject resultsPanel; // Game Over Panel

    [Header("Results UI")]
    [SerializeField] private TMP_Text resultsText;

    [Header("Results Buttons (keep these GameObjects ACTIVE in hierarchy)")]
    [SerializeField] private GameObject replayButtonObject;   // Try Again Button GO
    [SerializeField] private NextLevelButton nextLevelButton; // Next Level Button component

    private void Awake()
    {
        // Optional safety: auto-find if you forgot to assign
        if (!nextLevelButton)
            nextLevelButton = FindFirstObjectByType<NextLevelButton>(FindObjectsInactive.Include);
    }

    private void OnEnable()
    {
        if (round)
        {
            round.OnStateChanged += HandleState;
            round.OnResults += HandleResults;
        }

        HandleState(round ? round.CurrentState : TinderRoundManager.State.Intro);
    }

    private void OnDisable()
    {
        if (round)
        {
            round.OnStateChanged -= HandleState;
            round.OnResults -= HandleResults;
        }
    }

    private void HandleState(TinderRoundManager.State state)
    {
        if (introPanel) introPanel.SetActive(state == TinderRoundManager.State.Intro);
        if (gameplayPanel) gameplayPanel.SetActive(state == TinderRoundManager.State.Playing);
        if (resultsPanel) resultsPanel.SetActive(state == TinderRoundManager.State.Results);

        // BURADAKİ REPLAY VE NEXTLEVEL KAPATMA SATIRLARINI TAMAMEN SİL!
        // Çünkü bu state değiştiği anda HandleResults zaten çalışacak ve 
        // doğru butonu açıp yanlış olanı kapatacak.
    }

    private void HandleResults(int correct, int total)
    {
        if (resultsText) resultsText.text = $"Correct: {correct}/{total}";

        bool perfect = (total > 0) && (correct == total);

        if (replayButtonObject) 
        {
            replayButtonObject.SetActive(!perfect);
        }

        if (nextLevelButton) 
        {
            nextLevelButton.SetUnlocked(perfect);
        }
    }

    // Hook these to buttons
    public void OnStartArrowClicked()
    {
        if (introPanel) introPanel.SetActive(false);
        if (input) input.StartGame();
    }

    public void OnReplayClicked()
    {
        // RoundManager will switch to Playing and UI will update via events.
        if (input) input.Replay();
    }
}