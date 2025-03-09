using UnityEngine;
using UnityEngine.UI;

public class WarehouseManager : MonoBehaviour
{
    [SerializeField] private RectTransform warehouseRect;
    
    [SerializeField] private Color warehouseColor = Color.gray;

    [SerializeField] private Vector2 physicalSize = new Vector2(80, 60);

    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private ZoneFactory zoneFactory;

    private float ScaleRatio;
    private float OldScaleRatio;
    [SerializeField]private RectTransform parentRect;
    
    private void Awake()
    {
        if (warehouseRect == null)
            warehouseRect = GetComponent<RectTransform>();
        
        Image warehouseImage = warehouseRect.GetComponent<Image>();
        if (warehouseImage != null)
            warehouseImage.color = warehouseColor;

        if (parentRect == null)
            parentRect = GetComponentInParent<RectTransform>();

        if (zoneFactory == null)
            zoneFactory = GetComponent<ZoneFactory>();

        warehouseRect.anchoredPosition = Vector2.zero;
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
        return physicalSize;
    }

    bool notFirstUpdate = false;

    public void Update()
    {
        if (physicalSize.x < 0.1f)
            physicalSize.x = 0.1f;
        if (physicalSize.y < 0.1)
            physicalSize.y = 0.1f;
        UpdateWarehouseSize();
        if (notFirstUpdate)
            foreach (var zRect in zoneFactory.ZoneRects)
            {
                zRect.GetComponent<ZoneController>().UpdateScale(ScaleRatio);
            }
        else
            notFirstUpdate = true;
    }

    private void UpdateScaleRatio()
    {
        var arCanvas = parentRect.rect.width / parentRect.rect.height;
        var arWarehouse = physicalSize.x / physicalSize.y;
        OldScaleRatio = ScaleRatio;
        
        if (arWarehouse >= arCanvas)
            ScaleRatio = parentRect.rect.width / physicalSize.x;        
        else
            ScaleRatio = parentRect.rect.height / physicalSize.y;
        
        if (OldScaleRatio == 0)
            OldScaleRatio = ScaleRatio;
    }
    
    private void UpdateWarehouseSize()
    {
        UpdateScaleRatio();
        warehouseRect.sizeDelta = SizeByAspectRatio(physicalSize);
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