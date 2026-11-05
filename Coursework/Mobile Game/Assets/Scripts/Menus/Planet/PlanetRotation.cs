using UnityEngine;
using UnityEngine.InputSystem;

public class PlanetRotation : MonoBehaviour
{
    //Rotation variables
    private Vector3 rotationVector;
    [SerializeField] float rotationSpeed = 5;
    //Gyroscope variables
    public bool toggleGyroscope = true;
    private Vector3 gyroVector;
    private Vector3 defaultGyroRotation;
    private void Start()
    {
        //Set the Z rotation to rotation speed
        rotationVector = new Vector3(0,0, -rotationSpeed);
        if (SystemInfo.supportsGyroscope)
        {
            InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
            toggleGyroscope = true;
        }
        else
        {
            toggleGyroscope = false;
        }
    }
    private void OnDisable()
    {
        if (SystemInfo.supportsGyroscope)
        {
            InputSystem.DisableDevice(UnityEngine.InputSystem.Gyroscope.current);
            toggleGyroscope = true;
        }
    }
    private void Update()
    {
        //If gyroscope is enabled then use the gyroscope rotation. Otherwise just rotate the planet
        if (toggleGyroscope)
        {
            gyroVector = UnityEngine.InputSystem.Gyroscope.current.angularVelocity.value - defaultGyroRotation;
            transform.Rotate(new Vector3(0,0, -gyroVector.z));
        }
        else
        {
            //Planet Rotation
            transform.Rotate(rotationVector * Time.deltaTime);
        }

        Debug.Log(gyroVector.magnitude);
    }
    public void ResetGyroRotation()
    {
        defaultGyroRotation = UnityEngine.InputSystem.Gyroscope.current.angularVelocity.value;
    }
}
