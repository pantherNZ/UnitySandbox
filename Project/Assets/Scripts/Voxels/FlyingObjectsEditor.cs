using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

[CustomEditor( typeof( FlyingObjects ) )]
[CanEditMultipleObjects]
public class FlyingObjectsEditor : Editor
{
    FlyingObjects obj;
    int modifierIndex;
    float modifierDurationSec;


    void OnEnable()
    {
        obj = ( FlyingObjects )target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space( 10.0f );

        if( GUILayout.Button( "Explode" ) )
            obj.Explode();

        if( GUILayout.Button( "Freeze" ) )
            obj.Freeze();

        if( GUILayout.Button( "Update" ) )
            obj.Init();

        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space( 10.0f );
        EditorGUILayout.LabelField( "<b>Behaviour</b>", new GUIStyle() { richText = true } );

        obj.currentBehaviourData = ( BehaviourData )EditorGUILayout.ObjectField( "Current Behaviour", obj.currentBehaviourData, typeof( BehaviourData ), false );

        if( obj.GetBehaviour() != null )
        {
            if( GUILayout.Button( "Randomise Variables" ) )
                obj.GetBehaviour().Randomise();

            EditorGUILayout.Space( 10.0f );

            var modifiers = obj.GetBehaviour().GetModifierNames();

            if( modifiers.Length > 0 )
            {
                modifierDurationSec = EditorGUILayout.FloatField( "Modifier Duration (sec)", modifierDurationSec );
                modifierIndex = EditorGUILayout.Popup( "Modifier Index", modifierIndex, modifiers );

                if( GUILayout.Button( "Trigger Modifier" ) )
                    obj.GetBehaviour().ActivateModifier( obj, modifierIndex, modifierDurationSec );
            }
        }

        EditorGUILayout.EndVertical();
    }

    public override bool RequiresConstantRepaint()
    {
        return true;
    }
}

#endif