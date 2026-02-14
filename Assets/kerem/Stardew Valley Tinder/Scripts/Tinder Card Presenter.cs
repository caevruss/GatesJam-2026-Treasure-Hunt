// TinderCardPresenter.cs (UPDATED)
// 1) Only 2 prompts (title + answer), and prints them as "Prompt1: <title> - <answer>"
// 2) Picks a RANDOM photo from info.photos each time a card is shown

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TinderCardPresenter : MonoBehaviour
{
    [SerializeField] private TinderRoundManager round;

    [Header("UI Refs (Text)")]
    [SerializeField] private TMP_Text ageText;       // "Age: 21"
    [SerializeField] private TMP_Text locationText;  // "Location: Pelican Town"
    [SerializeField] private TMP_Text addressText;   // "Adress: 123..."
    [SerializeField] private TMP_Text birthdayText;  // "Birthday: Summer 13"

    [SerializeField] private TMP_Text prompt1Text;   // "Prompt1: <title> - <answer>"
    [SerializeField] private TMP_Text prompt2Text;   // "Prompt2: <title> - <answer>"

    [SerializeField] private TMP_Text interestsText; // "Interests: A • B • C"
    [SerializeField] private TMP_Text nameUnderPhotoText; // "ALEX"

    [Header("UI Refs (Image)")]
    [SerializeField] private Image photoImage;       // portrait

    private void OnEnable()
    {
        if (round) round.OnCardShown += HandleCardShown;
    }

    private void OnDisable()
    {
        if (round) round.OnCardShown -= HandleCardShown;
    }

    private void HandleCardShown(TinderCharacterData data, TinderProfileInfo info, int index, int total)
    {
        // Name under portrait
        if (nameUnderPhotoText) nameUnderPhotoText.text = (info.displayName ?? "").ToUpperInvariant();

        // "Age: 21" etc.
        if (ageText)      ageText.text      = $"Age: {info.age}";
        if (locationText) locationText.text = $"Location: {Safe(info.location)}";
        if (addressText)  addressText.text  = $"Adress: {Safe(info.address)}";
        if (birthdayText) birthdayText.text = $"Birthday: {Safe(info.birthday)}";

        // Prompts (2)
        if (prompt1Text) prompt1Text.text = $"Prompt1: {FormatPrompt(info.prompt1Title, info.prompt1Answer)}";
        if (prompt2Text) prompt2Text.text = $"Prompt2: {FormatPrompt(info.prompt2Title, info.prompt2Answer)}";

        // Interests
        if (interestsText)
        {
            string joined = (info.interests != null && info.interests.Length > 0)
                ? string.Join(" • ", info.interests)
                : "";
            interestsText.text = $"Interests: {joined}";
        }

        // Photo (RANDOM)
        if (photoImage)
        {
            var sprite = PickRandomPhoto(info.photos);
            if (sprite != null)
            {
                photoImage.enabled = true;
                photoImage.sprite = sprite;
            }
            else
            {
                photoImage.enabled = false;
                photoImage.sprite = null;
            }
        }
    }

    private static string Safe(string s) => string.IsNullOrEmpty(s) ? "" : s;

    private static string FormatPrompt(string title, string answer)
    {
        title = Safe(title);
        answer = Safe(answer);

        if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(answer)) return "";
        if (string.IsNullOrEmpty(title)) return answer;
        if (string.IsNullOrEmpty(answer)) return title;

        return $"{title} - {answer}";
    }

    private static Sprite PickRandomPhoto(Sprite[] photos)
    {
        if (photos == null || photos.Length == 0) return null;

        // try a few times to skip nulls
        for (int i = 0; i < 5; i++)
        {
            int idx = Random.Range(0, photos.Length);
            if (photos[idx] != null) return photos[idx];
        }

        // fallback: first non-null
        for (int i = 0; i < photos.Length; i++)
            if (photos[i] != null) return photos[i];

        return null;
    }
}
