using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CamToUI : MonoBehaviour
{
    public Image image; // Ссылка на элемент Image
    public Camera Cam; // Ссылка на камеру, с которой будем получать изображение
    // public int width { get => (int)image.GetComponent<RectTransform>().rect.size.x; } 
    public int width = 959;
    public int height = 942;
    // public int height { get => (int)image.GetComponent<RectTransform>().rect.size.y; }

    private RenderTexture renderTexture;

    void Start()
    {
        // Создаем RenderTexture
        renderTexture = new RenderTexture(width, height, 24);
        renderTexture.filterMode = FilterMode.Point;
        Cam.targetTexture = renderTexture;
        Cam.aspect = (float)width / height;

        // Создаем текстуру и устанавливаем ее в Image
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        image.sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));

        // Устанавливаем размер Image
        image.rectTransform.sizeDelta = new Vector2(width, height);
    }

    void Update()
    {
        // Считываем данные с RenderTexture в текстуру
        RenderTexture.active = renderTexture;
        Texture2D texture = (Texture2D)image.sprite.texture;
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();
        RenderTexture.active = null;
    }

    void OnDestroy()
    {
        // Освобождаем ресурсы
        if (renderTexture != null)
        {
            renderTexture.Release();
        }
    }
}
