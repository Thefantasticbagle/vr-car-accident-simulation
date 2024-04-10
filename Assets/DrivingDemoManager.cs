#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Splines;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Interpolators = UnityEngine.Splines.Interpolators;

public class DrivingDemoManager : MonoBehaviour
{
    public GameObject    Car;
    public SplineAnimate CarAnimator;
    public GameObject    XROrigin;
    public GameObject    PlayerLocomotionSystem;

    // Start is called before the first frame update
    void Start()
    {
        InitDefaultStartingState();
    }

    /// <summary>
    /// Initializes the default starting state for SceneState.
    /// </summary>
    void InitDefaultStartingState()
    {
        SceneState.allSteps = new List<Step>
        {
            new Step 
            (
                "HazardLights",
                1,
                "Turn on hazard lights"
            ),
            new Step 
            (
                "ReflectiveVest",
                2,
                "Retrieve and wear reflective vest"
            ),            
            new Step 
            (
                "EmergencyTriangle",
                3,
                "Place emergency triangle 150-200m away"
            ),
            new Step 
            (
                "CallEmergencyServices",
                4,
                "Call the emergency services"
            ),
            new Step
            (
                "DisableIncidentCar",
                5,
                "Disable the incident car"
            ),
            new Step
            (
                "HMS",
                6,
                "Give medical attention to the person"
            ),
        };
    }

    /// <summary>
    /// Drives the car until it has reached its destination and places the player outside of the car.
    /// </summary>
    /// <returns></returns>
    IEnumerator CarThread()
    {
        // Start car animation and disable movement
        CarAnimator.Play();
        PlayerLocomotionSystem.SetActive(false);

        while (CarAnimator.NormalizedTime < 1f)
            yield return null;

        // Once the car has arrived, teleport player out of car and enable movement again 
        PlayerLocomotionSystem.SetActive(true);
        XROrigin.transform.parent = null;
        Vector3 carOffset = Car.transform.localToWorldMatrix * new Vector4(-1.3f, -0.5f, 0.2f, 0f);
        XROrigin.transform.position = Car.transform.position + carOffset;
    }
}
