using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class DisplayCapturedImage : MonoBehaviour
{
    public RawImage imageDisplay;
    public static string imagePath;

    // Start is called before the first frame update
    void Start()
    {
        DisplayImage();
    }

    void DisplayImage()
    {
        string directoryPath = Application.persistentDataPath;
        string fileName = "capturedPhoto.jpg";
        imagePath = Path.Combine(directoryPath, fileName);

        if (File.Exists(imagePath))
        {
            Debug.Log("Found captured image at: " + imagePath);
            byte[] imageData = File.ReadAllBytes(imagePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            imageDisplay.texture = texture;
            
        }
        else
        {
            Debug.LogWarning("Captured image not found or path is invalid: " + imagePath);
        }
    }
}
