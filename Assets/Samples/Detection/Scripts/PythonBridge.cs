using UnityEngine;
using UnityEngine.UI;
using TensorFlowLite;
using System.IO;
using System.Linq;
using UnityEngine.Networking;
using System.Collections;

public class ImageClassifier : MonoBehaviour
{
    [SerializeField] private RawImage rawImage; // Reference to the RawImage element
    [SerializeField] private Text resultText; // Reference to the UI Text element
    [SerializeField] private GameObject boxPrefab; // Reference to the box prefab for highlighting
    [SerializeField] private RectTransform canvasRect; // Reference to the canvas rect transform
    [SerializeField] private string modelFileName = "project.tflite";
    [SerializeField] private string labelFileName = "class_labels.txt";
    [SerializeField] private Texture2D inputImage;

    private Interpreter interpreter;
    private string[] labels;
    private float[] imgData;
    private float[,] output;

    private GameObject boxObject;

    void Start()
    {
        string modelPath = Path.Combine(Application.streamingAssetsPath, modelFileName);
        string labelPath = Path.Combine(Application.streamingAssetsPath, labelFileName);

        Debug.Log("Model path: " + modelPath);
        Debug.Log("Label path: " + labelPath);

        if (!LoadModelAndLabels(modelPath, labelPath))
        {
            Debug.LogError("Failed to load model and labels.");
            return;
        }

        // Load image from previous scene and preprocess it
        string imagePath = DisplayCapturedImage.imagePath;
        Debug.Log("Image path: " + imagePath);
        if (!string.IsNullOrEmpty(imagePath))
        {
            StartCoroutine(LoadImageAndRunInference(imagePath));
        }
        else
        {
            Debug.LogError("Image path is not set or is empty.");
        }
    }

    private bool LoadModelAndLabels(string modelPath, string labelPath)
    {
        Debug.Log("Loading model from path: " + modelPath);
        if (File.Exists(modelPath))
        {
            byte[] modelData = File.ReadAllBytes(modelPath);
            if (modelData == null || modelData.Length == 0)
            {
                Debug.LogError("Model data is null or empty");
                return false;
            }
            interpreter = new Interpreter(modelData);
            interpreter.AllocateTensors();
        }
        else
        {
            Debug.LogError("Model file not found at " + modelPath);
            return false;
        }

        Debug.Log("Loading labels from path: " + labelPath);
        if (File.Exists(labelPath))
        {
            labels = File.ReadAllLines(labelPath)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
        }
        else
        {
            Debug.LogError("Label file not found at " + labelPath);
            return false;
        }

        Debug.Log("Model and labels loaded successfully");
        int outputCount = interpreter.GetOutputTensorInfo(0).shape[1];
        output = new float[1, outputCount];
        return true;
    }

    IEnumerator LoadImageAndRunInference(string imagePath)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + imagePath);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            rawImage.texture = texture; // Display the input image
            PreprocessImage(texture);
            RunInference();
        }
    }

    void PreprocessImage(Texture2D image)
    {
        // Resize image to the model input size
        Texture2D resizedImage = Resize(image, 224, 224);

        int height = resizedImage.height;
        int width = resizedImage.width;
        int channels = 3; // Assuming the model expects a 3-channel RGB image

        imgData = new float[height * width * channels]; // Flatten the image data array

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = resizedImage.GetPixel(x, y);

                // Normalize pixel values to [0, 1] if needed
                int i = (y * width + x) * channels;
                imgData[i + 0] = color.r;
                imgData[i + 1] = color.g;
                imgData[i + 2] = color.b;
            }
        }

        // Dispose of the resized image to free up memory
        Destroy(resizedImage);
    }

    Texture2D Resize(Texture2D source, int newWidth, int newHeight)
    {
        source.filterMode = FilterMode.Bilinear;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Bilinear;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D result = new Texture2D(newWidth, newHeight);
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    void RunInference()
    {
        interpreter.SetInputTensorData(0, imgData);
        interpreter.Invoke();

        float[] outputData = new float[output.GetLength(1)]; // Flatten the output array
        interpreter.GetOutputTensorData(0, outputData);

        int predictedIndex = GetPredictedIndex(outputData);
        string predictedLabel = labels[predictedIndex];
        Debug.Log($"Predicted class: {predictedLabel}");

        // Update the UI Text element with the predicted label
        resultText.text = $"Predicted class: {predictedLabel}";

        // Debug statement to show the predicted label in the Android log
        Debug.Log("Predicted class: " + predictedLabel);

        // Create and position the box object to cover the image
        if (boxObject != null)
        {
            Destroy(boxObject);
        }
        boxObject = Instantiate(boxPrefab, canvasRect);
        boxObject.GetComponentInChildren<Text>().text = predictedLabel;
        boxObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        boxObject.GetComponent<RectTransform>().sizeDelta = canvasRect.sizeDelta;
    }

    int GetPredictedIndex(float[] probabilities)
    {
        int predictedIndex = 0;
        float maxProbability = probabilities[0];

        for (int i = 1; i < probabilities.Length; i++)
        {
            if (probabilities[i] > maxProbability)
            {
                maxProbability = probabilities[i];
                predictedIndex = i;
            }
        }

        return predictedIndex;
    }

    void OnDestroy()
    {
        if (interpreter != null)
        {
            interpreter.Dispose();
            interpreter = null;
        }
    }
}
