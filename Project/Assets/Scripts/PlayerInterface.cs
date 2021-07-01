using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInterface : MonoBehaviour
{
    [SerializeField]
    private GameObject toolUIElementPrefab;

    [SerializeField]
    private GameObject toolbarUIPanel;
    private HorizontalLayoutGroup toolbarUILayout;

    void Start()
    {
        var controller = FindObjectOfType<PlayerController>();
        controller.ToolSelectedEvent += OnToolSelectedEvent;
        controller.ToolBoundEvent += OnToolBoundEvent;

        toolbarUILayout = toolbarUIPanel.GetComponentInChildren<HorizontalLayoutGroup>();

        for( int i = 0; i < PlayerController.ToolBarMaxItems; ++i )
            Instantiate( toolUIElementPrefab, toolbarUILayout.transform );

        foreach( var (index, tool) in Utility.Enumerate( controller.GetBoundTools() ) )
            OnToolBoundEvent( new ToolBoundEventArgs() { player = controller, newTool = tool, toolIndex = index }  );

        OnToolSelectedEvent( new ToolSelectedEventArgs() { player = controller, oldToolIndex = 0, newToolIndex = controller.GetCurrentToolIndex() } );
    }

    private void OnToolBoundEvent( ToolBoundEventArgs args )
    {
        var toolElement = toolbarUILayout.transform.GetChild( args.toolIndex );
        var text = args.newTool == null ? "Unbound" : args.newTool.GetName();
        var image = args.newTool == null ? null : Utility.CreateSprite( args.newTool.GetImage() );
        toolElement.GetComponent<ToolUIElement>().SetData( text, image );
    }

    private void OnToolSelectedEvent( ToolSelectedEventArgs args )
    {
        toolbarUILayout.transform.GetChild( args.oldToolIndex ).GetComponent<ToolUIElement>().SetIsHighlighted( false );
        toolbarUILayout.transform.GetChild( args.newToolIndex ).GetComponent<ToolUIElement>().SetIsHighlighted( true );
    }
}
