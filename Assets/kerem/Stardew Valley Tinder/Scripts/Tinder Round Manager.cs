// TinderRoundManager.cs
// Deck kurar: havuzdan 8 karakter seçer, her birine runtime Fake/Real atar (Fake ise fake variant seçer)
// Oyuncu swipe yapar: Right = Fake, Left = Real
// Hata olursa retry: yeni 8 seçer + yeniden fake/real dağıtır

using System;
using System.Collections.Generic;
using UnityEngine;

public class TinderRoundManager : MonoBehaviour
{
    [Header("Pool (12 adet TinderCharacterData buraya)")]
    [SerializeField] private List<TinderCharacterData> characterPool = new List<TinderCharacterData>();

    [Header("Round Settings")]
    [SerializeField] private TinderRoundSettings settings = new TinderRoundSettings();

    [Header("Debug")]
    [SerializeField] private bool logRoundSetup = false;

    // Active deck
    private readonly List<TinderCharacterData> _activeDeck = new();
    private int _deckIndex = 0;

    // Progress
    private int _mistakes = 0;

    // RNG
    private System.Random _rng;

    // Events (UI bağlamak için)
    public event Action<TinderCharacterData, TinderProfileInfo, int, int> OnCardShown; 
    // (data, activeInfo, index(0..n-1), total)

    public event Action OnRoundWon;
    public event Action<int> OnRoundFailed; // mistakes count

    public IReadOnlyList<TinderCharacterData> ActiveDeck => _activeDeck;
    public int DeckIndex => _deckIndex;
    public int Mistakes => _mistakes;

    private void Awake()
    {
        InitRng();
    }

    private void Start()
    {
        StartNewRound();
    }

    private void InitRng()
    {
        int seed = settings.seed != 0 ? settings.seed : Environment.TickCount;
        _rng = new System.Random(seed);
    }

    // ---------------------------
    // Public API
    // ---------------------------

    public void StartNewRound()
    {
        BuildDeck();
        _deckIndex = 0;
        _mistakes = 0;

        ShowCurrentCard();
    }

    public void RetryRound()
    {
        StartNewRound();
    }

    public void SubmitSwipe(SwipeDecision decision)
    {
        if (_activeDeck.Count == 0) return;
        if (_deckIndex < 0 || _deckIndex >= _activeDeck.Count) return;

        var current = _activeDeck[_deckIndex];

        bool playerSaysFake = decision == SwipeDecision.Right_Fake;
        bool isActuallyFake = current.IsFakeRuntime();

        if (playerSaysFake != isActuallyFake)
        {
            _mistakes++;
            // Anında fail istiyorsan burada bitir:
            FailRound();
            return;
        }

        // doğruysa sıradaki karta geç
        _deckIndex++;

        if (_deckIndex >= _activeDeck.Count)
        {
            WinRound();
            return;
        }

        ShowCurrentCard();
    }

    // ---------------------------
    // Core
    // ---------------------------

    private void BuildDeck()
    {
        _activeDeck.Clear();

        if (characterPool == null || characterPool.Count == 0)
        {
            Debug.LogError("[TinderRoundManager] characterPool boş.");
            return;
        }

        int pickCount = Mathf.Clamp(settings.pickCount, 1, characterPool.Count);
        int fakeCount = Mathf.Clamp(settings.fakeCount, 0, pickCount);

        // 1) Havuzdan pickCount tane seç
        var poolCopy = new List<TinderCharacterData>(characterPool);

        Shuffle(poolCopy);

        if (!settings.allowSameCharacterAgainOnRetry && _lastRoundIds.Count > 0)
        {
            // Basitçe: önce yeni olanları al, yetmezse kalanlardan tamamla
            var fresh = new List<TinderCharacterData>();
            var old = new List<TinderCharacterData>();

            foreach (var c in poolCopy)
            {
                if (c == null) continue;
                if (_lastRoundIds.Contains(c.characterId)) old.Add(c);
                else fresh.Add(c);
            }

            _activeDeck.AddRange(fresh);
            if (_activeDeck.Count < pickCount) _activeDeck.AddRange(old);

            if (_activeDeck.Count > pickCount) _activeDeck.RemoveRange(pickCount, _activeDeck.Count - pickCount);
        }
        else
        {
            for (int i = 0; i < pickCount; i++)
            {
                if (poolCopy[i] != null) _activeDeck.Add(poolCopy[i]);
            }
        }

        // 2) Fake/Real dağıt (tam fakeCount fake olacak şekilde)
        // Önce tümünü Real yap
        foreach (var c in _activeDeck)
        {
            if (!c) continue;
            c.SetRuntime(TinderAccountType.Real, 0);
        }

        // Sonra rastgele fakeCount tanesini Fake yap
        var indices = new List<int>();
        for (int i = 0; i < _activeDeck.Count; i++) indices.Add(i);
        Shuffle(indices);

        for (int i = 0; i < fakeCount; i++)
        {
            int idx = indices[i];
            var c = _activeDeck[idx];
            if (!c) continue;

            int variant = 0;
            int variantCount = c.FakeVariantCount;
            if (variantCount > 0) variant = _rng.Next(0, variantCount);

            c.SetRuntime(TinderAccountType.Fake, variant);
        }

        // 3) Deck’i karıştır (fake/real dağıtımı sıraya bağlı olmasın)
        Shuffle(_activeDeck);

        // Retry tekrarını takip
        _lastRoundIds.Clear();
        foreach (var c in _activeDeck)
            if (c) _lastRoundIds.Add(c.characterId);

        if (logRoundSetup)
            DebugLogDeck();
    }

    private void ShowCurrentCard()
    {
        if (_activeDeck.Count == 0) return;
        if (_deckIndex < 0 || _deckIndex >= _activeDeck.Count) return;

        var data = _activeDeck[_deckIndex];
        if (!data)
        {
            // null çıkarsa geç
            _deckIndex++;
            if (_deckIndex >= _activeDeck.Count) { WinRound(); return; }
            ShowCurrentCard();
            return;
        }

        var info = data.GetActiveInfo();
        OnCardShown?.Invoke(data, info, _deckIndex, _activeDeck.Count);
    }

    private void WinRound()
    {
        OnRoundWon?.Invoke();
    }

    private void FailRound()
    {
        OnRoundFailed?.Invoke(_mistakes);
    }

    // ---------------------------
    // Helpers
    // ---------------------------

    private readonly HashSet<string> _lastRoundIds = new();

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void DebugLogDeck()
    {
        Debug.Log($"[TinderRoundManager] Deck built. Total={_activeDeck.Count} (Fake={settings.fakeCount})");
        for (int i = 0; i < _activeDeck.Count; i++)
        {
            var c = _activeDeck[i];
            if (!c) continue;
            var type = c.IsFakeRuntime() ? "FAKE" : "REAL";
            Debug.Log($"  #{i} {c.characterId} -> {type} (variant={GetRuntimeVariantDebug(c)})");
        }
    }

    private int GetRuntimeVariantDebug(TinderCharacterData c)
    {
        // runtimeFakeIndex private; debug amaçlı activeInfo indexini dışarıdan bilmiyoruz.
        // İstersen TinderCharacterData içine RuntimeFakeIndex read-only property ekleyebilirsin.
        return c.IsFakeRuntime() ? 1 : 0;
    }
    public enum SwipeDecision
    {
        Left_Real,   // Player says: this profile is REAL
        Right_Fake   // Player says: this profile is FAKE
    }

    [Serializable]
    public class TinderRoundSettings
    {
        [Range(1, 32)] public int pickCount = 8;
        [Range(0, 32)] public int fakeCount = 3; // 8 içinden kaç fake olsun
        public bool allowSameCharacterAgainOnRetry = true;
        public int seed = 0; // 0 = random seed, >0 = deterministic
    }
}
