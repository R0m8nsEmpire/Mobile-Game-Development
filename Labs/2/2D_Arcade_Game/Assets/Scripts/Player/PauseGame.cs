using UnityEditor;
using UnityEngine;

public class PauseGame : MonoBehaviour
{
    public static bool paused = false;
    float timeScale;
    public void PauseButton()
    {
        if (!paused)
        {
            paused = true;
            timeScale = Time.timeScale;
            Time.timeScale = 0;
        }
        else if (paused)
        {
            paused = false;
            Time.timeScale = timeScale;
        }
        Debug.Log(timeScale);
    }


    private void Update()
    {
        Debug.Log(Time.timeScale);
    }
}
