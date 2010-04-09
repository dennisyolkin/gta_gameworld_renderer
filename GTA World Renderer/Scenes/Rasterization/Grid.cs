using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GTAWorldRenderer.Rendering;

namespace GTAWorldRenderer.Scenes.Rasterization
{
   /// <summary>
   /// Одномерная регулярная сетка.
   /// По ячейкам сетки будут растеризовываться все объекты.
   /// Ячейка сетки хранит списки низкодетализированных и высокодетализированных объектов, пересекающих ячейку.
   /// По точке можно получать объекты из близлежащих ячеек.
   /// </summary>
   class Grid
   {
      private readonly List<RawSceneObject> sceneObjects;
      private BoundingBox boundingRectangle;
      private List<Line2D> gridLines = new List<Line2D>();

      public Grid(List<RawSceneObject> sceneObjects)
      {
         this.sceneObjects = sceneObjects;
         boundingRectangle = new BoundingBox();
      }


      public void Build()
      {
         foreach (var obj in sceneObjects)
            foreach (var mesh in obj.Model.Meshes)
            {
               var vertices = new Vector3[mesh.Vertices.Count];
               for(var i = 0; i < vertices.Length; ++i)
                  vertices[i] = Vector3.Transform(mesh.Vertices[i], obj.WorldMatrix);

               var curBox = BoundingBox.CreateFromPoints(vertices);
               boundingRectangle = BoundingBox.CreateMerged(boundingRectangle, curBox);
            }

         const float inflateSize = 5;
         float minX = boundingRectangle.Min.X - inflateSize;
         float maxX = boundingRectangle.Max.X + inflateSize;
         float minY = boundingRectangle.Min.Z - inflateSize;
         float maxY = boundingRectangle.Max.Z + inflateSize;

         for (float x = minX; x <= maxX; x += Config.Instance.Rasterization.GridCellSize)
            gridLines.Add(new Line2D(Utils.Point2ToVector3(x, minY), Utils.Point2ToVector3(x, maxY)));

         for (float y = minY; y <= maxY; y += Config.Instance.Rasterization.GridCellSize)
            gridLines.Add(new Line2D(Utils.Point2ToVector3(minX, y), Utils.Point2ToVector3(maxX, y)));
      }

      /// <summary>
      /// Отрисовывает сетку.
      /// В качестве effect должен быть effect2d.
      /// Должен быть установлен цвет (xColor).
      /// </summary>
      public void DrawGridLines(Effect effect)
      {
         foreach (var line in gridLines)
            line.Draw(effect);
      }


      public IEnumerable<short> GetVisibleObjects(Vector3 cameraPos)
      {
         return null;
      }
   }
}
