using System;
using System.Collections;
using System.IO;
using UnityEngine;

[Serializable]
public class VoxelData : BehaviourData
{
    [ScriptPicker]
    [SerializeField] string voxelFilePath;

    public VoxReader.Interfaces.IVoxFile voxelFile;
    public VoxReader.Chunks.VoxelChunk voxelData;

    public override Vector3 Animate( Vector3 pos )
    {
        return pos;
    }

    public override IEnumerator Construct()
    {
        try
        {
            voxelFile = VoxReader.VoxReader.Read( Path.Combine( Directory.GetCurrentDirectory(), voxelFilePath ) );
        }
        catch( Exception e )
        {
            Debug.LogError( "Failed to load voxel data: " + e.Message );
        }

        if( voxelFile == null )
            yield return null;

        voxelData = voxelFile.ChunkRoot.GetChild<VoxReader.Chunks.VoxelChunk>();

        foreach( var voxel in voxelData.Voxels )
        {
            var voxelPos = voxel.Position;
            var localPos = new Vector3( voxelPos.X, voxelPos.Z, voxelPos.Y ) * cubeScale;
            yield return localPos;
        }
    }

    public override void Update( float delta )
    {
        time += delta;
    }

    float time;
}