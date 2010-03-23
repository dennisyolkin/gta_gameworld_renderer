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

            if (mesh.Normals == null)
               EvaluateNormals(mesh);

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
               vertexBuffer, indexBuffer, mesh.TriangleStrip, VertexPositionNormalFormat.SizeInBytes, "Default");
         }


         public static Model3D CreateModel(ModelData modelData)
         {
            Model3D model = new Model3D();
            foreach (var mesh in modelData.Meshes)
               model.AddMesh(CreateModelMesh(mesh));
            return model;
         }

         // TODO :: возможно, это стоит перенести в какой-нибудь GeometryUtils (или MeshUtils?)
         private static void EvaluateNormals(ModelMeshData mesh)
         {
            Vector3[] normals = new Vector3[mesh.Vertices.Count];
            for (int i = 0; i != mesh.Vertices.Count; ++i )
               normals[i] = Vector3.Zero;

            Action<int, int, int> ProcessTriangle = delegate(int idx1, int idx2, int idx3) 
            {
               int[] idx = { mesh.Indices[idx1], mesh.Indices[idx2], mesh.Indices[idx3] };
               Vector3 side1 = mesh.Vertices[idx[1]] - mesh.Vertices[idx[0]];
               Vector3 side2 = mesh.Vertices[idx[2]] - mesh.Vertices[idx[0]];
               Vector3 norm = -Vector3.Cross(side1, side2);
               //norm.Normalize();
               for (int i = 0 ; i != 3; ++i)
                  normals[idx[i]] += norm;
            };

            if (mesh.TriangleStrip)
               for (int i = 2; i < mesh.Indices.Count; ++i)
                  ProcessTriangle(i - 2, i - 1, i);
            else
               for (int i = 0; i < mesh.Indices.Count; i += 3)
                  ProcessTriangle(i, i + 1, i + 2);

            for (int i = 0; i != normals.Length; ++i)
            {
               normals[i].Normalize();
               normals[i] = -normals[i];
            }

            mesh.Normals = new List<Vector3>(normals);
         }

      }

   }
}
