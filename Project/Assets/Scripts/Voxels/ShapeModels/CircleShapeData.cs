﻿using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu( fileName = "CircleShapeData", menuName = "ScriptableObjs/CircleShapeData" )]
public class CircleShapeData : MathShapeData
{
    [Flags]
    public enum ModifierTypes
    {
        Rotate = 1,
        Bounce = 2,
        Expand = 4,
        PerVoxel = 8,
        Vertical = 16,
    }

    [SerializeField] float radius = 1.0f;
    [SerializeField] bool matchCircleRotation;
    [SerializeField] float speedPerVoxel;
    [SerializeField] ModifierTypes modifiers;

    [Header( "Modifier Settings" )]
    [SerializeField] float perVoxelExpand = 2.0f;
    [SerializeField] float perVoxelHeight = 1.0f;
    [SerializeField] float bounceHeight = 1.0f;
    [SerializeField] float expandWidth = 1.0f;

    float perVoxelTimer;

    public override Enum Modifiers { get => modifiers; set => modifiers = ( ModifierTypes )value; }

    public override Vector3 GetPosition( int idx )
    {
        float polarIncrement = 2.0f * MathF.PI / objectCount;
        float polarAngle = polarIncrement * idx;
        
        if( modifiers.HasFlag( ModifierTypes.Rotate ) )
            polarAngle = ( polarIncrement + ( modifiers.HasFlag( ModifierTypes.PerVoxel ) ? perVoxelTimer : 0.0f ) ) * idx + timer;

        float expandOffset = modifiers.HasFlag( ModifierTypes.PerVoxel ) ? Mathf.PI * perVoxelExpand / objectCount * idx : 0.0f;
        float extraRadius = modifiers.HasFlag( ModifierTypes.Expand ) ? expandWidth * Mathf.Sin( timer + expandOffset ) : 0.0f;
        float r = radius + extraRadius;
        var pos = new Vector3( r * MathF.Cos( polarAngle ), 0.0f, r * MathF.Sin( polarAngle ) );

        if( modifiers.HasFlag( ModifierTypes.Bounce ) )
        {
            float heightOffset = modifiers.HasFlag( ModifierTypes.PerVoxel ) ? Mathf.PI * perVoxelHeight / objectCount * idx : 0.0f;
            pos.y += bounceHeight * Mathf.Sin( timer + heightOffset );
        }

        if( modifiers.HasFlag( ModifierTypes.Vertical ) )
            pos = new Vector3( pos.x, pos.z, pos.y );

        return pos;
    }

    public override Quaternion GetRotation( int idx )
    {
        if( matchCircleRotation )
            return Quaternion.LookRotation( ( GetPosition( idx ).RotateY( 90.0f ) * 0.001f + GetPosition( idx ) ).normalized, Vector3.up );
        return Quaternion.identity;
    }

    public override void ActivateModifier( MonoBehaviour obj, Enum newModifiers, float durationSec )
    {
        modifiers |= ( ModifierTypes )newModifiers;
        obj.CallWithDelay( durationSec, () => modifiers &= ~( ModifierTypes )( newModifiers ) );
    }

    public override void Process()
    {
        base.Process();

        if( modifiers.HasFlag( ModifierTypes.PerVoxel ) )
            perVoxelTimer += Time.deltaTime * speedPerVoxel;
    }

    public override void Randomise()
    {
        base.Randomise();
        radius = UnityEngine.Random.Range( 0.5f, 4.0f );
        speedPerVoxel = UnityEngine.Random.value > 0.5f ? UnityEngine.Random.Range( 0.0f, 2.0f ) : 0.0f;
        perVoxelExpand = UnityEngine.Random.value > 0.5f ? UnityEngine.Random.Range( 0.0f, 8.0f ) : UnityEngine.Random.Range( 0, 4 ) * 2;
        perVoxelHeight = UnityEngine.Random.value > 0.5f ? UnityEngine.Random.Range( 0.0f, 8.0f ) : UnityEngine.Random.Range( 0, 4 ) * 2;
        bounceHeight = UnityEngine.Random.Range( 0.0f, 4.0f );
        expandWidth = UnityEngine.Random.Range( 0.0f, 4.0f );
        matchCircleRotation = UnityEngine.Random.value > 0.8f;
    }
}