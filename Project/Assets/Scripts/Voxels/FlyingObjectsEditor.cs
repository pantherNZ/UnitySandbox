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
        {
            //obj.Init();
        }

        if( GUILayout.Button( "Explode" ) )
            obj.Explode();

        if( GUILayout.Button( "Re-Initialise" ) )
            obj.Init();
    }

    public override bool RequiresConstantRepaint()
    {
        return true;
    }
}

#endif