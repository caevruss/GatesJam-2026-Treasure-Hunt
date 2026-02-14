using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PatternDot : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    public int id; // 0'dan 15'e kadar her noktaya inspector'dan farklı sayı ver.
    private PatternManager manager;

    void Start()
    {
        manager = GetComponentInParent<PatternManager>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Tıklama başladığında çizimi başlat
        manager.StartDrawing(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Fare basılıyken bu noktanın üzerine gelirse
        manager.AddToPattern(this);
    }
}