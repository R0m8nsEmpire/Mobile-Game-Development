using UnityEngine;
using UnityEngine.InputSystem;

public class PlanetRotation : MonoBehaviour
{
    //Rotation variables
    private Vector3 rotationVector;
    [SerializeField] float rotationSpeed = 5;
    //Gravity variables
    public bool toggleGravity = true;
    private Vector3 gravityVector;
    private float defaultGravityRotation;
    private void Start()
    {
        //Set the Z rotation to rotation speed
        rotationVector = new Vector3(0,0, -rotationSpeed);
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
        //If gyroscope is enabled then use the gyroscope rotation. Otherwise just rotate the planet
        if (toggleGravity)
        {
            //Set the gavity Vector to the gravity sensor value
            gravityVector = UnityEngine.InputSystem.GravitySensor.current.gravity.ReadValue();
            //Get an angle from the X and Y values
            float angle = Mathf.Atan2(gravityVector.x, -gravityVector.y) * Mathf.Rad2Deg;
            //Set the target rotation to the angle - the default gavity value
            Quaternion targetRotation = Quaternion.Euler(0,0,angle - defaultGravityRotation);
            //Set the rotation to the target rotation
            transform.rotation = targetRotation;
        }
        else
        {
            //Planet Rotation
            transform.Rotate(rotationVector * Time.deltaTime);
        }
    }
    public void ResetGravityDefaultRotation()
    {
        //Set the resetRot Vector to the gravity sensor value
        Vector3 resetRot = UnityEngine.InputSystem.GravitySensor.current.gravity.ReadValue();
        //Set the default gravity rotation from the X and Y values
        defaultGravityRotation = Mathf.Atan2(resetRot.x, -resetRot.y) * Mathf.Rad2Deg;
    }
}
