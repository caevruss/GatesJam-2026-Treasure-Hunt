using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class SafeLock : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    [Header("Ayarlar")]
    public string correctPassword = "UNITY";
    public float snapSpeed = 10f;
    public GameObject letterPrefab;

    [Header("Referanslar")]
    public TextMeshProUGUI displayText;
    public Transform wheelContainer;

    private List<char> alphabet = new List<char>() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'R', 'S', 'T', 'U', 'V', 'Y', 'Z' };
    private float anglePerItem;
    private string currentInput = "";
    
    // Sürükleme Mantığı
    private bool isDragging = false;
    private float dragOffset;
    private Camera canvasCam;

    void Start()
    {
        // UI tıklamaları için Canvas kamerasını bul
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            canvasCam = canvas.worldCamera;
        }

        // Harfleri Diz
        anglePerItem = 360f / alphabet.Count;
        float radius = GetComponent<RectTransform>().rect.width / 2 - 40;

        for (int i = 0; i < alphabet.Count; i++)
        {
            GameObject letterObj = Instantiate(letterPrefab, wheelContainer);
            float angle = i * anglePerItem;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 pos = new Vector3(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius, 0);
            letterObj.transform.localPosition = pos;
            letterObj.transform.localRotation = Quaternion.Euler(0, 0, -angle);

            letterObj.GetComponent<TextMeshProUGUI>().text = alphabet[i].ToString();
            letterObj.name = alphabet[i].ToString();
        }

        UpdateDisplay();
    }

    void Update()
    {
        // Fareyi bıraktığımızda mıknatıs gibi en yakın harfe yapışması için
        if (!isDragging)
        {
            // Unity'nin kendi gerçek açısını referans alıyoruz (Işınlanmayı önler)
            float currentZ = transform.localEulerAngles.z;

            // En yakın harfin açısını hesapla
            float targetZ = Mathf.Round(currentZ / anglePerItem) * anglePerItem;

            // Oraya yumuşakça dön
            float newZ = Mathf.LerpAngle(currentZ, targetZ, Time.deltaTime * snapSpeed);
            transform.localRotation = Quaternion.Euler(0, 0, newZ);
        }
    }

    // --- SÜRÜKLEME (DRAG) KONTROLLERİ ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        
        // Farenin merkezden açısını bul
        Vector2 center = RectTransformUtility.WorldToScreenPoint(canvasCam, transform.position);
        Vector2 dir = eventData.position - center;
        float mouseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Farenin açısı ile çarkın o anki "gerçek" açısı arasındaki farkı hafızaya al
        dragOffset = transform.localEulerAngles.z - mouseAngle;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 center = RectTransformUtility.WorldToScreenPoint(canvasCam, transform.position);
        Vector2 dir = eventData.position - center;
        float mouseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Farenin anlık açısına ilk başta hesapladığımız farkı ekle
        float targetAngle = mouseAngle + dragOffset;
        
        // Quaternion ile rotasyonu ver (Trigonometrik sınırları otomatik çözer)
        transform.localRotation = Quaternion.Euler(0, 0, targetAngle);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    // --- BUTON VE OYUN MANTIĞI ---

    public void SubmitCurrentLetter()
    {
        if (currentInput.Length >= correctPassword.Length) return;

        float angle = transform.localEulerAngles.z;
        if (angle < 0) angle += 360; // Negatif açı düzeltmesi

        int index = Mathf.RoundToInt(angle / anglePerItem);
        if (index >= alphabet.Count) index = 0;

        char selectedChar = alphabet[index];
        currentInput += selectedChar;

        UpdateDisplay();
        CheckPassword();
    }

    public void ResetCode()
    {
        currentInput = "";
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (displayText != null) displayText.text = currentInput;
    }

    void CheckPassword()
    {
        if (currentInput == correctPassword)
        {
            Debug.Log("KASA AÇILDI!");
            if (displayText != null) displayText.color = Color.green;
            // Kazanma olayını buraya ekleyebilirsin
        }
        else if (currentInput.Length == correctPassword.Length)
        {
            Debug.Log("YANLIŞ ŞİFRE");
            if (displayText != null) displayText.color = Color.red;
            Invoke("ResetCode", 1f); // 1 saniye sonra kırmızı yazıyı temizle
            Invoke("ResetColor", 1f);
        }
    }

    void ResetColor()
    {
        if (displayText != null) displayText.color = Color.white;
    }
}