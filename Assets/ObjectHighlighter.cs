using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ObjectHighlighter : MonoBehaviour
{
    private XRSimpleInteractable[] interactables;
    private InputActionReference input;

    // Start is called before the first frame update
    void Start()
    {
        input = GetComponent<InputActionReference>();
        if (input == null) Debug.Log("Error: Object Highlighter could not find the Input Action Manager component!");
    }

    /// <summary>
    /// Gets all interactables in the scene.
    /// </summary>
    void getInteractables()
    {
        interactables = FindObjectsOfType<XRSimpleInteractable>();
    }

    // Update is called once per frame
    void Update()
    {
        if (input.action.ReadValue<float>() > 0.5f)
        {
            Debug.Log("Action button pressed!");
        }
    }
}
