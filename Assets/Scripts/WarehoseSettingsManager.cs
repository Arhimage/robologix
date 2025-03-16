// using UnityEngine;

// // Класс для игрового режима
// public class WarehoseSettingsManager : MonoBehaviour
// {
//     // Singleton для доступа к менеджеру из любого места
//     public static WarehoseSettingsManager Instance { get; private set; }

//     public class ZoneBase
    
//     // Данные для хранения настроек
//     [System.Serializable]
//     public class ObjectSettings
//     {
//         public Vector3 object1Position;
//         public Vector3 object2Position;
//         public Vector3 object3Position;
//         public Vector3 object1Size;
//         public Vector3 object2Size;
//         public Vector3 object3Size;
//         public bool enableObject1;
//     }
    
//     // Текущие настройки
//     public static ObjectSettings CurrentSettings = new ObjectSettings();
    
//     // Временные настройки для редактирования
//     private ObjectSettings tempSettings = new ObjectSettings();
    
//     // Состояние окна
//     private bool showSettings = false;
    
//     // Прокрутка для окна
//     private Vector2 scrollPosition;
    
//     // Инициализация синглтона
//     private void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject); // Опционально, если нужно сохранять между сценами
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }
    
//     // Публичный статический метод для показа окна настроек
//     // Может быть вызван из любого места: WarehoseSettingsManager.ShowSettings()
//     public static void ShowSettings()
//     {
//         if (Instance != null)
//         {
//             Instance.ShowSettingsWindow();
//         }
//         else
//         {
//             Debug.LogError("WarehoseSettingsManager не найден в сцене!");
//         }
//     }
    
//     // Приватный метод для показа окна настроек
//     private void ShowSettingsWindow()
//     {
//         // Копируем текущие настройки во временные
//         tempSettings.object1Position = CurrentSettings.object1Position;
//         tempSettings.object2Position = CurrentSettings.object2Position;
//         tempSettings.object3Position = CurrentSettings.object3Position;
//         tempSettings.object1Size = CurrentSettings.object1Size;
//         tempSettings.object2Size = CurrentSettings.object2Size;
//         tempSettings.object3Size = CurrentSettings.object3Size;
//         tempSettings.enableObject1 = CurrentSettings.enableObject1;
        
//         // Открываем окно
//         showSettings = true;
//     }
    
//     // Метод для отрисовки GUI
//     private void OnGUI()
//     {
//         if (!showSettings) return;

//         GUI.color = new Color(1f, 1f, 1f, 1f);
        
//         // Создаем окно в центре экрана
//         int windowWidth = 350;
//         int windowHeight = 500;
//         int leftPosition = (Screen.width - windowWidth) / 2;
//         int topPosition = (Screen.height - windowHeight) / 2;
        
//         // Начало окна
//         GUI.Window(0, new Rect(leftPosition, topPosition, windowWidth, windowHeight), DrawSettingsWindow, "Настройки объектов");
//     }
    
//     // Метод для отрисовки содержимого окна
//     private void DrawSettingsWindow(int windowID)
//     {
//         // Начало области прокрутки
//         scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        
//         GUILayout.Space(10);
        
//         // Первый объект
//         GUILayout.Label("Зона складирования", GUI.skin.box);
//         tempSettings.enableObject1 = GUILayout.Toggle(tempSettings.enableObject1, "Активировать");
        
//         // Позиция для первого объекта
//         GUILayout.Label("Позиция:");
//         GUILayout.BeginHorizontal();
//         GUILayout.Label("X:", GUILayout.Width(20));
//         tempSettings.object1Position.x = float.Parse(GUILayout.TextField(tempSettings.object1Position.x.ToString(), GUILayout.Width(70)));
//         GUILayout.Label("Y:", GUILayout.Width(20));
//         tempSettings.object1Position.y = float.Parse(GUILayout.TextField(tempSettings.object1Position.y.ToString(), GUILayout.Width(70)));
//         GUILayout.Label("Z:", GUILayout.Width(20));
//         tempSettings.object1Position.z = float.Parse(GUILayout.TextField(tempSettings.object1Position.z.ToString(), GUILayout.Width(70)));
//         GUILayout.EndHorizontal();
        
//         // Размер для первого объекта
//         GUILayout.Label("Размер:");
//         GUILayout.BeginHorizontal();
//         GUILayout.Label("X:", GUILayout.Width(20));
//         tempSettings.object1Size.x = float.Parse(GUILayout.TextField(tempSettings.object1Size.x.ToString(), GUILayout.Width(70)));
//         GUILayout.Label("Y:", GUILayout.Width(20));
//         tempSettings.object1Size.y = float.Parse(GUILayout.TextField(tempSettings.object1Size.y.ToString(), GUILayout.Width(70)));
//         GUILayout.Label("Z:", GUILayout.Width(20));
//         tempSettings.object1Size.z = float.Parse(GUILayout.TextField(tempSettings.object1Size.z.ToString(), GUILayout.Width(70)));
//         GUILayout.EndHorizontal();
        
//         GUILayout.Space(15);
        
//         // Второй объект
//         GUILayout.Label("Зона стоянки/зарядки", GUI.skin.box);
        
//         // Позиция для второго объекта
//         GUILayout.Label("Позиция:");
//         GUILayout.BeginHorizontal();
//         GUILayout.Label("X:", GUILayout.Width(20));
//         tempSettings.object2Position.x = float.Parse(GUILayout.TextField(tempSettings.object2Position.x.ToString(), GUILayout.Width(70)));
//         GUILayout.Label("Y:", GUILayout.Width(20));
//         tempSettings.object2Position.y = float.Parse(GUILayout.TextField(tempSettings.object2Position.y.ToString(), GUILayout.Width(70)));
//         GUILayout.Label("Z:", GUILayout.Width(20));
//         tempSettings.object2Position.z = float.Parse(GUILayout.TextField(tempSettings.object2Position.z.ToString(), GUILayout.Width(70)));
//         GUILayout.EndHorizontal();
        
//         // Размер для второго объекта
//         GUILayout.Label("Размер:");
//         GUILayout.BeginHorizontal();
//         GUILayout.Label("X:", GUILayout.Width(20));
//         tempSettings.object2Size.x = float.Parse(GUILayout.TextField(tempSettings.object2Size.x.ToString(), GUILayout.Width(70)));
//         GUILayout.Label("Y:", GUILayout.Width(20));
//         tempSettings.object2Size.y = float.Parse(GUILayout.TextField(tempSettings.object2Size.y.ToString(), GUILayout.Width(70)));
//         GUILayout.Label("Z:", GUILayout.Width(20));
//         tempSettings.object2Size.z = float.Parse(GUILayout.TextField(tempSettings.object2Size.z.ToString(), GUILayout.Width(70)));
//         GUILayout.EndHorizontal();
        
//         GUILayout.Space(15);
        
//         // Третий объект
//         GUILayout.Label("Зона погрузки/разгрузки", GUI.skin.box);
        
//         // Позиция для третьего объекта
//         GUILayout.Label("Позиция:");
//         GUILayout.BeginHorizontal();
//         GUILayout.Label("X:", GUILayout.Width(20));
//         tempSettings.object3Position.x = float.Parse(GUILayout.TextField(tempSettings.object3Position.x.ToString(), GUILayout.Width(70)));
//         GUILayout.Label("Y:", GUILayout.Width(20));
//         tempSettings.object3Position.y = float.Parse(GUILayout.TextField(tempSettings.object3Position.y.ToString(), GUILayout.Width(70)));
//         GUILayout.Label("Z:", GUILayout.Width(20));
//         tempSettings.object3Position.z = float.Parse(GUILayout.TextField(tempSettings.object3Position.z.ToString(), GUILayout.Width(70)));
//         GUILayout.EndHorizontal();
        
//         // Размер для третьего объекта
//         GUILayout.Label("Размер:");
//         GUILayout.BeginHorizontal();
//         GUILayout.Label("X:", GUILayout.Width(20));
//         tempSettings.object3Size.x = float.Parse(GUILayout.TextField(tempSettings.object3Size.x.ToString(), GUILayout.Width(70)));
//         GUILayout.Label("Y:", GUILayout.Width(20));
//         tempSettings.object3Size.y = float.Parse(GUILayout.TextField(tempSettings.object3Size.y.ToString(), GUILayout.Width(70)));
//         GUILayout.Label("Z:", GUILayout.Width(20));
//         tempSettings.object3Size.z = float.Parse(GUILayout.TextField(tempSettings.object3Size.z.ToString(), GUILayout.Width(70)));
//         GUILayout.EndHorizontal();
        
//         GUILayout.Space(20);
        
//         // Кнопки
//         GUILayout.BeginHorizontal();
        
//         if (GUILayout.Button("Сохранить", GUILayout.Height(30)))
//         {
//             SaveSettings();
//         }
        
//         if (GUILayout.Button("Отменить", GUILayout.Height(30)))
//         {
//             CancelChanges();
//         }
        
//         GUILayout.EndHorizontal();
        
//         GUILayout.EndScrollView();
//     }
    
//     // Метод для сохранения настроек
//     private void SaveSettings()
//     {
//         // Копируем временные настройки в текущие
//         CurrentSettings.object1Position = tempSettings.object1Position;
//         CurrentSettings.object2Position = tempSettings.object2Position;
//         CurrentSettings.object3Position = tempSettings.object3Position;
//         CurrentSettings.object1Size = tempSettings.object1Size;
//         CurrentSettings.object2Size = tempSettings.object2Size;
//         CurrentSettings.object3Size = tempSettings.object3Size;
//         CurrentSettings.enableObject1 = tempSettings.enableObject1;
        
//         // Применяем настройки к объектам
//         ApplySettingsToObjects();
        
//         // Закрываем окно
//         showSettings = false;
        
//         Debug.Log("Настройки сохранены");
//         Debug.Log($"Зона складирования: {(CurrentSettings.enableObject1 ? "Активен" : "Неактивен")}, " +
//                   $"Позиция: {CurrentSettings.object1Position}, Размер: {CurrentSettings.object1Size}");
//         Debug.Log($"Зона стоянки/зарядки: Позиция: {CurrentSettings.object2Position}, Размер: {CurrentSettings.object2Size}");
//         Debug.Log($"Зона погрузки/разгрузки: Позиция: {CurrentSettings.object3Position}, Размер: {CurrentSettings.object3Size}");
//     }
    
//     // Метод для отмены изменений
//     private void CancelChanges()
//     {
//         // Просто закрываем окно без сохранения
//         showSettings = false;
//         Debug.Log("Изменения отменены");
//     }
    
//     // Метод для применения настроек к объектам
//     private void ApplySettingsToObjects()
//     {
//         // Здесь идет логика применения настроек к игровым объектам
//         // Например:
        
//         // GameObject object1 = GameObject.Find("Object1");
//         // if (object1 != null)
//         // {
//         //     object1.transform.position = CurrentSettings.object1Position;
//         //     object1.transform.localScale = CurrentSettings.object1Size;
//         //     object1.SetActive(CurrentSettings.enableObject1);
//         // }
        
//         // Аналогично для других объектов
//     }
    
//     // Инициализация значений по умолчанию
//     private void Start()
//     {
//         // Устанавливаем начальные значения
//         CurrentSettings.object1Position = Vector3.zero;
//         CurrentSettings.object2Position = Vector3.zero;
//         CurrentSettings.object3Position = Vector3.zero;
//         CurrentSettings.object1Size = Vector3.one;
//         CurrentSettings.object2Size = Vector3.one;
//         CurrentSettings.object3Size = Vector3.one;
//         CurrentSettings.enableObject1 = false;
//     }
// }