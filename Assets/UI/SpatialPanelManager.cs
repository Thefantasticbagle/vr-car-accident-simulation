using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// A simple class which manages the spatial panel manipulator.
/// </summary>
public class SpatialPanelManager : MonoBehaviour
{
    public GameObject SpatialPanel;

    private bool spatialPanelOpen = false;
    private GameObject XROrigin;
    private LazyFollow spatialPanelLazyFollowScript;

    // Start is called before the first frame update
    void Start()
    {
        // Check if Spatial Panel is set
        if (SpatialPanel == null)
            Debug.LogError("Spatial Panel Manager has no SpatialPanel GameObject!");

        // Check if parent object (XR Origin Set up) is valid
        XROrigin = gameObject;
        if (XROrigin == null)
            Debug.LogError("Spatial Panel Manager had invalid parent gameObject!");

        // Fetch lazy follow script
        spatialPanelLazyFollowScript = SpatialPanel.GetComponent<LazyFollow>();
        if (spatialPanelLazyFollowScript == null)
            Debug.LogError("Spatial Panel Manager could not find Spatial Panel's Lazy Follow script!");

        ClosePanel();
    }

    // Getters
    public bool isSpatialPanelOpen() { return spatialPanelOpen; }

    /// <summary>
    /// Closes the spatial panel.
    /// </summary>
    public void ClosePanel()
    {
        spatialPanelOpen = false;

        // Disable spatial panel object
        SpatialPanel.SetActive(false);
    }

    /// <summary>
    /// Opens the spatial panel next to the player.
    /// </summary>
    public void OpenPanel()
    {
        spatialPanelOpen = true;

        // Align the spatial panel
        SpatialPanel.transform.position = XROrigin.transform.position; // TODO: Place in front of player, not on top of them
        SpatialPanel.transform.LookAt(XROrigin.transform.position);

        // Activate the spatial panel object
        SpatialPanel.SetActive(true);
    }
}
