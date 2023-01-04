using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public static class GlobalKeyEvents
{
    // Exposed delegates to hook up your methods to them.
    public static event Action<KeyCode, EventModifiers> GlobalKeyDown;
    public static event Action<KeyCode, EventModifiers> GlobalKeyUp;
    public static event Action AnyShortcutTriggered;

    [InitializeOnLoadMethod]
    private static void EditorInit()
    {
        RegisterToGlobalEventHandler();
        RegisterToTriggeredAnyShortcut();
    }

    private static bool OnAnyShortcutTriggered()
    {
        AnyShortcutTriggered?.Invoke();
        return true;
    }

    private static void RegisterToGlobalEventHandler()
    {
        FieldInfo info = typeof( EditorApplication ).GetField( "globalEventHandler", BindingFlags.Static | BindingFlags.NonPublic );
        EditorApplication.CallbackFunction value = ( EditorApplication.CallbackFunction )info.GetValue( null );
        value += OnGlobalKeyPressed;
        info.SetValue( null, value );
    }

    private static void RegisterToTriggeredAnyShortcut()
    {
        FieldInfo info = typeof( EditorApplication ).GetField( "doPressedKeysTriggerAnyShortcut", BindingFlags.Static | BindingFlags.NonPublic );
        Func<bool> value = ( Func<bool> )info.GetValue( null );
        value += OnAnyShortcutTriggered;
        info.SetValue( null, value );
    }

    private static void OnGlobalKeyPressed()
    {
        if( Event.current.type == EventType.KeyDown )
        {
            KeyCode key1 = Event.current.keyCode;

            switch( key1 )
            {
                case KeyCode.F10:
                    {

                        if( Application.isPlaying )
                        {
#if UNITY_EDITOR
                            UnityEditor.EditorApplication.isPlaying = false;
#endif
                        }
                        else
                        {
                            UnityEditor.AssetDatabase.Refresh();
                            UnityEditor.EditorApplication.EnterPlaymode();
                        }
                    }
                    break;

            }

            GlobalKeyDown?.Invoke( Event.current.keyCode, Event.current.modifiers );
        }
        else if( Event.current.type == EventType.KeyUp )
        {
            GlobalKeyUp?.Invoke( Event.current.keyCode, Event.current.modifiers );
        }
    }
}