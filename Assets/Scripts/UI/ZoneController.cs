using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ZoneController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private Vector2 minPhysicalSize = new Vector2(4f, 4f);
    private RectTransform rect;
    [SerializeField] private Zone data;
    private float scaleFactor = 1f;
    private WarehouseManager warehouseManager;
    private Image zoneImage;
    private TextMeshProUGUI zoneText;
    private bool isDragging = false;
    private bool isResizing = false;
    private Vector2 resizeDirection;
    private Vector2 lastPointerPosition;
    
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        zoneImage = GetComponent<Image>();
        warehouseManager = GetComponentInParent<WarehouseManager>();
        Debug.Log(warehouseManager.name);
    }

    private void Start()
    {
        zoneText = GetComponentInChildren<TextMeshProUGUI>();
        if (zoneText != null)
        {
            zoneText.text = data.Name;
        }
        else
        {
            GameObject textObj = new GameObject("ZoneText");
            textObj.transform.SetParent(transform, false);
            zoneText = textObj.AddComponent<TextMeshProUGUI>();
            zoneText.fontSize = 14;
            zoneText.alignment = TextAlignmentOptions.Center;
            zoneText.text = data.Name;
            zoneText.color = Color.black;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
    }

    public void SetData(Zone data)
    {
        if (data != null)
            data.ZoneChanged -= UpdateData;

        this.data = data;
        this.data.ZoneChanged += UpdateData;
        UpdateData();
    }

    public Zone GetData()
    {
        return data;
    }

    public RectTransform GetRect()
    {
        return rect;
    }

    private void UpdateData()
    {
        SetPhysicalSize(data.PhysicalSize);
        SetPhysicalPosition(data.PhysicalPosition);
        SetColor(data.Color);
        ValidatePosition();
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
        rect.sizeDelta = new Vector2(
            data.PhysicalSize.x * scaleFactor,
            data.PhysicalSize.y * scaleFactor
        );
        
        rect.anchoredPosition = new Vector2(
            data.PhysicalPosition.x * scaleFactor,
            data.PhysicalPosition.y * scaleFactor
        );
    }

    public void SetPhysicalSize(Vector2 size)
    {
        if (!size.Equals(data.PhysicalSize))
        {
            data.PhysicalSize = size;
        }
    }

    public void SetPhysicalPosition(Vector2 position)
    {
        if (!data.PhysicalPosition.Equals(position))
        {
            data.PhysicalPosition = position;
        }
    }

    public Color GetColor()
    {
        return data.Color;
    }
    
    // Получение физических размеров зоны
    public Vector2 GetPhysicalSize()
    {
        return data.PhysicalSize;
    }
    
    // Получение физической позиции зоны
    public Vector2 GetPhysicalPosition()
    {
        return data.PhysicalPosition;
    }
    
    // Установка физических размеров и позиции
    public void SetPhysicalProperties(Vector2 position, Vector2 size)
    {
        data.PhysicalPosition = position;
        data.PhysicalSize = size;
        UpdateDisplay();
    }
    
    public void SetColor(Color color)
    {
        if (!data.Color.Equals(color))
        {
            data.Color = color;
            if (zoneImage != null)
            {
                zoneImage.color = color;
            }
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        lastPointerPosition = eventData.position;
        
        Vector2 localPointerPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect, eventData.position, eventData.pressEventCamera, out localPointerPosition);
        
        float edgeThreshold = 10f;
        bool nearRightEdge = localPointerPosition.x > rect.rect.width - edgeThreshold;
        bool nearLeftEdge = localPointerPosition.x < edgeThreshold;
        bool nearTopEdge = localPointerPosition.y > rect.rect.height - edgeThreshold;
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
            data.PhysicalPosition += physicalDelta;
            
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
            
            Vector2 newPhysicalSize = data.PhysicalSize;
            Vector2 newPhysicalPosition = data.PhysicalPosition;
            
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
                data.PhysicalSize = newPhysicalSize;
                data.PhysicalPosition = newPhysicalPosition;
                
                // Обновляем визуальное отображение и проверяем границы
                ValidatePosition();
            }
        }
        
        lastPointerPosition = eventData.position;
    }
    
    public void ValidatePosition()
    {        
        Vector2 warehousePhysicalSize =  warehouseManager.GetPhysicalSize();
        
        Vector2 calcPhysSize = new Vector2(
            Mathf.Abs(data.PhysicalSize.x),
            Mathf.Abs(data.PhysicalSize.y));
        // // Валидация физических размеров
        
        if (calcPhysSize.x < minPhysicalSize.x)
            calcPhysSize.x = minPhysicalSize.x;

        if (calcPhysSize.y < minPhysicalSize.y) calcPhysSize.y = minPhysicalSize.y;
        
        if (calcPhysSize.x > warehousePhysicalSize.x) calcPhysSize.x = warehousePhysicalSize.x;
        if (calcPhysSize.y > warehousePhysicalSize.y) calcPhysSize.y = warehousePhysicalSize.y;

        if (data.PhysicalSize.x != calcPhysSize.x || data.PhysicalSize.y != calcPhysSize.y)
            data.PhysicalSize = calcPhysSize; 

        Vector2 calcPhyPos = new Vector2(data.PhysicalPosition.x, data.PhysicalPosition.y);
        
        // Валидация физической позиции
        if (calcPhyPos.x < 0) calcPhyPos.x = 0;
        if (calcPhyPos.y < 0) calcPhyPos.y = 0;
        
        if (calcPhyPos.x + data.PhysicalSize.x > warehousePhysicalSize.x)
            calcPhyPos.x = warehousePhysicalSize.x - data.PhysicalSize.x;
        
        if (calcPhyPos.y + data.PhysicalSize.y > warehousePhysicalSize.y)
            calcPhyPos.y = warehousePhysicalSize.y - data.PhysicalSize.y;

        if (data.PhysicalPosition.x != calcPhyPos.x || data.PhysicalPosition.y != calcPhyPos.y)
            data.PhysicalPosition = calcPhyPos;
        
        // Обновляем визуальное отображение
        UpdateDisplay();
    }
}