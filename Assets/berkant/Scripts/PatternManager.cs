using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PatternManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject linePrefab; 
    public Transform lineParent;  
    
    [Header("Çözüm")]
    public List<int> correctSequence; 

    private List<int> currentSequence = new List<int>();
    private List<GameObject> finishedLines = new List<GameObject>(); // Sabitlenmiş çizgiler
    
    private GameObject currentLine; // Mouse'u takip eden uçtaki çizgi
    private RectTransform currentLineRect;
    private Vector3 lastDotPosition; // Çizginin başladığı son nokta

    private bool isDrawing = false;
    private Camera uiCamera; // Canvas render modu Screen Space - Camera ise gerekir, Overlay ise null kalır.

    void Start()
    {
        // Eğer Canvas "Screen Space - Overlay" ise kamera null olabilir, sorun yok.
        // Ama "Camera" modundaysan buraya Camera.main atamak gerekebilir.
        uiCamera = null; 
    }

    void Update()
    {
        // 1. Mouse Takibi (Çizim yapıyorsak)
        if (isDrawing && currentLine != null)
        {
            Vector2 localMousePos;
            // Mouse pozisyonunu UI (Canvas) koordinatlarına çevir
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                lineParent as RectTransform, 
                Input.mousePosition, 
                uiCamera, 
                out localMousePos
            );

            // Çizgiyi son noktadan mouse'a doğru uzat
            UpdateLineSize(currentLineRect, lastDotPosition, lineParent.TransformPoint(localMousePos));
        }

        // 2. Çizimi Bitirme
        if (Input.GetMouseButtonUp(0))
        {
            EndDrawing();
        }
    }

    public void StartDrawing(PatternDot dot)
    {
        if (isDrawing) return;

        ResetPattern();
        isDrawing = true;
        currentSequence.Add(dot.id);
        lastDotPosition = dot.transform.position;
        
        Debug.Log("Başlangıç: " + dot.id);

        // Hemen mouse'u takip edecek ilk çizgiyi oluştur
        CreateActiveLine();
    }

    public void AddToPattern(PatternDot dot)
    {
        if (!isDrawing) return;
        if (currentSequence.Contains(dot.id)) return; 

        // 1. Mevcut "takipçi" çizgiyi, bu yeni noktaya sabitle (Bitir)
        UpdateLineSize(currentLineRect, lastDotPosition, dot.transform.position);
        finishedLines.Add(currentLine); // Artık bu çizgi bitti, listeye ekle
        
        // 2. Sırayı güncelle
        currentSequence.Add(dot.id);
        lastDotPosition = dot.transform.position;
        Debug.Log("Eklendi: " + dot.id);

        // 3. Yeni bir takipçi çizgi oluştur (Yeni noktadan mouse'a gidecek olan)
        CreateActiveLine();
    }

    void CreateActiveLine()
    {
        currentLine = Instantiate(linePrefab, lineParent);
        currentLineRect = currentLine.GetComponent<RectTransform>();
        // Başlangıçta uzunluğu 0 olsun, Update'de uzayacak
        currentLineRect.sizeDelta = new Vector2(0, currentLineRect.sizeDelta.y);
    }

    void UpdateLineSize(RectTransform lineRect, Vector3 startPos, Vector3 endPos)
    {
        Vector3 dir = endPos - startPos;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        lineRect.position = startPos;
        lineRect.pivot = new Vector2(0, 0.5f); // Sol orta
        
        // Canvas scale faktörüne bölerek doğru uzunluğu bul
        float canvasScale = lineParent.GetComponentInParent<Canvas>().scaleFactor;
        // Eğer Canvas Overlay ise scaleFactor 1 olabilir veya UI scale'e göre değişir.
        // Genelde distance UI world space'tedir, direkt atayabiliriz ama
        // Parent'ın scale'ini hesaba katmak en garantisidir:
        
        lineRect.sizeDelta = new Vector2(distance / lineParent.lossyScale.x, 5); // Yükseklik 20px (İsteğe göre değiştir)
        lineRect.rotation = Quaternion.Euler(0, 0, angle);
    }

    void EndDrawing()
    {
        isDrawing = false;
        
        // Mouse'u takip eden son boş çizgiyi yok et (Çünkü bir yere bağlanmadı)
        if (currentLine != null) Destroy(currentLine);
        
        CheckResult();
    }

    void CheckResult()
    {
        if (currentSequence.Count != correctSequence.Count)
        {
            Debug.Log("Yanlış: Uzunluk uyuşmuyor.");
            ResetPattern();
            return;
        }

        for (int i = 0; i < correctSequence.Count; i++)
        {
            if (currentSequence[i] != correctSequence[i])
            {
                Debug.Log("Yanlış: Sıra hatalı.");
                ResetPattern();
                return;
            }
        }

        Debug.Log("TEBRİKLER! Kilit Açıldı.");
    }

    void ResetPattern()
    {
        currentSequence.Clear();
        foreach (var line in finishedLines) Destroy(line);
        finishedLines.Clear();
        if (currentLine != null) Destroy(currentLine);
    }
}