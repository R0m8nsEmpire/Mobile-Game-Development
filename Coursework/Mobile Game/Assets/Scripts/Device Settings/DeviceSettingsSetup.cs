using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class DeviceSettingsSetup : MonoBehaviour
{
    [SerializeField] RectTransform canvasPanel;
    Vector2Int lastSize;
    Rect lastSafe;
    void Start()
    {
        //Set the screen orientation to portrait
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;
        //Set the min max of the canvas to the Screens safe area
        var size = new Vector2Int(Screen.width, Screen.height);
        if (Screen.safeArea == lastSafe && size == lastSize)
        {
            return;
        }
        lastSafe = Screen.safeArea;
        lastSize = size;
        Vector2 min = lastSafe.min, max = lastSafe.max;
        min.x /= size.x; min.y /= size.y;
        max.x /= size.x; max.y /= size.y;
        canvasPanel.anchorMin = min;
        canvasPanel.anchorMax = max;
        canvasPanel.offsetMin = canvasPanel.offsetMax = Vector2.zero;
    }
}
