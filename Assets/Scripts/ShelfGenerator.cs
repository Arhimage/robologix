using System.Collections.Generic;
using UnityEngine;

public class ShelfGenerator : MonoBehaviour
{
    [Header("Префабы")]
    [SerializeField] private GameObject shelfPrefab;
    [SerializeField] private GameObject supportPrefab;

    [Header("Параметры области")]
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(10f, 2f, 10f);
    [SerializeField] private bool spawnAlongX = true;
    [SerializeField] private int floorCount = 2;

    [Header("Параметры полок")]
    [SerializeField] private float levelHeight = 0.5f;
    [SerializeField] private float shelfWidth = 1.0f;
    [SerializeField] private float maxShelfLength = 5.0f;
    [SerializeField] private float minShelfLength = 2.0f;
    [SerializeField] private float minAisleWidth = 2.0f;

    private Transform shelveParent;
    private Dictionary<Vector2, List<float>> supportPositions = new Dictionary<Vector2, List<float>>();

    private void Start()
    {
        GenerateShelves();
    }

    public void GenerateShelves()
    {
        ClearShelves();
        supportPositions.Clear();

        float areaLength = spawnAlongX ? spawnAreaSize.x : spawnAreaSize.z;
        float areaWidth = spawnAlongX ? spawnAreaSize.z : spawnAreaSize.x;

        int possibleRowCount = Mathf.FloorToInt((areaWidth + minAisleWidth) / (shelfWidth + minAisleWidth));
        if (possibleRowCount < 2)
        {
            Debug.LogWarning("Недостаточно места для размещения полок с проходами!");
            return;
        }

        float rowsSpacing = (areaWidth - (possibleRowCount * shelfWidth)) / (possibleRowCount - 1);
        for (int floor = 1; floor <= floorCount; floor++)
        {
            float floorPosition = floor * levelHeight;
            for (int row = 0; row < possibleRowCount; row++)
            {
                float rowPosition = (row * (shelfWidth + rowsSpacing)) - (areaWidth / 2) + (shelfWidth / 2);
                float remainingLength = areaLength;
                float currentPos = -areaLength / 2;
                while (remainingLength > minShelfLength)
                {
                    float currentShelfLength = Mathf.Min(maxShelfLength, remainingLength);
                    Vector3 shelfPosition;
                    if (spawnAlongX)
                    {
                        shelfPosition = new Vector3(
                            currentPos + (currentShelfLength / 2),
                            floorPosition,
                            rowPosition
                        );
                    }
                    else
                    {
                        shelfPosition = new Vector3(
                            rowPosition,
                            floorPosition,
                            currentPos + (currentShelfLength / 2)
                        );
                    }
                    CreateShelf(shelfPosition, currentShelfLength, shelfWidth, floor);
                    currentPos += currentShelfLength;
                    remainingLength -= currentShelfLength;
                }
            }
        }
        CreateSupports();
    }

    private GameObject CreateShelf(Vector3 position, float length, float width, int floor)
    {
        GameObject shelfContainer = new GameObject($"Shelf_Floor{floor}_Pos{position}");
        shelfContainer.transform.parent = shelveParent;
        shelfContainer.transform.position = position;

        GameObject shelfInstance = Instantiate(shelfPrefab, Vector3.zero, Quaternion.identity, shelfContainer.transform);
        Vector3 shelfScale;
        if (spawnAlongX)
        {
            shelfScale = new Vector3(length, 1f, width);
            shelfInstance.transform.localRotation = Quaternion.identity;
        }
        else
        {
            shelfScale = new Vector3(width, 1f, length);
            shelfInstance.transform.localRotation = Quaternion.identity;
        }
        shelfInstance.transform.localScale = shelfScale;
        shelfInstance.transform.localPosition = Vector3.zero;
        RegisterSupportPositions(position, length, width, floor);
        return shelfContainer;
    }
    
    private void RegisterSupportPositions(Vector3 shelfPosition, float length, float width, int floor)
    {
        float halfLength = length / 2;
        float halfWidth = width / 2;
        Vector2[] supportCoords;
        if (spawnAlongX)
        {
            supportCoords = new Vector2[]
            {
                new Vector2(shelfPosition.x - halfLength, shelfPosition.z - halfWidth),
                new Vector2(shelfPosition.x + halfLength, shelfPosition.z - halfWidth),
                new Vector2(shelfPosition.x - halfLength, shelfPosition.z + halfWidth),
                new Vector2(shelfPosition.x + halfLength, shelfPosition.z + halfWidth)
            };
        }
        else
        {
            supportCoords = new Vector2[]
            {
                new Vector2(shelfPosition.x - halfWidth, shelfPosition.z - halfLength),
                new Vector2(shelfPosition.x + halfWidth, shelfPosition.z - halfLength),
                new Vector2(shelfPosition.x - halfWidth, shelfPosition.z + halfLength),
                new Vector2(shelfPosition.x + halfWidth, shelfPosition.z + halfLength)
            };
        }
        foreach (Vector2 coord in supportCoords)
        {
            if (!supportPositions.ContainsKey(coord))
            {
                supportPositions[coord] = new List<float>();
            }
            float floorHeight = floor == 0 ? 0 : floor * levelHeight;
            if (!supportPositions[coord].Contains(floorHeight))
            {
                supportPositions[coord].Add(floorHeight);
            }
        }
    }
    
    private void CreateSupports()
    {
        Transform supportsParent = new GameObject("Supports").transform;
        supportsParent.parent = shelveParent;
        foreach (var supportData in supportPositions)
        {
            Vector2 coord = supportData.Key;
            List<float> heights = supportData.Value;
            heights.Sort();
            float prevHeight = 0;
            foreach (float height in heights)
            {
                if (height > prevHeight)
                {
                    float supportHeight = height - prevHeight;
                    float supportYPosition = prevHeight + (supportHeight / 2);
                    // Сдвигаем поддержку вниз на половину её высоты
                    float adjustedY = supportYPosition - (supportHeight / 2);
                    GameObject support = Instantiate(supportPrefab, supportsParent);
                    support.transform.position = new Vector3(coord.x, adjustedY, coord.y);
                    // Масштабируем поддержку по оси Z вместо оси Y
                    Vector3 scale = support.transform.localScale;
                    scale.z = supportHeight;
                    support.transform.localScale = scale;
                }
                prevHeight = height;
            }
        }
    }
    
    private void ClearShelves()
    {
        if (shelveParent == null) return;
        while (shelveParent.childCount > 0)
        {
            DestroyImmediate(shelveParent.GetChild(0).gameObject);
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.8f, 0.2f, 0.2f, 0.3f);
        Gizmos.DrawCube(transform.position, spawnAreaSize);
    }
}