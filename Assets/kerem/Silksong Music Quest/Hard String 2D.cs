using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HarpString2D : MonoBehaviour
{
    [SerializeField] private HarpSound harpSound;
    [SerializeField] private MelodyChecker melodyChecker;
    [SerializeField] private int midiNote = 60; // bu telin notası (C4)
    [Range(0f, 1f)] [SerializeField] private float volume = 1f;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnMouseDown()
    {
        if (harpSound) harpSound.PlayMidi(midiNote, volume);
        if (melodyChecker) melodyChecker.RegisterNote(midiNote);
    }
    
    public void Configure(HarpSound sound, int newMidiNote)
    {
        harpSound = sound;
        midiNote = newMidiNote;
    }
}