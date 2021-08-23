using System;
using UnityEngine;

public partial class SandboxObject
{
    [HideInInspector]
    public readonly Int32 ownerId;

    [HideInInspector]
    public readonly UInt32 uniqueId;

    [HideInInspector]
    public GameObject gameObject { get; private set; }

    [HideInInspector]
    public readonly string prefabResource;

    [HideInInspector]
    private byte[] storedData;

    public void Save()
    {
        if( gameObject == null )
        {
            Debug.LogError( "SandboxObject::Save: Object is invalid" );
            return;
        }

        storedData = ObjectManager.Instance.SerialiseObject( gameObject );
    }

    public void SaveAndDestroy()
    {
        if( gameObject == null )
        {
            Debug.LogError( "SandboxObject::SaveAndDestroy: Object already destroyed" );
            return;
        }

        Save();
        gameObject.Destroy();
    }

    public void Load()
    {
        if( storedData == null )
        {
            Debug.LogError( "SandboxObject::Load: No valid data exists to load from" );
            return;
        }

        if( gameObject )
            gameObject.Destroy();

        gameObject = ObjectManager.Instance.DeserialiseObject( storedData );
    }

    public void RecreateFromSave()
    {
        if( gameObject != null )
            Debug.LogError( "SandboxObject::RecreateFromSave: Object already exists" );

        Load();
    }

    public SandboxObject( Int32 ownerId, UInt32 uniqueId, string prefabResource, GameObject gameObject )
    {
        this.ownerId = ownerId;
        this.uniqueId = uniqueId;
        this.gameObject = gameObject;
        this.prefabResource = prefabResource;
    }

    public static implicit operator bool( SandboxObject obj )
    {
        return obj is object && obj.gameObject;
    }

}