using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
    bool _useVoxelPositions;

    [Header( "Physics" )]
    [SerializeField] bool physicsMovement;
    [SerializeField] bool simulateInEditor;
    [SerializeField] float physicsSpeed = 1.0f;
    [SerializeField] float physicsDamping = 1.0f;

    [Header( "Explode" )]
    [SerializeField] float explosionForce = 10.0f;
    [SerializeField] float explosionForceVariance = 10.0f;
    [SerializeField] float explodeDelaySec = 10.0f;
    [SerializeField] float explosionHeightScaleMin;
    [SerializeField] float explosionHeightScaleMax;

    [Header( "Voxels" )]
    VoxReader.Interfaces.IVoxFile voxelFile;
    VoxReader.Chunks.VoxelChunk voxelData;
    [SerializeField] float voxelScale = 1.0f;

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
        rigidBodies.Back().transform.localScale = new Vector3( voxelScale, voxelScale, voxelScale ) / 2.0f;
        SetPhysicsEnabled( rigidBodies.Back(), physicsMovement );
        UpdatePosition( false, rigidBodies.Back().transform, rigidBodies.Count - 1 );
        UpdateColour( false, rigidBodies.Back().transform, rigidBodies.Count - 1 );
    }

    private void ReadVoxelData()
    {
        try
        {
            voxelFile = VoxReader.VoxReader.Read( Path.Combine( Directory.GetCurrentDirectory(), voxelFilePath ) );
        }
        catch( Exception e )
        {
            Debug.LogError( "Failed to load voxel data: " + e.Message );
            useVoxelPositions = false;
            return;
        }

        voxelData = voxelFile.ChunkRoot.GetChild<VoxReader.Chunks.VoxelChunk>();
    }

    public void Init()
    {
        for( int i = transform.childCount - 1; i >= 0; --i )
            transform.GetChild( i ).DestroyGameObject();

        rigidBodies.Clear();

        if( objectPrefab != null )
        {
            if( useVoxelPositions )
                ReadVoxelData();

            for( int i = 0; i < GetObjectCount(); ++i )
                ConstructCube();
        }

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
            SetPhysicsEnabled( child, true );
            var direction = UnityEngine.Random.insideUnitSphere.SetY( explosionHeightScaleMin + UnityEngine.Random.value * ( explosionHeightScaleMax - explosionHeightScaleMin ) );
            var force = explosionForce + ( UnityEngine.Random.value - 0.5f ) * explosionForceVariance;
            child.AddForce( direction * force );
            inactiveTimer = explodeDelaySec;
        }
    }

    void Update()
    {
        if( useVoxelPositions && voxelData == null )
            ReadVoxelData();

        while( rigidBodies.Count > GetObjectCount() )
            rigidBodies.PopBack().DestroyGameObject();

        while( transform.childCount < GetObjectCount() )
            ConstructCube();

        // Changed
        if( useVoxelPositions != _useVoxelPositions )
        {
            _useVoxelPositions = useVoxelPositions;

            if( useVoxelPositions && voxelData != null )
            {
                UpdateColours( true );
            }
        }

        if( !physicsMovement || !Application.isPlaying )
            UpdatePositions( true );

        if( inactiveTimer > 0.0f )
        {
            inactiveTimer = Mathf.Max( 0.0f, inactiveTimer - Time.deltaTime );

            if( inactiveTimer == 0.0f )
                foreach( var child in rigidBodies )
                    SetPhysicsEnabled( child, physicsMovement );
        }
    }

    int GetObjectCount()
    {
        return ( useVoxelPositions && voxelData != null ) ? voxelData.Voxels.Length : objectCount;
    }

    void FixedUpdate()
    {
        if( physicsMovement && Application.isPlaying )
            UpdatePositions( true );
    }

    Vector3 CalculatePos( float angle, float offset )
    {
        return new Vector3( Mathf.Sin( angle ), 0.0f, Mathf.Cos( angle ) ) * offset;
    }

    void MoveToPos( bool interp, Transform obj, Rigidbody body, Vector3 desiredPos )
    {
        var direction = desiredPos - obj.localPosition;

        if( interpSpeed <= 0.0f || !interp )
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

    void UpdateColour( bool interp, Transform child, int idx )
    {
        if( useVoxelPositions && voxelData != null )
        {
            var voxelColour = voxelFile.Palette.Colors[voxelData.Voxels[idx].ColorIndex];
            var colour = new Color( 
                voxelColour.R / 255.0f
                , voxelColour.G / 255.0f
                , voxelColour.B / 255.0f
                , voxelColour.A / 255.0f );
            var mesh = child.GetComponent<MeshRenderer>();
            mesh.sharedMaterial = new Material( mesh.sharedMaterial )
            {
                color = colour
            };
        }
        else
        {
            float angle = ( Mathf.PI * 2.0f / GetObjectCount() ) * idx + time;
            float offset = radius + ( scaleRadiusByObjectCount * GetObjectCount() );
            
        }
    }

    void UpdateColours( bool interp )
    {
        for( int i = 0; i < transform.childCount; ++i )
        {
            var child = transform.GetChild( i );
            UpdateColour( interp, child, i );
        }
    }

    void UpdatePosition( bool interp, Transform child, int idx )
    {
        if( useVoxelPositions && voxelData != null )
        {
            var voxelPos = voxelData.Voxels[idx].Position;
            var localPos = new Vector3( voxelPos.X, voxelPos.Z, voxelPos.Y ) * voxelScale;
            MoveToPos( interp, child, rigidBodies[idx], localPos );
        }
        else
        {
            float angle = ( Mathf.PI * 2.0f / GetObjectCount() ) * idx + time;
            float offset = radius + ( scaleRadiusByObjectCount * GetObjectCount() );
            MoveToPos( interp, child, rigidBodies[idx], CalculatePos( angle, offset ) );
        }
    }

    void UpdatePositions( bool interp )
    {
        if( inactiveTimer == 0.0f )
        {
            time += Time.deltaTime * spinSpeed * Mathf.Deg2Rad;

            for( int i = 0; i < transform.childCount; ++i )
            {
                var child = transform.GetChild( i );
                UpdatePosition( interp, child, i );
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
