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
   /// Ячейка сетки.
   /// Хранит списки низкодетализированных и высокодетализированных объектов, пересекающих ячейку.
   /// </summary>
   class Cell
   {
      private List<Line2D> gridLines = new List<Line2D>();


      public Cell(float x, float y)
      {
         float cellSize = Config.Instance.Rasterization.GridCellSize;
         gridLines.Add(new Line2D(Utils.Point2ToVector3(x, y), Utils.Point2ToVector3(x + cellSize, y)));
         gridLines.Add(new Line2D(Utils.Point2ToVector3(x, y), Utils.Point2ToVector3(x, y + cellSize)));
         gridLines.Add(new Line2D(Utils.Point2ToVector3(x + cellSize, y + cellSize), Utils.Point2ToVector3(x + cellSize, y)));
         gridLines.Add(new Line2D(Utils.Point2ToVector3(x + cellSize, y + cellSize), Utils.Point2ToVector3(x, y + cellSize)));
      }


      public void DrawCellBounds(Effect effect)
      {
         foreach (var line in gridLines)
            line.Draw(effect);
      }
   }


   /// <summary>
   /// Одномерная регулярная сетка.
   /// По ячейкам сетки будут растеризовываться все объекты.
   /// Ячейка сетки хранит списки низкодетализированных и высокодетализированных объектов, пересекающих ячейку.
   /// По точке можно получать объекты из близлежащих ячеек.
   /// </summary>
   class Grid
   {
      public int GridRows { get; private set; }
      public int GridColumns { get; private set; }

      private readonly List<RawSceneObject> sceneObjects;
      private BoundingBox boundingRectangle;
      private List<Line2D> gridLines = new List<Line2D>();
      private readonly float cellSize = Config.Instance.Rasterization.GridCellSize;
      private Cell[,] cells;

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

         GridColumns = (int)((maxX - minX) / cellSize);
         if (GridColumns * cellSize < maxX - minX)
         {
            ++GridColumns;
            maxX = minX + GridColumns * cellSize;
         }

         GridRows = (int)((maxY - minY) / cellSize);
         if (GridRows * cellSize < maxY - minY)
         {
            ++GridColumns;
            maxY = minY + GridRows * cellSize;
         }

         for (float x = minX; x <= maxX; x += cellSize)
            gridLines.Add(new Line2D(Utils.Point2ToVector3(x, minY), Utils.Point2ToVector3(x, maxY)));

         for (float y = minY; y <= maxY; y += cellSize)
            gridLines.Add(new Line2D(Utils.Point2ToVector3(minX, y), Utils.Point2ToVector3(maxX, y)));

         cells = new Cell[GridRows, GridColumns];
         for (var i = 0; i != GridRows; ++i)
            for (var j = 0; j < GridColumns; ++j)
               cells[i, j] = new Cell(minX + j * cellSize, minY + i * cellSize);
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


      public void DrawHighlightedCell(Point cell, Effect effect)
      {
         cells[cell.Y, cell.X].DrawCellBounds(effect);
      }


      public IEnumerable<short> GetVisibleObjects(Vector3 cameraPos)
      {
         return null;
      }
   }
}
