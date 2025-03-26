using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("Box Settings")]
    public Vector3 boxSize = new Vector3(10f, 3f, 10f);
    public GameObject wallPrefab;
    
    [Header("Areas Settings")]
    [Tooltip("Charging Station Area (Red)")]
    public Vector3 chargingAreaSize = new Vector3(2f, 0.1f, 2f);
    public Vector3 chargingAreaPosition = new Vector3(-4f, 0f, -4f);
    public Material chargingAreaMaterial; // Шейдер для зоны зарядки
    
    [Tooltip("Shelves Generation Area")]
    public Vector3 shelvesAreaSize = new Vector3(6f, 0.1f, 6f);
    public Vector3 shelvesAreaPosition = new Vector3(0f, 0f, 0f);
    public Material shelvesAreaMaterial; // Шейдер для зоны стеллажей
    
    [Tooltip("Loading/Unloading Area (Yellow)")]
    public Vector3 loadingAreaSize = new Vector3(2f, 0.1f, 2f);
    public Vector3 loadingAreaPosition = new Vector3(4f, 0f, 4f);
    public Material loadingAreaMaterial; // Шейдер для зоны погрузки/разгрузки
    
    [Header("Vehicle")]
    public GameObject vehiclePrefab;
    
    private GameObject chargingArea;
    private GameObject loadingArea;
    private GameObject shelvesArea;
    private List<GameObject> generatedObjects = new List<GameObject>();

    public void GenerateRoom()
    {
        foreach (GameObject obj in generatedObjects)
        {
            Destroy(obj);
        }
        generatedObjects.Clear();
        
        GenerateBox();
        
        GenerateChargingArea();
        GenerateLoadingArea();
        
        PlaceVehicle();
    }

    private void GenerateBox()
    {
        GameObject floor = CreateWall(new Vector3(0, 0, 0), new Vector3(boxSize.x, boxSize.z, 0.1f), Quaternion.Euler(-90, 0, 0));
        floor.name = "Floor";
        generatedObjects.Add(floor);
        
        GameObject ceiling = CreateWall(new Vector3(0, boxSize.y, 0), new Vector3(boxSize.x, boxSize.z, 0.1f), Quaternion.Euler(90, 0, 0));
        ceiling.name = "Ceiling";
        generatedObjects.Add(ceiling);
        
        
        GameObject frontWall = CreateWall(
            new Vector3(0, boxSize.y/2, boxSize.z/2), 
            new Vector3(boxSize.x, boxSize.y, 0.1f), 
            Quaternion.Euler(-180, 0, 0)
        );
        frontWall.name = "FrontWall";
        generatedObjects.Add(frontWall);
        
        GameObject backWall = CreateWall(
            new Vector3(0, boxSize.y/2, -boxSize.z/2), 
            new Vector3(boxSize.x, boxSize.y, 0.1f), 
            Quaternion.Euler(-180, -180, 0)
        );
        backWall.name = "BackWall";
        generatedObjects.Add(backWall);
        
        GameObject leftWall = CreateWall(
            new Vector3(-boxSize.x/2, boxSize.y/2, 0), 
            new Vector3(boxSize.z, boxSize.y, 0.1f), 
            Quaternion.Euler(0, 90, 0)
        );
        leftWall.name = "LeftWall";
        generatedObjects.Add(leftWall);
        
        GameObject rightWall = CreateWall(
            new Vector3(boxSize.x/2, boxSize.y/2, 0), 
            new Vector3(boxSize.z, boxSize.y, 0.1f), 
            Quaternion.Euler(0, -90, 0)
        );
        rightWall.name = "RightWall";
        generatedObjects.Add(rightWall);
    }

    private GameObject CreateWall(Vector3 position, Vector3 size, Quaternion rotation)
    {
        GameObject wall = Instantiate(wallPrefab, position, rotation);
        wall.transform.localScale = size;
        wall.transform.parent = transform;
        return wall;
    }

    private void GenerateChargingArea()
    {
        chargingArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chargingArea.name = "ChargingArea";
        chargingArea.transform.position = chargingAreaPosition + new Vector3(0, chargingAreaSize.y/2, 0);
        chargingArea.transform.localScale = chargingAreaSize;
        chargingArea.transform.parent = transform;
        
        // Применяем переданный материал или создаем полупрозрачный красный
        Renderer renderer = chargingArea.GetComponent<Renderer>();
        if (chargingAreaMaterial != null)
        {
            renderer.material = chargingAreaMaterial;
        }
        else
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(1f, 0f, 0f, 0.3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            renderer.material = material;
        }
        
        // No collision with the area
        Collider collider = chargingArea.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
        
        generatedObjects.Add(chargingArea);
    }

    private void GenerateLoadingArea()
    {
        loadingArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
        loadingArea.name = "LoadingArea";
        loadingArea.transform.position = loadingAreaPosition + new Vector3(0, loadingAreaSize.y/2, 0);
        loadingArea.transform.localScale = loadingAreaSize;
        loadingArea.transform.parent = transform;
        
        // Применяем переданный материал или создаем полупрозрачный желтый
        Renderer renderer = loadingArea.GetComponent<Renderer>();
        if (loadingAreaMaterial != null)
        {
            renderer.material = loadingAreaMaterial;
        }
        else
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(1f, 1f, 0f, 0.3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            renderer.material = material;
        }
        
        // No collision with the area
        Collider collider = loadingArea.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
        
        generatedObjects.Add(loadingArea);
    }

    private void GenerateShelvesArea()
    {
        shelvesArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shelvesArea.name = "ShelvesArea";
        shelvesArea.transform.position = shelvesAreaPosition + new Vector3(0, 0, 0);
        shelvesArea.transform.localScale = shelvesAreaSize;
        shelvesArea.transform.parent = transform;
        
        // Применяем переданный материал или создаем полупрозрачный синий
        Renderer renderer = shelvesArea.GetComponent<Renderer>();
        if (shelvesAreaMaterial != null)
        {
            renderer.material = shelvesAreaMaterial;
        }
        else
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(0f, 0.5f, 1f, 0.2f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            renderer.material = material;
        }
        
        // No collision with the area
        Collider collider = shelvesArea.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
        
        generatedObjects.Add(shelvesArea);
    }

    private void PlaceVehicle()
    {
        if (vehiclePrefab != null && chargingArea != null)
        {
            GameObject vehicle = Instantiate(vehiclePrefab, chargingAreaPosition + new Vector3(0, 0.2f, 0), Quaternion.identity);
            vehicle.name = "Vehicle";
            vehicle.transform.parent = transform;
            generatedObjects.Add(vehicle);
        }
    }
}