using System;
using UnityEngine;

public class DeleteTool : IBasePlayerTool
{
    // Select target
    public override bool OnMouse1( bool pressed )
    {
        if( pressed && playerController.Raycast( out var hitInfo ) )
        {
            if( playerController.IsValidOwnedObject( hitInfo.collider.gameObject ) )
            {
                var data = hitInfo.collider.gameObject.GetComponent<SandboxObjectData>();
                var sandboxObject = data.data;

                UndoRedoSystem.GetInstance().ExecuteAction( () =>
                {
                    if( sandboxObject.gameObject )
                        hitInfo.collider.gameObject.Destroy();
                    else
                        Debug.LogError( String.Format( "Failed to delete object as it doesn't exist (ID: {0})", sandboxObject.uniqueId ) );
                }, () =>
                {

                } );
            }
        }

        return true;
    }

    // Freeze target
    public override bool OnMouse2( bool pressed )
    {
        if( pressed )
        {
            
            return true;
        }
        return false;
    }

    public override bool OnMouseWheel( bool pressed ) { return false; }

    public override bool OnSpecialAction( bool pressed ) { return false; }

    public override bool OnSpecialActionAlt( bool pressed ) { return false; }
}