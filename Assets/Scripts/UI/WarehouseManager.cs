using UnityEngine;
using UnityEngine.UI;

public class WarehouseManager : MonoBehaviour
{
    [SerializeField] private RectTransform warehouseRect;
    
    [SerializeField] private float padding = 0f;
    
    [SerializeField] private Color warehouseColor = Color.gray;

    private Vector2 _targetSize = new Vector2(800, 600);
    
    [SerializeField]
    private Vector2 TargetSize
    {
        get => _targetSize;
        set
        {
            _targetSize = value;
            UpdateWarehouseSize();
        }
    }

    [SerializeField] private CanvasScaler canvasScaler;

    private float ScaleRatio;
    private float OldScaleRatio;
    [SerializeField]private RectTransform _parentRect;
    
    private void Awake()
    {
        if (warehouseRect == null)
            warehouseRect = GetComponent<RectTransform>();
        
        Image warehouseImage = warehouseRect.GetComponent<Image>();
        if (warehouseImage != null)
            warehouseImage.color = warehouseColor;

        if (_parentRect == null)
            _parentRect = GetComponentInParent<RectTransform>();

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

    public void Update()
    {
        UpdateWarehouseSize();
    }

    private void UpdateScaleRatio()
    {
        var arCanvas = _parentRect.rect.width / _parentRect.rect.height;
        var arWarehouse = TargetSize.x / TargetSize.y;
        OldScaleRatio = ScaleRatio;
        
        if (arWarehouse >= arCanvas)
            ScaleRatio = _parentRect.rect.width / TargetSize.x;        
        else
            ScaleRatio = _parentRect.rect.height / TargetSize.y;
        
        if (OldScaleRatio == 0)
            OldScaleRatio = ScaleRatio;
    }
    
    private void UpdateWarehouseSize()
    {
        UpdateScaleRatio();
        warehouseRect.sizeDelta = SizeByAspectRatio(TargetSize, padding);
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

    public Vector2 SizeByAspectRatio(Vector2 originalSize, float padding = 0f)
    {
        var updSize = originalSize * ScaleRatio;
        updSize.x -= 2f * padding;
        updSize.y -= 2f * padding;
        return updSize;
    }
}