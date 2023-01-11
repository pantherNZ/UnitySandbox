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

    [Header( "Behaviour" )]
    [SerializeField] BehaviourData currentBehaviourData;
    BehaviourData _currentBehaviourData;

    [Header( "Explode" )]
    [SerializeField] float explosionForce = 10.0f;
    [SerializeField] float explosionForceVariance = 10.0f;
    [SerializeField] float explodeDelaySec = 10.0f;
    [SerializeField] float explosionHeightScaleMin;
    [SerializeField] float explosionHeightScaleMax;

    private float time;
    private float inactiveTimer;
    private float physicsTimestep;
    private IEnumerator constuctionIterator;

    private readonly List<ObjectData> objects = new();

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
    }

    public void Explode()
    {
        if( inactiveTimer > 0.0f )
            return;

        foreach( Transform child in transform )
        {
            var rb = child.GetComponent<Rigidbody>();
            SetPhysicsEnabled( rb, true );
            var direction = UnityEngine.Random.insideUnitSphere.SetY( explosionHeightScaleMin + UnityEngine.Random.value * ( explosionHeightScaleMax - explosionHeightScaleMin ) );
            var force = explosionForce + ( UnityEngine.Random.value - 0.5f ) * explosionForceVariance;
            rb.AddForce( direction * force );
            inactiveTimer = explodeDelaySec;
        }
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
    }

    void InterpolatePosition( ObjectData obj, int idx )
    {
        var transform = obj.obj.transform;
        var desiredPos = currentBehaviourData.GetPosition( idx );
        var direction = desiredPos - transform.localPosition;
        var directionNorm = direction.normalized;
        var speed = direction.magnitude * positionInterpSpeed;
        var delta = Mathf.Min( 1.0f, Time.deltaTime * speed );

        var interpPos = transform.localPosition + new Vector3(
            Mathf.Min( Mathf.Abs( direction.x ), Mathf.Abs( directionNorm.x ) * delta ) * Mathf.Sign( direction.x ),
            Mathf.Min( Mathf.Abs( direction.y ), Mathf.Abs( directionNorm.y ) * delta ) * Mathf.Sign( direction.y ),
            Mathf.Min( Mathf.Abs( direction.z ), Mathf.Abs( directionNorm.z ) * delta ) * Mathf.Sign( direction.z ) );

        transform.localPosition = interpPos;
        transform.localRotation = Quaternion.Slerp( transform.localRotation, currentBehaviourData.GetRotation( idx ), Time.deltaTime * rotationInterpSpeed );
    }

    void InterpolateScale( ObjectData obj, int idx )
    {
        obj.obj.transform.localScale = Vector3.Lerp( obj.obj.transform.localScale, new Vector3().Set( cubeScale * currentBehaviourData.cubeScale ), Time.deltaTime * scaleInterpSpeed );
    }

    void InterpolateColour( ObjectData obj, int idx )
    {
        obj.renderer.sharedMaterial.color = Color.Lerp( obj.renderer.sharedMaterial.color, currentBehaviourData.GetColour( idx ) ?? baseColour, Time.deltaTime * colourInterpSpeed );
    }

    void UpdateVoxels()
    {
        if( inactiveTimer == 0.0f )
        {
            currentBehaviourData.Process();

            for( int i = 0; i < objects.Count; ++i )
            {
                var obj = objects[i];
                InterpolatePosition( obj, i );
                InterpolateScale( obj, i );
                InterpolateColour( obj, i );
            }
        }
    }

    void UpdatePhysics()
    {
#if UNITY_EDITOR
        if( inactiveTimer > 0.0f && !Application.isPlaying )
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

        if( inactiveTimer > 0.0f )
        {
            inactiveTimer = Mathf.Max( 0.0f, inactiveTimer - Time.deltaTime );

            if( inactiveTimer == 0.0f )
                foreach( var child in objects )
                    SetPhysicsEnabled( child.obj, false );
        }
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
