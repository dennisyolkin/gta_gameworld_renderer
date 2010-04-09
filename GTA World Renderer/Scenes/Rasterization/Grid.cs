using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GTAWorldRenderer.Rendering;
using System.Collections;
using GTAWorldRenderer.Logging;

namespace GTAWorldRenderer.Scenes.Rasterization
{

   /// <summary>
   /// Ячейка сетки.
   /// Хранит списки низкодетализированных и высокодетализированных объектов, пересекающих ячейку.
   /// </summary>
   class Cell
   {
      private List<Line2D> gridLines = new List<Line2D>();

      /// <summary>
      /// Отсортированный список номеров объектов, лежащих в этой ячейке
      /// </summary>
      public List<int> Objects { get; private set; }


      public Cell(float x, float y)
      {
         float cellSize = Config.Instance.Rasterization.GridCellSize;
         gridLines.Add(new Line2D(Utils.Point2ToVector3(x, y), Utils.Point2ToVector3(x + cellSize, y)));
         gridLines.Add(new Line2D(Utils.Point2ToVector3(x, y), Utils.Point2ToVector3(x, y + cellSize)));
         gridLines.Add(new Line2D(Utils.Point2ToVector3(x + cellSize, y + cellSize), Utils.Point2ToVector3(x + cellSize, y)));
         gridLines.Add(new Line2D(Utils.Point2ToVector3(x + cellSize, y + cellSize), Utils.Point2ToVector3(x, y + cellSize)));
      }


      public void SetObjects(List<int> objects)
      {
         Objects = objects;
         Objects.Sort();
      }


      public void DrawCellBounds(Effect effect)
      {
         foreach (var line in gridLines)
            line.Draw(effect);
      }
   }


   struct ObjectVertices
   {
      public int Idx;
      public Vector3[] Vertices;
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
      private float minX, minY;

      public Grid(List<RawSceneObject> sceneObjects)
      {
         this.sceneObjects = sceneObjects;
         boundingRectangle = new BoundingBox();
      }


      public void Build()
      {
         using (Log.Instance.EnterTimingStage("Building grid"))
         {
            Log.Instance.Print("Processing vertices...");
            List<ObjectVertices> objVertices = new List<ObjectVertices>();
            for (var i = 0; i != sceneObjects.Count; ++i)
            {
               var curObj = new ObjectVertices() { Idx = i };

               foreach (var mesh in sceneObjects[i].Model.Meshes)
               {
                  curObj.Vertices = new Vector3[mesh.Vertices.Count];
                  for (var j = 0; j < curObj.Vertices.Length; ++j)
                     curObj.Vertices[j] = Vector3.Transform(mesh.Vertices[j], sceneObjects[i].WorldMatrix);

                  objVertices.Add(curObj);

                  var curBox = BoundingBox.CreateFromPoints(curObj.Vertices);
                  boundingRectangle = BoundingBox.CreateMerged(boundingRectangle, curBox);
               }
            }

            Log.Instance.Print("Creating cells...");
            CreateCells();

            Log.Instance.Print("Rasterizing objects...");
            RasterizeObjects(objVertices);

         }
      }


      private void RasterizeObjects(List<ObjectVertices> objects)
      {
         HashSet<int>[,] cellsTmp = new HashSet<int>[GridRows, GridColumns];
         for (int i = 0; i != GridRows; ++i)
            for (int j = 0; j != GridColumns; ++j)
               cellsTmp[i, j] = new HashSet<int>();

         foreach (var obj in objects)
         {
            foreach (var vert in obj.Vertices)
            {
               Point cellIdx = CellByPoint(vert.X, vert.Z);
               cellsTmp[cellIdx.Y, cellIdx.X].Add(obj.Idx);
            }
         }

         for (int i = 0; i != GridRows; ++i)
            for (int j = 0; j != GridColumns; ++j)
               cells[i, j].SetObjects(new List<int>(cellsTmp[i, j]));

      }


      private Point CellByPoint(float x, float y)
      {
         int row = (int)((y - minY) / cellSize);
         int col = (int)((x - minX) / cellSize);
         return new Point(col, row);
      }


      private void CreateCells()
      {
         const float inflateSize = 5;
         minX = boundingRectangle.Min.X - inflateSize;
         float maxX = boundingRectangle.Max.X + inflateSize;
         minY = boundingRectangle.Min.Z - inflateSize;
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
            ++GridRows;
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
