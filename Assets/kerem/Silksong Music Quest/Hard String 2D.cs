using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HarpString2D : MonoBehaviour
{
    [SerializeField] private HarpSound harpSound;
    [SerializeField] private int midiNote = 60;
    [Range(0f, 1f)] [SerializeField] private float volume = 1f;

    private HarpStageManager manager;
    private int myStringIndex;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnMouseDown()
    {
        if (harpSound) harpSound.PlayMidi(midiNote, volume);
        if (manager) manager.RegisterStringHit(myStringIndex);
    }

    public void SetMidi(int newMidi) => midiNote = newMidi;

    public void SetManagerAndIndex(HarpStageManager m, int index)
    {
        manager = m;
        myStringIndex = index;
    }
}
