using System;
using System.Collections;
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
    [Range(1,500)]
    [SerializeField] protected int objectCount = 10;
    [SerializeField] protected float speed;
    public float cubeScale = 1.0f;
    protected float timer;

    public virtual int ObjectCount => objectCount;
    public abstract Vector3 GetPosition( int idx );
    public virtual Color? GetColour( int idx ) { return null; }
    public virtual Quaternion GetRotation( int idx ) { return Quaternion.identity; }

    public virtual void Process() 
    {
        timer += Time.deltaTime * speed;
    }
}