using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlowOnHover : MonoBehaviour
{
    public Material glowMaterial;

    private Dictionary<MeshRenderer, Material> renderMaterials = new Dictionary<MeshRenderer, Material>();

    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {

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
