using UnityEngine;

public class CameraUpOrthagonal : MonoBehaviour
{
    public Camera cam;                         // Ссылка на камеру
    public Transform targetObject;             // Объект, для которого рассчитываем
    public bool fitHorizontally = true;        // true – подгоняем по ширине, false – по длине (глубине)
    public float wallHeightMargin = 0f;        // Дополнительный запас для расчета

    void Start()
    {
        // Переключаем камеру в ортографический режим
        cam.orthographic = true;
        
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
        float objectDepth = bounds.size.z;
        float objectHeight = bounds.size.y;

        // Получаем соотношение сторон камеры
        float aspect = (float)cam.pixelWidth / cam.pixelHeight;

        // Определяем нужный размер orthographicSize
        if (fitHorizontally)
        {
            // Если подгоняем по ширине
            float sizeForWidth = (objectWidth / 2f) / aspect;
            
            // При взгляде сверху вниз, глубина (Z) соответствует высоте на экране
            float sizeForDepth = objectDepth / 2f;
            
            // Выбираем большее значение, чтобы объект полностью помещался
            cam.orthographicSize = Mathf.Max(sizeForWidth, sizeForDepth) + wallHeightMargin;
        }
        else
        {
            // Если подгоняем по глубине
            float sizeForDepth = objectDepth / 2f;
            
            // При взгляде сверху вниз, ширина (X) соответствует ширине на экране с учетом aspect
            float sizeForWidth = (objectWidth / 2f) / aspect;
            
            // Выбираем большее значение, чтобы объект полностью помещался
            cam.orthographicSize = Mathf.Max(sizeForWidth, sizeForDepth) + wallHeightMargin;
        }

        // Устанавливаем позицию и поворот камеры
        // Обязательно устанавливаем камеру выше самой высокой точки объекта
        float cameraHeight = objectCenter.y + (objectHeight / 2f) + 10f; // Добавляем запас
        
        cam.transform.position = new Vector3(objectCenter.x, cameraHeight, objectCenter.z);
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Камера смотрит строго вниз
        
        // Настраиваем дальнюю плоскость отсечения, чтобы обеспечить видимость объекта
        cam.farClipPlane = objectHeight + 20f;

        Debug.Log("Ортографический размер камеры: " + cam.orthographicSize + 
                  ". Высота объекта: " + objectHeight +
                  ". Позиция камеры по Y: " + cameraHeight);
    }
}