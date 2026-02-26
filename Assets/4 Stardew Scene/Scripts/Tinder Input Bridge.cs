// TinderInputBridge.cs (UPDATED)
// - Left/Right buttons call these during Playing
// - Start button calls StartGame()
// - Replay button calls Replay()

using UnityEngine;

public class TinderInputBridge : MonoBehaviour
{
    [SerializeField] private TinderRoundManager round;

    public void StartGame()
    {
        if (round) round.StartGame();
    }

    public void SwipeLeft_Real()
    {
        if (round) round.SubmitSwipe(TinderRoundManager.SwipeDecision.Left_Real);
    }

    public void SwipeRight_Fake()
    {
        if (round) round.SubmitSwipe(TinderRoundManager.SwipeDecision.Right_Fake);
    }

    public void Replay()
    {
        if (round) round.Replay();
    }
}