using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
public class UpgradeMenu : MonoBehaviour
{
    //Buttons
    [SerializeField] GameObject[] hideButtons;
    [SerializeField] GameObject upgradeMenu;
    //DEFENCES
    public List<GameObject> shieldDefences = new List<GameObject>();
    //Button GameObjects
    private GameObject upgradeButton;

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
    //DEFENCE UPGRADE
    public void ShieldDefence(GameObject button)
    {
        //Set shield defences active to 0
        int shieldDefencesActive = 0;
        //Set the upgrade button to the button that triggered the function
        if(upgradeButton == null)
        {
            upgradeButton = button;
        }
        //Iterate through the shield defences
        for (int i = 0; i < shieldDefences.Count; i++)
        {
            //Add 1 to how many is active
            shieldDefencesActive++;
            //If they are all active, then set the button color to red
            if (shieldDefencesActive == shieldDefences.Count)
            {
                upgradeButton.GetComponent<Image>().color = Color.red;
            }
            //If the upgrade isn't active, then activate it
            if (!shieldDefences[i].activeInHierarchy)
            {
                shieldDefences[i].SetActive(true);
                //Return to stop the iterate loop going through them all
                return;
            }
        }
    }
}
