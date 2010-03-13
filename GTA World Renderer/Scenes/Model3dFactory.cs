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


         public static Model3D CreateModel(ModelData modelData)
         {
            /*
             * TOTO :: здесь должен быть большоя switch, по которому должен определяться VertexFormat.
             * Но пока поддерживается только простейший VertexPositionNormal формат, поэтому всё просто.
             */

            // создаём VertexBuffer
            VertexPositionNormalFormat[] vertices = new VertexPositionNormalFormat[modelData.Vertices.Count];
            for (var i = 0; i != modelData.Vertices.Count; ++i)
               vertices[i] = new VertexPositionNormalFormat(modelData.Vertices[i], modelData.Normals[i]);
            VertexBuffer vertexBuffer = new VertexBuffer(GraphicsDeviceHolder.Device, 
               modelData.Vertices.Count * VertexPositionNormalFormat.SizeInBytes, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
 
            // создаём IndexBuffer
            IndexBuffer indexBuffer = new IndexBuffer(GraphicsDeviceHolder.Device, modelData.Indices.Count * sizeof(short), 
               BufferUsage.WriteOnly, IndexElementSize.SixteenBits);
            indexBuffer.SetData(modelData.Indices.ToArray());

            return new Model3D(new VertexDeclaration(GraphicsDeviceHolder.Device, VertexPositionNormalFormat.VertexElements),
               vertexBuffer, indexBuffer, VertexPositionNormalFormat.SizeInBytes, "Default");
         }


      }

   }
}
