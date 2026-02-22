using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HarpStageManager : MonoBehaviour
{
    [Header("Stages (size=3)")]
    [SerializeField] private HarpStageData[] stages = new HarpStageData[3];

    [Header("Strings (size=6) in order")]
    [SerializeField] private HarpString2D[] strings = new HarpString2D[6];

    [Header("UI")]
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject continueButtonObject;

    [Header("Stage Complete Audio")]
    [SerializeField] private AudioSource stageAudioSource;

    [Header("Timing")]
    [SerializeField] private float delayBeforeStageSfx = 1f;

    private int stageIndex = 0;
    private int progress = 0;

    private bool stageCompleted = false;
    private bool inputLocked = false;

    private void Awake()
    {
        HideContinue();

        if (continueButton)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void Start()
    {
        LoadStage(0);
    }

    private void LoadStage(int index)
    {
        stageIndex = Mathf.Clamp(index, 0, stages.Length - 1);
        progress = 0;
        stageCompleted = false;
        inputLocked = false;

        HideContinue();

        var data = stages[stageIndex];
        if (!ValidateStage(data)) return;

        for (int i = 0; i < 6; i++)
        {
            if (!strings[i])
            {
                Debug.LogError($"String reference missing at strings[{i}]");
                continue;
            }

            strings[i].SetManagerAndIndex(this, i);
            strings[i].SetMidi(data.stringMidi[i]);
        }

        Debug.Log($"Loaded Harp Stage {stageIndex + 1}/3");
    }

    private bool ValidateStage(HarpStageData data)
    {
        if (data == null)
        {
            Debug.LogError($"StageData missing at index {stageIndex}");
            return false;
        }

        if (data.stringMidi == null || data.stringMidi.Length != 6)
        {
            Debug.LogError($"Stage {stageIndex} stringMidi must be length 6");
            return false;
        }

        if (data.targetStringSequence == null || data.targetStringSequence.Length == 0)
        {
            Debug.LogError($"Stage {stageIndex} targetStringSequence is empty");
            return false;
        }

        return true;
    }

    public void RegisterStringHit(int stringIndex)
    {
        if (inputLocked) return;
        if (stageCompleted) return;

        var data = stages[stageIndex];
        int expected = data.targetStringSequence[progress];

        if (stringIndex == expected)
        {
            progress++;
            if (progress >= data.targetStringSequence.Length)
            {
                StartCoroutine(CompleteStageRoutine());
            }
        }
        else
        {
            progress = 0;
            if (stringIndex == data.targetStringSequence[0])
                progress = 1;
        }
    }

    private IEnumerator CompleteStageRoutine()
    {
        stageCompleted = true;
        inputLocked = true;

        // 1) 1 saniye bekle
        if (delayBeforeStageSfx > 0f)
            yield return new WaitForSeconds(delayBeforeStageSfx);

        // 2) stage'e özel ses çal
        float wait = 0.01f;
        var data = stages[stageIndex];

        if (stageAudioSource && data.stageCompleteClip)
        {
            stageAudioSource.Stop();
            stageAudioSource.pitch = 1f;
            stageAudioSource.PlayOneShot(data.stageCompleteClip, 1f);
            wait = Mathf.Max(0.01f, data.stageCompleteClip.length);
        }

        // 3) ses bitene kadar bekle
        yield return new WaitForSeconds(wait);

        // 4) input açma: stage bittiği için artık sadece butonla ilerleniyor
        // inputLocked = false;  // İSTERSEN bunu açabilirsin ama oyuncu tekrar tel çalmasın diye kapalı bırakıyorum

        // 5) butonu aç
        ShowContinue();
    }

    private void OnContinueClicked()
    {
        if (!stageCompleted) return;

        if (stageIndex < 2)
        {
            LoadStage(stageIndex + 1);
        }
        else
        {
            Debug.Log("NEXT LEVEL (not implemented yet)");
        }
    }

    private void HideContinue()
    {
        if (continueButtonObject) continueButtonObject.SetActive(false);
        if (continueButton) continueButton.interactable = false;
    }

    private void ShowContinue()
    {
        if (continueButtonObject) continueButtonObject.SetActive(true);
        if (continueButton) continueButton.interactable = true;
    }
}