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
         public List<string> Textures { get; set; }
         public List<Vector2> TextureCoords { get; set; }
         public List<short> Indices { get; set; }
         public List<Vector3> Vertices { get; set; }
         public List<Vector3> Normals { get; set; }

         public ModelData()
         {
            Textures = new List<string>();
         }


         public string Info
         {
            get
            {
               Func<IList, string> ToStr = x => (x == null ? "no" : x.Count.ToString());

               return String.Format("Vertices: {0}, Triangles: {1}",
                  ToStr(Vertices),
                  Indices == null ? "no" : (Indices.Count / 3).ToString() // TODO :: не учитывается TrianglesStrip
                  );
            }
         }
      }
   }
}
