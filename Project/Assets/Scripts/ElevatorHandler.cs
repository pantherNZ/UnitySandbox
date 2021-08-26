using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ElevatorHandler : MonoBehaviour
{
    public enum State
    {
        Stationary,
        MovingUp,
        MovingDown,
    }

    [SerializeField] float elevatorSpeed = 10.0f;
    [SerializeField] float doorOpenWidth = 2.0f;
    [SerializeField] float doorOpenSpeed = 1.0f;
    [SerializeField] bool allowChangingDirectionWhenOccupied = false;

    [Serializable]
    public class FloorData
    {
        public GameObject leftDoor;
        public GameObject rightDoor;
        public float height;
    }

    [SerializeField] GameObject leftDoor;
    [SerializeField] GameObject rightDoor;
    [SerializeField] List<FloorData> floors = new List<FloorData>();

    Int32 currentFloor = 0;
    State state = State.Stationary;
    List<Int32> internalRequests = new List<Int32>();
    List<Pair<Int32, State>> externalRequests = new List<Pair<Int32, State>>();

    private void Start()
    {
        if( floors.IsEmpty() )
        {
            Debug.LogError( "ElevatorHandler has no floor data specified" );
            return;
        }

        transform.position = transform.position.SetY( floors.Front().height );

        StartCoroutine( MoveDoors( true ) );
    }

    public void CallElevatorFromInside( Int32 floorIndex )
    {
        if( floors.IsEmpty() )
            return;
        if( internalRequests.Contains( floorIndex ) )
            return;

        internalRequests.Add( floorIndex );
        ProcessNextMovement();
    }

    public void CallElevatorUpFromFloor( Int32 floorIndex )
    {
        if( floors.IsEmpty() )
            return;

        var request = new Pair<Int32, State>() { First = floorIndex, Second = State.MovingUp };
        if( externalRequests.Contains( request ) )
            return;

        externalRequests.Add( request );
        ProcessNextMovement();
    }

    public void CallElevatorDownFromFloor( Int32 floorIndex )
    {
        if( floors.IsEmpty() )
            return;

        var request = new Pair<Int32, State>() { First = floorIndex, Second = State.MovingDown };
        if( externalRequests.Contains( request ) )
            return;

        externalRequests.Add( request );
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
        if( floors.IsEmpty() || currentFloor >= floors.Count - 1 )
            return;

        ++currentFloor;
        var distance = Mathf.Abs( floors[currentFloor].height - floors[currentFloor - 1].height );
        StartCoroutine( MoveTo( true, transform.position.SetY( floors[currentFloor].height ), distance / elevatorSpeed ) );
    }

    private void MoveDown()
    {
        if( floors.IsEmpty() || currentFloor <= 0 )
            return;

        --currentFloor;
        var distance = Mathf.Abs( floors[currentFloor].height - floors[currentFloor + 1].height );
        StartCoroutine( MoveTo( false, transform.position.SetY( floors[currentFloor].height ), distance / elevatorSpeed ) );
    }

    private IEnumerator MoveTo( bool movingUp, Vector3 targetPos, float duration )
    {
        state = movingUp ? State.MovingUp : State.MovingDown;
        yield return Utility.InterpolatePosition( transform, targetPos, duration );
        ProcessNextMovement();
    }

    private void ProcessNextMovement()
    {
        if( !IsMoving() )
        {
            // Serve next internal request
            if( !internalRequests.IsEmpty() )
            {
                if( currentFloor > internalRequests.Front() )
                    MoveDown();
                else if( currentFloor < internalRequests.Front() )
                    MoveUp();
            }
            // Serve next external request
            else if( !externalRequests.IsEmpty() )
            {
                if( currentFloor > externalRequests.Front().First )
                    MoveDown();
                else if( currentFloor < externalRequests.Front().First )
                    MoveUp();
            }
            else
            {
                // After 5 sec of no requests, rest back to ground floor
                Utility.FunctionTimer.CreateTimer( 5.0f, () =>
                {
                    if( !IsMoving() && 
                        internalRequests.IsEmpty() &&
                        externalRequests.IsEmpty() )
                    {
                        CallElevatorFromInside( 0 );
                    }
                } );
            }
        }
        else
        {
            // Stop if we have any internal requests for this floor or any external ones at this floor in the same direction that we are moving
            if( internalRequests.Any( x => x == currentFloor ) ||
                externalRequests.Any( x => x.First == currentFloor && ( internalRequests.IsEmpty() || x.Second == state ) ) )
                StartCoroutine( StopAtFloor() );
        }
    }

    private IEnumerator StopAtFloor()
    {
        yield return MoveDoors( true );
        yield return new WaitForSeconds( 5.0f );

        internalRequests.Remove( currentFloor );
        externalRequests.Remove( x => x.First == currentFloor && ( internalRequests.IsEmpty() || x.Second == state ) );
       
        state = State.Stationary;
        yield return MoveDoors( false );
        ProcessNextMovement();
    }

    private IEnumerator MoveDoors( bool open )
    {
        StartCoroutine( MoveDoors( leftDoor.transform, rightDoor.transform, open ) );
        yield return MoveDoors( floors[currentFloor].leftDoor.transform, floors[currentFloor].rightDoor.transform, open );
    }

    private IEnumerator MoveDoors( Transform left, Transform right, bool open )
    {
        var direction = open ? 1.0f : -1.0f;
        StartCoroutine( Utility.InterpolatePosition( left, left.position - left.up * doorOpenWidth * direction, doorOpenWidth / doorOpenSpeed ) );
        yield return Utility.InterpolatePosition( right, right.position + right.up * doorOpenWidth * direction, doorOpenWidth / doorOpenSpeed );
    }
}