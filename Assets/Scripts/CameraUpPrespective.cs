using UnityEngine;

public class CameraUpPrespective : MonoBehaviour
{
    public Camera cam;                         // Ссылка на камеру
    public Transform targetObject;             // Объект, для которого рассчитываем
    public bool fitHorizontally = true;        // true – подгоняем по ширине, false – по длине (глубине)
    public float wallHeightMargin = 0f;        // Дополнительный запас для расчета (например, высоты стен)

    void Start()
    {
        // Получаем bounding box объекта:
        Renderer rend = targetObject.GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            Debug.LogError("У объекта отсутствует Renderer для получения bounding box!");
            return;
        }
        Bounds bounds = rend.bounds;
        Vector3 objectCenter = bounds.center;
        float objectWidth = bounds.size.x;
        // По умолчанию размер по 'высоте' берется вдоль оси Z (если объект укладывается на плоскости XZ)
        float objectDepth = bounds.size.z;
        // Если требуется учитывать фактическую вертикальную размерность (например, для наклонной камеры), можно использовать bounds.size.y

        // Получаем размеры экрана (текстуры) камеры.
        float textureWidth = cam.pixelWidth;
        float textureHeight = cam.pixelHeight;
        float aspect = textureWidth / textureHeight;

        // Выбираем нужный размер L – он зависит от настройки: по ширине (X) или по глубине (Z)
        float L = fitHorizontally ? objectWidth : objectDepth;

        // Получаем вертикальный FOV камеры (в радианах)
        float vFOV = cam.fieldOfView * Mathf.Deg2Rad;

        // Если подгоняем по ширине, вычисляем горизонтальный FOV, чтобы корректно учесть соотношение сторон
        float effectiveFOV = vFOV;
        if (fitHorizontally)
        {
            effectiveFOV = 2f * Mathf.Atan(Mathf.Tan(vFOV / 2f) * aspect);
        }

        // Рассчитываем оптимальную высоту камеры.
        // Добавляя wallHeightMargin, мы гарантируем, что к половине выбранного размера (футпринта объекта)
        // прибавится запас на стены или другой дополнительный вертикальный элемент.
        float H = ((L / 2f) + wallHeightMargin) / Mathf.Tan(effectiveFOV / 2f);

        // Устанавливаем позицию и поворот камеры.
        // Предполагается, что объект расположен на плоскости XZ, а камера расположена по оси Y.
        cam.transform.position = new Vector3(objectCenter.x, H, objectCenter.z);
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Камера смотрит строго вниз

        Debug.Log("Оптимальная высота камеры: " + H + ". Дополнительный запас для стен: " + wallHeightMargin);
    }
}