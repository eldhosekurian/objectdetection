using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    public void StartUpPage()
    {
        SceneManager.LoadScene("StartUpPage");
    }

    public void HomePage()
    {
        SceneManager.LoadScene("HomePage");
    }
    public void CameraPage()
    {
        SceneManager.LoadScene("Camera");
    }
    public void PredictPage()
    {
        SceneManager.LoadScene("Prediction");
    }

}
