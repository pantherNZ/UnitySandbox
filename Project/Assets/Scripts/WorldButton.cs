using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class WorldButton : MonoBehaviour
{
    [Serializable]
    public class WorldButtonClickedEvent : UnityEvent { }

    [SerializeField]
    WorldButtonClickedEvent m_OnClick = new WorldButtonClickedEvent();

    [SerializeField] Color highlightColour;
    Color startColour;
    MeshRenderer mesh;

    private void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        startColour = mesh.material.color;
    }

    private void OnMouseEnter()
    {
        mesh.material.color = highlightColour;
    }

    private void OnMouseDown()
    {
        m_OnClick.Invoke();   
    }

    private void OnMouseExit()
    {
        mesh.material.color = startColour;
    }
}
