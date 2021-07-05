using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

// Define object manager inside sandbox object so it can access the private members of the object
using static SandboxObject;

public partial class SandboxObject
{
    public class ObjectManager : ISavableComponent
    {
        static ObjectManager instance;
        public static ObjectManager Instance 
        { 
            get 
            {
                if( instance == null )
                    instance = new ObjectManager();
                return instance; 
            } 
        }

        private readonly Dictionary<UInt32, SandboxObject> allObjects = new Dictionary<UInt32, SandboxObject>();
        private readonly HashSet<UInt32> usedIds = new HashSet<UInt32>();

        public UInt32 GenerateUniqueId()
        {
            UInt32 newId;

            do
            {
                newId = xxHashSharp.xxHash.CalculateHash( Guid.NewGuid().ToByteArray() );
            }
            while( usedIds.Contains( newId ) );

            return newId;
        }

        public SandboxObject CreateObject( GameObject obj, Int32 ownerId )
        {
            var sandboxObj = new SandboxObject(ownerId, GenerateUniqueId(), obj );
            SetupObject( sandboxObj, obj );
            allObjects[sandboxObj.uniqueId] = sandboxObj;
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

        //https://www.gamasutra.com/blogs/SamanthaStahlke/20170621/300187/Building_a_Simple_System_for_Persistence_with_Unity.php
        // https://answers.unity.com/questions/1333022/how-to-get-every-public-variables-from-a-script-in.html
        // https://answers.unity.com/questions/627090/convert-serializedproperty-to-custom-class.html
        // https://answers.unity.com/questions/1347203/a-smarter-way-to-get-the-type-of-serializedpropert.html
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
                var dummyObject = new GameObject();
                foreach( var cmp in obj.GetComponents<Component>() )
                {
                    var type = cmp.GetType();
                    var defaultInstance = dummyObject.GetComponent( type );
                    if( defaultInstance == null )
                        defaultInstance = dummyObject.AddComponent( type );

                    foreach( var thisVar in type.GetProperties() )
                    {
                        if( ( thisVar.GetSetMethod() != null || thisVar.IsDefined( typeof( SerializeField ) ) ) &&
                            !thisVar.IsDefined( typeof( HideInInspector ) ) &&
                            !thisVar.IsDefined( typeof( NonSerializedAttribute ) ) &&
                            thisVar.GetValue( defaultInstance ) != null &&
                            !thisVar.GetValue( defaultInstance ).Equals( thisVar.GetValue( cmp ) ) )
                        {
                            if( thisVar.PropertyType.IsClass )
                            {
                            }
                            else
                            {
                                //Debug.Log( String.Format( "{0}: {1} - ({2}: {3})", thisVar.PropertyType.Name, thisVar.Name, thisVar.GetValue( defaultInstance ), thisVar.GetValue( cmp ) ) );
                                writer.Write( thisVar.PropertyType.Name );
                                writer.Write( ( dynamic )thisVar.GetValue( cmp ) );
                            }
                        }
                    }
                }

                dummyObject.Destroy();
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