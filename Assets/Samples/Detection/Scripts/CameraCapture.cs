using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class CameraCapture : MonoBehaviour
{
    public RawImage previewImage; // UI element to display camera preview
    private WebCamTexture webcamTexture;
    private string capturedImagePath;

    // Start is called before the first frame update
    void Start()
    {
        // Start the camera
        StartCamera();
    }

    void StartCamera()
    {
        webcamTexture = new WebCamTexture();
        previewImage.texture = webcamTexture;

        // Rotate the RawImage to match the device orientation
        RotatePreviewImage();

        webcamTexture.Play();
    }

    // Capture a photo from the camera
    public void CapturePhoto()
    {
        Texture2D photo = new Texture2D(webcamTexture.width, webcamTexture.height);

        // Apply rotation if needed
        if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
        {
            // Rotate the camera texture 90 degrees clockwise
            photo.SetPixels(webcamTexture.GetPixels());
            photo = RotateTexture(photo, true, false);
        }
        else
        {
            // No rotation needed
            photo.SetPixels(webcamTexture.GetPixels());
        }

        photo.Apply();

        // Convert photo to bytes
        byte[] bytes = photo.EncodeToPNG();

        // Save photo
        capturedImagePath = Path.Combine(Application.persistentDataPath, "capturedPhoto.jpg");
        File.WriteAllBytes(capturedImagePath, bytes);

        Debug.Log("Photo captured and saved at: " + capturedImagePath);
    }

    // Rotate a texture
    private Texture2D RotateTexture(Texture2D originalTexture, bool clockwise, bool flipVertically)
    {
        Texture2D rotatedTexture = new Texture2D(originalTexture.height, originalTexture.width);

        for (int x = 0; x < originalTexture.width; x++)
        {
            for (int y = 0; y < originalTexture.height; y++)
            {
                int newX = clockwise ? y : originalTexture.height - y - 1;
                int newY = clockwise ? originalTexture.width - x - 1 : x;

                if (flipVertically)
                {
                    newY = originalTexture.width - newY - 1;
                }

                rotatedTexture.SetPixel(newX, newY, originalTexture.GetPixel(x, y));
            }
        }

        rotatedTexture.Apply();
        return rotatedTexture;
    }

    // Rotate the RawImage based on the device orientation
    private void RotatePreviewImage()
    {
        Quaternion rotation = Quaternion.identity;

        switch (Screen.orientation)
        {
            case ScreenOrientation.Portrait:
                rotation = Quaternion.Euler(0, 0, -90);
                break;
            case ScreenOrientation.PortraitUpsideDown:
                rotation = Quaternion.Euler(0, 0, 90);
                break;
            case ScreenOrientation.LandscapeRight:
                rotation = Quaternion.Euler(0, 0, 180);
                break;
        }

        previewImage.rectTransform.rotation = rotation;
    }

    // Load the next scene
    public void LoadNextScene()
    {
        SceneManager.LoadScene("ViewImage");
    }

    // Get the path of the captured image
    public string GetCapturedImagePath()
    {
        return capturedImagePath;
    }
}
