using System;
using UnityEngine;

public class WeldTool : IBasePlayerTool
{

    public override void OnEnabledChanged( bool enabled )
    {

    }

    // Select target
    public override bool OnMouse1( bool pressed )
    {
        if( pressed )
        {

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