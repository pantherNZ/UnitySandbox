using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( FlyingObjects ) )]
[CanEditMultipleObjects]
public class FlyingObjectsEditor : Editor
{
    FlyingObjects obj;

    void OnEnable()
    {
        obj = ( FlyingObjects )target;
    }

    public override void OnInspectorGUI()
    {
        if( DrawDefaultInspector() )
            obj.Init();
    }
}