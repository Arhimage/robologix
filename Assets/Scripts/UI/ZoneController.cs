using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ZoneController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public string zoneName;
    
    [SerializeField] private Color zoneColor;
    
    // Физические размеры в метрах
    [SerializeField] private readonly Vector2 minPhysicalSize = new Vector2(0.5f, 0.5f);
    
    // Физические размеры и положение зоны в метрах
    [SerializeField] private Vector2 physicalSize;
    [SerializeField] private Vector2 physicalPosition;
    
    // Коэффициент масштабирования (пиксели на метр)
    private float scaleFactor = 1f; // По умолчанию 100 пикселей на метр
    
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
        
        // Обновить отображение с учетом физических размеров
        // UpdateDisplay();
    }
    
    // Новый метод для обновления масштаба отображения
    public void UpdateScale(float pixelToMeterScale)
    {
        scaleFactor = pixelToMeterScale;
        ValidatePosition();
    }
    
    // Метод для обновления визуального отображения на основе физических размеров
    private void UpdateDisplay()
    {
        // Преобразуем физические размеры в пиксели для отображения
        rectTransform.sizeDelta = new Vector2(
            physicalSize.x * scaleFactor,
            physicalSize.y * scaleFactor
        );
        
        rectTransform.anchoredPosition = new Vector2(
            physicalPosition.x * scaleFactor,
            physicalPosition.y * scaleFactor
        );
    }

    public void SetPhysicalSize(Vector2 size)
    {
        physicalSize = size;
        Debug.Log(physicalSize);
        ValidatePosition();
    }

    public void SetPhysicalPosition(Vector2 position)
    {
        physicalPosition = position;
        ValidatePosition();
    }

    public Color GetColor()
    {
        return zoneColor;
    }
    
    // Получение физических размеров зоны
    public Vector2 GetPhysicalSize()
    {
        return physicalSize;
    }
    
    // Получение физической позиции зоны
    public Vector2 GetPhysicalPosition()
    {
        return physicalPosition;
    }
    
    // Установка физических размеров и позиции
    public void SetPhysicalProperties(Vector2 position, Vector2 size)
    {
        physicalPosition = position;
        physicalSize = size;
        UpdateDisplay();
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
        
        float edgeThreshold = 10f;
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
            
            // Конвертируем дельту пикселей в физические единицы
            Vector2 physicalDelta = new Vector2(
                delta.x / scaleFactor,
                delta.y / scaleFactor
            );
            
            // Обновляем физическое положение
            physicalPosition += physicalDelta;
            
            // Обновляем визуальное отображение и проверяем границы
            ValidatePosition();
        }
        else if (isResizing)
        {
            Vector2 delta = eventData.position - lastPointerPosition;
            
            // Конвертируем дельту пикселей в физические единицы
            Vector2 physicalDelta = new Vector2(
                delta.x / scaleFactor,
                delta.y / scaleFactor
            );
            
            Vector2 newPhysicalSize = physicalSize;
            Vector2 newPhysicalPosition = physicalPosition;
            
            if (resizeDirection.x > 0)
            {
                newPhysicalSize.x += physicalDelta.x;
            }
            else if (resizeDirection.x < 0)
            {
                float width = newPhysicalSize.x - physicalDelta.x;
                if (width >= minPhysicalSize.x) {
                    newPhysicalSize.x = width;
                    newPhysicalPosition.x += physicalDelta.x;
                }
            }
            
            if (resizeDirection.y > 0)
            {
                newPhysicalSize.y += physicalDelta.y;
            }
            else if (resizeDirection.y < 0)
            {
                float height = newPhysicalSize.y - physicalDelta.y;
                if (height >= minPhysicalSize.y) {
                    newPhysicalSize.y = height;
                    newPhysicalPosition.y += physicalDelta.y;
                }
            }
            
            if (newPhysicalSize.x >= minPhysicalSize.x && newPhysicalSize.y >= minPhysicalSize.y)
            {
                physicalSize = newPhysicalSize;
                physicalPosition = newPhysicalPosition;
                
                // Обновляем визуальное отображение и проверяем границы
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
        
        Vector2 warehousePhysicalSize =  warehouseManager.GetPhysicalSize();
        
        // // Валидация физических размеров
        physicalSize.x = Mathf.Abs(physicalSize.x);
        physicalSize.y = Mathf.Abs(physicalSize.y);
        
        if (physicalSize.x < minPhysicalSize.x)
        {
            physicalSize.x = minPhysicalSize.x;
        } 
        if (physicalSize.y < minPhysicalSize.y) physicalSize.y = minPhysicalSize.y;
        
        if (physicalSize.x > warehousePhysicalSize.x) physicalSize.x = warehousePhysicalSize.x;
        if (physicalSize.y > warehousePhysicalSize.y) physicalSize.y = warehousePhysicalSize.y;
        
        // Валидация физической позиции
        if (physicalPosition.x < 0) physicalPosition.x = 0;
        if (physicalPosition.y < 0) physicalPosition.y = 0;
        
        if (physicalPosition.x + physicalSize.x > warehousePhysicalSize.x)
            physicalPosition.x = warehousePhysicalSize.x - physicalSize.x;
        
        if (physicalPosition.y + physicalSize.y > warehousePhysicalSize.y)
            physicalPosition.y = warehousePhysicalSize.y - physicalSize.y;
        
        // Обновляем визуальное отображение
        UpdateDisplay();
    }
}