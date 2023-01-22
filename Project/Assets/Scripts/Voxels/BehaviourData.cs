using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum FlyingBehaviourType
{
    MathShape,
    Steering,
    VoxelShape,
}

[Serializable]
public enum TransitionType
{
    Linear,
    Reverse,
    Random,
}

// Base class
[Serializable]
public abstract class BehaviourData : ScriptableObject
{
    [Flags]
    public enum GenericModifierTypes
    {
        RotateX = 1,
        RotateY = 1,
        RotateZ = 1,
        Bounce = 2,
        Expand = 4,
        PerVoxel = 8,
        Vertical = 16,
    }


    [Range(1,256)]
    [SerializeField] protected int objectCount = 10;
    [SerializeField] protected float speed;
    public float cubeScale = 1.0f;
    protected float timer;

    public virtual int ObjectCount => objectCount;
    public virtual Enum Modifiers { get => null; set { } }
    public abstract Vector3 GetPosition( int idx );
    public virtual Color? GetColour( int idx ) { return null; }
    public virtual Quaternion GetRotation( int idx ) { return Quaternion.identity; }
    public virtual void ActivateModifier( MonoBehaviour obj, Enum modifiers, float durationSec ) { }
    public virtual void Randomise()
    {
        objectCount = UnityEngine.Random.Range( 1, 128 );
        speed = UnityEngine.Random.Range( 0.0f, 10.0f );
    }

    public virtual void Process() 
    {
        timer += Time.deltaTime * speed;
    }
}