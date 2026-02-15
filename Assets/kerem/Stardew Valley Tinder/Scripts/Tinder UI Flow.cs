// TinderUIFlow.cs (NEW)
// - Intro panel shows at level entry
// - Start arrow hides intro, starts game
// - Results panel shows after deck finished
// - Results text: "Correct: X/Y"
// - Replay hides results and starts a new round

using UnityEngine;
using TMPro;

public class TinderUIFlow : MonoBehaviour
{
    [SerializeField] private TinderRoundManager round;
    [SerializeField] private TinderInputBridge input;

    [Header("Panels")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private GameObject gameplayPanel; // profile UI root (optional but recommended)
    [SerializeField] private GameObject resultsPanel;

    [Header("Results UI")]
    [SerializeField] private TMP_Text resultsText; // "Correct: 6/8"

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
        if (resultsPanel) resultsPanel.SetActive(state == TinderRoundManager.State.Results);
        if (gameplayPanel) gameplayPanel.SetActive(state == TinderRoundManager.State.Playing);
    }

    private void HandleResults(int correct, int total)
    {
        if (resultsText) resultsText.text = $"Correct: {correct}/{total}";
    }

    // Hook these to buttons
    public void OnStartArrowClicked()
    {
        if (introPanel) introPanel.SetActive(false);
        if (input) input.StartGame();
    }

    public void OnReplayClicked()
    {
        if (resultsPanel) resultsPanel.SetActive(false);
        if (input) input.Replay();
    }
}