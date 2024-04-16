using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningTrianglePlacedCorrectly : MonoBehaviour
{


    public GameObject legalPlacementRegion;
    public GameObject warningTriangle;


    // Start is called before the first frame update
    void Start()
    {
        print("Initiated WarningTrianglePlacedCorrectly Script");
    }


    void OnTriggerEnter(Collider c){
        if(c.gameObject == legalPlacementRegion){
            Debug.Log("Warning triangle is in an appropriate region");
            SceneState.CompleteItem("EmergencyTriangle");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        

    }
}
