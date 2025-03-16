using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class ZoneFactory : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    // Поля для создания зон склада
    [SerializeField] private GameObject zonePrefab;
    [SerializeField] private RectTransform warehouseRect;

    public LinkedList<ZoneConstructor> Zones { get => _zones; }
    private LinkedList<ZoneConstructor> _zones = new LinkedList<ZoneConstructor>();

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
    public class ZoneConstructor
    {
        public GameObject Zone;
        public ZoneController Controller;
        public ZoneConstructor(Zone data, GameObject zonePrefab, RectTransform warehouseRect)
        {
            Zone = Instantiate(zonePrefab, warehouseRect);
            Controller = Zone.GetComponent<ZoneController>();
            if (Controller == null)
                Controller = Zone.AddComponent<ZoneController>();

            Controller.SetData(data);
            Zone.name = data.Name;
        }
    }

    private void Awake()
    {
        if (warehouseRect == null)
            warehouseRect = GetComponent<RectTransform>();
        
        // if (saveButton != null)
        //     saveButton.onClick.AddListener(SaveZoneDefinitions);
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

        foreach (var zone in WarehouseDataController.Settings.Zones)
        {
            RegisterZone(new ZoneConstructor(zone, zonePrefab, warehouseRect));
        }
    }

    /// <summary>
    /// Регистрирует зону в списке для последующей проверки при обновлении курсора.
    /// </summary>
    public void RegisterZone(ZoneConstructor zone)
    {
        if (zone != null && !Zones.Contains(zone))
            Zones.AddFirst(zone);
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
        foreach (var zone in Zones)
        {
            var zoneRect = zone.Controller.GetRect();
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