using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFitting : MonoBehaviour
{
    public Camera cam;                    // Ссылка на камеру
    public Transform targetObject;        // Объект, для которого рассчитываем
    public bool fitHorizontally = true;   // true - подгоняем по ширине, false - по высоте

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
        float objectHeight = bounds.size.z; // если объект располагается на плоскости X-Z, то «высота» для просмотра сверху может быть по Z
        // Если же требуется использовать размеры Y, измените выбор соответствующим образом.

        // Получаем размеры текстуры (экран) камеры.
        // Обычно aspect ratio камеры можно взять напрямую.
        float textureWidth = cam.pixelWidth;
        float textureHeight = cam.pixelHeight;
        float aspect = textureWidth / textureHeight;

        // Выбираем нужный размер L:
        float L = fitHorizontally ? objectWidth : objectHeight;

        // Получаем вертикальный FOV камеры (в радианах)
        float vFOV = cam.fieldOfView * Mathf.Deg2Rad;

        // Если нужно вписать выбранную сторону горизонтально, вычисляем горизонтальный FOV:
        float effectiveFOV = vFOV;
        if (fitHorizontally)
        {
            // Вычисляем горизонтальный FOV
            effectiveFOV = 2f * Mathf.Atan(Mathf.Tan(vFOV / 2f) * aspect);
        }
        // Рассчитываем оптимальную высоту:
        float H = (L / 2f) / Mathf.Tan(effectiveFOV / 2f);

        // Устанавливаем позицию и поворот камеры.
        // Предполагается, что объект лежит на плоскости XZ, а камера расположена по оси Y.
        cam.transform.position = new Vector3(objectCenter.x, H + 0.5f, objectCenter.z);
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // камера смотрит строго вниз

        Debug.Log("Оптимальная высота камеры: " + H);
    }
}
