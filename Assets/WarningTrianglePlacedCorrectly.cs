using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WarningTrianglePlacedCorrectly : MonoBehaviour
{
    public TextMeshPro Text;
    public GameObject PlayerCar;
    public GameObject legalPlacementRegion;
    public GameObject warningTriangle;

    void OnTriggerEnter(Collider c){
        if(c.gameObject == legalPlacementRegion){
            SceneState.CompleteItem("EmergencyTriangle");
        }
    }

    void FixedUpdate()
    {
        int distance = (int)( (PlayerCar.transform.position - transform.position).magnitude * 6.5f );
        Text.text = distance.ToString() + "m";

        if (transform.position.y < -2f)
            transform.position = transform.position + new Vector3(0,4,0);
    }
}
