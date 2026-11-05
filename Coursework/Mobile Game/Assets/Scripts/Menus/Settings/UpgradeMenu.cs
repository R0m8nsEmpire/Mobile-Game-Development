using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
public class UpgradeMenu : MonoBehaviour
{
    [SerializeField] GameObject[] hideButtons;
    [SerializeField] GameObject upgradeMenu;
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
        //Only run if the upgrade menu is open
        if (upgradeMenu.activeSelf)
        {
            //Detect Touches
            foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
            {
                //Is the finger on the screen
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    //Is the finger not on the main UI
                    if (!RectTransformUtility.RectangleContainsScreenPoint(upgradeMenu.GetComponent<RectTransform>(), touch.screenPosition, null))
                    {
                        //Disable the upgrade Menu
                        upgradeMenu.SetActive(false);
                        //Show the UI elements behind it
                        foreach (GameObject uiElement in hideButtons)
                        {
                            uiElement.SetActive(true);
                        }
                    }
                }
            }
        }
    }
    //BUTTON SETTINGS
    //Show the Upgrade menu
    public void ShowUpgrades()
    {
        //Show the upgrade menu when the upgrade button is pushed
        upgradeMenu.SetActive(true);
        //Hide the UI elements behind it
        foreach(GameObject uiElement in hideButtons)
        {
            uiElement.SetActive(false);
        }
    }
}
