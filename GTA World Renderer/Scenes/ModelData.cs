using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;

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
      }


      public class ModelMeshPartData
      {
         public List<short> Indices { get; set; }
         public int MaterialId { get; set; }

         // TODO :: set default value for materialId (after upgrade to C# 4.0)
         public ModelMeshPartData(int indicesCapacity, int materialId)
         {
            this.MaterialId = materialId;
            Indices = new List<short>(indicesCapacity);
         }
      }


      public class ModelMeshData
      {
         public List<Material> Materials { get; set;}
         public List<Vector2> TextureCoords { get; set; }
         public List<Vector3> Vertices { get; set; }
         public List<Vector3> Normals { get; set; }
         public List<Color> Colors { get; set; }

         public List<ModelMeshPartData> MeshParts{ get; set; }

         public bool TriangleStrip { get; set; }
         public int SumIndicesCount { get; set; } // Количество индексов во всех частях меша суммарно

         public ModelMeshData()
         {
            Materials = new List<Material>();
         }

      }
   }
}
