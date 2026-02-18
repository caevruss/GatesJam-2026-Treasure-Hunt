using UnityEngine;

public class HarpAutoAssign : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HarpSound harpSound;

    [Header("Note Range")]
    [Tooltip("İlk telin MIDI notası. C4=60, D4=62, E4=64 ...")]
    [SerializeField] private int startMidiNote = 60; // C4

    [Tooltip("Kromatik mi (60,61,62...) yoksa majör dizi mi (60,62,64...)?")]
    [SerializeField] private bool majorScale = true;

    private static readonly int[] MajorSteps = { 0, 2, 4, 5, 7, 9, 11, 12 };

    [ContextMenu("Auto Assign Strings")]
    public void AutoAssign()
    {
        if (!harpSound) harpSound = GetComponentInChildren<HarpSound>();
        var strings = GetComponentsInChildren<HarpString2D>(true);

        for (int i = 0; i < strings.Length; i++)
        {
            strings[i].SendMessage("SetHarpSound", harpSound, SendMessageOptions.DontRequireReceiver);

            int midi = majorScale
                ? startMidiNote + MajorSteps[Mathf.Clamp(i, 0, MajorSteps.Length - 1)]
                : startMidiNote + i;

            // Inspector’dan set edebilmek için reflection yerine serialized alanı public yapmadım,
            // o yüzden basitçe doğrudan field'a erişmek için küçük bir helper ekleyelim:
            SetString(strings[i], harpSound, midi);
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
    }

    private static void SetString(HarpString2D s, HarpSound sound, int midi)
    {
        // Unity serialized private field'ları burada doğrudan set edemiyoruz, o yüzden HarpString2D'ye 2 method ekleyeceğiz.
        s.Configure(sound, midi);
    }
}