using UnityEngine;

public class PlanetManager : MonoBehaviour
{
    //Planet health variable
    public int planetHealth = 100;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        planetHealth -= 10;
        Debug.Log("HIT" + planetHealth);
        //collision.gameObject.SetActive(false);
    }
}
