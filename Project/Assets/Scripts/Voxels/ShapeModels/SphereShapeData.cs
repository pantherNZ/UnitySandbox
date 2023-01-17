using System;
using System.Collections;
using UnityEngine;

[Serializable]
[CreateAssetMenu( fileName = "SphereShapeData", menuName = "ScriptableObjs/SphereShapeData" )]
public class SphereShapeData : MathShapeData
{
    [SerializeField] float radius;
    [SerializeField] bool matchSphereRotation;

    public override Vector3 GetPosition( int idx )
    {
        var phi = Mathf.PI * ( 3.0f - Mathf.Sqrt( 5.0f ) );  // golden angle in radians
        var y = 1 - ( idx / ( ObjectCount - 1.0f ) ) * 2.0f;  // y goes from 1 to -1
        var r = Mathf.Sqrt( 1.0f - y * y );  // radius at y
        var theta = phi * idx; // golden angle increment
        var x = Mathf.Cos( theta ) * r;
        var z = Mathf.Sin( theta ) * r;
        return new Vector3( x, y, z ) * radius ;
    }

    public override Quaternion GetRotation( int idx )
    {
        if( matchSphereRotation )
            return Quaternion.LookRotation( GetPosition( idx ).normalized, Vector3.up );
        return Quaternion.identity;
    }
}