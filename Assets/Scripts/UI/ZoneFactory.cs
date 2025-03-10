using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

public class ZoneFactory : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    // Поля для создания зон склада
    [SerializeField] private GameObject zonePrefab;
    [SerializeField] private RectTransform warehouseRect;
    [SerializeField] private Button saveButton;

    [System.Serializable]
    public class ZoneDefinition
    {
        public string name;
        public Color color;
        public Vector2 physicalSize;
        public Vector2 physicalPosition;
    }

    // [Header("Определения зон склада")]
    // [SerializeField]
    private ZoneDefinition[] zoneDefinitions = new ZoneDefinition[]
    {
        new ZoneDefinition { name = "Зона стоянки/зарядки", color = Color.yellow, physicalSize = new Vector2(10.0f, 10.0f), physicalPosition = new Vector2(10, 10) },
        new ZoneDefinition { name = "Зона погрузки/разгрузки", color = Color.red, physicalSize = new Vector2(10.0f, 10.0f), physicalPosition = new Vector2(30, 10) },
        new ZoneDefinition { name = "Зона складирования", color = Color.blue, physicalSize = new Vector2(25.0f, 20.0f), physicalPosition = new Vector2(20, 25) },
        new ZoneDefinition { name = "Офис управления", color = new Color(0.2f, 0.8f, 0.2f), physicalSize = new Vector2(7.0f, 15.0f), physicalPosition = new Vector2(40, 30) }
    };

    // Список для хранения RectTransform созданных зон для проверки позиции курсора

    public LinkedList<RectTransform> ZoneRects { get => zoneRects; }
    private LinkedList<RectTransform> zoneRects = new LinkedList<RectTransform>();

    // Параметры для работы с курсором (набор текстур и пороговое значение)
    [Header("Курсоры")]
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D resizeHorizontalCursor;
    [SerializeField] private Texture2D resizeVerticalCursor;
    [SerializeField] private Texture2D resizeDiagonalCursorLeft;
    [SerializeField] private Texture2D resizeDiagonalCursorRight;
    [SerializeField] private Texture2D moveCursor;
    [SerializeField] private float edgeThreshold = 20f;

    private bool isOver = false;

    private Texture2D lastCursor;
    private Vector2 lastOffset;
    
    [System.Serializable]
    public class ZoneDefinitionWrapper
    {
        public List<ZoneDefinition> zones = new List<ZoneDefinition>();
    }

    private string savePath;

    private void Awake()
    {
        // Если warehouseRect не задан в инспекторе, получаем его из текущего объекта
        if (warehouseRect == null)
            warehouseRect = GetComponent<RectTransform>();
            
        savePath = Path.Combine(Application.persistentDataPath, "zoneDefinitions.xml");
        LoadZoneDefinitions();
        
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveZoneDefinitions);
    }

    private void Start()
    {
        CreateZones();
    }
    
    private void LoadZoneDefinitions()
    {
        if (File.Exists(savePath))
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ZoneDefinitionWrapper));
                using (FileStream stream = new FileStream(savePath, FileMode.Open))
                {
                    ZoneDefinitionWrapper wrapper = serializer.Deserialize(stream) as ZoneDefinitionWrapper;
                    if (wrapper != null && wrapper.zones.Count > 0)
                    {
                        zoneDefinitions = wrapper.zones.ToArray();
                        Debug.Log("Зоны загружены из XML");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Ошибка при загрузке зон: " + e.Message);
            }
        }
        else
        {
            Debug.Log("Файл сохранения не найден, используются стандартные значения");
        }
    }
    
    public void SaveZoneDefinitions()
    {
        try
        {
            // Обновляем зоны из текущих созданных объектов
            List<ZoneDefinition> updatedZones = new List<ZoneDefinition>();
            foreach (RectTransform zoneRect in zoneRects)
            {
                ZoneController controller = zoneRect.GetComponent<ZoneController>();
                if (controller != null)
                {
                    ZoneDefinition zone = new ZoneDefinition
                    {
                        name = controller.zoneName,
                        color = controller.GetColor(),
                        physicalSize = controller.GetPhysicalSize(),
                        physicalPosition = controller.GetPhysicalPosition()
                    };
                    updatedZones.Add(zone);
                }
            }
            
            if (updatedZones.Count > 0)
            {
                ZoneDefinitionWrapper wrapper = new ZoneDefinitionWrapper { zones = updatedZones };
                XmlSerializer serializer = new XmlSerializer(typeof(ZoneDefinitionWrapper));
                using (FileStream stream = new FileStream(savePath, FileMode.Create))
                {
                    serializer.Serialize(stream, wrapper);
                }
                Debug.Log("Зоны сохранены в XML: " + savePath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка при сохранении зон: " + e.Message);
        }
    }

    /// <summary>
    /// Создаёт все зоны на основе заданных определений.
    /// </summary>
    private void CreateZones()
    {
        if (zonePrefab == null)
        {
            Debug.LogError("Zone шаблон не определен в ZoneFactory!");
            return;
        }

        foreach (ZoneDefinition definition in zoneDefinitions)
        {
            CreateZone(definition.name, definition.physicalPosition, definition.physicalSize, definition.color);
        }
    }

    /// <summary>
    /// Создаёт отдельную зону, задаёт ей позицию, размеры и цвет, а также регистрирует её для проверки курсора.
    /// </summary>
    public GameObject CreateZone(string name, Vector2 physPosition, Vector2 physSize, Color color)
    {
        GameObject zoneObject = Instantiate(zonePrefab, warehouseRect);
        zoneObject.name = name;

        RectTransform zoneRect = zoneObject.GetComponent<RectTransform>();
        if (zoneRect != null)
        {
            zoneRect.sizeDelta = physSize;
            zoneRect.anchoredPosition = physPosition;
            zoneRect.anchorMin = new Vector2(0, 0);
            zoneRect.anchorMax = new Vector2(0, 0);
            zoneRect.pivot = new Vector2(0, 0);

            RegisterZone(zoneRect);
        }

        // Настройка или добавление ZoneController для задания имени и цвета зоны
        ZoneController controller = zoneObject.GetComponent<ZoneController>();
        if (controller != null)
        {
            controller.zoneName = name;
            Debug.Log(physSize);
            controller.SetPhysicalSize(physSize);
            controller.SetPhysicalPosition(physPosition);
            controller.SetColor(color);
        }
        else
        {
            controller = zoneObject.AddComponent<ZoneController>();
            controller.zoneName = name;
            controller.SetPhysicalSize(physSize);
            controller.SetPhysicalPosition(physPosition);
            controller.SetColor(color);
        }
        controller.ValidatePosition();

        return zoneObject;
    }

    /// <summary>
    /// Регистрирует зону в списке для последующей проверки при обновлении курсора.
    /// </summary>
    public void RegisterZone(RectTransform zoneRect)
    {
        if (zoneRect != null && !zoneRects.Contains(zoneRect))
            zoneRects.AddFirst(zoneRect);
    }

    // Обработчики событий указателя

    public void OnPointerEnter(PointerEventData eventData)
    {
        isOver = true;
        UpdateCursor(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isOver = false;
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (isOver)
        {
            UpdateCursor(eventData);
        }
    }

    /// <summary>
    /// При обновлении данных о позиции курсора перебираем все созданные зоны и проверяем, находится ли курсор внутри зоны,
    /// а если да – определяем, близок ли он к краям для отображения соответствующего кастомного курсора.
    /// Если ни одна зона не удовлетворяет условию, то устанавливаем стандартный курсор.
    /// </summary>
    private void UpdateCursor(PointerEventData eventData)
    {
        bool customCursorSet = false;
        if (Input.GetMouseButton(0) && lastCursor != null)
        {
            Cursor.SetCursor(lastCursor, lastOffset, CursorMode.Auto);
            return;
        }
        else
            lastCursor = null;
        foreach (RectTransform zoneRect in zoneRects)
        {
            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(zoneRect, eventData.position, eventData.pressEventCamera, out localPointerPosition))
            {
                if (zoneRect.rect.Contains(localPointerPosition))
                {

                    bool nearRightEdge = localPointerPosition.x > zoneRect.rect.width - edgeThreshold;
                    bool nearLeftEdge = localPointerPosition.x < edgeThreshold;
                    bool nearTopEdge = localPointerPosition.y > zoneRect.rect.height - edgeThreshold;
                    bool nearBottomEdge = localPointerPosition.y < edgeThreshold;

                    if ((nearRightEdge && nearTopEdge) || (nearLeftEdge && nearBottomEdge))
                    {
                        Cursor.SetCursor(resizeDiagonalCursorRight, new Vector2(16, 16), CursorMode.Auto);
                        lastCursor = resizeDiagonalCursorRight;
                        customCursorSet = true;
                        break;
                    }
                    else if ((nearLeftEdge && nearTopEdge) || (nearRightEdge && nearBottomEdge))
                    {
                        Cursor.SetCursor(resizeDiagonalCursorLeft, new Vector2(16, 16), CursorMode.Auto);
                        lastCursor = resizeDiagonalCursorLeft;
                        customCursorSet = true;
                        break;
                    }
                    else if (nearLeftEdge)
                    {
                        Cursor.SetCursor(resizeHorizontalCursor, new Vector2(48, 16), CursorMode.Auto);
                        lastCursor = resizeHorizontalCursor;
                        lastOffset = new Vector2(48, 16);
                        customCursorSet = true;
                        break;
                    }
                    else if (nearRightEdge)
                    {
                        Cursor.SetCursor(resizeHorizontalCursor, new Vector2(16, 16), CursorMode.Auto);
                        lastCursor = resizeHorizontalCursor;
                        lastOffset = new Vector2(16, 16);
                        customCursorSet = true;
                        break;
                    }
                    else if (nearTopEdge)
                    {
                        Cursor.SetCursor(resizeVerticalCursor, new Vector2(16, 48), CursorMode.Auto);
                        lastCursor = resizeVerticalCursor;
                        lastOffset = new Vector2(16, 48);
                        customCursorSet = true;
                        break;
                    }
                    else if (nearBottomEdge)
                    {
                        Cursor.SetCursor(resizeVerticalCursor, new Vector2(16, 16), CursorMode.Auto);
                        lastCursor = resizeVerticalCursor;
                        lastOffset = new Vector2(16, 16);
                        customCursorSet = true;
                        break;
                    }
                    else
                    {
                        Cursor.SetCursor(moveCursor, new Vector2(16, 16), CursorMode.Auto);
                        lastCursor = moveCursor;
                        lastOffset = new Vector2(16, 16);
                        customCursorSet = true;
                        break;
                    }
                }
            }
        }
        if (!customCursorSet)
        {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        }
    }
}