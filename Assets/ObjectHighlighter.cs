using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ObjectHighlighter : MonoBehaviour
{
    public GameObject           player;
    public InputActionReference input;
    public Material             glowMaterial;

    private XRSimpleInteractable[]  interactables;
    private XRGrabInteractable[]    grabbables;
    private List<GameObject>        interactableObjectClones = new List<GameObject>();
    private List<GameObject>        interactableObjectClonesOriginals = new List<GameObject>();

    private int highlightButtonHeld = 0;
    private bool objectsHighlighted = false;
    private float pulseTime = 0;

    /// <summary>
    /// Types which will be allowed for the cloned temporary objects.
    /// </summary>
    private List<Type> allowedComponentTypes = new List<Type>
    {
        typeof(GameObject),
        typeof(Transform),
        typeof(MeshFilter),
        typeof(MeshRenderer)
    };

    /// <summary>
    /// Gets all interactables in the scene.
    /// </summary>
    void getInteractables()
    {
        interactables = FindObjectsOfType<XRSimpleInteractable>();
        //grabbables = FindObjectsOfType<XRGrabInteractable>();
    }

    /// <summary>
    /// Begins highlighting all highlightable objects.
    /// </summary>
    void beginHightlight()
    {
        pulseTime = 0f;
        
        // Find all XR interactable objects
        getInteractables();

        List<GameObject> interactableObjects = new List<GameObject>();
        foreach (var interactable in interactables)
        {
            GameObject interactableObject = interactable.gameObject;
            if (interactableObject != null && interactable.enabled)
                interactableObjects.Add(interactableObject);
        }

        //foreach (var grabbable in grabbables)
        //{
        //    GameObject grabbableObject = grabbable.gameObject;
        //    if (grabbableObject != null && grabbable.enabled)
        //        interactableObjects.Add(grabbableObject);
        //}

        // Create a slightly larger clone of the to-highlight objects
        foreach (GameObject interactableObject in interactableObjects)
        {
            // Make the clone slightly larger than its original and parent it
            GameObject interactableObjectClone = Instantiate(interactableObject, interactableObject.transform);
            interactableObjectClone.transform.localScale = new Vector3(1.012f, 1.012f, 1.012f);
            interactableObjectClone.transform.localPosition = new Vector3(0,0,0);
            interactableObjectClone.transform.localRotation = Quaternion.Euler(new Vector3(0,0,0));

            // Remove unnecessary components from the object
            Component[] interactableObjectCloneComponents = interactableObjectClone.GetComponents(typeof(Component));
            foreach (var interactableObjectCloneComponent in interactableObjectCloneComponents)
            {
                // (While we're iterating, make the material glow)
                if (interactableObjectCloneComponent.GetType() == typeof(MeshRenderer))
                {
                    MeshRenderer meshRenderer = ((MeshRenderer)interactableObjectCloneComponent);
                    meshRenderer.material = glowMaterial;
                    Material[] materials = Enumerable.Repeat(glowMaterial, meshRenderer.materials.Length).ToArray();
                    meshRenderer.materials = materials;
                }

                // (Destroy unpermitted component types)
                bool allowed = false;
                foreach (var allowedType in allowedComponentTypes) { if (interactableObjectCloneComponent.GetType() == allowedType) allowed = true; }
                if (!allowed) Destroy(interactableObjectCloneComponent);
            }

            // Finally, add the object to the list of managed clones
            interactableObjectClonesOriginals.Add(interactableObject);
            interactableObjectClones.Add(interactableObjectClone);
        }

        objectsHighlighted = true;
    }

    /// <summary>
    /// Ends the object highlighting.
    /// </summary>
    void endHighlight()
    {
        foreach (var interactableObjectClone in interactableObjectClones)
            Destroy(interactableObjectClone);

        interactableObjectClones.Clear();
        interactableObjectClonesOriginals.Clear();
        objectsHighlighted = false;
    }

    // Update is called once per frame
    void Update()
    {
        bool highLightButtonDropped = false;

        // Check for button press
        if (input.action.ReadValue<float>() > 0f)
            highlightButtonHeld++;
        else
        {
            if (highlightButtonHeld > 0) highLightButtonDropped = true;
            highlightButtonHeld = 0;
        }

        // Enable/Disable highlight
        if (highlightButtonHeld == 1 || highLightButtonDropped)
            if (objectsHighlighted) endHighlight();
            else beginHightlight();

        // Pulse
        if (objectsHighlighted)
        {
            pulseTime += Time.deltaTime;
            float brightness = (Mathf.Sin(pulseTime * 1.5f - 45) * 0.5f + 0.5f) * 5f + 0.85f;
            Vector4 newColor = new Vector4(brightness, brightness, brightness, 1);

            foreach (var interactableObjectClone in interactableObjectClones)
            {
                MeshRenderer meshRenderer = interactableObjectClone.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    float distanceToPlayer = (player.transform.position - meshRenderer.transform.position).magnitude / 0.1f;

                    meshRenderer.material.SetColor("_EmissionColor", newColor * Mathf.Clamp(Mathf.SmoothStep(1, 150, distanceToPlayer), 1, 150));
                    foreach (var material in meshRenderer.materials)
                        material.SetColor("_EmissionColor", newColor);
                }
            }

            // Also check if highlighted objects originals still have their interactable
            for (int i = 0; i < interactableObjectClonesOriginals.Count; i++)
            {
                GameObject interactableObject = interactableObjectClonesOriginals[i];
                XRSimpleInteractable interactable = interactableObject.GetComponent<XRSimpleInteractable>();
                GameObject interactableObjectClone = interactableObjectClones[i];
                if (!interactable.enabled || interactable == null || !interactableObject.activeSelf)
                {
                    interactableObjectClonesOriginals.RemoveAt(i);
                    interactableObjectClones.RemoveAt(i);
                    i--;
                    Destroy(interactableObjectClone);
                }
            }
        }
    }
}
