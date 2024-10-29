using System.Collections.Generic;
using GameWorld.Core;
using GameWorld.Core.Components;
using System.Windows.Input;
using System.Windows.Navigation;
using Editors.ImportExport.Exporting.Exporters.GltfSkeleton;
using Editors.ImportExport.Geometry;
using Shared.GameFormats.Animation;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Shared.Core.Events;
using Shared.Core.PackFiles;
using SharpDX.Direct3D9;
using SharpGLTF.Schema2;
using Matrix4x4 = System.Numerics.Matrix4x4;
using GameWorld.Core.Animation;


namespace Editors.ImportExport.Exporting.Exporters.GltfSkeleton
{
    public class TransformData
    {

        public Vector3 LocalTranslation { get; set; } = new Vector3(0, 0, 0);
        public Quaternion LocalRotation { get; set; } = Quaternion.Identity;

        public Vector3 GlobalTranslation { get; set; } = new Vector3(0, 0, 0);
        public Quaternion GlobalRotation { get; set; } = Quaternion.Identity;
    }

    public class SkeletonFrameNode
    {
        public string Name { get; set; } = "";
        public int ParentId { get; set; }
        public int Id { get; set; }
        public TransformData Transform { get; set; } = new TransformData();

        public Matrix GlobalTransform
        {
            get
            {
                var translationMatrix = Matrix.CreateTranslation(Transform.GlobalTranslation);
                var rotationMatrix = Matrix.CreateFromQuaternion(Transform.GlobalRotation);
                var framePoseMatrix = translationMatrix * rotationMatrix;
                return framePoseMatrix;
            }
        }
    }

    internal class FramePoseMatrixCalculator
    {
        private readonly AnimationFile _animationFile;
        private readonly SkeletonFrameNode[] _nodeList;
        private Matrix[] _worldTransform;

        public FramePoseMatrixCalculator(AnimationFile file)
        {
            ValidDateAnimationFIle(file);

            _animationFile = file;
            _nodeList = new SkeletonFrameNode[file.Bones.Length];
            _worldTransform = Enumerable.Repeat(Matrix.Identity, file.Bones.Length).ToArray();

            for (var boneIndex = 0; boneIndex < file.Bones.Length; boneIndex++)
            {
                _nodeList[boneIndex] = new SkeletonFrameNode()
                {
                    Id = file.Bones[boneIndex].Id,
                    ParentId = file.Bones[boneIndex].ParentId
                };
            }
        }

        public List<Matrix> GetInverseBindPoseMatrices()
        {
            RebuildSkeletonGlobalMatrices();

            var output = new List<Matrix>();
            for (var i = 0; i < _worldTransform.Length; i++)
            {
                var invBindPoseMatrix = Matrix.Invert(_worldTransform[i]);
                output.Add(invBindPoseMatrix);
            }

            return output;
        }

        private static void ValidDateAnimationFIle(AnimationFile file)
        {
            if (file.Bones.Length == 0)
                throw new Exception("No bones!");

            if (file.AnimationParts.Count == 0)
                throw new Exception("No anim parts!");

            if (file.AnimationParts[0].DynamicFrames.Count == 0)
                throw new Exception("No anim frames!");

            if (file.AnimationParts[0].DynamicFrames.Count == 0)
                throw new Exception("No anim frames!");

            if (file.AnimationParts[0].DynamicFrames[0].Quaternion.Count != file.Bones.Length)
                throw new Exception("No anim frames!");
        }

        private Vector3 GetLocalTranslateDiscrete(int boneIndex, int frameIndex = 0)
        {
            var translation = _animationFile.AnimationParts[0].DynamicFrames[frameIndex].Transforms[boneIndex].ToVector3();
            return Handedness.TranslationSwizzle(translation);

        }

        private Quaternion GetLocalQuaternionDiscrete(int boneIndex, int frameIndex = 0)
        {
            var q = _animationFile.AnimationParts[0].DynamicFrames[frameIndex].Quaternion[boneIndex].ToQuaternion();
            return Handedness.RotationSwizzle(q);

            //return new Quaternion(q.X, q.Y, q.Z, q.W);
            //return new Quaternion(q.X, q.Y, q.Z, q.W) * new Quaternion(0.5f, 0.5f, 0.5f, 0.5f);
            //return new Quaternion(q.Y, q.Z, q.X, q.W);
        }


        SkeletonFrameNode? GetParentNode(int boneIndex)
        {
            if (boneIndex >= _nodeList.Length)
            {
                throw new IndexOutOfRangeException("Bone Index Out Of Range!");
            }

            if (boneIndex == -1)
            {
                return null;
            }
            else
            {
                if (_nodeList[boneIndex].ParentId >= _nodeList.Length)
                {
                    throw new IndexOutOfRangeException("Parent Bone Index Out Of Range!");
                }

                if (_nodeList[boneIndex].ParentId == -1)
                {
                    return null;
                }
                else
                {
                    return _nodeList[_nodeList[boneIndex].ParentId];
                }
            }
        }



        public void UpdateFrameTransFormValuesDiscrete(int frameIndex)
        {
            for (var boneIndex = 0; boneIndex < _animationFile.Bones.Length; boneIndex++)
            {
                var parentNode = GetParentNode(boneIndex);
                if (parentNode != null)
                {
                    _nodeList[boneIndex].Transform.GlobalRotation = Quaternion.Multiply(GetLocalQuaternionDiscrete(boneIndex, frameIndex), parentNode.Transform.GlobalRotation);

                    _nodeList[boneIndex].Transform.GlobalTranslation = parentNode.Transform.GlobalTranslation +
                            Vector3.Transform(GetLocalTranslateDiscrete(boneIndex, frameIndex), parentNode.Transform.GlobalRotation);
                }
                else
                {
                    if (boneIndex == 0)
                    {
                        _nodeList[boneIndex].Transform.GlobalTranslation = new Vector3(0, 0, 0);
                        _nodeList[boneIndex].Transform.GlobalRotation = Quaternion.Identity;
                    }
                    else
                    {
                        _nodeList[boneIndex].Transform.GlobalTranslation = new Vector3(0, 0, 0);
                        _nodeList[boneIndex].Transform.GlobalRotation = Quaternion.Identity;
                    }
                }
            }
        }


        public List<Matrix> CalcuteFramePoseMatrixDiscrete(int frameIndex = 0)
        {
            var outPutMatrices = new List<Matrix>();

            UpdateFrameTransFormValuesDiscrete(frameIndex);

            // TODO: interpolate between frames, make _nodeList contain 2 frames and interpolate between them

            for (var boneIndex = 0; boneIndex < _animationFile.Bones.Length; boneIndex++)
            {
                var ztoYUpMatrix = new Matrix(
                    0, 0, 1, 0,
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 0, 1);


                var translationMatrix = ztoYUpMatrix * Matrix.CreateTranslation(_nodeList[boneIndex].Transform.GlobalTranslation);
                var rotationMatrix = ztoYUpMatrix * Matrix.CreateFromQuaternion(_nodeList[boneIndex].Transform.GlobalRotation);
                var scaleMatrix = Matrix.Identity; // ztoYUpMatrix * Matrix.CreateScale(0.1f, 0.1f, 1);
                var framePoseMatrix = scaleMatrix * translationMatrix * rotationMatrix;
                var invseframePoseMatrix = Matrix.Invert(framePoseMatrix);

                outPutMatrices.Add(invseframePoseMatrix);
            }

            return outPutMatrices;
        }

        private void RebuildSkeletonGlobalMatrices()
        {
            _worldTransform = new Matrix[_animationFile.Bones.Length];
            for (var i = 0; i < _animationFile.Bones.Length; i++)
            {
                var translationMatrix = Matrix.CreateTranslation(_animationFile.AnimationParts[0].DynamicFrames[0].Transforms[i].ToVector3());
                var rotationMatrix = Matrix.CreateFromQuaternion(_animationFile.AnimationParts[0].DynamicFrames[0].Quaternion[i].ToQuaternion());
                var scaleMatrix = Matrix.CreateScale(1, 1, 1);
                var transform = translationMatrix* rotationMatrix;
                _worldTransform[i] = transform;
            }

            for (var i = 0; i < _worldTransform.Length; i++)
            {
                var parentIndex = _animationFile.Bones[i].ParentId;

                if (parentIndex == -1)
                    continue;

                _worldTransform[i] = _worldTransform[i] * _worldTransform[parentIndex];
            }
        }
    };

    public class GltfAnimationBuilder
    {
        private readonly AnimationsContainerComponent _animationsContainerComponent = new AnimationsContainerComponent();
        private readonly AnimationPlayer _animationPlayer;
        private readonly AnimationFile _skeletonAnimFile;

        public GltfSkeletonContainer GltfSkeleton { get; private set; }

        public GltfAnimationBuilder(GltfSkeletonContainer gltfSkeletonContainer, AnimationFile skeletonAnimFile)
        {
            GltfSkeleton = gltfSkeletonContainer;
            _animationPlayer = _animationsContainerComponent.RegisterAnimationPlayer(new AnimationPlayer(), "MainPlayer");
            _skeletonAnimFile = skeletonAnimFile;
        }

        public GltfSkeletonContainer BuildFromTWAnim(AnimationFile animationFile, ModelRoot modelRoot)
        {
            var gameSkeleton = new GameSkeleton(_skeletonAnimFile, _animationPlayer);
            var animationClip = new AnimationClip(animationFile, gameSkeleton);

            var secondsPerFrame = animationClip.PlayTimeInSec / animationClip.DynamicFrames.Count;

            for (var boneIndex = 0; boneIndex < animationClip.AnimationBoneCount; boneIndex++)
            {
                var rotationKeyFrames = new List<(float Key, System.Numerics.Quaternion Value)>();
                var translationKeyFrames = new List<(float Key, System.Numerics.Vector3 Value)>();



                // fille key frames
                for (var frameIndex = 0; frameIndex < animationFile.AnimationParts[0].DynamicFrames.Count; frameIndex++)
                {



                    //rotationKeyFrames.Add((
                    //    secondsPerFrame * frameIndex, ToQuaternion(Quaternion.Normalize(new Quaternion(0.5f, 0.5f, 0.5f, 0.5f) * (1.0f + secondsPerFrame * frameIndex)))));

                    //translationKeyFrames.Add(((float)secondsPerFrame * frameIndex, ToVector3(new Vector3(1, 1, 1) * (1.0f + secondsPerFrame * frameIndex))));

                    //nodeTest.
                    //WithTranslationAnimation($"{gameSkeleton.BoneNames[boneIndex]}_trans", (, ToVector3(animationClip.DynamicFrames[frameIndex].Position[boneIndex]) * (secondsPerFrame * (float)frameIndex)));

                    //nodeTest.
                    //WithRotationAnimation($"{gameSkeleton.BoneNames[boneIndex]}_quat", (secondsPerFrame * (float)frameIndex, ToQuaternion(animationClip.DynamicFrames[frameIndex].Rotation[boneIndex])));






                }

                var nodeTest = GltfSkeleton.GetNode(boneIndex);
                nodeTest.
                WithTranslationAnimation($"{animationFile.Bones[boneIndex].Name}_trans", (0.0f, new System.Numerics.Vector3(1, 2, 1))).
                WithRotationAnimation($"{animationFile.Bones[boneIndex].Name}_quat", (0.0f, new System.Numerics.Quaternion(0.5f, 0.5f, 0.5f, 0.5f)));

                //nodeTest.
                //    WithTranslationAnimation($"{animationFile.Bones[boneIndex].Name}_trans", translationKeyFrames.ToArray()).
                //    WithRotationAnimation($"{animationFile.Bones[boneIndex].Name}_quat", rotationKeyFrames.ToArray());

            }

            return GltfSkeleton;
        }





        static private System.Numerics.Vector3 ToVector3(Vector3 v) { return new System.Numerics.Vector3(v.X, v.Y, v.Z); }
        static private System.Numerics.Quaternion ToQuaternion(Quaternion q) { return new System.Numerics.Quaternion(q.X, q.Y, q.Z, q.W); }

    }
}
