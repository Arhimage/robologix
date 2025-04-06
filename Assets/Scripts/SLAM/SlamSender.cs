using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json;
using System.Globalization;

public class StereoSlamSender : MonoBehaviour
{
    public GameObject robot;
    public Camera LeftCamera;
    public Camera RightCamera;
    public RawImage slamMapImage;
    public RawImage processedLeftImage;
    public RawImage processedRightImage;
    public Text statusText;

    private Vector2 robotPostionNext;
    private Vector3 robotRotationNext;
    private bool isSlammingComplete = false;
    private const string serverAddress = "http://localhost:5002/api/slam/processstereoframe";

    public byte[] CaptureCameraImage(Camera camera)
    {
        if (camera == null) return null;
        
        RenderTexture rt = camera.targetTexture;
        camera.Render();
        RenderTexture.active = rt;

        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        RenderTexture.active = null;

        return texture.EncodeToPNG();
    }

    public Texture2D GetTextureFromBytes(byte[] imageData)
    {
        if (imageData == null || imageData.Length == 0)
        {
            Debug.LogWarning("Получены пустые или недействительные данные изображения");
            return null;
        }
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        try
        {
            bool isLoaded = texture.LoadImage(imageData);
            if (!isLoaded)
            {
                Debug.LogError("Ошибка загрузки изображения из байтов");
                return null;
            }
            texture.Apply();
            return texture;
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка при обработке изображения: {e.Message}");
            return null;
        }
    }

    public void Send()
    {
        _ = SendStereoFrameAsync();
    }

    public async Task SendStereoFrameAsync()
    {
        byte[] leftImageData = CaptureCameraImage(LeftCamera);
        byte[] rightImageData = CaptureCameraImage(RightCamera);

        if (leftImageData == null || rightImageData == null)
        {
            Debug.LogError("Одно или оба изображения не были захвачены. Отмена отправки.");
            return;
        }

        WWWForm form = new WWWForm();

        // Позиция и ориентация робота
        form.AddField("robotX", robot.transform.position.x.ToString("F6"));
        form.AddField("robotY", robot.transform.position.z.ToString("F6"));
        form.AddField("robotAngleX", robot.transform.rotation.eulerAngles.x.ToString("F6"));
        form.AddField("robotAngleY", robot.transform.rotation.eulerAngles.y.ToString("F6"));
        form.AddField("robotAngleZ", robot.transform.rotation.eulerAngles.z.ToString("F6"));

        // Параметры левой камеры
        form.AddField("leftCameraOffsetX", LeftCamera.transform.localPosition.x.ToString("F6"));
        form.AddField("leftCameraOffsetY", LeftCamera.transform.localPosition.y.ToString("F6"));
        form.AddField("leftCameraOffsetZ", LeftCamera.transform.localPosition.z.ToString("F6"));
        form.AddField("leftCameraFocalLength", LeftCamera.focalLength.ToString("F6"));
        form.AddField("leftCameraPrincipalPointX", (LeftCamera.pixelWidth / 2).ToString("F6"));
        form.AddField("leftCameraPrincipalPointY", (LeftCamera.pixelHeight / 2).ToString("F6"));

        // Параметры правой камеры
        form.AddField("rightCameraOffsetX", RightCamera.transform.localPosition.x.ToString("F6"));
        form.AddField("rightCameraOffsetY", RightCamera.transform.localPosition.y.ToString("F6"));
        form.AddField("rightCameraOffsetZ", RightCamera.transform.localPosition.z.ToString("F6"));
        form.AddField("rightCameraFocalLength", RightCamera.focalLength.ToString("F6"));
        form.AddField("rightCameraPrincipalPointX", (RightCamera.pixelWidth / 2).ToString("F6"));
        form.AddField("rightCameraPrincipalPointY", (RightCamera.pixelHeight / 2).ToString("F6"));

        // Добавление изображений
        form.AddBinaryData("leftImageData", leftImageData, "left.png", "image/png");
        form.AddBinaryData("rightImageData", rightImageData, "right.png", "image/png");

        using (UnityWebRequest request = UnityWebRequest.Post(serverAddress, form))
        {
            request.timeout = 30;
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Ошибка запроса: {request.error}\nДетали: {request.downloadHandler?.text}");
                return;
            }

            try
            {
                var response = JsonConvert.DeserializeObject<StereoFrameResponse>(request.downloadHandler.text);
                if (response != null)
                {
                    robotPostionNext = new Vector2(response.NextX, response.NextY);
                    robotRotationNext = new Vector3(response.NextAngleX, response.NextAngleY, response.NextAngleZ);
                    robot.transform.position = new Vector3(robotPostionNext.x, robot.transform.position.y, robotPostionNext.y);
                    robot.transform.rotation = Quaternion.Euler(robotRotationNext);
                    
                    isSlammingComplete = response.IsSlammingComplete;
                    if (statusText != null)
                        statusText.text = isSlammingComplete ? "SLAM завершен" : "SLAM в процессе";
                    
                    Debug.Log(isSlammingComplete ? "SLAM завершен" : "SLAM в процессе");

                    if (response.SlamMapImage != null && response.SlamMapImage.Length > 0)
                    {
                        var slamMap = GetTextureFromBytes(response.SlamMapImage);
                        if (slamMap != null && slamMapImage != null) 
                            slamMapImage.texture = slamMap;
                    }

                    if (response.ProcessedLeftImage != null && response.ProcessedLeftImage.Length > 0)
                    {
                        var leftImg = GetTextureFromBytes(response.ProcessedLeftImage);
                        if (leftImg != null && processedLeftImage != null) 
                            processedLeftImage.texture = leftImg;
                    }

                    if (response.ProcessedRightImage != null && response.ProcessedRightImage.Length > 0)
                    {
                        var rightImg = GetTextureFromBytes(response.ProcessedRightImage);
                        if (rightImg != null && processedRightImage != null) 
                            processedRightImage.texture = rightImg;
                    }
                }
                else
                {
                    Debug.LogError("Не удалось десериализовать ответ сервера");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Ошибка обработки ответа: {e.Message}\nОтвет сервера: {request.downloadHandler.text}");
            }
        }
    }
}

[Serializable]
public class StereoFrameResponse
{
    public byte[] ProcessedLeftImage;
    public byte[] ProcessedRightImage;
    public byte[] SlamMapImage;
    public float NextX;
    public float NextY;
    public float NextAngleX;
    public float NextAngleY;
    public float NextAngleZ;
    public bool IsSlammingComplete;
}