using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System;
using System.Linq;
using UnityEngine.UIElements;

public static class WarehouseDataController
{
    const string ConfigName = "ZoneDefinitions.xml";

    public delegate void DataImpactEvent();

    public static bool Loaded = false;

    public static event DataImpactEvent DataLoaded;
    public static event DataImpactEvent DataSaved;

    private static readonly WarehouseSettings baseSettings = new WarehouseSettings()
    {
        Warehouse = new ZoneBase()
        {
            Name = "Склад",
            Color = new Color(0.2f, 0.2f, 0.2f),
            PhysicalSize = new Vector2(80, 60),
        },
        Zones = new List<ZoneMovable>()
        {
            new ZoneMovable()
            {
                Name = "Зона стоянки/зарядки",
                Color = Color.yellow,
                PhysicalSize = new Vector2(10.0f, 10.0f),
                PhysicalPosition = new Vector2(10, 10)
            },
            new ZoneMovable()
            {
                Name = "Зона погрузки/разгрузки",
                Color = Color.red,
                PhysicalSize = new Vector2(10.0f, 10.0f),
                PhysicalPosition = new Vector2(30, 10) 
            },
            new ZoneDirectional()
            {
                Name = "Зона складирования",
                Color = Color.blue, 
                PhysicalSize = new Vector2(25.0f, 20.0f), 
                PhysicalPosition = new Vector2(20, 25),
                IsVertical = true
            }
        }
    };
    private static WarehouseSettings _settings;

    public static WarehouseSettings Settings 
    {
        get => _settings;
    }

    static WarehouseDataController()
    {
        LoadSettings();
    }

    public static void LoadSettings()
    {
        Debug.Log(Application.persistentDataPath);
        var savePath = Path.Combine(Application.persistentDataPath, ConfigName);
        if (File.Exists(savePath))
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(WarehouseSettings));
                using (FileStream stream = new FileStream(savePath, FileMode.Open))
                {
                    WarehouseSettings wrapper = serializer.Deserialize(stream) as WarehouseSettings;
                    if (wrapper != null)
                    {
                        _settings = wrapper;
                        Debug.Log("Зоны загружены из XML");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Ошибка при загрузке зон: " + e.Message);
            }
        }
        else
        {
            _settings = baseSettings;
            SaveZoneDefinitions();
        }
        Loaded = true;
        DataLoaded?.Invoke();
    }

    public static void SaveZoneDefinitions()
    {
        var savePath = Path.Combine(Application.persistentDataPath, ConfigName);
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(WarehouseSettings));
            using (FileStream stream = new FileStream(savePath, FileMode.Create))
            {
                serializer.Serialize(stream, _settings);
            }
            Debug.Log("Зоны сохранены в XML: " + savePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Ошибка при сохранении зон: " + e.Message);
        }
        DataSaved?.Invoke();
    }
}

public class WhDataController : MonoBehaviour
{
    public void Awake()
    {
        if (WarehouseDataController.Loaded)
            UpdateGeneration();
        else
            WarehouseDataController.DataLoaded += UpdateGeneration;

        WarehouseDataController.DataSaved += UpdateGeneration;
    }

    public RoomGenerator RG;
    public ShelfGenerator SG;

    public void UpdateGeneration()
    {
        if (RG != null && SG != null)
        {
            var whSize = WarehouseDataController.Settings.Warehouse.PhysicalSize;
            RG.boxSize = new Vector3(whSize.x, 3, whSize.y);
            var chargingArea = WarehouseDataController.Settings.Zones.First(z => z.Name == "Зона стоянки/зарядки");
            var loadingArea = WarehouseDataController.Settings.Zones.First(z => z.Name == "Зона погрузки/разгрузки");
            var shelvesArea = WarehouseDataController.Settings.Zones.First(z => z.Name == "Зона складирования") as ZoneDirectional;
            RG.chargingAreaPosition = chargingArea.GetWorldPosition(whSize);
            RG.chargingAreaSize = chargingArea.GetWorldSize();
            RG.loadingAreaPosition = loadingArea.GetWorldPosition(whSize);
            RG.loadingAreaSize = loadingArea.GetWorldSize();
            RG.GenerateRoom();
            SG.spawnAreaOffset = shelvesArea.GetWorldPosition(whSize);
            SG.spawnAreaSize = shelvesArea.GetWorldSize();
            SG.spawnAlongX = !shelvesArea.IsVertical;
            SG.GenerateShelves();
        }
    }

    public static void OnButtonClick()
    {
        WarehouseDataController.SaveZoneDefinitions();
    }
}