using System;
using System.Collections;
using UnityEngine;

// Base class
[Serializable]
public abstract class MathShapeData : BehaviourData
{
}

// Circle
[Serializable]
public class CircleShapeData : MathShapeData
{
    [SerializeField] float radius = 1.0f;
    [SerializeField] bool rotateWithMovement;

    public override IEnumerator Construct()
    {
        float polarIncrement = MathF.PI / ( totalObjectCount / 2 );

        for( int i = 0; i < totalObjectCount; i++ )
        {
            float polarAngle = polarIncrement * i;
            float x = radius * MathF.Cos( polarAngle );
            float z = radius * MathF.Sin( polarAngle );
            yield return new Vector3( x, 0.0f, z );
        }
    }

    public override Vector3 Animate( Vector3 pos )
    {
        return pos.RotateY( speed );
    }
}

// Sphere
[Serializable]
public class SphereShapeData : MathShapeData
{
    public float radius;

    public override IEnumerator Construct()
    {
        float polarIncrement = MathF.PI / ( totalObjectCount / 2 );
        float azimuthalIncrement = 2 * MathF.PI / totalObjectCount;

        for( int i = 0; i < totalObjectCount / 2; i++ )
        {
            float polarAngle = polarIncrement * i;
            float r = radius * MathF.Cos( polarAngle );

            for( int j = 0; j < totalObjectCount; j++ )
            {
                float azimuthalAngle = azimuthalIncrement * j;
                float x = r * MathF.Cos( azimuthalAngle );
                float y = r * MathF.Sin( azimuthalAngle );
                float z = radius * MathF.Sin( polarAngle );
                yield return new Vector3( x, y, z );
            }
        }
    }

    public override Vector3 Animate( Vector3 pos )
    {
        return pos.RotateY( speed );
    }
}

// Wall

// Cube

// Rubics cube style animation

// snake

// Sin wave

// Sphere

// Spinning pyramid

// Double pendulum simulation