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
    [Range(1,256)]
    [SerializeField] protected int objectCount = 10;
    [SerializeField] protected float speed;
    public float cubeScale = 1.0f;
    protected float timer;

    public virtual int ObjectCount => objectCount;
    public abstract Vector3 GetPosition( int idx );
    public virtual Color? GetColour( int idx ) { return null; }
    public virtual Quaternion GetRotation( int idx ) { return Quaternion.identity; }
    public int GetNumModifiers() { var names = GetModifierNames(); return names == null ? 0 : names.Length; }
    public virtual string[] GetModifierNames() { return null; }
    public virtual void ActivateModifier( MonoBehaviour obj, int idx, float durationSec ) { }
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