using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System;

public partial class PlayerController : MonoBehaviour, PlayerInput.IPlayerActions, PlayerInput.IConsoleActions, ISavableComponent
{
    public const Int32 ToolBarMaxItems = 8;

    public event Action<ToolBoundEventArgs> ToolBoundEvent;
    public event Action<ToolSelectedEventArgs> ToolSelectedEvent;

    [SerializeField]
    private float moveSpeed = 10.0f;
    [SerializeField]
    private float rotateSpeed = 60.0f;
    [SerializeField]
    private float raycastLength = 50.0f;

    [HideInInspector]
    public Int32 playerId { get; private set; }
    private PlayerInput input;
    private Camera fpsCamera;
    private Vector2 moveVector;
    private Vector2 lookVector;
    private Vector2 rotation;

    private List<IBasePlayerTool> allTools;
    private IBasePlayerTool[] boundTools = new IBasePlayerTool[ToolBarMaxItems];
    private int currentToolIndex;

    private void Start()
    {
        fpsCamera = GetComponentInChildren<Camera>();

        InitialiseTools();
        InitialiseInput();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void InitialiseInput()
    {
        input = new PlayerInput();
        input.Console.SetCallbacks( this );
        input.Player.SetCallbacks( this );
        input.Player.Enable();
    }

    private void InitialiseTools()
    {
        allTools = GetComponentsInChildren<IBasePlayerTool>().ToList();

        for( int i = 0; i < ToolBarMaxItems; ++i )
            BindTool( i < allTools.Count ? allTools[i] : null, i, true );
    }

    public IBasePlayerTool GetCurrentTool()
    {
        if( currentToolIndex < 0 || currentToolIndex >= boundTools.Length )
            return null;
        return boundTools[currentToolIndex];
    }

    public Int32 GetCurrentToolIndex()
    {
        return currentToolIndex;
    }

    public void OnMove( InputAction.CallbackContext context )
    {
        moveVector = context.ReadValue<Vector2>();
    }

    public void OnLook( InputAction.CallbackContext context )
    {
        if( !GetCurrentTool().OnLook( lookVector ) )
            lookVector = context.ReadValue<Vector2>();
    }

    public void OnFire( InputAction.CallbackContext context )
    {
        if( context.started )
            GetCurrentTool().OnMouse1( true );
        else if( context.canceled )
            GetCurrentTool().OnMouse1( false );
    }

    public void OnAltFire( InputAction.CallbackContext context )
    {
        if( context.started )
            GetCurrentTool().OnMouse2( true );
        else if( context.canceled )
            GetCurrentTool().OnMouse2( false );
    }

    public void OnSpecialAction( InputAction.CallbackContext context )
    {
        if( context.started )
            GetCurrentTool().OnSpecialAction( true );
        else if( context.canceled )
            GetCurrentTool().OnSpecialAction( false );
    }

    public void OnSpecialActionAlt( InputAction.CallbackContext context )
    {
        if( context.started )
            GetCurrentTool().OnSpecialActionAlt( true );
        else if( context.canceled )
            GetCurrentTool().OnSpecialActionAlt( false );
    }

    public void OnMouseWheel( InputAction.CallbackContext context )
    {
        if( context.started )
            GetCurrentTool().OnMouseWheel( true );
        else if( context.canceled )
            GetCurrentTool().OnMouseWheel( false );
    }

    public void OnMouseWheelScroll( InputAction.CallbackContext context )
    {
        var v = context.ReadValue<float>();
        if( !GetCurrentTool().OnMouseWheelScroll( v ) )
        {
            var diff = ( int )Mathf.Sign( v );
            SelectTool( ( currentToolIndex + diff ) % ToolBarMaxItems );
        }
    }

    public void OnShowPauseMenu( InputAction.CallbackContext context )
    {
        if( context.started )
            Application.Quit();
    }

    public void OnShowToolsMenu( InputAction.CallbackContext context )
    {

    }

    public void OnToolSelections1( InputAction.CallbackContext context ) { if( context.started ) SelectTool( 0 ); }
    public void OnToolSelections2( InputAction.CallbackContext context ) { if( context.started ) SelectTool( 1 ); }
    public void OnToolSelections3( InputAction.CallbackContext context ) { if( context.started ) SelectTool( 2 ); }
    public void OnToolSelections4( InputAction.CallbackContext context ) { if( context.started ) SelectTool( 3 ); }
    public void OnToolSelections5( InputAction.CallbackContext context ) { if( context.started ) SelectTool( 4 ); }
    public void OnToolSelections6( InputAction.CallbackContext context ) { if( context.started ) SelectTool( 5 ); }
    public void OnToolSelections7( InputAction.CallbackContext context ) { if( context.started ) SelectTool( 6 ); }
    public void OnToolSelections8( InputAction.CallbackContext context ) { if( context.started ) SelectTool( 7 ); }
    public void OnToolSelections9( InputAction.CallbackContext context ) { if( context.started ) SelectTool( 8 ); }

    void PlayerInput.IConsoleActions.OnShowConsole( InputAction.CallbackContext context )
    {
        if( context.started )
            ToggleConsole();
    }

    void PlayerInput.IPlayerActions.OnShowConsole( InputAction.CallbackContext context )
    {
        if( context.started )
            ToggleConsole();
    }

    public void OnUndo( InputAction.CallbackContext context )
    {
        if( context.started )
            if( UndoRedoSystem.GetInstance().UndoAction() )
                Debug.Log( "Undo" );
    }

    public void OnRedo( InputAction.CallbackContext context )
    {
        if( context.started )
            if( UndoRedoSystem.GetInstance().RedoAction() )
                Debug.Log( "Redo" );
    }

    private void ToggleConsole()
    {
        if( input.Player.enabled )
        {
            input.Console.Enable();
            input.Player.Disable();
        }
        else
        {
            input.Console.Disable();
            input.Player.Enable();
        }

        UI.ConsoleManager.Instance.Toggle();
    }

    private void SelectTool( int index )
    {
        int newToolIdx = Mathf.Clamp( index, 0, ToolBarMaxItems - 1 );
        if( newToolIdx != currentToolIndex && boundTools[newToolIdx] != null )
        {
            GetCurrentTool().SetEnabled( false );

            var oldToolIndex = currentToolIndex;
            currentToolIndex = newToolIdx;

            GetCurrentTool().SetEnabled( true );
            ToolSelectedEvent?.Invoke( new ToolSelectedEventArgs() { player = this, oldToolIndex = oldToolIndex, newToolIndex = currentToolIndex } );
            Debug.Log( String.Format( "Selecting tool {0} at index {1}", GetCurrentTool().GetName(), index ) );
        }
    }

    public void BindTool( IBasePlayerTool tool, Int32 index, bool forceUpdate = false )
    {
        if( index >= ToolBarMaxItems || index < 0 || ( boundTools[index] == tool && !forceUpdate ) )
            return;

        // Current tool will be null on startup when we bind our first tool
        if( index == currentToolIndex && GetCurrentTool() != null )
            GetCurrentTool().SetEnabled( false );

        boundTools[index] = tool;
        ToolBoundEvent?.Invoke( new ToolBoundEventArgs() { player = this, newTool = tool, toolIndex = index } );
        Debug.Log( String.Format( "Binding tool {0} to index {1}", GetCurrentTool().GetName(), index ) );

        if( index == currentToolIndex )
        {
            GetCurrentTool().SetEnabled( true );
            ToolSelectedEvent?.Invoke( new ToolSelectedEventArgs() { player = this, oldToolIndex = index, newToolIndex = index } );
            Debug.Log( String.Format( "Selecting tool {0} at index {1}", GetCurrentTool().GetName(), index ) );
        }
    }

    public IBasePlayerTool[] GetBoundTools()
    {
        return boundTools;
    }

    public void FixedUpdate()
    {
        // Update orientation first, then move. Otherwise move orientation will lag
        // behind by one frame.
        Look( lookVector );
        Move( moveVector );
    }

    private void Move( Vector2 direction )
    {
        if( direction.sqrMagnitude < 0.01 )
            return;
        var scaledMoveSpeed = moveSpeed * Time.fixedDeltaTime;
        // For simplicity's sake, we just keep movement in a single plane here. Rotate
        // direction according to world Y rotation of player.
        var move = Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(direction.x, 0, direction.y);
        transform.position += move * scaledMoveSpeed;
    }

    private void Look( Vector2 rotate )
    {
        if( rotate.sqrMagnitude < 0.01 )
            return;
        var scaledRotateSpeed = rotateSpeed * Time.fixedDeltaTime;
        rotation.y += rotate.x * scaledRotateSpeed;
        rotation.x = Mathf.Clamp( rotation.x - rotate.y * scaledRotateSpeed, -89, 89 );
        fpsCamera.transform.localEulerAngles = rotation.SetY( 0.0f );
        transform.localEulerAngles = rotation.SetX( 0.0f );
    }

    public Vector3 GetLookRotation()
    {
        return transform.localEulerAngles.SetX( fpsCamera.transform.localEulerAngles.x );
    }

    public Camera GetFPSCamera()
    {
        return fpsCamera;
    }

    public bool Raycast( out RaycastHit hitInfo )
    {
        var origin = fpsCamera.transform.position;
        var direction = fpsCamera.transform.forward;
        var result = Physics.Raycast( origin, direction, out hitInfo, raycastLength, LayerMask.GetMask( "Default" ) );
        Debug.DrawLine( origin, origin + direction * raycastLength, Color.blue, 10.0f );
        return result;
    }

    public bool IsValidOwnedObject( GameObject obj )
    {
        if( obj == null )
            return false;
        var data = obj.GetComponent<SandboxObjectData>();
        return data != null && data.ownerId == playerId;
    }
    public void Serialise( System.IO.BinaryWriter writer )
    {
        writer.Write( transform.position );
    }

    public void Deserialise( System.IO.BinaryReader reader )
    {
        transform.position = reader.ReadVector3();
    }
}