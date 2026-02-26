using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TinderProfileInfo
{
    [Header("Basic")]
    public string displayName;
    [Range(18, 40)] public int age = 22;
    public string location;
    public string address;
    public string birthday;

    [Header("Prompts")]
    public string prompt1Title;
    [TextArea(1, 4)] public string prompt1Answer;
    public string prompt2Title;
    [TextArea(1, 4)] public string prompt2Answer;

    [Header("Interests")]
    public string[] interests;

    [Header("Photos")]
    public Sprite[] photos;
}

[CreateAssetMenu(menuName = "TreasureHunt/Tinder Character Data", fileName = "TinderCharacterData_")]
public class TinderCharacterData : ScriptableObject
{
    [Header("ID")]
    public string characterId;

    [Header("Real profile")]
    public TinderProfileInfo realInfo;

    [Header("Fake profile")]
    public List<TinderProfileInfo> fakeInfos = new List<TinderProfileInfo>();

    [Header("Runtime")]
    public TinderAccountType runtimeType;
    
    [SerializeField] private int runtimeFakeIndex = 0;

    public void SetRuntime(TinderAccountType type, int fakeIndex = 0)
    {
        runtimeType = type;
        runtimeFakeIndex = fakeIndex;
    }

    public TinderProfileInfo GetActiveInfo()
    {
        if (runtimeType == TinderAccountType.Real)
            return realInfo;

        if (fakeInfos == null || fakeInfos.Count == 0)
            return realInfo;

        int idx = Mathf.Clamp(runtimeFakeIndex, 0, fakeInfos.Count - 1);
        return fakeInfos[idx];
    }

    public bool IsFakeRuntime() => runtimeType == TinderAccountType.Fake;

    public int FakeVariantCount => fakeInfos != null ? fakeInfos.Count : 0;
}

public enum TinderAccountType
{
    Real,
    Fake
}

