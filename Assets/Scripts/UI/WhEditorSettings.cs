using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

public class WHEditorSettings : MonoBehaviour
{
    private bool showSettings = false;
    private Vector2 scrollPosition;
    static float ExtractDigit (string input)
    {
        string temp = Regex.Replace(input, @"[^0-9.,]", "");
        if (temp == "." || temp == ",")
            return 0;
        int sepIndex = -1;
        for (int i = 0; i < temp.Length; i++)
        {
            if (temp[i] == '.' || temp[i] == ',')
            {
                sepIndex = i;
                break;
            }
        }
        StringBuilder sb = new StringBuilder();
        if (sepIndex != -1)
        {
            sb.Append(temp.Substring(0, sepIndex + 1));
            for (int i = sepIndex + 1; i < temp.Length; i++)
            {
                if (temp[i] != '.' && temp[i] != ',')
                    sb.Append(temp[i]);
            }
        }
        else
        {
            sb.Append(temp);
        }
        string result = sb.ToString().Replace(',', '.');
        if (result == "." || result == "," || string.IsNullOrEmpty(result))
            return 0;
        if (result[0] == '.' || result[0] == ',')
            result = '0' + result;
        return float.Parse(result, NumberStyles.Float, CultureInfo.InvariantCulture);
    }
    List<object> tempSettings;

    public void ShowSettings()
    {
        tempSettings = new List<object>()
        {
            WarehouseDataController.Settings.Warehouse.Clone()
        };
        WarehouseDataController.Settings.Zones.ForEach(a => tempSettings.Add(a.Clone()));
        showSettings = true;
    }

    private void SaveSettings()
    {
        var wh = (ZoneBase)tempSettings.First(a => ((ZoneBase)a).Name == WarehouseDataController.Settings.Warehouse.Name);
        WarehouseDataController.Settings.Warehouse.Fill(wh);
        tempSettings.Remove(wh);
        tempSettings.ForEach(a =>
        {
            var zone = (ZoneMovable)a;
            var saveZone = WarehouseDataController.Settings.Zones.First(b => b.Name == zone.Name);
            if (zone is ZoneDirectional z)
                (saveZone as ZoneDirectional).Fill(z);
            else
                saveZone.Fill(zone);
        });
        WarehouseDataController.SaveZoneDefinitions();
        tempSettings.Clear();
    }

    private void OnGUI()
    {
        if (!showSettings) return;

        GUI.color = new Color(1f, 1f, 1f, 1f);
        
        // Создаем окно в центре экрана
        int windowWidth = 350;
        int windowHeight = 500;
        int leftPosition = (Screen.width - windowWidth) / 2;
        int topPosition = (Screen.height - windowHeight) / 2;
        
        // Начало окна
        GUI.Window(0, new Rect(leftPosition, topPosition, windowWidth, windowHeight), DrawSettingsWindow, "Настройки объектов");
    }

    private void DrawZone(ZoneDirectional zone)
    {
        zone.IsVertical = GUILayout.Toggle(zone.IsVertical, "Вертикальное положение");
        DrawZone(zone as ZoneMovable);

        GUILayout.Space(15);
    }

    private void DrawZone(ZoneMovable zone)
    {
        GUILayout.Label("Позиция:");
        GUILayout.BeginHorizontal();
        Vector2 physPos = new Vector2(0, 0);
        GUILayout.Label("X:", GUILayout.Width(20));
        physPos.x = ExtractDigit(GUILayout.TextField(zone.PhysicalPosition.x.ToString("F2"), GUILayout.Width(70)));
        GUILayout.Label("Y:", GUILayout.Width(20));
        physPos.y = ExtractDigit(GUILayout.TextField(zone.PhysicalPosition.y.ToString("F2"), GUILayout.Width(70)));
        GUILayout.EndHorizontal();
        DrawZone(zone as ZoneBase);
        
        GUILayout.Space(15);
    }

    private void DrawZone(ZoneBase zone)
    {
        GUILayout.Label("Размер:");
        GUILayout.BeginHorizontal();
        GUILayout.Label("X:", GUILayout.Width(20));
        Vector2 physSize = new Vector2(0, 0);
        physSize.x = ExtractDigit(GUILayout.TextField(zone.PhysicalSize.x.ToString("F2"), GUILayout.Width(70)));
        GUILayout.Label("Y:", GUILayout.Width(20));
        physSize.y = ExtractDigit(GUILayout.TextField(zone.PhysicalSize.y.ToString("F2"), GUILayout.Width(70)));
        zone.PhysicalSize = physSize;
        GUILayout.EndHorizontal();

        GUILayout.Space(15);
    }
    
    // Метод для отрисовки содержимого окна
    private void DrawSettingsWindow(int windowID)
    {
        // Начало области прокрутки
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Space(10);

        foreach (ZoneBase zone in tempSettings)
        {
            GUILayout.Label(zone.Name, GUI.skin.box);

            switch (zone)
            {
                case ZoneDirectional directional:
                    DrawZone(directional);
                    break;
                case ZoneMovable movable:
                    DrawZone(movable);
                    break;
                case ZoneBase baseZone:
                    DrawZone(baseZone);
                    break;
                default:
                    Debug.LogError($"Неопределенный тип зоны: + {zone.GetType()}");
                    break;
            }
        }
        
        GUILayout.Space(20);
        
        // Кнопки
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Сохранить", GUILayout.Height(30)))
        {
            SaveSettings();
            showSettings = false;
        }
        
        if (GUILayout.Button("Отменить", GUILayout.Height(30)))
        {
            showSettings = false;
        }
        
        GUILayout.EndHorizontal();
        
        GUILayout.EndScrollView();
    }
}
