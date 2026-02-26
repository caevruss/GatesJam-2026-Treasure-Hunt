using UnityEngine;

[CreateAssetMenu(menuName = "TreasureHunt/Harp Stage Data", order = 0)]
public class HarpStageData : ScriptableObject
{
    [Header("6 strings => 6 MIDI notes (length must be 6)")]
    public int[] stringMidi = new int[6];

    [Header("Target melody as STRING INDEX sequence (0..5)")]
    public int[] targetStringSequence;

    [Header("Stage complete SFX")]
    public AudioClip stageCompleteClip;
}