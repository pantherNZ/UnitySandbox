using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

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

        if( GUILayout.Button( "Explode" ) )
            obj.Explode();
    }

    public override bool RequiresConstantRepaint()
    {
        return true;
    }
}

#endif