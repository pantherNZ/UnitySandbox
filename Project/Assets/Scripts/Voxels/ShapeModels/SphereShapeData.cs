using System;
using System.Collections;
using UnityEngine;

[Serializable]
[CreateAssetMenu( fileName = "SphereShapeData", menuName = "ScriptableObjs/SphereShapeData" )]
public class SphereShapeData : MathShapeData
{
    public override int ObjectCount { get 
        {
            if( _objectCount == null )
            {
                lines = Mathf.RoundToInt( Mathf.Sqrt( 2.0f * objectCount ) );
                _objectCount = lines * lines / 2;
            }

            return _objectCount.Value;
        } }
    int? _objectCount;
    int lines;
    [SerializeField] float radius;

    public override Vector3 GetPosition( int idx )
    {
        float increment = 2.0f * MathF.PI / lines;
        int i = idx / ( lines / 2 );
        int j = idx % lines;
        float polarAngle = increment * i;
        float r = radius * MathF.Cos( polarAngle );
        float azimuthalAngle = increment * j;
        float x = r * MathF.Cos( azimuthalAngle );
        float y = r * MathF.Sin( azimuthalAngle );
        float z = radius * MathF.Sin( polarAngle );
        return new Vector3( x, y, z );

        //for( int i = 0; i < totalObjectCount / ; i++ )
        //{
        //    float polarAngle = polarIncrement * i;
        //    float r = radius * MathF.Cos( polarAngle );
        //
        //    for( int j = 0; j < totalObjectCount; j++ )
        //    {
        //        float azimuthalAngle = azimuthalIncrement * j;
        //        float x = r * MathF.Cos( azimuthalAngle );
        //        float y = r * MathF.Sin( azimuthalAngle );
        //        float z = radius * MathF.Sin( polarAngle );
        //        yield return new Vector3( x, y, z );
        //    }
        //}
    }
}