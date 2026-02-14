// TinderInputBridge.cs
// UI butonlarına bağla: "Swipe Left (Real)" ve "Swipe Right (Fake)"
using UnityEngine;

public class TinderInputBridge : MonoBehaviour
{
    [SerializeField] private TinderRoundManager round;

    public void SwipeLeft_Real()
    {
        if (round) round.SubmitSwipe(TinderRoundManager.SwipeDecision.Left_Real);
        Debug.Log("a");
    }

    public void SwipeRight_Fake()
    {
        if (round) round.SubmitSwipe(TinderRoundManager.SwipeDecision.Right_Fake);
    }

    public void Retry()
    {
        if (round) round.RetryRound();
    }

    public void NewRound()
    {
        if (round) round.StartNewRound();
    }
}