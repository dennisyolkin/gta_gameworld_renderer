using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using GTAWorldRenderer.Scenes.Loaders;

namespace GTAWorldRenderer
{
   static class GeometryUtils
   {

      public static void EvaluateNormals(ModelMeshData mesh)
      {
         Vector3[] normals = new Vector3[mesh.Vertices.Count];
         for (int i = 0; i != mesh.Vertices.Count; ++i )
            normals[i] = Vector3.Zero;

         Action<int, int, int, int> ProcessTriangle = delegate(int partIdx, int idx1, int idx2, int idx3) 
         {
            List<short> indices = mesh.MeshParts[partIdx].Indices;
            int[] idx = { indices[idx1], indices[idx2], indices[idx3] };
            Vector3 side1 = mesh.Vertices[idx[1]] - mesh.Vertices[idx[0]];
            Vector3 side2 = mesh.Vertices[idx[2]] - mesh.Vertices[idx[0]];
            Vector3 norm = Vector3.Cross(side1, side2);
            norm.Normalize();
            for (int i = 0 ; i != 3; ++i)
               normals[idx[i]] += norm;
         };

         if (mesh.TriangleStrip)
            for (int j = 0; j != mesh.MeshParts.Count; ++j )
               for (int i = 2; i < mesh.MeshParts[j].Indices.Count; ++i)
                  ProcessTriangle(j, i - 2, i - 1, i);
         else
            for (int j = 0; j != mesh.MeshParts.Count; ++j)
               for (int i = 0; i < mesh.MeshParts[j].Indices.Count; i += 3)
                  ProcessTriangle(j, i, i + 1, i + 2);

         for (int i = 0; i != normals.Length; ++i)
         {
            normals[i].Normalize();
            normals[i] = normals[i];
         }

         mesh.Normals = new List<Vector3>(normals);
      }



   }
}
