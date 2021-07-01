using System;
using UnityEngine;

public class PhysTool : IBasePlayerTool
{
	private GameObject target;
    [SerializeField]
    private float rotationLockAngle = 22.5f;
    [SerializeField]
    private float zoomSpeed = 1.0f;
    [SerializeField]
	private float objectRotateSpeedYaw = 0.1f;
    [SerializeField]
    private float objectRotateSpeedPitch = 0.1f;
	private bool rotatingTarget;
	private bool rotateAxisAligned;

	private float targetDistance;
	private Vector3 targetLocationOffset;
	private Vector3 targetRotationOffset;
	private Vector3 rotationOffset;
	private TransformData startTransform;
     
    public override void OnEnabledChanged( bool enabled ) 
    {
        if( !enabled && target )
            ReleaseTarget();
    }

    // Select target
    public override bool OnMouse1( bool pressed ) 
    {
        if( !pressed && target )
        {
            var startTransformCopy = startTransform;
            var currentTransform = new TransformData()
            {
                translation = target.transform.position,
                rotation = target.transform.rotation,
                scale = target.transform.localScale,
            };
            var targ = SandboxObject.ObjectManager.Instance.CreateObject( target, playerController.playerId ); ;

            UndoRedoSystem.GetInstance().AddAction( () =>
            {
                targ.gameObject.transform.SetTransformData( currentTransform );
            }, () =>
            {
                targ.gameObject.transform.SetTransformData( startTransformCopy );
            } );

            ReleaseTarget();
        }
        else if( pressed && !target )
        {
            var result = playerController.Raycast( out var hitInfo );
            Debug.Log( "Fire: " + result + " " + ( hitInfo.rigidbody ? hitInfo.rigidbody.gameObject.name : "" ) );

            if( result && hitInfo.rigidbody )
            {
                Debug.Log( "Hit: " + hitInfo.rigidbody.gameObject.name );
                target = hitInfo.rigidbody.gameObject;
                hitInfo.rigidbody.isKinematic = true;
                targetDistance = ( hitInfo.point - playerController.GetFPSCamera().transform.position ).magnitude;
                targetLocationOffset = Vector3.zero;
                targetLocationOffset = Quaternion.Euler( playerController.GetLookRotation() ).UnrotateVector( target.transform.position - GetTargetLockLocation() );
                targetRotationOffset = target.transform.rotation.eulerAngles - playerController.transform.rotation.eulerAngles;

                startTransform = new TransformData()
                {
                    translation = target.transform.position,
                    rotation = target.transform.rotation,
                    scale = target.transform.localScale,
                };
            }
        }

        return true;
    }

    // Freeze target
    public override bool OnMouse2( bool pressed )
    {
        if( pressed )
        {
            target = null;
            return true;
        }
		return false; 
    }

    public override bool OnMouseWheel( bool pressed ) { return false; }
    
    // Move target closer / further away
    public override bool OnMouseWheelScroll( float axis ) 
    {
        if( target )
        {
            targetDistance = Mathf.Max( 0.0f, targetDistance + Time.deltaTime * zoomSpeed * axis );
            return true;
        }

        return false;
    }

    // Move target around
    public override bool OnLook( Vector2 axis ) 
    {
        if( rotatingTarget && target )
        {
            var direction = Quaternion.Euler( playerController.GetLookRotation() ) * Vector3.forward; 
            var targetRotation = Quaternion.AngleAxis( axis.x * objectRotateSpeedYaw * Time.deltaTime, playerController.GetFPSCamera().transform.up ) * Quaternion.Euler( target.transform.rotation.eulerAngles + targetRotationOffset );
            targetRotationOffset = targetRotation.eulerAngles - playerController.transform.rotation.eulerAngles;
            return true;
        }

        return false;
    }

    // E - Rotation of target
    public override bool OnSpecialAction( bool pressed ) 
    {
		rotatingTarget = pressed;
        return true;
    }

    // Shift E - Axis aligned rotation of target
    public override bool OnSpecialActionAlt( bool pressed ) 
    {
		rotateAxisAligned = pressed;
		return true;
    }

    private void Update()
	{
		if( target )
		{
			target.transform.position = GetTargetLockLocation();
			target.transform.rotation = GetTargetLockRotation();
		}
	}

	Vector3 GetTargetLockLocation()
	{
        var direction = Quaternion.Euler( playerController.GetLookRotation() ) * Vector3.forward;
        var targetLoc = playerController.GetFPSCamera().transform.position + direction * targetDistance + Quaternion.Euler( playerController.GetLookRotation() ).RotateVector( targetLocationOffset );
        return targetLoc;
    }

	Quaternion GetTargetLockRotation()
	{
        var euler = targetRotationOffset;
        
        if( rotateAxisAligned )
        {
        	euler.x = Mathf.RoundToInt( ( euler.x + 360 ) / rotationLockAngle ) * rotationLockAngle;
        	euler.y = Mathf.RoundToInt( ( euler.y + 360 ) / rotationLockAngle ) * rotationLockAngle;
        	euler.z = Mathf.RoundToInt( ( euler.z + 360 ) / rotationLockAngle ) * rotationLockAngle;
        }
        
        return Quaternion.Euler( playerController.transform.rotation.eulerAngles + euler );
	}

	void ReleaseTarget()
	{
        if( target )
        {
            Debug.Log( "Target Released" );
            target.GetComponent<Rigidbody>().isKinematic = false;
            target = null;
        }
	}
}