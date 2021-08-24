using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorHandler : MonoBehaviour
{
    public enum State
    {
        Stationary,
        MovingUp,
        MovingDown,
    }

    [SerializeField] float elevatorSpeed = 10.0f;
    [SerializeField] bool allowChangingDirectionWhenOccupied = false;
    [SerializeField] List<float> floorHeights = new List<float>();

    Int32 currentFloor = 0;
    State state = State.Stationary;
    List<Int32> internalRequests = new List<Int32>();
    List<Pair<Int32, bool>> externalRequests = new List<Pair<Int32, bool>>();

    private void Start()
    {
        if( floorHeights.IsEmpty() )
        {
            Debug.LogError( "ElevatorHandler has no floor heights specified or elevator gameobject is invalid" );
            return;
        }

        transform.position = transform.position.SetY( floorHeights.Front() );
    }

    public void CallElevatorFromInside( Int32 floorIndex )
    {
        if( floorHeights.IsEmpty() )
            return;

        internalRequests.Add( floorIndex );
        ProcessNextMovement();
    }

    public void CallElevatorUpFromFloor( Int32 floorIndex )
    {
        if( floorHeights.IsEmpty() )
            return;

        externalRequests.Add( new Pair<Int32, bool>() { First = floorIndex, Second = true } );
        ProcessNextMovement();
    }

    public void CallElevatorDownFromFloor( Int32 floorIndex )
    {
        if( floorHeights.IsEmpty() )
            return;

        externalRequests.Add( new Pair<Int32, bool>() { First = floorIndex, Second = false } );
        ProcessNextMovement();
    }

    public bool IsMoving()
    {
        return state != State.Stationary;
    }

    public State GetState()
    {
        return state;
    }

    public Int32 GetCurrentFloor()
    {
        return currentFloor;
    }

    private void MoveUp()
    {
        if( floorHeights.IsEmpty() || currentFloor >= floorHeights.Count - 1 )
            return;

        ++currentFloor;
        var distance = Mathf.Abs( floorHeights[currentFloor] - floorHeights[currentFloor - 1] );
        StartCoroutine( MoveTo( true, transform.position.SetY( floorHeights[currentFloor] ), distance / elevatorSpeed ) );
    }

    private void MoveDown()
    {
        if( floorHeights.IsEmpty() || currentFloor <= 0 )
            return;

        --currentFloor;
        var distance = Mathf.Abs( floorHeights[currentFloor] - floorHeights[currentFloor + 1] );
        StartCoroutine( MoveTo( false, transform.position.SetY( floorHeights[currentFloor] ), distance / elevatorSpeed ) );
    }

    private IEnumerator MoveTo( bool movingUp, Vector3 targetPos, float duration )
    {
        state = movingUp ? State.MovingUp : State.MovingDown;
        yield return Utility.InterpolatePosition( transform, targetPos, duration );
        state = State.Stationary;
        ProcessNextMovement();
    }

    private void ProcessNextMovement()
    {
        if( currentFloor > internalRequests.Front() )
            MoveDown();
        else if( currentFloor < internalRequests.Front() )
            MoveUp();
        else
            internalRequests.PopFront();
    }
}