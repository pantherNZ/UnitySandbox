using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class ConsoleManager : MonoBehaviour
    {
        [SerializeField] InputField inputField = null;
        [SerializeField] Text consoleText = null;
        [SerializeField] Text autoCompleteText = null;
        [SerializeField] CanvasGroup autoCompleteCanvas = null;
        [SerializeField] int autoCompleteDisplayCount = 10;
        CanvasGroup group;
        string lastCommand;

        // Singleton
        [HideInInspector] static ConsoleManager console;
        [HideInInspector] public static ConsoleManager Instance { get { return console; } }

        Dictionary<string, Action<string, string>> commands = new Dictionary<string, Action<string, string>>();
        List<string> autoCompleteEntries = new List<string>();
        string autoCompleteString;
        int autoCompleteIdx = -1;
        bool lockAutoComplete = false;

        void Awake()
        {
            console = this;

            // Help command
            AddCommand( "help", ( name, param ) =>
            {
                foreach( var command in commands )
                    LogSystem( command.Key );
            } );
        }

        void Start()
        {
            group = GetComponent<CanvasGroup>();
            group.SetVisibility( false );
            autoCompleteCanvas.SetVisibility( false );

            inputField.onEndEdit.AddListener( ProcessCommand );
            inputField.onValueChanged.AddListener( ProcessAutoComplete );
        }

        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        void HandleLog( string logString, string stackTrace, LogType type )
        {
            if( type == LogType.Assert || type == LogType.Error )
            {
                LogError( String.Format( "[{0}]: {1}", type, logString ) );
            }
            else if( type == LogType.Warning )
            {
                LogSystem( String.Format( "[{0}]: {1}", type, logString ) );
            }
            else if( type == LogType.Exception )
            {
                LogError( String.Format( "[{0}]: {1}", type, logString ) );
                LogError( stackTrace );
            }
            else if( type == LogType.Log )
            {
                LogCommand( logString );
            }
        }

        public void Toggle()
        {
            group.ToggleVisibility();
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject( group.interactable ? inputField.gameObject : null );
            Cursor.visible = group.IsVisible();
            Cursor.lockState = group.IsVisible() ? CursorLockMode.None : CursorLockMode.Locked;
        }

        void Update()
        {
            var upArrowPressed = Keyboard.current[Key.UpArrow].wasPressedThisFrame;
            var downArrowPressed = Keyboard.current[Key.DownArrow].wasPressedThisFrame;

            if( upArrowPressed || downArrowPressed )
            {
                if( lastCommand != null && lastCommand.Length > 0 && autoCompleteEntries.IsEmpty() && upArrowPressed )
                {
                    inputField.text = lastCommand;
                }
                else if( !autoCompleteEntries.IsEmpty() )
                {
                    lockAutoComplete = true;
                    var max = Math.Min( autoCompleteDisplayCount, autoCompleteEntries.Count );
                    autoCompleteIdx = Utility.Mod( ( autoCompleteIdx + ( upArrowPressed ? 1 : ( autoCompleteIdx == -1 ? 0 : -1 ) ) ), max );
                    inputField.text = autoCompleteEntries[max - autoCompleteIdx - 1];
                    lockAutoComplete = false;
                }
            }
        }

        void ProcessCommand( string str )
        {
            str = str.Trim().ToLower().Replace( "_", string.Empty );
            var command = str;
            var space = str.IndexOf( ' ' );

            if( space != -1 )
                command = str.Substring( 0, space ).Trim();

            if( commands.ContainsKey( command ) )
            {
                LogCommand( str );
                commands[command].Invoke( command, space == -1 ? string.Empty : str.Substring( space ).Trim() );
            }
            else
            {
                consoleText.text += str + "\n";
            }

            lastCommand = inputField.text;
            inputField.text = string.Empty;
            if( UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != inputField.gameObject )
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject( inputField.gameObject );
        }

        void ProcessAutoComplete( string str )
        {
            if( lockAutoComplete )
                return;

            if( ( autoCompleteString == null || autoCompleteString.Length == 0 ) && str.Length > 0 )
            {
                autoCompleteEntries = commands.Keys.ToList();
            }
            else if( str.Length < autoCompleteString.Length )
            {
                autoCompleteEntries = commands.Keys.Where( ( String s ) => { return !s.Contains( str ); } ).ToList();
            }
            else
            {
                autoCompleteEntries.RemoveAll( ( String s ) => { return !s.Contains( str ); } );
            }

            autoCompleteIdx = -1;
            autoCompleteCanvas.SetVisibility( !autoCompleteEntries.IsEmpty() );
            autoCompleteText.text = String.Join( "\n", autoCompleteEntries.Take( autoCompleteDisplayCount ) );
            autoCompleteString = str;
        }

        public void LogCommand( string str )
        {
            consoleText.text += String.Format( "<color=#0000ffff>{0}</color>\n", str );
        }

        public void LogSystem( string str )
        {
            consoleText.text += String.Format( "<color=#FF8C00ff>{0}</color>\n", str );
        }

        public void LogError( string str )
        {
            consoleText.text += String.Format( "<color=#FF0000ff>{0}</color>\n", str );
        }

        public void AddCommand( string command, Action<string, string> action )
        {
            commands.Add( command.ToLower().Trim().Replace( "_", string.Empty ), action );
        }
    }
}