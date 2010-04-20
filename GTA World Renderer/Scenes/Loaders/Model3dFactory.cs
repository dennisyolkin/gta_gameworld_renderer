using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GTAWorldRenderer.Scenes.Loaders
{

   static class Model3dFactory
   {

      private static VertexBuffer CreateColoredVertexBuffer(ModelMeshData mesh)
      {
         var vertices = new VertexPositionColor[mesh.Vertices.Count];
         for (var i = 0; i != mesh.Vertices.Count; ++i)
            vertices[i] = new VertexPositionColor(mesh.Vertices[i], mesh.Colors[i]);
         var vertexBuffer = new VertexBuffer(GraphicsDeviceHolder.Device,
            mesh.Vertices.Count * VertexPositionColor.SizeInBytes, BufferUsage.WriteOnly);
         vertexBuffer.SetData(vertices);
         return vertexBuffer;
      }

      private static VertexBuffer CreateTexturedVertexBuffer(ModelMeshData mesh)
      {
         var vertices = new VertexPositionColorTexture[mesh.Vertices.Count];
         for (var i = 0; i != mesh.Vertices.Count; ++i)
            vertices[i] = new VertexPositionColorTexture(mesh.Vertices[i], mesh.Colors[i], mesh.TextureCoords[i]);
         VertexBuffer vertexBuffer = new VertexBuffer(GraphicsDeviceHolder.Device,
            mesh.Vertices.Count * VertexPositionColorTexture.SizeInBytes, BufferUsage.WriteOnly);
         vertexBuffer.SetData(vertices);
         return vertexBuffer;
      }


      private static IndexBuffer CreateIndexBuffer(ModelMeshData mesh, out List<ModelMeshPart3D> meshParts3d)
      {
         var indexBuffer = new IndexBuffer(GraphicsDeviceHolder.Device, mesh.SumIndicesCount * sizeof(short),
            BufferUsage.WriteOnly, IndexElementSize.SixteenBits);

         meshParts3d = new List<ModelMeshPart3D>();
         int offset = 0;
         foreach (ModelMeshPartData part in mesh.MeshParts)
         {
            indexBuffer.SetData(offset * sizeof(short), part.Indices.ToArray(), 0, part.Indices.Count);
            meshParts3d.Add(new ModelMeshPart3D(offset, mesh.TriangleStrip ? part.Indices.Count - 2 : part.Indices.Count / 3, part.MaterialId));
            offset += part.Indices.Count;
         }

         if (offset != mesh.SumIndicesCount)
            Utils.TerminateWithError("Incorrect total indices amount!");

         return indexBuffer;
      }


      private static ModelMesh3D CreateModelMesh(ModelMeshData mesh)
      {
         bool textured = mesh.TextureCoords != null;
         var vertexBuffer = textured ? CreateTexturedVertexBuffer(mesh) : CreateColoredVertexBuffer(mesh);

         List<ModelMeshPart3D> meshParts3d;
         var indexBuffer = CreateIndexBuffer(mesh, out meshParts3d);

         return new ModelMesh3D(
            new VertexDeclaration(GraphicsDeviceHolder.Device, textured? VertexPositionColorTexture.VertexElements : VertexPositionColor.VertexElements),
               vertexBuffer,
               indexBuffer,
               mesh.TriangleStrip,
               textured? VertexPositionColorTexture.SizeInBytes : VertexPositionColor.SizeInBytes,
               mesh.Materials,
               meshParts3d
            );
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
