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
         Model3D model = new Model3D();
         foreach (var mesh in modelData.Meshes)
            model.AddMesh(CreateModelMesh(mesh));
         return model;
      }

   }

}
