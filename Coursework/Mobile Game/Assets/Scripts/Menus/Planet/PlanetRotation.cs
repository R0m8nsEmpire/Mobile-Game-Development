using UnityEngine;

public class PlanetRotation : MonoBehaviour
{
    private Vector3 rotationVector;
    [SerializeField] float rotationSpeed = 5;
    private void Start()
    {
        //Set the Z rotation to rotation speed
        rotationVector = new Vector3(0,0, -rotationSpeed);
    }
    private void Update()
    {
        //Planet Rotation
        transform.Rotate(rotationVector * Time.deltaTime);
    }
}
