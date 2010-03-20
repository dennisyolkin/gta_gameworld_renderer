using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GTAWorldRenderer.Scenes.VertexFormats;

namespace GTAWorldRenderer.Scenes
{
   partial class SceneLoader
   {

      static class Model3dFactory
      {

         public static ModelMesh3D CreateModelMesh(ModelMeshData mesh)
         {
            /*
             * TOTO :: здесь должен быть большоя switch, по которому должен определяться VertexFormat.
             * Но пока поддерживается только простейший VertexPositionNormal формат, поэтому всё просто.
             */

            // создаём VertexBuffer
            VertexPositionNormalFormat[] vertices = new VertexPositionNormalFormat[mesh.Vertices.Count];
            for (var i = 0; i != mesh.Vertices.Count; ++i)
               vertices[i] = new VertexPositionNormalFormat(mesh.Vertices[i], mesh.Normals[i]);
            VertexBuffer vertexBuffer = new VertexBuffer(GraphicsDeviceHolder.Device,
               mesh.Vertices.Count * VertexPositionNormalFormat.SizeInBytes, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            // создаём IndexBuffer
            IndexBuffer indexBuffer = new IndexBuffer(GraphicsDeviceHolder.Device, mesh.Indices.Count * sizeof(short),
               BufferUsage.WriteOnly, IndexElementSize.SixteenBits);
            indexBuffer.SetData(mesh.Indices.ToArray());

            return new ModelMesh3D(new VertexDeclaration(GraphicsDeviceHolder.Device, VertexPositionNormalFormat.VertexElements),
               vertexBuffer, indexBuffer, VertexPositionNormalFormat.SizeInBytes, "Default");
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
}
