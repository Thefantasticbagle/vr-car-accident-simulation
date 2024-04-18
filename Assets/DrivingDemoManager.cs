#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Splines;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Interpolators = UnityEngine.Splines.Interpolators;
using Unity.Services.Analytics;
using UnityEngine.UI;
using TMPro;

public class DrivingDemoManager : MonoBehaviour
{   
    public GameObject    Car;
    public SplineAnimate CarAnimator;
    public GameObject    XROrigin;
    public GameObject    PlayerLocomotionSystem;
    public GameObject    Smarthphone;
    public GlowOnHover   HazardLightsButtonGlowScript;

    public TMP_Text     popupTextField;

    public GameObject    popupObj;

    public AudioSource soundFXSource;

    public AudioClip soundFXClip;

    public TMP_Text timerText;
    float elapsedTime;
    float tempTime;

    // Start is called before the first frame update
    void Start()
    {
        InitDefaultStartingState();
        popupObj.SetActive(false);
    }

    void Update()
    {
        // Update timer
        elapsedTime += Time.deltaTime;
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        
    }

    /// <summary>
    /// Initializes the default starting state for SceneState.
    /// </summary>
    void InitDefaultStartingState()
    {
        // Set up all steps, as well as their ordering
        SceneState.allItems = new Dictionary<string, ProtocolItem>
        {
            { "HazardLights", new ProtocolItem
            (
                1,
                "Turn on hazard lights"
            )},
            { "EmergencyTriangle", new ProtocolItem
            (
                3,
                "Place emergency triangle 150-200m away"
            )},
            { "CallEmergencyServices", new ProtocolItem
            (
                4,
                "Call the emergency services"
            )},
            { "DisableIncidentCar", new ProtocolItem
            (
                5,
                "Disable the incident car"
            )},
            { "HMS", new ProtocolItem
            (
                6,
                "Give medical attention to the person"
            )}
        };

        // Mark all items as unfinished
        SceneState.unfinishedItems = new List<string> { };
        foreach (var it in SceneState.allItems.Keys)
            SceneState.unfinishedItems.Add( it );
    }


    IEnumerator ListenForProtocolItemCompletion()
    {
        int oldCompletedCount = SceneState.finishedItems.Count;

        while(true){
            int newCompletedCount = SceneState.finishedItems.Count;
            if(oldCompletedCount != newCompletedCount)
            {
                string lastItemName = SceneState.finishedItems[^1];
                ProtocolItem lastItem = SceneState.allItems[lastItemName];

                Debug.Log("COMPLETED NEW OBJECTIVE: " + lastItemName + " " + lastItem.description);

                
                // Set popup-text and show it
                popupTextField.text = "Completed: " + lastItem.description;
                
                popupObj.SetActive(true);
                soundFXSource.PlayOneShot(soundFXClip);
                yield return (new WaitForSeconds(6));
                popupObj.SetActive(false);
                /**                
                */

                oldCompletedCount = SceneState.finishedItems.Count;
            } else {
                oldCompletedCount = SceneState.finishedItems.Count;
                yield return (new WaitForSeconds(1));
            }

        }

    }

    /// <summary>
    /// Begins the gameloop.
    /// </summary>
    public void Begin()
    {
        Debug.Log("Started");
        StartCoroutine(GameLoop());
    }

    /// <summary>
    /// The game loop.
    /// Starts threads and checks for end-of-game condition.
    /// </summary>
    IEnumerator GameLoop()
    {
        // Place player within their car
        XROrigin.transform.SetParent(Car.transform, false);
        XROrigin.transform.position = new Vector3(0, 0, 0);
        XROrigin.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        XROrigin.transform.localPosition = new Vector3(-0.3f, -0.417f, 0.11f);
        XROrigin.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, -1.314f, 0.0f));

        // Wait for car driving to finish
        yield return StartCoroutine(CarThread());

        // Start threads
        // Each thread runs independently and writes to the SceneState in order to communicate
        StartCoroutine(HazardLightsThread());
        StartCoroutine(ReflectiveVestThread());
        StartCoroutine(EmergencyTriangleThread());
        StartCoroutine(CallEmergencyServicesThread());
        StartCoroutine(DisableIncidentCarThread());
        StartCoroutine(HMSThread());

        StartCoroutine(ListenForProtocolItemCompletion());

        // Wait for end-of-game condition
        while (!SceneState.finishedItems.Contains("HMS")) // For now, the 6th step (HMS) ends the games
            yield return null;

        // End of game.
        Debug.Log("Game complete!");
    }

    /// <summary>
    /// Enables the hazard lights of the player car.
    /// </summary>
    public void EnableHazardLights()
    {
        // Early exit conditions
        if (SceneState.finishedItems.Contains("HazardLights")) return;

        // (For now, simply make the button glow red and make it uninteractible)
        HazardLightsButtonGlowScript.StartPulsingRed();
        SceneState.CompleteItem("HazardLights");
    }
    IEnumerator HazardLightsThread()
    {
        // (For now, wait for EnableHazardLights() to close the thread for us)
        while (SceneState.unfinishedItems.Contains("HazardLights"))
            yield return null;
    }

    IEnumerator ReflectiveVestThread()
    {
        yield return null;
    }

    IEnumerator EmergencyTriangleThread()
    {
        yield return null;
    }

    /// <summary>
    /// "Calls" the emergency services.
    /// This function is activated by the button-component of a spatial manager when "Call an ambulance" is pressed.
    /// </summary>
    public void CallEmergencyServices()
    {
        // (For now, simply disable the phone and close the thread...)
        Smarthphone.SetActive(false);
        SceneState.CompleteItem("CallEmergencyServices");
    }
    IEnumerator CallEmergencyServicesThread()
    {
        // (For now, wait for CallEmergencyService() to close the thread for us)
        while (SceneState.unfinishedItems.Contains("CallEmergencyServices"))
            yield return null;
    }

    IEnumerator DisableIncidentCarThread()
    {
        yield return null;
    }   

    IEnumerator HMSThread()
    {
        yield return new WaitForSeconds(5);
        SceneState.CompleteItem("HMS");
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
        XROrigin.transform.SetParent(null);
        Vector3 carOffset = Car.transform.localToWorldMatrix * new Vector4(-1.3f, -0.5f, 0.2f, 0f);
        XROrigin.transform.position = Car.transform.position + carOffset;
    }
}
