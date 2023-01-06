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
public abstract class BehaviourData
{
    public int totalObjectCount;
    public float speed;
    public float cubeScale;

    public abstract IEnumerator Construct();
    public abstract Vector3 Animate( Vector3 pos );
    public virtual void Update( float delta ) { }
}