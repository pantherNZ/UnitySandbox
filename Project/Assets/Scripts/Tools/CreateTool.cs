using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class CreateTool : IBasePlayerTool
{
    //[SerializeField]
    public GameObject cubePrefab;

    [SerializeField]
    float createOffsetDistance = 10.0f;

    private List<GameObject> props;

    new protected void Start()
    {
        base.Start();

        props = Resources.LoadAll<GameObject>( "Prefabs/Props" ).ToList();
    }

    // Select target
    public override bool OnMouse1( bool pressed )
    {
        if( pressed )
        {
            var position = playerController.GetFPSCamera().transform.position + playerController.GetFPSCamera().transform.forward * createOffsetDistance;
            var rotation = Quaternion.identity;

            if( playerController.Raycast( out RaycastHit hitInfo ) )
            {
                position = hitInfo.point;
                rotation = Quaternion.FromToRotation( Vector3.forward, Vector3.up ) * Quaternion.LookRotation( hitInfo.normal );

                var mesh = cubePrefab.GetComponent<Renderer>().bounds;
                position -= playerController.GetFPSCamera().transform.forward * Mathf.Max( mesh.size.x, mesh.size.y, mesh.size.z );
            }

            var newObject = Instantiate( cubePrefab, position, rotation );
            SandboxObject.ObjectManager.Instance.CreateObject( newObject, cubePrefab, playerController.playerId );
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