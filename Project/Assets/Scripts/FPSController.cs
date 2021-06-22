using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class FPSController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 10.0f;
    [SerializeField]
    private float rotateSpeed = 60.0f;

    private Camera fpsCamera;
    private Vector2 moveVector;
    private Vector2 lookVector;
    private Vector2 rotation;
    private List<IBasePlayerTool> tools;
    private int currentTool;

    private void Start()
    {
        fpsCamera = GetComponentInChildren<Camera>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        tools = GetComponentsInChildren<IBasePlayerTool>().ToList();
    }

    public IBasePlayerTool GetCurrentTool()
    {
        return tools[currentTool];
    }

    public void OnMove( InputValue value )
    {
        moveVector = value.Get<Vector2>();
    }

    public void OnLook( InputValue value )
    {
        if( !GetCurrentTool().OnLook( lookVector ) )
            lookVector = value.Get<Vector2>();
    }

    public void OnFire( InputValue value )
    {
        GetCurrentTool().OnMouse1( value.isPressed );
    }

    public void OnAltFire( InputValue value )
    {
        GetCurrentTool().OnMouse2( value.isPressed );
    }

    public void OnSpecialAction( InputValue value )
    {
        GetCurrentTool().OnSpecialAction( value.isPressed );
    }

    public void OnSpecialActionAlt( InputValue value )
    {
        GetCurrentTool().OnSpecialAction( value.isPressed );
    }

    public void OnMouseWheel( InputValue value )
    {
        GetCurrentTool().OnMouseWheel( value.isPressed );
    }

    public void OnMouseWheelScroll( InputValue value )
    {
        var v = value.Get<float>();
        if( !GetCurrentTool().OnMouseWheelScroll( v ) )
        {
            var diff = ( int )Mathf.Sign( v );
            currentTool = ( currentTool + diff ) % tools.Count;
        }
    }

    public void OnShowPauseMenu( InputValue value )
    {
        Application.Quit();
    }

    public void OnShowToolsMenu( InputValue value )
    {

    }

    public void OnToolSelections1( InputValue value ) { if( value.isPressed ) SelectTool( 0 ); }
    public void OnToolSelections2( InputValue value ) { if( value.isPressed ) SelectTool( 1 ); }
    public void OnToolSelections3( InputValue value ) { if( value.isPressed ) SelectTool( 2 ); }
    public void OnToolSelections4( InputValue value ) { if( value.isPressed ) SelectTool( 3 ); }
    public void OnToolSelections5( InputValue value ) { if( value.isPressed ) SelectTool( 4 ); }
    public void OnToolSelections6( InputValue value ) { if( value.isPressed ) SelectTool( 5 ); }
    public void OnToolSelections7( InputValue value ) { if( value.isPressed ) SelectTool( 6 ); }
    public void OnToolSelections8( InputValue value ) { if( value.isPressed ) SelectTool( 7 ); }
    public void OnToolSelections9( InputValue value ) { if( value.isPressed ) SelectTool( 8 ); }

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

    public void Update()
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
        var scaledMoveSpeed = moveSpeed * Time.deltaTime;
        // For simplicity's sake, we just keep movement in a single plane here. Rotate
        // direction according to world Y rotation of player.
        var move = Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(direction.x, 0, direction.y);
        transform.position += move * scaledMoveSpeed;
    }

    private void Look(Vector2 rotate)
    {
        if (rotate.sqrMagnitude < 0.01)
            return;
        var scaledRotateSpeed = rotateSpeed * Time.deltaTime;
        rotation.y += rotate.x * scaledRotateSpeed;
        rotation.x = Mathf.Clamp(rotation.x - rotate.y * scaledRotateSpeed, -89, 89);
        fpsCamera.transform.localEulerAngles = rotation.SetY( 0.0f );
        transform.localEulerAngles = rotation.SetX( 0.0f );
    }

    public Vector3 GetLookRotation()
    {
        return transform.localEulerAngles.SetX( fpsCamera.transform.localEulerAngles.x );
    }
}
