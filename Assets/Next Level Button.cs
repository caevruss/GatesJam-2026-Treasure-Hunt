using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class NextLevelButton : MonoBehaviour
{
    [SerializeField] private bool hideUntilEnabled = true;

    private Button _btn;

    private void Awake()
    {
        CacheButton();

        if (_btn)
        {
            _btn.onClick.RemoveListener(GoNext);
            _btn.onClick.AddListener(GoNext);
        }

        // Başlangıç durumu
        if (hideUntilEnabled) gameObject.SetActive(false);
        else if (_btn) _btn.interactable = false;
    }

    private void CacheButton()
    {
        if (_btn == null) _btn = GetComponent<Button>();
    }

    public void SetUnlocked(bool unlocked)
    {
        CacheButton(); 
        if (hideUntilEnabled)
        {
            gameObject.SetActive(unlocked);
        }
    
        if (_btn != null) 
        {
            _btn.interactable = unlocked;
        }
    }

    public void GoNext()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        int next = current + 1;

        if (next >= SceneManager.sceneCountInBuildSettings)
        {
            return;
        }

        SceneManager.LoadScene(next);
    }
}