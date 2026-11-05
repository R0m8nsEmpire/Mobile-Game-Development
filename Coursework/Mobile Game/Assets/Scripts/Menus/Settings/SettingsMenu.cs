using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
public class SettingsMenu : MonoBehaviour
{
    [SerializeField] GameObject settingsMenu;
    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }
    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }
    private void Update()
    {
        //Only run if the settings menu is open
        if(settingsMenu.activeSelf)
        {
            //Detect Touches
            foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
            {
                //Is the finger on the screen
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    //Is the finger not on the main UI
                    if(!RectTransformUtility.RectangleContainsScreenPoint(settingsMenu.GetComponent<RectTransform>(), touch.screenPosition, null))
                    {
                        //Disable the Settings Menu
                        settingsMenu.SetActive(false);
                    }
                }
            }
        }
    }
    //BUTTON SETTINGS
    //Show the settings menu
    public void ShowSettings()
    {
        //Show the settings menu when the settings button is pushed
        settingsMenu.SetActive(true);
    }
    //Toggle phone vibration
    public void ToggleVibration()
    {

    }
    //Toggle Audio
    public void ToggleAudio()
    {

    }
    //Reset Phone Gyro
    public void ReserGyro()
    {

    }
}
