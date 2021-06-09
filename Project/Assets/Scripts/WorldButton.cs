using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldButton : MonoBehaviour
{
    Color startColour;
    MeshRenderer mesh;

    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        startColour = mesh.material.color;
    }

    void OnMouseOver()
    {
        mesh.material.color = new Color(0.0f, 1.0f, 0.0f);
    }

    void OnMouseExit()
    {
        mesh.material.color = startColour;
    }
}
