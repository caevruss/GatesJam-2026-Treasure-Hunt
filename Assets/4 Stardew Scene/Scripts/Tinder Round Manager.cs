using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TinderRoundManager : MonoBehaviour
{
    public enum State { Intro, Playing, Results }
    public enum SwipeDecision { Left_Real, Right_Fake }

    [Serializable]
    public class RoundSettings
    {
        [Range(1, 32)] public int pickCount = 8;
        [Range(0, 32)] public int fakeCount = 3;
        public int seed = 0; // 0=random
    }

    [Header("Pool (12+ TinderCharacterData)")]
    [SerializeField] private List<TinderCharacterData> characterPool = new();

    [Header("Settings")]
    [SerializeField] private RoundSettings settings = new();

    [Header("Debug")]
    [SerializeField] private bool logRoundSetup = false;

    public event Action<State> OnStateChanged;
    public event Action<TinderCharacterData, TinderProfileInfo, int, int> OnCardShown;
    public event Action<int, int> OnResults;

    public State CurrentState { get; private set; } = State.Intro;

    private readonly List<TinderCharacterData> _deck = new();
    private int _index = 0;

    private int _correct = 0;
    private int _total = 0;

    private System.Random _rng;
    private Coroutine _startRoutine;

    private void Awake()
    {
        InitRng();
        SetState(State.Intro);
    }

    private void InitRng()
    {
        int seed = settings.seed != 0 ? settings.seed : Environment.TickCount;
        _rng = new System.Random(seed);
    }

    public void StartGame()
    {
        BuildDeck();

        _index = 0;
        _correct = 0;
        _total = _deck.Count;

        SetState(State.Playing);

        // ✅ UI enable/subscription için 1 frame bekle
        if (_startRoutine != null) StopCoroutine(_startRoutine);
        _startRoutine = StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        yield return null;

        if (_deck.Count == 0)
        {
            SetState(State.Results);
            OnResults?.Invoke(0, 0);
            yield break;
        }

        ShowCurrentCard();
    }

    public void Replay()
    {
        StartGame();
    }

    public void SubmitSwipe(SwipeDecision decision)
    {
        if (CurrentState != State.Playing) return;
        if (_deck.Count == 0) return;
        if (_index < 0 || _index >= _deck.Count) return;

        var current = _deck[_index];
        if (!current) { Advance(); return; }

        bool playerSaysFake = (decision == SwipeDecision.Right_Fake);
        bool actuallyFake = current.IsFakeRuntime();

        if (playerSaysFake == actuallyFake)
            _correct++;

        Advance();
    }

    private void Advance()
    {
        _index++;

        if (_index >= _deck.Count)
        {
            SetState(State.Results);
            OnResults?.Invoke(_correct, _total);
            return;
        }

        ShowCurrentCard();
    }

    private void ShowCurrentCard()
    {
        if (_index < 0 || _index >= _deck.Count) return;

        var data = _deck[_index];
        if (!data)
        {
            Advance();
            return;
        }

        var info = data.GetActiveInfo();
        OnCardShown?.Invoke(data, info, _index, _deck.Count);
    }

    private void SetState(State s)
    {
        CurrentState = s;
        OnStateChanged?.Invoke(s);
    }

    private void BuildDeck()
    {
        _deck.Clear();

        if (characterPool == null || characterPool.Count == 0)
        {
            Debug.LogError("[TinderRoundManager] characterPool boş.");
            return;
        }

        int pickCount = Mathf.Clamp(settings.pickCount, 1, characterPool.Count);
        int fakeCount = Mathf.Clamp(settings.fakeCount, 0, pickCount);

        var poolCopy = new List<TinderCharacterData>(characterPool);
        Shuffle(poolCopy);

        for (int i = 0; i < pickCount; i++)
            if (poolCopy[i] != null) _deck.Add(poolCopy[i]);

        foreach (var c in _deck)
            if (c) c.SetRuntime(TinderAccountType.Real, 0);

        var fakeCandidates = new List<int>();
        for (int i = 0; i < _deck.Count; i++)
        {
            var c = _deck[i];
            if (c != null && c.FakeVariantCount > 0)
                fakeCandidates.Add(i);
        }
        Shuffle(fakeCandidates);

        int assignCount = Mathf.Min(fakeCount, fakeCandidates.Count);
        for (int i = 0; i < assignCount; i++)
        {
            int deckIdx = fakeCandidates[i];
            var c = _deck[deckIdx];

            int variant = _rng.Next(0, c.FakeVariantCount);
            c.SetRuntime(TinderAccountType.Fake, variant);
        }

        Shuffle(_deck);

        if (logRoundSetup) DebugLogDeck(assignCount);
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void DebugLogDeck(int assignedFake)
    {
        Debug.Log($"[TinderRoundManager] Deck built. Total={_deck.Count} AssignedFake={assignedFake}");
        for (int i = 0; i < _deck.Count; i++)
        {
            var c = _deck[i];
            if (!c) continue;
            Debug.Log($"  #{i} {c.characterId} -> {(c.IsFakeRuntime() ? "FAKE" : "REAL")} (fakeVariants={c.FakeVariantCount})");
        }
    }
}