﻿using GameWorld.Core.Components.Rendering;
using GameWorld.Core.Rendering.Geometry;
using GameWorld.Core.Rendering.Shading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameWorld.Core.Rendering.RenderItems
{
    public class GeometryRenderItem : IRenderItem
    {
        public MeshObject Geometry { get; set; }
        public IShader Shader { get; set; }
        public Matrix ModelMatrix { get; set; }

        public void Draw(GraphicsDevice device, CommonShaderParameters parameters, RenderingTechnique renderingTechnique)
        {
            if (Shader.SupportsTechnique(RenderingTechnique.Normal) == false)
                return;

            Shader.SetTechnique(renderingTechnique);
            Shader.SetCommonParameters(parameters, ModelMatrix);
            Shader.ApplyObjectParameters();

            ApplyMesh(Shader, device, Geometry.GetGeometryContext());
        }

        void ApplyMesh(IShader effect, GraphicsDevice device, IGraphicsCardGeometry geometry)
        {
            device.Indices = geometry.IndexBuffer;
            device.SetVertexBuffer(geometry.VertexBuffer);
            foreach (var pass in effect.GetEffect().CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, geometry.IndexBuffer.IndexCount);
            }
        }
    }
}

