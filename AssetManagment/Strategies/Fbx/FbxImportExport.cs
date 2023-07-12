﻿using AssetManagement.GenericFormats;
using CommonControls.FileTypes.PackFiles.Models;
using CommonControls.FileTypes.RigidModel;
using CommonControls.Interfaces.AssetManagement;
using System.IO;

namespace AssetManagement.Strategies.Fbx
{
    public class FbxImportExport : IAssetImporter
    {
        public string[] Formats => new string[] { ".fbx" };

        public PackFile ImportAsset(string diskFilePath)
        {
            var sceneContainer = SceneLoader.LoadScene(diskFilePath);

            var rmv2File = RmvFileBuilder.ConvertToRmv2(sceneContainer.Meshes, "");

            var factory = ModelFactory.Create();
            var buffer = factory.Save(rmv2File);

            var rmv2FileName = $"{Path.GetFileNameWithoutExtension(diskFilePath)}.rigid_model_v2";
            var packFile = new PackFile(rmv2FileName, new MemorySource(buffer));
            return packFile;
        }
    }
}