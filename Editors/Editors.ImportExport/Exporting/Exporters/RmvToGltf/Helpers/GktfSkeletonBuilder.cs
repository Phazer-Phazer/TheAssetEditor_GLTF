using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Shared.Core.Events;
using Shared.Core.PackFiles;
using Shared.GameFormats.Animation;
using SharpDX.Direct3D9;
using SharpGLTF.Schema2;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace Editors.ImportExport.Exporting.Exporters.GltfSkeleton
{
    public class GktfSkeletonBuilder
    {
        public static GltfSkeletonContainer Build(ModelRoot model, AnimationFile animSkeletonFil)
        {
            var framePoseMatrixCalculator = new FramePoseMatrixCalculator(animSkeletonFil);
            var invMatrices = framePoseMatrixCalculator.GetInverseBindPoseMatrices();

            var output = new List<(Node node, Matrix4x4 invMatrix)>();

            var scene = model.UseScene("default");
            var parentIdToGltfNode = new Dictionary<int, Node>();
            var frame = animSkeletonFil.AnimationParts[0].DynamicFrames[0];
            Node? parentNode = null;

            parentIdToGltfNode[-1] = scene.CreateNode(""); // bone with not parnrts will be children of the scene

            for (var boneIndex = 0; boneIndex < animSkeletonFil.Bones.Length; boneIndex++)
            {
                parentNode = parentIdToGltfNode[animSkeletonFil.Bones[boneIndex].ParentId];

                if (parentNode == null)
                    throw new Exception("Parent Node not found!");

                parentIdToGltfNode[boneIndex] = parentNode.CreateNode(animSkeletonFil.Bones[boneIndex].Name);

                parentIdToGltfNode[boneIndex].
                    WithLocalTranslation(new System.Numerics.Vector3(frame.Transforms[boneIndex].X, frame.Transforms[boneIndex].Y, frame.Transforms[boneIndex].Z)).
                    WithLocalRotation(new System.Numerics.Quaternion(frame.Quaternion[boneIndex].X, frame.Quaternion[boneIndex].Y, frame.Quaternion[boneIndex].Z, frame.Quaternion[boneIndex].W));

                var invBindPoseMatrix4x4 = /*Matrix4x4.Transpose*/(Create4x4SysMatrix(invMatrices, boneIndex));

                output.Add((parentIdToGltfNode[boneIndex], invBindPoseMatrix4x4));
            }

            return new GltfSkeletonContainer(output);
        }

        private static Matrix4x4 Create4x4SysMatrix(List<Matrix> invMatrices, int boneIndex) => new Matrix4x4(
                                invMatrices[boneIndex].M21, invMatrices[boneIndex].M22, invMatrices[boneIndex].M23, invMatrices[boneIndex].M24,
                                invMatrices[boneIndex].M21, invMatrices[boneIndex].M22, invMatrices[boneIndex].M23, invMatrices[boneIndex].M24,
                                invMatrices[boneIndex].M31, invMatrices[boneIndex].M32, invMatrices[boneIndex].M33, invMatrices[boneIndex].M34,
                                invMatrices[boneIndex].M41, invMatrices[boneIndex].M42, invMatrices[boneIndex].M43, invMatrices[boneIndex].M44);

    }
    public class GltfSkeletonContainer
    {
        public GltfSkeletonContainer(List<(Node node, Matrix4x4 invMatrix)> skeletonData)
        {
            SkeletonData = skeletonData;
        }

        public List<(Node node, Matrix4x4 invMatrix)> SkeletonData;

        public Node GetNode(int boneIndex)
        {
            return SkeletonData[boneIndex].node;
        }
    }

}


