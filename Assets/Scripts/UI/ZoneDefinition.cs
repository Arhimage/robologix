using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System;

[Serializable]
[XmlInclude(typeof(ZoneDirectional))]
public class Zone
{
    [SerializeField] private string _name;
    [SerializeField] private Color _color;
    [SerializeField] private Vector2 _physicalSize;
    [SerializeField] private Vector2 _physicalPosition;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            ZoneChanged?.Invoke();
        }
    }

    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            ZoneChanged?.Invoke();
        }
    }

    public Vector2 PhysicalSize
    {
        get => _physicalSize;
        set
        {
            _physicalSize = value;
            ZoneChanged?.Invoke();
        }
    }

    public Vector2 PhysicalPosition
    {
        get => _physicalPosition;
        set
        {
            _physicalPosition = value;
            ZoneChanged?.Invoke();
        }
    }

    public void Update()
    {
        ZoneChanged?.Invoke();
    }

    [XmlIgnore]
    [NonSerialized]
    public ZoneChangedEvent ZoneChanged;
}

[Serializable]
public class ZoneDirectional : Zone
{
    private bool _isVertical;

    public bool IsVertical
    {
        get => _isVertical;
        set
        {
            _isVertical = value;
            ZoneChanged?.Invoke();
        }
    }
}

[Serializable]
public class WarehouseSettings
{
    public Zone Warehouse;
    public List<Zone> Zones = new List<Zone>();
}

public delegate void ZoneChangedEvent();

public static class WarehouseDataController
{
    const string ConfigName = "ZoneDefinitions.xml";

    private static readonly WarehouseSettings baseSettings = new WarehouseSettings()
    {
        Warehouse = new Zone()
        {
            Name = "Склад",
            Color = new Color(0.2f, 0.2f, 0.2f),
            PhysicalSize = new Vector2(80, 60),
            PhysicalPosition = new Vector2(0, 0)
        },
        Zones = new List<Zone>()
        {
            new Zone()
            {
                Name = "Зона стоянки/зарядки",
                Color = Color.yellow,
                PhysicalSize = new Vector2(10.0f, 10.0f),
                PhysicalPosition = new Vector2(10, 10)
            },
            new Zone()
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
        // Debug.Log(Application.persistentDataPath);
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