using System;
using UnityEngine;

public class DeleteTool : IBasePlayerTool
{
    public override void OnEnabledChanged( bool enabled )
    {
       
    }

    // Select target
    public override bool OnMouse1( bool pressed )
    {
        if( pressed && playerController.Raycast( out var hitInfo ) )
            if( playerController.IsValidOwnedObject( hitInfo.collider.gameObject ) )
                hitInfo.collider.gameObject.Destroy();
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