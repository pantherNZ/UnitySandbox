using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Define object manager inside sandbox object so it can access the private members of the object
using static SandboxObject;

public partial class SandboxObject
{
    public class ObjectManager : MonoBehaviour, ISavableComponent
    {
        [HideInInspector] static ObjectManager objectManager;
        [HideInInspector] public static ObjectManager Instance { get { return objectManager; } }

        private Int32 nextObjectIndex;
        private readonly Dictionary<Int32, SandboxObject> allObjects = new Dictionary<int, SandboxObject>();

        private void Awake()
        {
            objectManager = this;
        }

        public SandboxObject CreateObject( GameObject obj, Int32 ownerId )
        {
            ++nextObjectIndex;
            var sandboxObj = new SandboxObject(ownerId, nextObjectIndex, obj );
            SetupObject( sandboxObj, obj );
            allObjects[nextObjectIndex] = sandboxObj;
            return sandboxObj;
        }

        public void CreateObject( SandboxObject sandboxObj, GameObject obj )
        {
            SetupObject( sandboxObj, obj );
            allObjects[sandboxObj.uniqueId] = sandboxObj;
        }

        public void RemoveObject( SandboxObject sandboxObj )
        {
            allObjects.Remove( sandboxObj.uniqueId );
        }

        private void SetupObject( SandboxObject sandboxObj, GameObject obj )
        {
            sandboxObj.gameObject = obj;

            var dataCmp = sandboxObj.gameObject.GetComponent<SandboxObjectData>();

            if( !dataCmp )
                dataCmp = sandboxObj.gameObject.AddComponent<SandboxObjectData>();

            dataCmp.data = sandboxObj;
        }

        public void DeserialiseObject( GameObject obj, byte[] storedData )
        {
            if( obj == null )
            {
                Debug.LogError( "DeserialiseObject: Object is NULL" );
                return;
            }

            if( storedData == null || storedData.Length == 0 )
            {
                Debug.LogError( "DeserialiseObject: Cannot recreate game object as there is no stored data" );
                return;
            }
        }

        public byte[] SerialiseObject( GameObject obj )
        {
            if( obj == null )
            {
                Debug.LogError( "DeserialiseObject: Object is NULL" );
                return null;
            }

            var memoryStream = new MemoryStream();

            using( var writer = new BinaryWriter( memoryStream ) )
            {
                foreach( var cmp in obj.GetComponents<Component>() )
            }

            memoryStream.Dispose();
            return memoryStream.ToArray();
    }

        public void Serialise( System.IO.BinaryWriter writer )
        {

        }

        public void Deserialise( System.IO.BinaryReader reader )
        {

        }
    }
}