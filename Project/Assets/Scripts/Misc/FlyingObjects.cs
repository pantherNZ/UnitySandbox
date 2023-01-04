using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

[ExecuteAlways]
public class FlyingObjects : MonoBehaviour
{
    [Header( "Data" )]
    [SerializeField] GameObject objectPrefab;
    [Range( 1, 100 )]
    [SerializeField] int objectCount = 5;
    [ScriptPicker]
    [SerializeField] string voxelFilePath;

    [Header( "Positioning" )]
    [Range( -5.0f, 5.0f )]
    [SerializeField] float radius = 1.0f;
    [Range( 1.0f, 500.0f )]
    [SerializeField] float spinSpeed = 0.0f;
    [Range( 0.0f, 1.0f )]
    [SerializeField] float scaleRadiusByObjectCount = 0.0f;
    [SerializeField] bool rotateWithMovement;
    [SerializeField] float interpSpeed = 1.0f;
    [SerializeField] bool useVoxelPositions;

    [Header( "Physics" )]
    [SerializeField] bool physicsMovement;
    [SerializeField] bool simulateInEditor;
    [SerializeField] float physicsSpeed = 1.0f;
    [SerializeField] float physicsDamping = 1.0f;

    [Header( "Explode" )]
    [SerializeField] float explosionForce = 10.0f;
    [SerializeField] float explodeDelaySec = 10.0f;

    [Header( "Voxels" )]
    VoxReader.Interfaces.IVoxFile voxelData;

    private float time;
    private float inactiveTimer;
    private float physicsTimestep;

    private readonly List<Rigidbody> rigidBodies = new();

    private void OnEnable()
    {
        Init();
    }

    private void SetPhysicsEnabled( Rigidbody rb, bool enabled )
    {
        rb.isKinematic = !enabled;
        rb.detectCollisions = enabled;
    }

    private void ConstructCube()
    {
        rigidBodies.Add( Instantiate( objectPrefab, transform ).GetComponent<Rigidbody>() );
        SetPhysicsEnabled( rigidBodies.Back(), physicsMovement );
    }

    public void Init()
    {
        for( int i = transform.childCount - 1; i >= 0; --i )
            transform.GetChild( i ).DestroyGameObject();

        rigidBodies.Clear();

        if( objectPrefab != null )
        {
            if( useVoxelPositions )
            {
                voxelData = VoxReader.VoxReader.Read( Path.Combine( Directory.GetCurrentDirectory(), voxelFilePath ) );
                //model = voxFile.Models[0];
                //foreach( var voxel in voxelData.Chunks .Voxels )
                //    ConstructCube();
            }
            else
            {
                for( int i = 0; i < objectCount; ++i )
                    ConstructCube();
            }
        }

        UpdatePositions( true );

        //if( model != null )
            // If is NOT animation then pause movement processing
            //if( model.Voxels. )
            //inactiveTimer = -1.0f;
    }

    public void Explode()
    {
        if( inactiveTimer > 0.0f )
            return;

        foreach( var child in rigidBodies )
        {
            SetPhysicsEnabled( child, false );
            child.AddExplosionForce( explosionForce, transform.position, radius + ( scaleRadiusByObjectCount * objectCount ) );
            inactiveTimer = explodeDelaySec;
        }
    }

    void Update()
    {
        time += Time.deltaTime * spinSpeed * Mathf.Deg2Rad;

        if( !physicsMovement || !Application.isPlaying )
            UpdatePositions( false );

        if( inactiveTimer > 0.0f )
        {
            inactiveTimer = Mathf.Max( 0.0f, inactiveTimer - Time.deltaTime );

            if( inactiveTimer == 0.0f )
                foreach( var child in rigidBodies )
                    SetPhysicsEnabled( child, physicsMovement );
        }
    }

    void FixedUpdate()
    {
        if( physicsMovement && Application.isPlaying )
            UpdatePositions( false );
    }

    Vector3 CalculatePos( float angle, float offset )
    {
        return new Vector3( Mathf.Sin( angle ), 0.0f, Mathf.Cos( angle ) ) * offset;
    }

    void MoveToPos( bool init, Transform obj, Rigidbody body, Vector3 desiredPos )
    {
        var direction = desiredPos - obj.localPosition;

        if( interpSpeed <= 0.0f || init )
        {
            obj.localRotation = rotateWithMovement ? Quaternion.LookRotation( desiredPos - obj.localPosition, Vector3.up ) : Quaternion.identity;
            obj.localPosition = desiredPos;
            return;
        }

        if( physicsMovement )
        {
            var thrust = physicsSpeed * direction;
            var smoothing = body.velocity * 2.0f * physicsDamping * physicsSpeed;
            var finalThrust = thrust - smoothing;
            body.AddRelativeForce( finalThrust, ForceMode.Force );
            body.AddRelativeForce( -Physics.gravity, ForceMode.Acceleration );
            return;
        }

        var directionNorm = direction.normalized;
        var speed = direction.magnitude * interpSpeed;
        var delta = Mathf.Min( 1.0f, Time.deltaTime * speed );

        var interpPos = obj.localPosition + new Vector3(
            Mathf.Min( Mathf.Abs( direction.x ), Mathf.Abs( directionNorm.x ) * delta ) * Mathf.Sign( direction.x ),
            Mathf.Min( Mathf.Abs( direction.y ), Mathf.Abs( directionNorm.y ) * delta ) * Mathf.Sign( direction.y ),
            Mathf.Min( Mathf.Abs( direction.z ), Mathf.Abs( directionNorm.z ) * delta ) * Mathf.Sign( direction.z ) );

        obj.localPosition = interpPos;
        obj.localRotation = rotateWithMovement ? Quaternion.LookRotation( directionNorm, Vector3.up ) : Quaternion.identity;
    }

    void UpdatePositions( bool init )
    {
        if( inactiveTimer == 0.0f )
        {
            if( voxelData != null )
            {
                for( int i = 0; i < transform.childCount; ++i )
                {
                    var child = transform.GetChild( i );
                    //var voxelPos = voxelData. Voxels[i].Position;
                    //var localPos = new Vector3( voxelPos.X, voxelPos.Z, voxelPos.Y );
                    //MoveToPos( init, child, rigidBodies[i], localPos );
                }
            }
            else
            {
                for( int i = 0; i < transform.childCount; ++i )
                {
                    float angle = ( Mathf.PI * 2.0f / objectCount ) * i + time;
                    float offset = radius + ( scaleRadiusByObjectCount * objectCount );
                    var child = transform.GetChild( i );
                    MoveToPos( init, child, rigidBodies[i], CalculatePos( angle, offset ) );
                }
            }
        }

#if UNITY_EDITOR
        if( ( physicsMovement || inactiveTimer > 0.0f ) && !Application.isPlaying && ( simulateInEditor || inactiveTimer > 0.0f ) )
        {
            physicsTimestep += Time.deltaTime;

            while( physicsTimestep >= Time.fixedDeltaTime )
            {
                physicsTimestep -= Time.fixedDeltaTime;
                Physics.autoSimulation = false;
                Physics.Simulate( Time.fixedDeltaTime );
                Physics.autoSimulation = true;
            }
        }
#endif
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if( !Application.isPlaying )
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
#endif
    }
}
