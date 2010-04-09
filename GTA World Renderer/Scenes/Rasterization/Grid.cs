using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GTAWorldRenderer.Scenes.Rasterization
{
   /// <summary>
   /// 
   /// </summary>
   class Grid
   {
      private readonly List<RawSceneObject> sceneObjects;

      public BoundingBox BoundingRectangle { get; private set; }

      public Grid(List<RawSceneObject> sceneObjects) // TODO :: refactor
      {
         this.sceneObjects = sceneObjects;
         BoundingRectangle = new BoundingBox();
      }


      public void Build()
      {
         foreach (var obj in sceneObjects) // TODO :: refactor! it should be BOundingBox
            foreach (var mesh in obj.Model.Meshes)
            {
               var curBox = BoundingBox.CreateFromPoints(mesh.Vertices);
               BoundingRectangle = BoundingBox.CreateMerged(BoundingRectangle, curBox);
            }

      }


      public IEnumerable<short> GetVisibleObjects(Vector3 cameraPos)
      {
         return null;
      }
   }
}
