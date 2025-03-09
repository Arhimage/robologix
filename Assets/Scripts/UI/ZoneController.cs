using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ZoneController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public string zoneName;
    
    [SerializeField] private Color zoneColor;
    
    [SerializeField] private Vector2 minSize = new Vector2(50, 50);
    
    private WarehouseManager warehouseManager;
    
    private RectTransform rectTransform;
    private Image zoneImage;
    private TextMeshProUGUI zoneText;
    
    private bool isDragging = false;
    private bool isResizing = false;
    private Vector2 resizeDirection;
    private Vector2 lastPointerPosition;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        zoneImage = GetComponent<Image>();
        warehouseManager = GetComponentInParent<WarehouseManager>();
        
        if (zoneImage != null && zoneColor != Color.clear)
        {
            zoneImage.color = zoneColor;
        }
    }

    private void Start()
    {
        zoneText = GetComponentInChildren<TextMeshProUGUI>();
        if (zoneText != null)
        {
            zoneText.text = zoneName;
        }
        else
        {
            GameObject textObj = new GameObject("ZoneText");
            textObj.transform.SetParent(transform, false);
            zoneText = textObj.AddComponent<TextMeshProUGUI>();
            zoneText.fontSize = 14;
            zoneText.alignment = TextAlignmentOptions.Center;
            zoneText.text = zoneName;
            zoneText.color = Color.black;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
    }
    
    public void SetColor(Color color)
    {
        zoneColor = color;
        if (zoneImage != null)
        {
            zoneImage.color = color;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        lastPointerPosition = eventData.position;
        
        Vector2 localPointerPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out localPointerPosition);
        
        float edgeThreshold = 20f;
        bool nearRightEdge = localPointerPosition.x > rectTransform.rect.width - edgeThreshold;
        bool nearLeftEdge = localPointerPosition.x < edgeThreshold;
        bool nearTopEdge = localPointerPosition.y > rectTransform.rect.height - edgeThreshold;
        bool nearBottomEdge = localPointerPosition.y < edgeThreshold;
        
        if (nearRightEdge || nearLeftEdge || nearTopEdge || nearBottomEdge)
        {
            isResizing = true;
            isDragging = false;
            
            resizeDirection = new Vector2(
                nearRightEdge ? 1 : (nearLeftEdge ? -1 : 0),
                nearTopEdge ? 1 : (nearBottomEdge ? -1 : 0)
            );
        }
        else
        {
            isDragging = true;
            isResizing = false;
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        isResizing = false;
    }
    
public void OnDrag(PointerEventData eventData)
{
    if (isDragging)
    {
        Vector2 delta = eventData.position - lastPointerPosition;
        rectTransform.anchoredPosition += delta;
        ValidatePosition();
    }
    else if (isResizing)
    {
        Vector2 delta = eventData.position - lastPointerPosition;
        Vector2 newSize = rectTransform.sizeDelta;
        Vector2 newPosition = rectTransform.anchoredPosition;
        
        if (resizeDirection.x > 0)
        {
            newSize.x += delta.x;
        }
        else if (resizeDirection.x < 0)
        {
            float width = newSize.x - delta.x;
            if (width >= minSize.x) {
                newSize.x = width;
                newPosition.x += delta.x;
            }
        }
        
        if (resizeDirection.y > 0)
        {
            newSize.y += delta.y;
        }
        else if (resizeDirection.y < 0)
        {
            float height = newSize.y - delta.y;
            if (height >= minSize.y) {
                newSize.y = height;
                newPosition.y += delta.y;
            }
        }
        
        if (newSize.x >= minSize.x && newSize.y >= minSize.y)
        {
            rectTransform.sizeDelta = newSize;
            rectTransform.anchoredPosition = newPosition;
            ValidatePosition();
        }
    }
    
    lastPointerPosition = eventData.position;
}
    
    public void ValidatePosition()
    {
        if (warehouseManager == null)
        {
            warehouseManager = GetComponentInParent<WarehouseManager>();
            if (warehouseManager == null) return;
        }
        
        RectTransform warehouseRect = warehouseManager.GetWarehouseRect();
        if (warehouseRect == null) return;
        
        Vector2 localPosition = rectTransform.anchoredPosition;
        Vector2 zoneSize = rectTransform.sizeDelta;
        
        zoneSize.x = Mathf.Abs(zoneSize.x);
        zoneSize.y = Mathf.Abs(zoneSize.y);
        
        if (zoneSize.x < minSize.x) zoneSize.x = minSize.x;
        if (zoneSize.y < minSize.y) zoneSize.y = minSize.y;
        
        Vector2 warehouseSize = warehouseRect.rect.size;
        
        if (zoneSize.x > warehouseSize.x) zoneSize.x = warehouseSize.x;
        if (zoneSize.y > warehouseSize.y) zoneSize.y = warehouseSize.y;
        
        if (localPosition.x < 0) localPosition.x = 0;
        if (localPosition.y < 0) localPosition.y = 0;
        
        if (localPosition.x + zoneSize.x > warehouseSize.x)
            localPosition.x = warehouseSize.x - zoneSize.x;
        
        if (localPosition.y + zoneSize.y > warehouseSize.y)
            localPosition.y = warehouseSize.y - zoneSize.y;
        
        rectTransform.sizeDelta = zoneSize;
        rectTransform.anchoredPosition = localPosition;
    }
}