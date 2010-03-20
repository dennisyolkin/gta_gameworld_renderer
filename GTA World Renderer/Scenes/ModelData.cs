using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Collections;

namespace GTAWorldRenderer.Scenes
{
   partial class SceneLoader
   {

      class ModelData
      {
         public List<ModelMeshData> Meshes { get; set; }


         public ModelData()
         {
            Meshes = new List<ModelMeshData>();
         }


         public string[] Info
         {
            get
            {
               string[] res = new string[Meshes.Count + 1];
               res[0] = "Meshes in model: " + Meshes.Count;
               for (int i = 0; i < Meshes.Count; ++i )
                  res[i + 1] = String.Format(" {0}: {1}", i + 1, Meshes[i].Info);
               return res;
            }
         }
      }


      class ModelMeshData
      {
         public string Texture { get; set;}
         public List<Vector2> TextureCoords { get; set; }
         public List<short> Indices { get; set; }
         public List<Vector3> Vertices { get; set; }
         public List<Vector3> Normals { get; set; }
         public bool TriangleStrip { get; set; }

         public ModelMeshData()
         {
            Texture = null;
         }


         public string Info
         {
            get
            {
               Func<IList, string> ToStr = x => (x == null ? "no" : x.Count.ToString());

               return String.Format("Vertices: {0}, Triangles: {1}, Texture: {2}",
                  ToStr(Vertices),
                  Indices == null ? "no" : (Indices.Count / 3).ToString(), // TODO :: не учитывается TrianglesStrip
                  Texture?? "no texture"
                  );
            }
         }
      }
   }
}
