using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System;

public static class WarehouseDataController
{
    const string ConfigName = "ZoneDefinitions.xml";

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
    }
}

public class WhDataControllerWrapper : MonoBehaviour
{
    public static void OnButtonClick()
    {
        WarehouseDataController.SaveZoneDefinitions();
    }
}