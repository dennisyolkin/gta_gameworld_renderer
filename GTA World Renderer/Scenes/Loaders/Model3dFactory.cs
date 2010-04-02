using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GTAWorldRenderer.Scenes.Loaders
{

   static class Model3dFactory
   {

      public static ModelMesh3D CreateModelMesh(ModelMeshData mesh)
      {
         // создаём VertexBuffer
         VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[mesh.Vertices.Count];
         for (var i = 0; i != mesh.Vertices.Count; ++i)
            vertices[i] = new VertexPositionNormalTexture(mesh.Vertices[i], mesh.Normals[i], mesh.TextureCoords[i]);
         VertexBuffer vertexBuffer = new VertexBuffer(GraphicsDeviceHolder.Device,
            mesh.Vertices.Count * VertexPositionNormalTexture.SizeInBytes, BufferUsage.WriteOnly);
         vertexBuffer.SetData(vertices);

         // создаём IndexBuffer
         IndexBuffer indexBuffer = new IndexBuffer(GraphicsDeviceHolder.Device, mesh.SumIndicesCount * sizeof(short),
            BufferUsage.WriteOnly, IndexElementSize.SixteenBits);

         var meshParrts3d = new List<ModelMeshPart3D>();
         int offset = 0;
         foreach (ModelMeshPartData part in mesh.MeshParts)
         {
            indexBuffer.SetData(offset * sizeof(short), part.Indices.ToArray(), 0, part.Indices.Count);
            meshParrts3d.Add(new ModelMeshPart3D(offset, mesh.TriangleStrip? part.Indices.Count - 2 : part.Indices.Count / 3, part.MaterialId));
            offset += part.Indices.Count;
         }
         if (offset != mesh.SumIndicesCount)
            Utils.TerminateWithError("Incorrect total indices amount!");

         return new ModelMesh3D(new VertexDeclaration(GraphicsDeviceHolder.Device, VertexPositionNormalTexture.VertexElements),
            vertexBuffer, indexBuffer, mesh.TriangleStrip, VertexPositionNormalTexture.SizeInBytes, mesh.Materials, meshParrts3d);
      }


      public static Model3D CreateModel(ModelData modelData)
      {
         if (modelData.FrameNames == null)
            Utils.TerminateWithError("Incorrect model! No frames were presented!");

         Model3D model = new Model3D();

         for (int i = 0; i != modelData.FrameNames.Count; ++i)
         {
            /*
             * Если модель может преобразовываться (как например фонарный столб, который сначала ровный,
             * а если в него врезаться, от искривляется), то название фрейма будет оканчиваться на _lx, где x - число.
             * В этом случае нужно отрисовывать фрейм, оканчивающийся _l0, это недеформированная модель.
             */
            string s = modelData.FrameNames[i];
            if (s.IndexOf("_l") == s.Length - 3 && s[s.Length - 1] != '0')
               continue;

            // создаём меш, соответствующий нужному фрейму (если такой меш существует)
            int meshIdx = modelData.FrameToMesh[i];
            if (meshIdx != -1)
            {
               ModelMesh3D mesh = CreateModelMesh(modelData.Meshes[meshIdx]);
               model.AddMesh(mesh);
            }
         }

         return model;
      }

   }

}
