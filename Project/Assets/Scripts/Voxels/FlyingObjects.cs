using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[ExecuteAlways]
public class FlyingObjects : MonoBehaviour
{
    [Header( "Voxels" )]
    [SerializeField] GameObject cubePrefab;
    [SerializeField] float cubeScale = 1.0f;
    [SerializeField] float positionInterpSpeed = 1.0f;
    [SerializeField] float scaleInterpSpeed = 1.0f;
    [SerializeField] float rotationInterpSpeed = 1.0f;
    [SerializeField] float colourInterpSpeed = 1.0f;
    [SerializeField] Color baseColour = Color.white;

    [HideInInspector] public bool randomBehaviour;
    [HideInInspector] public BehaviourData currentBehaviourData;
    BehaviourData previousBehaviourData;
    [HideInInspector] public float behaviourInterpSpeed = 1.0f;
    [HideInInspector] public float behaviourChangeTimeMin = 0.0f;
    [HideInInspector] public float behaviourChangeTimeMax = 1.0f;
    [HideInInspector] public List<BehaviourData> allBehaviours;

    [Header( "Explode" )]
    [SerializeField] float explosionForce = 10.0f;
    [SerializeField] float explosionForceVariance = 10.0f;
    [SerializeField] float explodeDelaySec = 10.0f;
    [SerializeField] float explosionHeightScaleMin;
    [SerializeField] float explosionHeightScaleMax;

    [Header( "Freeze" )]
    [SerializeField] float freezeDurationSec = 1.0f;

    [Flags]
    enum Flags
    {
        NoInterp = 1,
        Physics = 2,
        Paused = 4,
    }

    private Flags flags;
    private float physicsTimestep;
    private float behaviourChangeTimer;
    private readonly List<ObjectData> objects = new();
    private readonly MathFuncGenerator funcGenerator = new();
    private int currentProcessingVoxel;

    public class ObjectData
    {
        public GameObject obj;
        public MeshRenderer renderer;
    }

    private void OnEnable()
    {
        Init();
    }

    private void SetPhysicsEnabled( GameObject obj, bool enabled )
    {
        SetPhysicsEnabled( obj.GetComponent<Rigidbody>(), enabled );
    }

    private void SetPhysicsEnabled( Rigidbody rb, bool enabled )
    {
        rb.isKinematic = !enabled;
        rb.detectCollisions = enabled;
    }

    private void ConstructCube()
    {
        var idx = objects.Count;
        var newObj = Instantiate( cubePrefab, transform );
        objects.Add( new ObjectData()
        {
            obj = newObj,
            renderer = newObj.GetComponent<MeshRenderer>(),
        } );

        objects.Back().renderer.sharedMaterial = new Material( objects.Back().renderer.sharedMaterial )
        {
            color = currentBehaviourData.GetColour( idx ) ?? baseColour
        };
        newObj.transform.localScale = new Vector3().Set( cubeScale * currentBehaviourData.cubeScale );
        newObj.transform.localPosition = currentBehaviourData.GetPosition( idx );
        newObj.transform.localRotation = currentBehaviourData.GetRotation( idx );
        SetPhysicsEnabled( newObj, false );
        //UpdateColour( false, objects.Back().transform, idx );
    }

    private bool isValid()
    {
        return currentBehaviourData != null && cubePrefab != null;
    }

    public void Init()
    {
        if( !isValid() )
            return;

        for( int i = transform.childCount - 1; i >= 0; --i )
            transform.GetChild( i ).DestroyGameObject();

        objects.Clear();

        if( cubePrefab != null )
        {
            for( int i = 0; i < currentBehaviourData.ObjectCount; ++i )
                ConstructCube();
        }

        flags = 0;

        int maxDepth = 3;
        funcGenerator.DefaultValues( -180.0f, 180.0f );
        funcGenerator.AddConstantFunc( () => Time.fixedTime, () => $"GameTime ({Time.fixedTime})" );
        funcGenerator.AddConstantFunc( () => currentProcessingVoxel, () => $"VoxelIdx ({currentProcessingVoxel})" );
        funcGenerator.ConstructTree( maxDepth );
        Debug.Log( $"Function: {funcGenerator.TreeToString()}" );
    }

    public void InitRandom()
    {
        behaviourChangeTimer = UnityEngine.Random.Range( behaviourChangeTimeMin, behaviourChangeTimeMax ); 
    }

    public void Explode()
    {
        if( flags.HasFlag( Flags.Physics ) || flags.HasFlag( Flags.Paused ) )
            return;

        foreach( Transform child in transform )
        {
            var rb = child.GetComponent<Rigidbody>();
            SetPhysicsEnabled( rb, true );
            var direction = UnityEngine.Random.insideUnitSphere.SetY( explosionHeightScaleMin + UnityEngine.Random.value * ( explosionHeightScaleMax - explosionHeightScaleMin ) );
            var force = explosionForce + ( UnityEngine.Random.value - 0.5f ) * explosionForceVariance;
            rb.AddForce( direction * force );
        }

        flags |= Flags.Physics;
        flags |= Flags.NoInterp;

        this.CallWithDelay( explodeDelaySec, () =>
        {
            foreach( var child in objects )
                SetPhysicsEnabled( child.obj, false );

            flags &= ~Flags.Physics;
            flags &= ~Flags.NoInterp;
        } );
    }

    public void Freeze()
    {
        flags |= Flags.Paused;

        this.CallWithDelay( freezeDurationSec, () =>
        {
            flags &= ~Flags.Paused;
        } );
    }

    void Update()
    {
        if( !isValid() )
            return;

        while( objects.Count > currentBehaviourData.ObjectCount )
            objects.PopBack().obj.Destroy();
        
        while( transform.childCount < currentBehaviourData.ObjectCount )
            ConstructCube();

        // Changed
        //if( useVoxelPositions != _useVoxelPositions )
        //{
        //    _useVoxelPositions = useVoxelPositions;
        //
        //    if( useVoxelPositions && voxelData != null )
        //    {
        //        UpdateColours( true );
        //    }
        //}

        UpdateVoxels();
        UpdatePhysics();
        UpdateBehaviour();
    }

    Vector3 GetVoxelPosition()
    {
        var pos = currentBehaviourData.GetPosition( currentProcessingVoxel );
        pos = pos.RotateX( funcGenerator.EvaluateTree() );
        //pos = pos.RotateX( Mathf.Sin( Time.fixedTime ) * Mathf.PI * Mathf.Rad2Deg );
        //pos = pos.RotateY( Mathf.Sin( Time.fixedTime ) * Mathf.PI * Mathf.Rad2Deg );
        //pos = pos.RotateZ( Mathf.Sin( Time.fixedTime ) * Mathf.PI * Mathf.Rad2Deg );
        return pos;
    }

    void InterpolatePosition( ObjectData obj )
    {
        var transform = obj.obj.transform;
        var desiredPos = GetVoxelPosition();
        var direction = desiredPos - transform.localPosition;

        if( Mathf.Abs( direction.sqrMagnitude ) <= float.Epsilon )
            return;

        var directionNorm = direction.normalized;
        var speed = direction.magnitude * positionInterpSpeed;
        var delta = Mathf.Min( 1.0f, Time.deltaTime * speed );

        var interpPos = transform.localPosition + new Vector3(
            Mathf.Min( Mathf.Abs( direction.x ), Mathf.Abs( directionNorm.x ) * delta ) * Mathf.Sign( direction.x ),
            Mathf.Min( Mathf.Abs( direction.y ), Mathf.Abs( directionNorm.y ) * delta ) * Mathf.Sign( direction.y ),
            Mathf.Min( Mathf.Abs( direction.z ), Mathf.Abs( directionNorm.z ) * delta ) * Mathf.Sign( direction.z ) );

        transform.localPosition = interpPos;
        transform.localRotation = Quaternion.Slerp( transform.localRotation, currentBehaviourData.GetRotation( currentProcessingVoxel ), Time.deltaTime * rotationInterpSpeed );
    }

    void InterpolateScale( ObjectData obj )
    {
        obj.obj.transform.localScale = Vector3.Lerp( obj.obj.transform.localScale, new Vector3().Set( cubeScale * currentBehaviourData.cubeScale ), Time.deltaTime * scaleInterpSpeed );
    }

    void InterpolateColour( ObjectData obj )
    {
        obj.renderer.sharedMaterial.color = Color.Lerp( obj.renderer.sharedMaterial.color, currentBehaviourData.GetColour( currentProcessingVoxel ) ?? baseColour, Time.deltaTime * colourInterpSpeed );
    }

    void UpdateVoxels()
    {
        if( !flags.HasFlag( Flags.NoInterp ) && !flags.HasFlag( Flags.Paused ) )
        {
            currentBehaviourData.Process();

            for( int i = 0; i < objects.Count; ++i )
            {
                currentProcessingVoxel = i;
                var obj = objects[i];
                InterpolatePosition( obj );
                InterpolateScale( obj );
                InterpolateColour( obj );
            }
        }
    }

    void UpdatePhysics()
    {
#if UNITY_EDITOR
        if( !flags.HasFlag( Flags.Paused ) && flags.HasFlag( Flags.Physics ) && !Application.isPlaying )
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

    void UpdateBehaviour()
    {
        if( randomBehaviour )
        {
            behaviourChangeTimer -= Time.deltaTime;

            if( behaviourChangeTimer <= 0.0f )
            {
                InitRandom();

                var prevBehaviour = currentBehaviourData;
                while( currentBehaviourData == prevBehaviour && allBehaviours.Count > 1 )
                {
                    currentBehaviourData = allBehaviours.RandomItem();
                }
            }
        }
    }

    public BehaviourData GetBehaviour()
    {
        return currentBehaviourData;
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
