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
    private FPSController playerController;

    private void Start()
    {
        playerController = GetComponentInParent<FPSController>();
    }
     
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
            ReleaseTarget();
        }
        else if( pressed && !target )
        {
            Debug.Log( "Fire" );
            Vector3 mouse = Camera.main.ScreenToWorldPoint( UnityEngine.InputSystem.Mouse.current.position.ReadValue() );
            var direction = ( mouse - transform.position );
            var result = Physics.Raycast( transform.position, direction, out var hitInfo, 50.0f, LayerMask.GetMask( "Object" ) );
            Debug.DrawRay( transform.position, direction, Color.blue );

            if( result && hitInfo.rigidbody && hitInfo.rigidbody.isKinematic )
            {
                Debug.Log( "Hit: " + hitInfo.rigidbody.gameObject.name );
                target = hitInfo.rigidbody.gameObject;
                target.GetComponent<Rigidbody>().isKinematic = false;
                targetDistance = ( target.transform.position - playerController.transform.position ).magnitude;
                targetLocationOffset = Quaternion.Euler( playerController.GetLookRotation() ).UnrotateVector( target.transform.position - GetTargetLockLocation() );
                targetRotationOffset = target.transform.rotation.eulerAngles - playerController.transform.rotation.eulerAngles;
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
            // const auto rotation = ( TargetActor->GetActorLocation() - Player->GetActorLocation() ).Rotation();
            // FVector x, y, z;
            // UKismetMathLibrary::GetAxes( rotation, x, y, z );
            // const auto target_rotation = FQuat( z, Rate * ObjectRotateSpeedYaw ) * ( Player->GetActorRotation() + targetRotationOffset ).Quaternion();
            // targetRotationOffset = target_rotation.Rotator() - Player->GetActorRotation();
            //
            // const auto rotation = ( TargetActor->GetActorLocation() - Player->GetActorLocation() ).Rotation();
            // FVector x, y, z;
            // UKismetMathLibrary::GetAxes( rotation, x, y, z );
            // const auto target_rotation = FQuat( y, -Rate * ObjectRotateSpeedPitch ) * ( Player->GetActorRotation() + targetRotationOffset ).Quaternion();
            // targetRotationOffset = target_rotation.Rotator() - Player->GetActorRotation();
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
        var direction = Matrix4x4.Rotate( Quaternion.Euler( playerController.GetLookRotation() ) ).GetUnitAxis( EAxis.X );
        var targetLoc = playerController.transform.position + direction * targetDistance + Quaternion.Euler( playerController.GetLookRotation() ).RotateVector( targetLocationOffset );
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
            // UUtilityLibrary::CustomLog( FString( TEXT( "Target Released" ) ) );
            target.GetComponent<Rigidbody>().isKinematic = false;
            target = null;
        }
	}
}