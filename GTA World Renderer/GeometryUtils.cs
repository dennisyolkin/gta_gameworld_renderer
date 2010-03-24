using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GTAWorldRenderer.Scenes;
using Microsoft.Xna.Framework;

namespace GTAWorldRenderer
{
   static class GeometryUtils
   {


      public static void EvaluateNormals(SceneLoader.ModelMeshData mesh)
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
