using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlowOnHover : MonoBehaviour
{
    public Material glowMaterial;
    public List<GameObject> otherObjects = new List<GameObject> { };

    private Dictionary<MeshRenderer, Material> renderMaterials = new Dictionary<MeshRenderer, Material>();
    private bool isPulsingRed = false;

    float pulseTime = 0;

    void FixedUpdate()
    {
        if (isPulsingRed)
        {
            pulseTime += Time.deltaTime;
            foreach (GameObject obj in otherObjects)
            {
                Material material = obj.GetComponent<MeshRenderer>().material;
                material.SetColor("_EmissionColor", new Vector4((Mathf.Sin(pulseTime * 2f) * 0.5f + 0.5f) * 200f + 10f, 0, 0, 1));
            }
        }
    }

    /// <summary>
    /// Makes the object begin to pulse red, indefinitely.
    /// Disables the standard glow.
    /// </summary>
    public void StartPulsingRed()
    {
        isPulsingRed = true;
    }

    public void StartGlow()
    {
        // Clear the materials list to prepare it for new ones
        renderMaterials.Clear();

        MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in meshRenderers)
        {
            renderMaterials[mr] = mr.material;
            mr.material = glowMaterial;
        }
    }

    public void EndGlow()
    {
        foreach (var (mr, m) in renderMaterials)
        {
            mr.material = m;
        }
    }
}
