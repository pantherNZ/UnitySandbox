using System;
using System.Collections;
using UnityEngine;

[Serializable]
[CreateAssetMenu( fileName = "CircleShapeData", menuName = "ScriptableObjs/CircleShapeData" )]
public class CircleShapeData : MathShapeData
{
    [SerializeField] float radius = 1.0f;
    [SerializeField] bool rotateWithMovement;
    [SerializeField] float speedPerVoxel;
    float perVoxelTimer;

    public override Vector3 GetPosition( int idx )
    {
        float polarIncrement = 2.0f * MathF.PI / objectCount + perVoxelTimer;
        float polarAngle = polarIncrement * idx + timer;
        float x = radius * MathF.Cos( polarAngle );
        float z = radius * MathF.Sin( polarAngle );
        return new Vector3( x, 0.0f, z );
    }

    public override Quaternion GetRotation( int idx )
    {
        if( rotateWithMovement )
            return Quaternion.LookRotation( ( GetPosition( idx ).RotateY( 90.0f ) * 0.001f + GetPosition( idx ) ).normalized, Vector3.up );
        return Quaternion.identity;
    }

    public override void Process()
    {
        base.Process();

        perVoxelTimer += Time.deltaTime * speedPerVoxel;
    }
}