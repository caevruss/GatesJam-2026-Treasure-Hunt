using UnityEngine;

public class HarpSound : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pluckBaseClip;

    [Header("Base note settings")]
    [Tooltip("pluckBaseClip hangi notaya karşılık geliyor? (ör: C4=60)")]
    [SerializeField] private int baseMidiNote = 60; // C4

    [Range(0f, 1f)]
    [SerializeField] private float defaultVolume = 1f;

    private void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    public void PlayMidi(int midiNote) => PlayMidi(midiNote, defaultVolume);

    public void PlayMidi(int midiNote, float volume)
    {
        if (!audioSource || !pluckBaseClip) return;

        float semitoneDelta = midiNote - baseMidiNote;
        float pitch = Mathf.Pow(2f, semitoneDelta / 12f);

        audioSource.pitch = pitch;
        audioSource.PlayOneShot(pluckBaseClip, Mathf.Clamp01(volume));
    }
}