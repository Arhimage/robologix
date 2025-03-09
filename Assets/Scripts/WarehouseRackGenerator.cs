using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShelfPlacementType
{
    Horizontal, // полки располагаются вдоль длинной стороны
    Vertical    // полки располагаются вдоль короткой стороны
}

public class WarehouseRackGenerator : MonoBehaviour
{
    [Header("Area Settings")]
    [SerializeField] private Vector2 areaSize = new Vector2(20f, 30f); // ширина и длина области
    [SerializeField] private Vector3 areaCenter = Vector3.zero; // центр области для размещения

    [Header("Shelf Settings")]
    [SerializeField] private Vector2 minShelfSize = new Vector2(1f, 2f); // минимальный размер полки (ширина, длина)
    [SerializeField] private float minDistanceBetweenRacks = 2f; // минимальное расстояние между стеллажами
    [SerializeField] private int shelfLevels = 4; // количество уровней полок
    [SerializeField] private float levelHeight = 0.5f; // высота между уровнями
    [SerializeField] private ShelfPlacementType placementType = ShelfPlacementType.Horizontal; // тип размещения

    [Header("Prefabs")]
    [SerializeField] private GameObject verticalSupportPrefab; // префаб вертикальной стойки
    [SerializeField] private GameObject horizontalShelfPrefab; // префаб горизонтальной полки

    [Header("Generation")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool visualizeArea = true;
    [SerializeField] private Color areaColor = new Color(0.5f, 0.5f, 1f, 0.2f);

    private List<GameObject> generatedRacks = new List<GameObject>();

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateRacks();
        }
    }

    [ContextMenu("Generate Racks")]
    public void GenerateRacks()
    {
        ClearRacks();
        CalculateAndPlaceRacks();
    }

    [ContextMenu("Clear Racks")]
    public void ClearRacks()
    {
        foreach (var rack in generatedRacks)
        {
            if (Application.isPlaying)
                Destroy(rack);
            else
                DestroyImmediate(rack);
        }
        generatedRacks.Clear();
    }

    private void CalculateAndPlaceRacks()
    {
        if (verticalSupportPrefab == null || horizontalShelfPrefab == null)
        {
            Debug.LogError("Prefabs not assigned!");
            return;
        }

        // Определяем размеры для расчета
        float areaWidth = areaSize.x;
        float areaLength = areaSize.y;

        // Получаем размеры области с учетом заданного типа размещения
        float widthForCalculation = (placementType == ShelfPlacementType.Horizontal) ? areaWidth : areaLength;
        float lengthForCalculation = (placementType == ShelfPlacementType.Horizontal) ? areaLength : areaWidth;
        
        // Определяем минимальные размеры полки в соответствии с типом размещения
        float minShelfWidth = (placementType == ShelfPlacementType.Horizontal) ? minShelfSize.x : minShelfSize.y;
        float minShelfLength = (placementType == ShelfPlacementType.Horizontal) ? minShelfSize.y : minShelfSize.x;

        // Рассчитываем, сколько стеллажей можно разместить по ширине и длине
        int racksInWidth = Mathf.Max(1, Mathf.FloorToInt((widthForCalculation + minDistanceBetweenRacks) / (minShelfWidth + minDistanceBetweenRacks)));
        int racksInLength = Mathf.Max(1, Mathf.FloorToInt((lengthForCalculation + minDistanceBetweenRacks) / (minShelfLength + minDistanceBetweenRacks)));

        // Рассчитываем оптимальные размеры для равномерного распределения
        float actualShelfWidth = (widthForCalculation - ((racksInWidth - 1) * minDistanceBetweenRacks)) / racksInWidth;
        float actualShelfLength = (lengthForCalculation - ((racksInLength - 1) * minDistanceBetweenRacks)) / racksInLength;

        // Проверяем, не получились ли полки меньше минимально допустимого размера
        if (actualShelfWidth < minShelfWidth || actualShelfLength < minShelfLength)
        {
            Debug.LogWarning("Calculated shelf size is smaller than the minimum allowed size. Adjusting...");
            // При необходимости корректируем количество стеллажей
            if (actualShelfWidth < minShelfWidth)
            {
                racksInWidth--;
                actualShelfWidth = (widthForCalculation - ((racksInWidth - 1) * minDistanceBetweenRacks)) / racksInWidth;
            }
            if (actualShelfLength < minShelfLength)
            {
                racksInLength--;
                actualShelfLength = (lengthForCalculation - ((racksInLength - 1) * minDistanceBetweenRacks)) / racksInLength;
            }
        }

        // Корректируем размеры полок в соответствии с типом размещения
        Vector2 actualShelfSize = (placementType == ShelfPlacementType.Horizontal) 
            ? new Vector2(actualShelfWidth, actualShelfLength) 
            : new Vector2(actualShelfLength, actualShelfWidth);

        // Рассчитываем смещение от центра для начала размещения
        float startOffsetX = -widthForCalculation / 2f + actualShelfWidth / 2f;
        float startOffsetZ = -lengthForCalculation / 2f + actualShelfLength / 2f;

        // Создаем стеллажи
        for (int widthIndex = 0; widthIndex < racksInWidth; widthIndex++)
        {
            for (int lengthIndex = 0; lengthIndex < racksInLength; lengthIndex++)
            {
                // Рассчитываем позицию стеллажа
                float xPos = startOffsetX + widthIndex * (actualShelfWidth + minDistanceBetweenRacks);
                float zPos = startOffsetZ + lengthIndex * (actualShelfLength + minDistanceBetweenRacks);
                
                // Корректируем позиции с учетом типа размещения
                Vector3 rackPosition = (placementType == ShelfPlacementType.Horizontal)
                    ? new Vector3(xPos, 0, zPos) + areaCenter
                    : new Vector3(zPos, 0, xPos) + areaCenter;

                // Создаем стеллаж
                CreateRack(rackPosition, actualShelfSize);
            }
        }

        Debug.Log($"Generated {racksInWidth * racksInLength} racks in a {racksInWidth}x{racksInLength} grid.");
    }

    private void CreateRack(Vector3 position, Vector2 shelfSize)
    {
        float totalHeight = shelfLevels * levelHeight;

        // Создаем родительский объект для стеллажа
        GameObject rackParent = new GameObject($"Rack_{generatedRacks.Count}");
        rackParent.transform.position = position;
        rackParent.transform.SetParent(transform);
        generatedRacks.Add(rackParent);

        // Рассчитываем позиции для стоек (в локальных координатах)
        Vector3 frontLeftPos = new Vector3(-shelfSize.x / 2f, 0, -shelfSize.y / 2f);
        Vector3 frontRightPos = new Vector3(shelfSize.x / 2f, 0, -shelfSize.y / 2f);
        Vector3 backLeftPos = new Vector3(-shelfSize.x / 2f, 0, shelfSize.y / 2f);
        Vector3 backRightPos = new Vector3(shelfSize.x / 2f, 0, shelfSize.y / 2f);

        // Создаем вертикальные стойки
        GameObject frontLeftSupport = CreateSupport(frontLeftPos, totalHeight, rackParent.transform);
        GameObject frontRightSupport = CreateSupport(frontRightPos, totalHeight, rackParent.transform);
        GameObject backLeftSupport = CreateSupport(backLeftPos, totalHeight, rackParent.transform);
        GameObject backRightSupport = CreateSupport(backRightPos, totalHeight, rackParent.transform);

        // Создаем полки на каждом уровне
        for (int level = 0; level < shelfLevels; level++)
        {
            float yPos = level * levelHeight;
            CreateShelf(new Vector3(0, yPos, 0), shelfSize, rackParent.transform);
        }
    }

    private GameObject CreateSupport(Vector3 position, float height, Transform parent)
    {
        GameObject support = Instantiate(verticalSupportPrefab, parent);
        support.transform.localPosition = position;
        
        // Изменяем масштаб стойки по высоте
        // Предполагаем, что префаб имеет высоту 1 единицу по Y
        Vector3 scale = support.transform.localScale;
        scale.y = height;
        support.transform.localScale = scale;
        
        return support;
    }

    private GameObject CreateShelf(Vector3 position, Vector2 size, Transform parent)
    {
        GameObject shelf = Instantiate(horizontalShelfPrefab, parent);
        shelf.transform.localPosition = position;
        
        // Изменяем масштаб полки по размерам
        // Предполагаем, что префаб имеет размеры 1x1 по X и Z
        Vector3 scale = shelf.transform.localScale;
        scale.x = size.x;
        scale.z = size.y;
        shelf.transform.localScale = scale;
        
        return shelf;
    }

    private void OnDrawGizmos()
    {
        if (visualizeArea)
        {
            Gizmos.color = areaColor;
            Gizmos.DrawCube(areaCenter, new Vector3(areaSize.x, 0.1f, areaSize.y));
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(areaCenter, new Vector3(areaSize.x, 0.1f, areaSize.y));
        }
    }
}