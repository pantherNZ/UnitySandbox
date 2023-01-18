using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

#if UNITY_EDITOR

[CustomEditor( typeof( FlyingObjects ) )]
[CanEditMultipleObjects]
public class FlyingObjectsEditor : Editor
{
    FlyingObjects obj;
    int modifierIndex;
    float modifierDurationSec;
    ReorderableList behavioursList;

    SerializedProperty randomBehaviour;
    SerializedProperty currentBehaviourData;
    SerializedProperty behaviourInterpSpeed;
    SerializedProperty behaviourChangeTimeMin;
    SerializedProperty behaviourChangeTimeMax;

    void OnEnable()
    {
        obj = ( FlyingObjects )target;
        behavioursList = new ReorderableList( serializedObject,
                serializedObject.FindProperty( nameof( obj.allBehaviours ) ),
                true, true, true, true ); ;

        behavioursList.drawHeaderCallback = ( Rect rect ) =>
        {
            EditorGUI.LabelField( rect, "All Behaviours" );
        };

        behavioursList.drawElementCallback = ( Rect rect, int index, bool isActive, bool isFocused ) =>
        {
            var element = behavioursList.serializedProperty.GetArrayElementAtIndex( index );
            rect.y += 2;
            EditorGUI.PropertyField( new Rect( rect.x, rect.y, rect.width - 30, EditorGUIUtility.singleLineHeight ), element, GUIContent.none );
        };

        var prevRandom = obj.randomBehaviour;
        randomBehaviour = serializedObject.FindProperty( nameof( obj.randomBehaviour ) );
        currentBehaviourData = serializedObject.FindProperty( nameof( obj.currentBehaviourData ) );
        behaviourInterpSpeed = serializedObject.FindProperty( nameof( obj.behaviourInterpSpeed ) );
        behaviourChangeTimeMin = serializedObject.FindProperty( nameof( obj.behaviourChangeTimeMin ) );
        behaviourChangeTimeMax = serializedObject.FindProperty( nameof( obj.behaviourChangeTimeMax ) );

        if( obj.randomBehaviour && !prevRandom )
            obj.InitRandom();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        serializedObject.Update();

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

        EditorGUILayout.PropertyField( randomBehaviour );

        if( randomBehaviour.boolValue )
        {
            float changeTimeMin = behaviourChangeTimeMin.floatValue;
            float changeTimeMax = behaviourChangeTimeMax.floatValue;
            float min = 0.0f;
            float max = 30.0f;
            EditorGUILayout.MinMaxSlider( "Change Time Sec", ref changeTimeMin, ref changeTimeMax, min, max );

            var rect = GUILayoutUtility.GetLastRect();
            float minPercent = changeTimeMin / ( max - min );
            var minRect = new Rect( rect.width * 0.4f + rect.width * minPercent * 0.6f, rect.y + EditorGUIUtility.singleLineHeight, 40.0f, EditorGUIUtility.singleLineHeight );
            EditorGUI.LabelField( minRect, changeTimeMin.ToString( "0.00" ) );
            
            if( Mathf.Abs( changeTimeMin - changeTimeMax ) > 0.001f )
            {
                float maxPercent = changeTimeMax / ( max - min );
                var maxRect = new Rect( rect.width * 0.4f + rect.width * maxPercent * 0.6f, rect.y + EditorGUIUtility.singleLineHeight, 40.0f, EditorGUIUtility.singleLineHeight );
                EditorGUI.LabelField( maxRect, changeTimeMax.ToString( "0.00" ) );
            }

            behaviourChangeTimeMin.floatValue = changeTimeMin;
            behaviourChangeTimeMax.floatValue = changeTimeMax;

            EditorGUILayout.Space( EditorGUIUtility.singleLineHeight );
        }

        EditorGUILayout.PropertyField( currentBehaviourData );
        EditorGUILayout.PropertyField( behaviourInterpSpeed );

        if( obj.GetBehaviour() != null )
        {
            if( GUILayout.Button( "Randomise Variables" ) )
                obj.GetBehaviour().Randomise();

            EditorGUILayout.Space( 10.0f );

            var modifiers = obj.GetBehaviour().GetModifierNames();

            if( modifiers != null && modifiers.Length > 0 )
            {
                modifierDurationSec = EditorGUILayout.FloatField( "Modifier Duration (sec)", modifierDurationSec );
                modifierIndex = EditorGUILayout.Popup( "Modifier Index", modifierIndex, modifiers );

                if( GUILayout.Button( "Trigger Modifier" ) )
                    obj.GetBehaviour().ActivateModifier( obj, modifierIndex, modifierDurationSec );
            }

            EditorGUILayout.Space( 10.0f );
        }

        behavioursList.DoLayoutList();

        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    public override bool RequiresConstantRepaint()
    {
        return true;
    }
}

#endif