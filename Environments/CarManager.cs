using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    [SerializeField] CarBehaviour[] Cars;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("RandomlyInstantiateCars", 1f, 5f);
    }

    private void RandomlyInstantiateCars()
    {
        float camPos = Camera.main.transform.position.x;
        ushort selectedCarIndex = (ushort) Random.Range(0, Cars.Length);
        float scaleX = Random.Range(-1, 1);

        if (scaleX == 0)
            scaleX = 1;

        Vector3 carPos = new Vector3(camPos + (7f * scaleX) , 0.2f, 0);
        CarBehaviour carGO = Instantiate(Cars[selectedCarIndex], carPos, Quaternion.identity) as CarBehaviour;
        carGO.gameObject.transform.localScale = new Vector3(scaleX, 1, 1);
    }
}
