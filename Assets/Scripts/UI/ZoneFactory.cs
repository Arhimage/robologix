using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ZoneFactory : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    // Поля для создания зон склада
    [SerializeField] private GameObject zonePrefab;
    [SerializeField] private RectTransform warehouseRect;

    [System.Serializable]
    public class ZoneDefinition
    {
        public string name;
        public Color color;
        public Vector2 initialSize;
        public Vector2 initialPosition;
    }

    [Header("Определения зон склада")]
    [SerializeField]
    private ZoneDefinition[] zoneDefinitions = new ZoneDefinition[]
    {
        new ZoneDefinition { name = "Зона стоянки/зарядки", color = Color.yellow, initialSize = new Vector2(150, 120), initialPosition = new Vector2(100, 100) },
        new ZoneDefinition { name = "Зона погрузки/разгрузки", color = Color.red, initialSize = new Vector2(200, 100), initialPosition = new Vector2(300, 100) },
        new ZoneDefinition { name = "Зона складирования", color = Color.blue, initialSize = new Vector2(250, 200), initialPosition = new Vector2(200, 250) },
        new ZoneDefinition { name = "Офис управления", color = new Color(0.2f, 0.8f, 0.2f), initialSize = new Vector2(100, 100), initialPosition = new Vector2(400, 300) }
    };

    // Список для хранения RectTransform созданных зон для проверки позиции курсора
    private List<RectTransform> zoneRects = new List<RectTransform>();

    // Параметры для работы с курсором (набор текстур и пороговое значение)
    [Header("Курсоры")]
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D resizeHorizontalCursor;
    [SerializeField] private Texture2D resizeVerticalCursor;
    [SerializeField] private Texture2D resizeDiagonalCursorLeft;
    [SerializeField] private Texture2D resizeDiagonalCursorRight;
    [SerializeField] private Texture2D moveCursor;
    [SerializeField] private float edgeThreshold = 20f;

    // Переменные для управления событиями указателя
    private bool isOver = false;
    // RectTransform текущего объекта, на котором висит данный компонент (используется для событий указателя)
    private RectTransform rectTransform;

    private Texture2D lastCursor;

    private void Awake()
    {
        // Если warehouseRect не задан в инспекторе, получаем его из текущего объекта
        if (warehouseRect == null)
        {
            warehouseRect = GetComponent<RectTransform>();
        }
        rectTransform = GetComponent<RectTransform>();

        // Инициализация текстур курсоров, если они не назначены через инспектор
        // if (defaultCursor == null)
        //     defaultCursor = ResizeTexture()
        
        if (resizeHorizontalCursor == null)
            resizeHorizontalCursor = Resources.Load<Texture2D>("Cursors/resize_horizontal");
        if (resizeVerticalCursor == null)
            resizeVerticalCursor = Resources.Load<Texture2D>("Cursors/resize_vertical");
        if (resizeDiagonalCursorLeft == null)
            resizeDiagonalCursorLeft = Resources.Load<Texture2D>("Cursors/resize_diagonal");
        if (resizeDiagonalCursorRight == null)
            resizeDiagonalCursorRight = Resources.Load<Texture2D>("Cursors/resize_diagonal");
        if (moveCursor == null)
            moveCursor = Resources.Load<Texture2D>("Cursors/move");
    }

    private void Start()
    {
        CreateZones();
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
            CreateZone(definition.name, definition.initialPosition, definition.initialSize, definition.color);
        }
    }

    /// <summary>
    /// Создаёт отдельную зону, задаёт ей позицию, размеры и цвет, а также регистрирует её для проверки курсора.
    /// </summary>
    public GameObject CreateZone(string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject zoneObject = Instantiate(zonePrefab, warehouseRect);
        zoneObject.name = name;

        RectTransform zoneRect = zoneObject.GetComponent<RectTransform>();
        if (zoneRect != null)
        {
            // Обеспечиваем положительность размеров
            size.x = Mathf.Abs(size.x);
            size.y = Mathf.Abs(size.y);

            // Минимальные размеры
            if (size.x < 50) size.x = 50;
            if (size.y < 50) size.y = 50;

            zoneRect.sizeDelta = size;
            zoneRect.anchoredPosition = position;
            zoneRect.anchorMin = new Vector2(0, 0);
            zoneRect.anchorMax = new Vector2(0, 0);
            zoneRect.pivot = new Vector2(0, 0);

            // Регистрируем зону для последующей проверки положения курсора
            RegisterZone(zoneRect);
        }

        // Настройка или добавление ZoneController для задания имени и цвета зоны
        ZoneController controller = zoneObject.GetComponent<ZoneController>();
        if (controller != null)
        {
            controller.zoneName = name;
            controller.SetColor(color);
        }
        else
        {
            controller = zoneObject.AddComponent<ZoneController>();
            controller.zoneName = name;
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
            zoneRects.Add(zoneRect);
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
            Cursor.SetCursor(lastCursor, new Vector2(16, 16), CursorMode.Auto);
            customCursorSet = true;
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
                    else if (nearRightEdge || nearLeftEdge)
                    {
                        Cursor.SetCursor(resizeHorizontalCursor, new Vector2(16, 16), CursorMode.Auto);
                        lastCursor = resizeHorizontalCursor;
                        customCursorSet = true;
                        break;
                    }
                    else if (nearTopEdge || nearBottomEdge)
                    {
                        Cursor.SetCursor(resizeVerticalCursor, new Vector2(16, 16), CursorMode.Auto);
                        lastCursor = resizeVerticalCursor;
                        customCursorSet = true;
                        break;
                    }
                    else
                    {
                        Cursor.SetCursor(moveCursor, new Vector2(16, 16), CursorMode.Auto);
                        lastCursor = moveCursor;
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