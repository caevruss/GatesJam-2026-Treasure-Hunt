using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MelodyChecker : MonoBehaviour
{
    [Header("Target melody (MIDI) - 12 notes")]
    [SerializeField] private int[] targetMelody = new int[12];

    [Header("Success Feedback")]
    [SerializeField] private AudioSource successAudioSource;
    [SerializeField] private AudioClip successClip;

    [Tooltip("Ses bitince açılacak buton (GameObject aktif edilecek).")]
    [SerializeField] private GameObject nextLevelButtonObject;

    [Tooltip("İstersen ayrıca interactable da açsın (Button component'i varsa).")]
    [SerializeField] private Button nextLevelButton;

    [Header("Options")]
    [SerializeField] private bool lockInputWhileSuccessPlays = true;

    private int progressIndex = 0;
    private bool successSequenceRunning = false;

    private readonly List<int> playedNotes = new List<int>(32);

    private void Awake()
    {
        if (nextLevelButtonObject) nextLevelButtonObject.SetActive(false);
        if (nextLevelButton) nextLevelButton.interactable = false;
    }

    public void RegisterNote(int midiNote)
    {
        if (successSequenceRunning && lockInputWhileSuccessPlays) return;

        playedNotes.Add(midiNote);

        if (targetMelody == null || targetMelody.Length == 0) return;

        int expected = targetMelody[progressIndex];

        if (midiNote == expected)
        {
            progressIndex++;

            if (progressIndex >= targetMelody.Length)
            {
                StartCoroutine(SuccessSequence());
            }
        }
        else
        {
            // yanlış -> sıfırla
            progressIndex = 0;
            playedNotes.Clear();

            // yanlış bastı ama ilk notayı bastıysa direkt 1'den başlat
            if (targetMelody.Length > 0 && midiNote == targetMelody[0])
            {
                progressIndex = 1;
                playedNotes.Add(midiNote);
            }
        }
    }

    private IEnumerator SuccessSequence()
    {
        successSequenceRunning = true;

        // progress reset (melodiyi tekrar çalamasın diye burada resetliyoruz)
        progressIndex = 0;
        playedNotes.Clear();

        float wait = 0f;

        if (successAudioSource && successClip)
        {
            successAudioSource.Stop();
            successAudioSource.pitch = 1f;
            successAudioSource.PlayOneShot(successClip, 1f);
            wait = successClip.length;
        }

        // clip yoksa bile bir frame bekleyip butonu açalım
        if (wait <= 0f) wait = 0.01f;

        yield return new WaitForSeconds(wait);

        if (nextLevelButtonObject) nextLevelButtonObject.SetActive(true);
        if (nextLevelButton) nextLevelButton.interactable = true;

        successSequenceRunning = false;
    }
}
