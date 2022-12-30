using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

// Define object manager inside sandbox object so it can access the private members of the object
public partial struct SandboxObject
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

        public SandboxObject? GetObject( GameObject obj )
        {
            var sandboxObj = obj.GetComponent<SandboxObjectData>();
            return sandboxObj != null ? sandboxObj.data : ( SandboxObject? )null;
        }

        public SandboxObject CreateObject( GameObject obj, GameObject prefab, Int32 ownerId )
        {
            var prefabResource = Utility.GetResourcePath( prefab );
            var sandboxObj = new SandboxObject(ownerId, GenerateUniqueId(), prefabResource, obj );
            SetupObject( sandboxObj, obj );
            allObjects[sandboxObj.uniqueId] = sandboxObj;
            return sandboxObj;
        }

        public SandboxObject CreateOrCreateObject( GameObject obj, GameObject prefab, Int32 ownerId )
        {
            return GetObject( obj ) ?? CreateObject( obj, prefab, ownerId );
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
            var writer = new BinaryWriter( memoryStream );
            var dummyObject = new GameObject();

            var sandboxObject = obj.GetComponent<SandboxObjectData>();
            writer.Write( sandboxObject.data.uniqueId );

            var components = obj.GetComponents<Component>();
            var numComponents = 0;

            foreach( var cmp in components )
            {
                var type = cmp.GetType();
                if( type == typeof( SandboxObjectData ) || type == typeof( MeshFilter ) || type == typeof( MeshRenderer ) )
                    continue;
                ++numComponents;
            }

            writer.Write( numComponents );
            Debug.Log( "Writing num components: " + ( components.Length - 1 ) );

            foreach( var cmp in components )
            {
                var type = cmp.GetType();

                if( type == typeof( SandboxObjectData ) || type == typeof( MeshFilter ) || type == typeof( MeshRenderer ) )
                    continue;

                var defaultInstance = dummyObject.GetComponent( type );
                if( defaultInstance == null )
                    defaultInstance = dummyObject.AddComponent( type );

                writer.Write( type.AssemblyQualifiedName );
                Debug.Log( "Writing component type: " + type.AssemblyQualifiedName );

                var propertiesToSerialise = new List<PropertyInfo>();
                foreach( var thisVar in type.GetProperties() )
                {
                    if( thisVar.GetSetMethod() == null && !thisVar.IsDefined( typeof( SerializeField ) ) )
                        continue;
                    if( thisVar.IsDefined( typeof( HideInInspector ) ) )
                        continue;
                    if( thisVar.IsDefined( typeof( NonSerializedAttribute ) ) )
                        continue;
                    if( thisVar.IsDefined( typeof( ObsoleteAttribute ) ) )
                        continue;
                    if( thisVar.PropertyType == typeof( Mesh ) || thisVar.PropertyType == typeof( Material ) || thisVar.PropertyType == typeof( PhysicMaterial ) )
                        continue;

                    // Skip the mesh var in meshfilter because accessing it results in a side effect of copying the mesh and instantiating a new one,
                    // which breaks our serialisation because the path is no longer a valid resource path
                    //if( type == typeof( MeshFilter ) && thisVar.PropertyType == typeof( Mesh ) && thisVar.Name == "mesh" )
                    //    continue;

                    var defaultNull = thisVar.GetValue( defaultInstance ) == null;
                    var currentNull = thisVar.GetValue( cmp ) == null;

                    if( defaultNull && currentNull )
                        continue;
                    if( !defaultNull && thisVar.GetValue( defaultInstance ).Equals( thisVar.GetValue( cmp ) ) )
                        continue;

                    propertiesToSerialise.Add( thisVar );
                }

                writer.Write( propertiesToSerialise.Count );
                Debug.Log( "Writing num properties: " + propertiesToSerialise.Count );

                foreach( var thisVar in propertiesToSerialise )
                {
                    var value = thisVar.GetValue( cmp );

                    Debug.Log( String.Format( "{0}: {1} - ({2}: {3})", thisVar.PropertyType.Name, thisVar.Name, thisVar.GetValue( defaultInstance ), value ) );
                    Debug.Log( String.Format( "{0}: {1} - Is Pointer: {2}, Is Class: {3}", thisVar.PropertyType.Name, thisVar.Name, thisVar.PropertyType.IsClass, thisVar.PropertyType.IsPointer ) );

                    writer.Write( thisVar.Name );
                    writer.Write( thisVar.PropertyType.IsArray );
                    Debug.Log( "Writing property name: " + thisVar.Name );

                    if( thisVar.PropertyType.IsArray )
                    {
                        //writer.Write( thisVar.PropertyType.GetElementType().AssemblyQualifiedName );
                        //var array = value as Array;
                        //writer.Write( array.Length );
                        //foreach( var element in array )
                        //    writer.Write( element );
                    }
                    else
                    {
                        writer.Write( thisVar.PropertyType.AssemblyQualifiedName );
                        writer.Write( value );
                        Debug.Log( "Writing property type: " + thisVar.PropertyType.AssemblyQualifiedName );
                        Debug.Log( "Writing property value: " + value );
                    }
                }
            }

            dummyObject.Destroy();
            writer.Dispose();
            memoryStream.Dispose();
            return memoryStream.ToArray();
        }

        public GameObject DeserialiseObject( byte[] storedData )
        {
            if( storedData == null || storedData.Length == 0 )
            {
                Debug.LogError( "DeserialiseObject: Cannot recreate game object as there is no stored data" );
                return null;
            }

            var memoryStream = new MemoryStream( storedData, writable: false );
            var reader = new BinaryReader( memoryStream );

            var uniqueId = reader.ReadUInt32();
            var sandboxObjData = allObjects[uniqueId];
            var prefab = Resources.Load<GameObject>( sandboxObjData.prefabResource );
            var obj = GameObject.Instantiate( prefab );

            var sandboxObject = obj.AddComponent<SandboxObjectData>();
            sandboxObject.data = sandboxObjData;

            var componentCount = reader.ReadInt32();
            Debug.Log( "Reading num components: " + componentCount );

            for( int i = 0; i < componentCount; ++i )
            {
                var componentType = Type.GetType( reader.ReadString() );
                Debug.Log( "Reading component type: " + componentType.AssemblyQualifiedName );

                var cmp = obj.GetComponent( componentType );
                if( cmp == null )
                    cmp = obj.AddComponent( componentType );
                var numProperties = reader.ReadInt32();
                Debug.Log( "Reading num properties: " + numProperties );

                for( int j = 0; j < numProperties; ++j )
                {
                    var propName = reader.ReadString();
                    var isArray = reader.ReadBoolean();
                    Debug.Log( "Reading property name: " + propName );

                    if( isArray )
                    {
                        //var propElementType = Type.GetType( reader.ReadString() );
                        //var arrayLength = reader.ReadInt32();
                        //
                        //for( int k = 0; k < numProperties; ++k )
                        //{
                        //    //reader.Read
                        //}
                    }
                    else
                    {
                        var propType = Type.GetType( reader.ReadString() );
                        var value = reader.ReadObject( propType );
                        componentType.GetProperty( propName ).SetValue( cmp, value );

                        Debug.Log( "Reading property type: " + propType.AssemblyQualifiedName );
                        Debug.Log( "Reading property value: " + value );
                    }
                }
            }

            reader.Dispose();
            memoryStream.Dispose();
            return obj;
        }

        public void Serialise( System.IO.BinaryWriter writer )
        {

        }

        public void Deserialise( int saveVersion, System.IO.BinaryReader reader )
        {

        }
    }
}