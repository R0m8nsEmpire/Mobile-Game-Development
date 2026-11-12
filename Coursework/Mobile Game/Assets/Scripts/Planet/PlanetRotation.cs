using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlanetRotation : MonoBehaviour
{
    //Gravity variables
    private bool toggleGravity = true;
    private Vector3 gravityVector;
    private float defaultGravityRotation;

    //Planet Rotation Texture
    [SerializeField] RawImage planetTexture;
    //Planet Rotation Speed
    [SerializeField] float planetRotationSpeed;
    //Stuff to rotate
    [SerializeField] GameObject[] objectsToRotate;
    [SerializeField] GameObject defences;

    private void OnEnable()
    {
        //Check if the phone has a gravity sensor
        if (GravitySensor.current != null)
        {
            //Enable Device
            InputSystem.EnableDevice(UnityEngine.InputSystem.GravitySensor.current);
            //Get a default rotation variable
            ResetGravityDefaultRotation();
            //Enable gravity rotation
            toggleGravity = true;
        }
        else
        {
            //Disable gravity rotation
            toggleGravity = false;
        }
    }
    private void OnDisable()
    {
        //Check if the phone has a gravity sensor
        if (GravitySensor.current != null)
        {
            //Disable device
            InputSystem.DisableDevice(UnityEngine.InputSystem.GravitySensor.current);
            //Disable gravity rotation
            toggleGravity = false;
        }
    }
    private void Update()
    {
        //Rotate the planet
        RotatePlanetTexture();
        //Rotate the defences
        RotateTheDefences();
        //If gyroscope is enabled then use the gyroscope rotation + RotatePlanet(). Otherwise just RotatePlanet()
        if (toggleGravity)
        {
            GravityRotation();
        }
    }
    private void GravityRotation()
    {
        //Set the gavity Vector to the gravity sensor value
        gravityVector = UnityEngine.InputSystem.GravitySensor.current.gravity.ReadValue();
        //Check the gravity force
        float gravity = new Vector2(gravityVector.x, gravityVector.y).magnitude;
        //If the gravity is less than 2 don't rotate to gravity, rotate the planet normally
        if (gravity < .2f)
        {
            return;
        }
        //Get an angle from the X and Y values
        float angle = Mathf.Atan2(gravityVector.x, -gravityVector.y) * Mathf.Rad2Deg;
        //Set the target rotation to the angle - the default gavity value
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle - defaultGravityRotation);
        //Set the rotation to the target rotation
        foreach (GameObject obj in objectsToRotate)
        {
            obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, targetRotation, 5);
        }
    }
    private void RotateTheDefences()
    {
        defences.transform.Rotate(new Vector3(0, 0, -5) * 5 * Time.deltaTime);
    }
    private void RotatePlanetTexture()
    {
        //Rotate Planet Texture
        planetTexture.uvRect = new Rect(planetTexture.uvRect.position + new Vector2(planetRotationSpeed / 100, 0) * Time.deltaTime, planetTexture.uvRect.size);
    }
    public void ResetGravityDefaultRotation()
    {
        //Set the resetRot Vector to the gravity sensor value
        Vector3 resetRot = UnityEngine.InputSystem.GravitySensor.current.gravity.ReadValue();
        //Set the default gravity rotation from the X and Y values
        defaultGravityRotation = Mathf.Atan2(resetRot.x, -resetRot.y) * Mathf.Rad2Deg;
    }
}
