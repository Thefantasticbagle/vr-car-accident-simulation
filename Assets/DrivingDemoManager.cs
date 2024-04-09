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
        StartCoroutine(RunScene());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator RunScene()
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
