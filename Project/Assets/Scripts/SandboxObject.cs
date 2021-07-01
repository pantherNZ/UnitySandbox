using System;
using UnityEngine;

public partial class SandboxObject
{
    [HideInInspector]
    public readonly Int32 ownerId;

    [HideInInspector]
    public readonly Int32 uniqueId;

    [HideInInspector]
    public GameObject gameObject { get; private set; }

    [HideInInspector]
    private byte[] storedData;

    public void RecreateFromSave()
    {
        if( gameObject != null )
            Debug.LogError( "RecreateFromSave: Object already exists" );
        else
            gameObject = new GameObject();

        ObjectManager.Instance.DeserialiseObject( gameObject, storedData );
    }

    public void SaveAndDestroy()
    {
        if( gameObject == null )
        {
            Debug.LogError( "SaveAndDestroy: Object already destroyed" );
            return;
        }

        storedData = ObjectManager.Instance.SerialiseObject( gameObject );
        gameObject.Destroy();
    }

    public SandboxObject( Int32 ownerId, Int32 uniqueId, GameObject gameObject )
    {
        this.ownerId = ownerId;
        this.uniqueId = uniqueId;
        this.gameObject = gameObject;
    }

    public static implicit operator bool( SandboxObject obj )
    {
        return obj is object && obj.gameObject;
    }

}