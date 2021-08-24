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

    float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        if( timer >= 1.0f )
        {
            timer -= 1.0f;
            //ListRaycasts();
        }
    }

    private void ListRaycasts()
    {
        Ray ray = Camera.main.ScreenPointToRay( Mouse.current.position.ReadValue() );
        RaycastHit[] hits;
        hits = Physics.RaycastAll( ray );
        int i = 0;
        while( i < hits.Length )
        {
            RaycastHit hit = hits[i];
            Debug.Log( hit.collider.gameObject.name );
            hit.collider.gameObject.SendMessage( "OnMouseEnter" );
            i++;
        }
    }
}
