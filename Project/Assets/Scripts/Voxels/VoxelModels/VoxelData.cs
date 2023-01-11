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

    public override Vector3 GetPosition( int idx )
    {
        if( voxelFile == null )
        {
            try
            {
                voxelFile = VoxReader.VoxReader.Read( Path.Combine( Directory.GetCurrentDirectory(), voxelFilePath ) );
            }
            catch( Exception e )
            {
                Debug.LogError( "Failed to load voxel data: " + e.Message );
            }

            voxelData = voxelFile.ChunkRoot.GetChild<VoxReader.Chunks.VoxelChunk>();
        }

        var voxel = voxelData.Voxels[idx];
        var voxelPos = voxel.Position;
        var localPos = new Vector3( voxelPos.X, voxelPos.Z, voxelPos.Y ) * cubeScale;
        return localPos;
    }

    public override Color? GetColour( int idx )
    {
        var voxelColour = voxelFile.Palette.Colors[voxelData.Voxels[idx].ColorIndex];
        return new Color(
                voxelColour.R / 255.0f
                , voxelColour.G / 255.0f
                , voxelColour.B / 255.0f
                , voxelColour.A / 255.0f );
    }
}