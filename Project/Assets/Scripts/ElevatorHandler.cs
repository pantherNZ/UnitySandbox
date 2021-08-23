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
    [SerializeField] GameObject elevatorObject;

    Int32 currentFloor = 0;
    State state = State.Stationary;
    List<Int32> floorRequests = new List<Int32>();

    private void Start()
    {
        if( floorHeights.IsEmpty() || elevatorObject == null )
        {
            Debug.LogError( "ElevatorHandler has no floor heights specified or elevator gameobject is invalid" );
            return;
        }

        elevatorObject.transform.position = elevatorObject.transform.position.SetZ( floorHeights.Front() );
    }

    public void CallElevator( Int32 floorIndex, bool calledFromInsideElevator )
    {
        if( floorHeights.IsEmpty() || elevatorObject == null )
            return;
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
        if( floorHeights.IsEmpty() || elevatorObject == null || currentFloor >= floorHeights.Count - 1 )
            return;

        ++currentFloor;
        var distance = floorHeights[currentFloor] - floorHeights[currentFloor - 1];
        StartCoroutine( MoveTo( true, elevatorObject.transform.position.SetZ( floorHeights[currentFloor] ), distance / elevatorSpeed ) );
    }

    private void MoveDown()
    {
        if( floorHeights.IsEmpty() || elevatorObject == null || currentFloor <= 0 )
            return;

        --currentFloor;
        var distance = floorHeights[currentFloor] - floorHeights[currentFloor + 1];
        StartCoroutine( MoveTo( false, elevatorObject.transform.position.SetZ( floorHeights[currentFloor] ), distance / elevatorSpeed ) );
    }

    private IEnumerator MoveTo( bool movingUp, Vector3 targetPos, float duration )
    {
        state = movingUp ? State.MovingUp : State.MovingDown;
        yield return Utility.InterpolatePosition( elevatorObject.transform, targetPos, duration );
        state = State.Stationary;
    }
}