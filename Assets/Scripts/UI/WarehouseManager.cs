using UnityEngine;
using UnityEngine.UI;

public class WarehouseManager : MonoBehaviour
{
    [SerializeField] private RectTransform warehouseRect;
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private ZoneFactory zoneFactory;
    [SerializeField] private ZoneBase zone;

    private float ScaleRatio;
    private float OldScaleRatio;
    private Image warehouseImage;
    [SerializeField]private RectTransform parentRect;
    
    private void Awake()
    {
        if (warehouseRect == null)
            warehouseRect = GetComponent<RectTransform>();
        
        warehouseImage = warehouseRect.GetComponent<Image>();

        if (parentRect == null)
            parentRect = GetComponentInParent<RectTransform>();

        if (zoneFactory == null)
            zoneFactory = GetComponent<ZoneFactory>();

        warehouseRect.anchoredPosition = Vector2.zero;

        zone = WarehouseDataController.Settings.Warehouse;
    }

    private void Start()
    {
        UpdateWarehouseSize();
    }
    
    public RectTransform GetWarehouseRect()
    {
        return warehouseRect;
    }

    public Vector2 GetPhysicalSize()
    {
        return zone.PhysicalSize;
    }

    bool notFirstUpdate = false;

    public void Update()
    {
        if (warehouseImage != null)
            warehouseImage.color = zone.Color;
        Vector2 calcSize = new Vector2(zone.PhysicalSize.x, zone.PhysicalSize.y);

        if (calcSize.x < 0.1f)
            calcSize.x = 0.1f;
        if (calcSize.y < 0.1)
            calcSize.y = 0.1f;

        if (!calcSize.Equals(zone.PhysicalSize))
            zone.PhysicalSize = calcSize;

        UpdateWarehouseSize();

        if (notFirstUpdate)
            foreach (var zone in zoneFactory.Zones)
            {
                zone.Controller.UpdateScale(ScaleRatio);
            }
        else
            notFirstUpdate = true;
    }

    private void UpdateScaleRatio()
    {
        var arCanvas = parentRect.rect.width / parentRect.rect.height;
        var arWarehouse = zone.PhysicalSize.x / zone.PhysicalSize.y;
        OldScaleRatio = ScaleRatio;
        
        if (arWarehouse >= arCanvas)
            ScaleRatio = parentRect.rect.width / zone.PhysicalSize.x;        
        else
            ScaleRatio = parentRect.rect.height / zone.PhysicalSize.y;
        
        if (OldScaleRatio == 0)
            OldScaleRatio = ScaleRatio;
    }
    
    private void UpdateWarehouseSize()
    {
        UpdateScaleRatio();
        warehouseRect.sizeDelta = SizeByAspectRatio(zone.PhysicalSize);
    }
    
    public Rect GetWarehouseBounds()
    {
        Vector3[] corners = new Vector3[4];
        warehouseRect.GetWorldCorners(corners);
        
        float minX = corners[0].x;
        float minY = corners[0].y;
        float maxX = corners[2].x;
        float maxY = corners[2].y;
        
        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    public Vector2 SizeByAspectRatio(Vector2 originalSize)
    {
        var updSize = originalSize * ScaleRatio;
        return updSize;
    }
}