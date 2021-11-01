﻿using Filetypes.ByteParsing;
using Filetypes.RigidModel.Transforms;
using Filetypes.RigidModel.Vertex;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Filetypes.RigidModel
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RmvModelHeader
    {
        public RmvModelHeader(uint version, string skeletonName, uint lodCount)
        {
            _fileType = ByteParsers.String.Encode("RMV2", out _);
            Version = version;
            LodCount = lodCount;
            _skeletonName = ByteParsers.String.Encode(skeletonName, out _);
        }

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        byte[] _fileType;

        public uint Version;
        public uint LodCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        byte[] _skeletonName;

        public RmvVersionEnum GetVersion() => (RmvVersionEnum)Version;

        public string FileType
        {
            get
            {
                var result = ByteParsers.String.TryDecodeFixedLength(_fileType, 0, 4, out string value, out _);
                if (result == false)
                    throw new Exception();
                return Util.SanatizeFixedString(value);
            }
        }

        public string SkeletonName
        {
            get
            {
                var result = ByteParsers.String.TryDecodeFixedLength(_skeletonName, 0, 128, out string value, out _);
                if (result == false)
                    throw new Exception();
                return Util.SanatizeFixedString(value);
            }
            set
            {
                for (int i = 0; i < 128; i++)
                    _skeletonName[i] = 0;

                var byteValues = Encoding.UTF8.GetBytes(value);
                for (int i = 0; i < byteValues.Length; i++)
                {
                    _skeletonName[i] = byteValues[i];
                }
            }
        }

        public RmvModelHeader Clone()
        {
            return new RmvModelHeader()
            {
                _fileType = _fileType,
                Version = Version,
                LodCount = LodCount,
                _skeletonName = _skeletonName,
            };
        }
    };


    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct RmvSubModelHeader
    {
        public GroupTypeEnum ShaderFlag;
        public ushort RenderFlag;
        public uint MeshSectionSize;
        public uint VertexOffset;
        public uint VertexCount;
        public uint IndexOffset;
        public uint IndexCount;

        public RvmBoundingBox BoundingBox;
        public RmvShaderParams ShaderParams;

        ushort _vertexType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public  byte[] _modelName;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] _textureDir;
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] Filters;  // Always zero?

        public byte PaddingByte0;
        public byte PaddingByte1;

        public RmvTransform Transform;

        public int MatrixIndex;         // Used when meshes should be attached to a bone, eg for buildings being destroyed 
        public int ParentMatrixIndex;   // Always -1 it seems
        public uint AttachmentPointCount;
        public uint TextureCount;
        public uint StringParamCount;
        public uint FloatParamCount;
        public uint IntParamCount;
        public uint Vec4ParamCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 124)]
        public byte[] PaddingArray;


        public VertexFormat VertextType { get => (VertexFormat)_vertexType; set => _vertexType = (ushort)value; }

        public string ModelName
        {
            get
            {
                var result = ByteParsers.String.TryDecodeFixedLength(_modelName, 0, 32, out string value, out _);
                if (result == false)
                    throw new Exception();
                return Util.SanatizeFixedString(value);
            }
            set 
            {
                for (int i = 0; i < 32; i++)
                    _modelName[i] = 0;

                var byteValues = Encoding.UTF8.GetBytes(value);
                for (int i = 0; i < byteValues.Length; i++)
                {
                    _modelName[i] = byteValues[i];
                }
            }
        }

        public string TextureDirectory
        {
            get
            {
                var result = ByteParsers.String.TryDecodeFixedLength(_textureDir, 0, 256, out string value, out _);
                if (result == false)
                    throw new Exception();
                return Util.SanatizeFixedString(value);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        internal void UpdateHeader(RmvMesh mesh, RmvSubModel refSubMmodel, uint modelVersion)
        {
            var stringParamSize = 0;
            foreach (var str in refSubMmodel.StringParams)
            {
                var res = ByteParsers.String.Encode(str, out _);
                stringParamSize += res.Length;
            }

            var headerDataSize = (uint)(ByteHelper.GetSize<RmvSubModelHeader>() +
                (ByteHelper.GetSize<RmvAttachmentPoint>() * refSubMmodel.AttachmentPoints.Count) +
                (ByteHelper.GetSize<RmvTexture>() * refSubMmodel.Textures.Count) +
                (stringParamSize) +
                (4 * 2 * refSubMmodel.FloatParams.Count) + 
                (4 * 2 * refSubMmodel.IntParams.Count) + 
                (4 * 4 * 2 * refSubMmodel.Vec4Params.Count));

            AttachmentPointCount = (uint)refSubMmodel.AttachmentPoints.Count;
            TextureCount = (uint)refSubMmodel.Textures.Count;
            StringParamCount = (uint)refSubMmodel.StringParams.Count;
            FloatParamCount = (uint)refSubMmodel.FloatParams.Count;
            IntParamCount = (uint)refSubMmodel.IntParams.Count;
            Vec4ParamCount = (uint)refSubMmodel.Vec4Params.Count;

            VertexCount = (uint)mesh.VertexList.Length;
            IndexCount = (uint)mesh.IndexList.Length;
            VertexOffset = headerDataSize;
            IndexOffset = headerDataSize + (uint)(RmvMesh.GetVertexSize(VertextType, modelVersion) * VertexCount);

            MeshSectionSize = headerDataSize + (uint)(RmvMesh.GetVertexSize(VertextType, modelVersion) * VertexCount) + (sizeof(ushort) * IndexCount);

            BoundingBox.Recompute(mesh);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct RmvShaderParams
    {

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        byte[] _shaderName;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public byte[] UnknownValues;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public byte[] AllZeroValues;

        public string ShaderName
        {
            get
            {
                var result = ByteParsers.String.TryDecodeFixedLength(_shaderName, 0, 12, out string value, out _);
                if (result == false)
                    throw new Exception();
                return value;
            }
        }
    }


    public class RmvMesh
    {
        public BaseVertex[] VertexList { get; set; }
        public ushort[] IndexList;
        
        public static int GetVertexSize(VertexFormat vertexFormat, uint modelVersion)
        {
            var extraSize = 0;
            if (modelVersion == 8)
                extraSize = 4;
            switch (vertexFormat)
            {
                case VertexFormat.Static:
                    return ByteHelper.GetSize(typeof(DefaultVertex.Data));
                case VertexFormat.Weighted:
                    return ByteHelper.GetSize(typeof(WeightedVertex.Data)) + extraSize;
                case VertexFormat.Cinematic:
                    return ByteHelper.GetSize(typeof(CinematicVertex.Data)) + extraSize;
            }

            throw new Exception($"Unkown vertex format - {vertexFormat}");
        }

        internal void SaveToByteArray(BinaryWriter writer, VertexFormat vertexFormat)
        {
            for (int i = 0; i < VertexList.Length; i++) 
            {
                if (vertexFormat == VertexFormat.Static)
                    writer.Write(ByteHelper.GetBytes(((DefaultVertex)VertexList[i])._data)); 
                else if (vertexFormat == VertexFormat.Cinematic)
                    writer.Write(ByteHelper.GetBytes(((CinematicVertex)VertexList[i])._data));
                else if (vertexFormat == VertexFormat.Weighted)
                    writer.Write(ByteHelper.GetBytes(((WeightedVertex)VertexList[i])._data));
                else
                    throw new Exception($"Unkown vertex format - {vertexFormat}");
            }

            for (int i = 0; i < IndexList.Length; i++)
                writer.Write(IndexList[i]);
        }

        public RmvMesh(byte[] data, uint modelVersion, VertexFormat vertexFormat, int vertexStart, uint vertexCount, int faceStart, uint faceCount)
        {
            var vertexSize = GetVertexSize(vertexFormat, modelVersion);

            VertexList = new BaseVertex[vertexCount]; 
            for (int i = 0; i < vertexCount; i++)
            {
                if (vertexFormat == VertexFormat.Static)
                {
                    var vertexData = ByteHelper.ByteArrayToStructure<DefaultVertex.Data>(data, vertexStart + i * vertexSize);
                    VertexList[i] = new DefaultVertex(vertexData);

                }
                else if (vertexFormat == VertexFormat.Cinematic)
                {
                    var vertexData = ByteHelper.ByteArrayToStructure<CinematicVertex.Data>(data, vertexStart + i * vertexSize);
                    VertexList[i] = new CinematicVertex(vertexData);
                }
                else if (vertexFormat == VertexFormat.Weighted)
                {
                    var vertexData = ByteHelper.ByteArrayToStructure<WeightedVertex.Data>(data, vertexStart + i * vertexSize);
                    VertexList[i] = new WeightedVertex(vertexData);
                }
                else
                {
                    throw new Exception($"Unkown vertex format - {vertexFormat}");
                }
            }

            IndexList = new ushort[faceCount];
            for (int i = 0; i < faceCount; i++)
                IndexList[i] =  BitConverter.ToUInt16(data, faceStart + sizeof(ushort) * i);
        }

        public RmvMesh()
        {

        }
    }

    public class RmvSubModel
    {
        int _modelStart;

        public RmvSubModelHeader Header { get; set; }
        public List<RmvAttachmentPoint> AttachmentPoints;
        public List<RmvTexture> Textures;
        public List<string> StringParams;
        public List<float> FloatParams;
        public List<int> IntParams;
        public List<RmvVector4> Vec4Params;

        public RmvMesh Mesh { get; set; }
        public string ParentSkeletonName { get; set; }    // Not part of the model definition


        public RmvSubModel(byte[] dataArray, int offset, uint modelVersion, string skeletonName)
        {
            _modelStart = offset;

            var dataOffset = offset;
            ParentSkeletonName = skeletonName;

            Header = LoadHeader(dataArray, ref dataOffset);
            AttachmentPoints = LoadAttachmentPoints(dataArray, ref dataOffset);
            Textures = LoadTextures(dataArray, ref dataOffset);
            StringParams = LoadStringParams(dataArray, ref dataOffset);
            FloatParams = LoadFloatParams(dataArray, ref dataOffset);
            IntParams = LoadIntParams(dataArray, ref dataOffset);
            Vec4Params = LoadVec4Params(dataArray, ref dataOffset);
            Mesh = LoadMesh(dataArray, modelVersion, ref dataOffset);
        }

        public RmvSubModel()
        { }

        RmvSubModelHeader LoadHeader(byte[] dataArray, ref int dataOffset)
        {
            dataOffset = dataOffset + ByteHelper.GetSize<RmvSubModelHeader>();
            return ByteHelper.ByteArrayToStructure<RmvSubModelHeader>(dataArray, _modelStart);
        }

        List<RmvAttachmentPoint> LoadAttachmentPoints(byte[] dataArray, ref int dataOffset)
        {
            var attachmentPoints = new List<RmvAttachmentPoint>();
            for (int i = 0; i < Header.AttachmentPointCount; i++)
            {
                var attachmentPoint = ByteHelper.ByteArrayToStructure<RmvAttachmentPoint>(dataArray, dataOffset);
                dataOffset += ByteHelper.GetSize<RmvAttachmentPoint>();
                attachmentPoints.Add(attachmentPoint);
            }

            return attachmentPoints;
        }

        List<RmvTexture> LoadTextures(byte[] dataArray, ref int dataOffset)
        {
            var textures = new List<RmvTexture>();
            for (int i = 0; i < Header.TextureCount; i++)
            {
                var texture = ByteHelper.ByteArrayToStructure<RmvTexture>(dataArray, dataOffset);
                dataOffset += ByteHelper.GetSize<RmvTexture>();
                textures.Add(texture);
            }

            return textures;
        }

        List<string> LoadStringParams(byte[] dataArray, ref int dataOffset)
        {
            var output = new List<string>();
            for (int i = 0; i < Header.StringParamCount; i++)
            {
                var index = ByteParsers.UInt32.TryDecode(dataArray, dataOffset, out _, out _, out _);
                var result = ByteParsers.String.TryDecode(dataArray, dataOffset + 4, out var value, out var byteLength, out var error);
                if (!result)
                {
                    throw new Exception("Error reading string parameter - " + error);
                }
                dataOffset += byteLength + 4;
                output.Add(value);
            }
            return output;
        }


        List<float> LoadFloatParams(byte[] dataArray, ref int dataOffset)
        {
            var output = new List<float>();
            for (int i = 0; i < Header.FloatParamCount; i++)
            {
                var index = ByteParsers.UInt32.TryDecode(dataArray, dataOffset, out _, out _, out _);
                var result = ByteParsers.Single.TryDecodeValue(dataArray, dataOffset+4, out var value, out var byteLength, out var error);
                if (!result)
                {
                    throw new Exception("Error reading float parameter - " + error);
                }
                dataOffset += byteLength + 4;
                output.Add(value);
            }
            return output;
        }

        List<int> LoadIntParams(byte[] dataArray, ref int dataOffset)
        {
            var output = new List<int>();
            for (int i = 0; i < Header.IntParamCount; i++)
            {
                var index = ByteParsers.UInt32.TryDecode(dataArray, dataOffset, out _, out _, out _);
                var result = ByteParsers.Int32.TryDecodeValue(dataArray, dataOffset+4, out var value, out var byteLength, out var error);
                if (!result)
                {
                    throw new Exception("Error reading int parameter - " + error);
                }
                dataOffset += byteLength + 4 ;
                output.Add(value);
            }
            return output;
        }

        List<RmvVector4> LoadVec4Params(byte[] dataArray, ref int dataOffset)
        {
            var output = new List<RmvVector4>();
            for (int i = 0; i < Header.Vec4ParamCount; i++)
            {
                var index = ByteParsers.UInt32.TryDecode(dataArray, dataOffset, out _, out _, out _);

                var result = ByteParsers.Single.TryDecodeValue(dataArray, dataOffset+4, out var x, out var byteLength, out var error);
                if (!result)
                    throw new Exception("Error reading RmvVector4 parameter - " + error);

                result = ByteParsers.Single.TryDecodeValue(dataArray, dataOffset+8, out var y, out byteLength, out error);
                if (!result)
                    throw new Exception("Error reading RmvVector4 parameter - " + error);

                result = ByteParsers.Single.TryDecodeValue(dataArray, dataOffset+12, out var z, out byteLength, out error);
                if (!result)
                    throw new Exception("Error reading RmvVector4 parameter - " + error);

                result = ByteParsers.Single.TryDecodeValue(dataArray, dataOffset+16, out var w, out byteLength, out error);
                if (!result)
                    throw new Exception("Error reading RmvVector4 parameter - " + error);

                dataOffset += (4 * 4) + 4;
                output.Add(new RmvVector4(x,y,z,w));
            }
            return output;
        }

        public RmvTexture? GetTexture(TexureType texureType)
        {
            if(Textures.Count(x => x.TexureType == texureType) != 0)
                return Textures.FirstOrDefault(x => x.TexureType == texureType);
            return null;
        }

        public AlphaMode GetAlphaMode()
        {
            return (AlphaMode)IntParams.First();
        }

        public void SetAlphaMode(AlphaMode mode)
        {
            IntParams[0] = (int)mode;
        }

        RmvMesh LoadMesh(byte[] dataArray, uint modelVersion, ref int dataOffset)
        {
            var vertexStart =  Header.VertexOffset + _modelStart; 
            var faceStart = Header.IndexOffset + _modelStart;

            return new RmvMesh(dataArray, modelVersion, Header.VertextType, (int)vertexStart, Header.VertexCount, (int)faceStart, Header.IndexCount);
        }

       public RmvSubModel Clone()
       {       
           return new RmvSubModel()
           {
               _modelStart = _modelStart,
               ParentSkeletonName = ParentSkeletonName,
               Header = Header,
               AttachmentPoints = AttachmentPoints.Select(x => x).ToList(),
               Textures = Textures.Select(x => x).ToList(),
               StringParams = StringParams.Select(x => x).ToList(),
               FloatParams = FloatParams.Select(x => x).ToList(),
               IntParams = IntParams.Select(x => x).ToList(),
               Vec4Params = Vec4Params.Select(x => x).ToList(),
               Mesh = null
           };
       }

        public void UpdateAttachmentPointList(List<string> boneNames)
        {
            var header = Header;
            header.AttachmentPointCount = (uint)boneNames.Count;
            Header = header;

            AttachmentPoints.Clear();
            for (int i = 0; i < boneNames.Count; i++)
            {
                var a = new RmvAttachmentPoint
                {
                    BoneIndex = i,
                    Name = boneNames[i],
                    Matrix = RmvMatrix3x4.Identity()
                };
                AttachmentPoints.Add(a);
            }
        }
    }

}
