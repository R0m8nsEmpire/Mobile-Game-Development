using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] GameObject settingsMenu;
    public void ShowSettings()
    {
        //Show the settings menu when the settings button is pushed
        settingsMenu.SetActive(true);
    }
}
