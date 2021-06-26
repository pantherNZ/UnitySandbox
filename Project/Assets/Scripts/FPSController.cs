using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System;

public class FPSController : MonoBehaviour, PlayerInput.IPlayerActions
{
    [SerializeField]
    private float moveSpeed = 10.0f;
    [SerializeField]
    private float rotateSpeed = 60.0f;
    [SerializeField]
    private float raycastLength = 50.0f;

    private @PlayerInput input;
    private Camera fpsCamera;
    private Vector2 moveVector;
    private Vector2 lookVector;
    private Vector2 rotation;
    private List<IBasePlayerTool> tools;
    private int currentTool;
    private Int32 playerId;

    private void Start()
    {
        tools = GetComponentsInChildren<IBasePlayerTool>().ToList();
        input = new PlayerInput();
        input.Player.SetCallbacks( this );
        input.Player.Enable();
        fpsCamera = GetComponentInChildren<Camera>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public IBasePlayerTool GetCurrentTool()
    {
        return tools[currentTool];
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
            currentTool = ( currentTool + diff ) % tools.Count;
        }
    }

    public void OnShowPauseMenu( InputAction.CallbackContext context )
    {
        if( context.performed )
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

    private void SelectTool( int index )
    {
        int newToolIdx = Mathf.Min( index, tools.Count - 1 );
        if( newToolIdx != currentTool )
        {
            GetCurrentTool().SetEnabled( false );
            currentTool = newToolIdx;
            GetCurrentTool().SetEnabled( true );
        }
    }

    public void FixedUpdate()
    {
        // Update orientation first, then move. Otherwise move orientation will lag
        // behind by one frame.
        Look(lookVector);
        Move(moveVector);
    }

    private void Move(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.01)
            return;
        var scaledMoveSpeed = moveSpeed * Time.fixedDeltaTime;
        // For simplicity's sake, we just keep movement in a single plane here. Rotate
        // direction according to world Y rotation of player.
        var move = Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(direction.x, 0, direction.y);
        transform.position += move * scaledMoveSpeed;
    }

    private void Look(Vector2 rotate)
    {
        if (rotate.sqrMagnitude < 0.01)
            return;
        var scaledRotateSpeed = rotateSpeed * Time.fixedDeltaTime;
        rotation.y += rotate.x * scaledRotateSpeed;
        rotation.x = Mathf.Clamp(rotation.x - rotate.y * scaledRotateSpeed, -89, 89);
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
        var sandboxObj = obj.GetComponent<SandboxObject>();
        return sandboxObj != null && sandboxObj.ownerId == playerId;
    }
}
