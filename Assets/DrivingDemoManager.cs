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
using UnityEngine.XR.Interaction.Toolkit;

public class DrivingDemoManager : MonoBehaviour
{
    public GameObject MenuCube;
    public GameObject ReflectiveVest;

    public GameObject    Car;
    public AudioClip     CarDrivingClip;

    public GameObject WarningTriangle;
    private GameObject baggageRoom;
    private XRSimpleInteractable baggageRoomInteractable;

    public GameObject IncidentCarDoor;

    public GameObject JasonTheVictim;

    public SplineAnimate CarAnimator;
    public GameObject    XROrigin;
    public GameObject    PlayerLocomotionSystem;
    public GameObject    Smarthphone;
    public GlowOnHover   HazardLightsButtonGlowScript;

    public TMP_Text     popupTextField;

    public GameObject    popupObj;

    public AudioSource soundFXSource;

    public AudioClip soundFXClip;
    public AudioClip ZipperClip;

    public TMP_Text timerText;
    float elapsedTime;
    float tempTime;

    private GameObject XRRig;

    private bool gameLoopStarted = false;
    private bool insideCar = false;
    private bool carDoorEnabled = true;

    // Start is called before the first frame update
    void Start()
    {
        InitDefaultStartingState();
        popupObj.SetActive(false);

        foreach (Transform child in XROrigin.transform)
        {
            GameObject childObject = child.gameObject;

            if(childObject.name == "XR Origin (XR Rig)"){
                XRRig = childObject;
            }
        }

        foreach (Transform child in Car.transform)
        {
            GameObject childObject = child.gameObject;
            if (childObject.name == "Tocus_Hood_Back")
            {
                baggageRoom = childObject;
                baggageRoomInteractable = baggageRoom.GetComponent<XRSimpleInteractable>();
            }
        }
    }

    void Update()
    {
        // Update timer
        if(gameLoopStarted){
            elapsedTime += Time.deltaTime;
            int seconds = Mathf.FloorToInt(elapsedTime % 60);
            int minutes = Mathf.FloorToInt(elapsedTime / 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
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
                "Skru på varsellysene"
            )},
            { "ReflectiveVest", new ProtocolItem
            (
                1,
                "Ta på refleksvest"
            )},
            { "EmergencyTriangle", new ProtocolItem
            (
                2,
                "Plasser varseltrekanten ved veien 150-200m unna skadestedet"
            )},
            { "CallEmergencyServices", new ProtocolItem
            (
                3,
                "Ring nødetatene"
            )},
            { "DisableIncidentCar", new ProtocolItem
            (
                4,
                "Skru av tenningen på ulykkesbilen"
            )},
            { "HMS", new ProtocolItem
            (
                5,
                "Gi medisinsk tilsyn veiledet av nødetatene mens de er på vei"
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

                // Set popup-text and show it
                popupTextField.text = "Utført: " + lastItem.description;
                
                popupObj.SetActive(true);
                soundFXSource.PlayOneShot(soundFXClip);
                yield return (new WaitForSeconds(6));
                popupObj.SetActive(false);

                oldCompletedCount = SceneState.finishedItems.Count;
            } else {
                oldCompletedCount = SceneState.finishedItems.Count;
                yield return (new WaitForSeconds(1));
            }

        }

    }

    /// <summary>
    /// Toggles entering/leaving the car.
    /// </summary>
    public void ToggleEnterCar()
    {
        // The first time the car is entered begins the gameloop and locks the player until the ride is finished
        if ( gameLoopStarted == false ) {
            StartCoroutine(GameLoop());
            gameLoopStarted = true;
            carDoorEnabled = false;
        }
        else if ( carDoorEnabled )
        {
            if (insideCar) exitCar();
            else enterCar();
        }
    }

    /// <summary>
    /// The game loop.
    /// Starts threads and checks for end-of-game condition.
    /// </summary>
    IEnumerator GameLoop()
    {
        StartCoroutine(ListenForProtocolItemCompletion());

        // Start threads
        // Each thread runs independently and writes to the SceneState in order to communicate
        StartCoroutine(HazardLightsThread());
        StartCoroutine(ReflectiveVestThread());
        StartCoroutine(EmergencyTriangleThread());
        StartCoroutine(CallEmergencyServicesThread());
        StartCoroutine(DisableIncidentCarThread());
        StartCoroutine(HMSThread());

        // Wait for car driving to finish
        yield return StartCoroutine(CarThread());

        // Enable baggage room
        baggageRoomInteractable.enabled = true;

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

        XRSimpleInteractable HazardLightsInteractable = HazardLightsButtonGlowScript.gameObject.GetComponent<XRSimpleInteractable>();
        HazardLightsInteractable.enabled = false;
    }
    IEnumerator HazardLightsThread()
    {
        // (For now, wait for EnableHazardLights() to close the thread for us)
        while (SceneState.unfinishedItems.Contains("HazardLights"))
            yield return null;
    }

    /// <summary>
    /// Makes the player wear the reflective vest.
    /// </summary>
    public void WearReflectiveVest()
    {
        ReflectiveVest.SetActive(false);
        soundFXSource.PlayOneShot(ZipperClip);
        SceneState.CompleteItem("ReflectiveVest");
    }
    IEnumerator ReflectiveVestThread()
    {
        // (For now, wait for EnableHazardLights() to close the thread for us)
        while (SceneState.unfinishedItems.Contains("ReflectiveVest"))
            yield return null;
    }

    public void OpenBaggageRoom()
    {
        baggageRoomInteractable.enabled = false;
        baggageRoom.transform.localPosition = new Vector3(0f, -1.038f, -2.263f);
        baggageRoom.transform.localRotation = Quaternion.Euler(new Vector3(91.54999f, 191.17f, 190.709f));

        // When the baggage room is opened, reveal the warning triangle as well
        WarningTriangle.SetActive(true);
        Vector3 carOffset = Car.transform.localToWorldMatrix * new Vector3(0.03470612f, 0.3406601f, -2.002366f);
        Vector3 warningTriangleNewPos = Car.transform.position + carOffset;
        WarningTriangle.transform.position = warningTriangleNewPos;
        WarningTriangle.transform.rotation = Quaternion.Euler(new Vector3(73.098f, 1.726f, 0f));
    }
    IEnumerator EmergencyTriangleThread()
    {
        while (SceneState.unfinishedItems.Contains("EmergencyTriangle"))
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

    public void OpenIncidentCarDoor()
    {
        IncidentCarDoor.transform.localPosition = new Vector3(-1.738f, 0f, 0.226f);
        IncidentCarDoor.transform.localRotation = Quaternion.Euler(new Vector3(0f, 95f, 0f));
    }

    public void InitiateHMS()
    {
        SceneState.CompleteItem("HMS");
    }
    IEnumerator HMSThread()
    {
        while (SceneState.unfinishedItems.Contains("HMS"))
            yield return null;
    }

    /// <summary>
    /// Makes the player enter the car.
    /// </summary>
    private void enterCar()
    {
        PlayerLocomotionSystem.SetActive(false);
        XROrigin.transform.SetParent(Car.transform, false);
        XRRig.transform.localPosition = new Vector3(0, 0, 0);
        XRRig.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        XROrigin.transform.localPosition = new Vector3(-0.3f, -0.417f, 0.11f);
        XROrigin.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, -1.314f, 0.0f));

        insideCar = true;
    }

    /// <summary>
    /// Makes the player exit the car.
    /// </summary>
    private void exitCar()
    {
        PlayerLocomotionSystem.SetActive(true);
        XROrigin.transform.SetParent(null);
        Vector3 carOffset = Car.transform.localToWorldMatrix * new Vector4(-1.3f, -0.5f, 0.2f, 0f);
        XROrigin.transform.position = Car.transform.position + carOffset;

        insideCar = false;
    }

    /// <summary>
    /// Drives the car until it has reached its destination and places the player outside of the car.
    /// </summary>
    /// <returns></returns>
    IEnumerator CarThread()
    {
        // Start car animation and disable movement
        enterCar();
        CarAnimator.Play();
        soundFXSource.PlayOneShot(CarDrivingClip);

        // Disable menu cube
        MenuCube.SetActive(false);

        // Wait for car to arrive
        while (CarAnimator.NormalizedTime < 1f)
        {
            soundFXSource.volume = Mathf.Clamp(1f - Mathf.Pow(CarAnimator.NormalizedTime, 10), 0f, 1f);
            yield return null;
        }

        soundFXSource.Stop();
        soundFXSource.volume = 1f;

        // Once the car has arrived, allow the player to control again
        carDoorEnabled = true;
    }
}
