using System;
using UnityEngine;

public partial class PlayerController : MonoBehaviour, PlayerInput.IPlayerActions, PlayerInput.IConsoleActions, ISavableComponent
{
    void SetupCommands()
    {
        var console = UI.ConsoleManager.Instance;

        console.AddCommand( "SaveGame", ( name, param ) =>
        {
            if( !SaveGameSystem.SaveGame( param ) )
                console.LogSystem( "Failed to save game" );
            else
                console.LogSystem( "Successfully saved game" );
        } );

        console.AddCommand( "LoadGame", ( name, param ) =>
        {
            if( !SaveGameSystem.LoadGame( param ) )
                console.LogSystem( "No matching save game found" );
            else
                console.LogSystem( "Successfully loaded game" );
        } );

        console.AddCommand( "SetPosition", ( name, param ) =>
        {
            var components = param.Split( ' ' );

            if( components.Length < 2 || !float.TryParse( components[0], out float x ) || !float.TryParse( components[1], out float y ) )
            {
                console.LogError( "Invalid position (format = 'SetPosition X Y')" );
                return;
            }

            transform.position = new Vector3( x, y, transform.position.z );
        } );
    }
}