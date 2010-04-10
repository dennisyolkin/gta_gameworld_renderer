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

      public List<int> HighDetailedObjects { get; set; }
      public List<int> LowDetailedObjects { get; set; }

      public Cell(float x, float y)
      {
         float cellSize = Config.Instance.Rasterization.GridCellSize;
         gridLines.Add(new Line2D(Utils.Point2ToVector3(x, y), Utils.Point2ToVector3(x + cellSize, y)));
         gridLines.Add(new Line2D(Utils.Point2ToVector3(x, y), Utils.Point2ToVector3(x, y + cellSize)));
         gridLines.Add(new Line2D(Utils.Point2ToVector3(x + cellSize, y + cellSize), Utils.Point2ToVector3(x + cellSize, y)));
         gridLines.Add(new Line2D(Utils.Point2ToVector3(x + cellSize, y + cellSize), Utils.Point2ToVector3(x, y + cellSize)));

         HighDetailedObjects = new List<int>();
         LowDetailedObjects = new List<int>();
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


   class CachedRequest
   {
      public Point RequestedCell { get; private set; }
      public List<int> HighDetailedResult { get; private set; }
      public List<int> LowDetailedResult { get; private set; }

      public CachedRequest(Point requestedCell, List<int> highDetailedResult, List<int> lowDetailedResult)
      {
         this.RequestedCell = requestedCell;
         this.HighDetailedResult = highDetailedResult;
         this.LowDetailedResult = lowDetailedResult;
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

      private RawSceneObjectsList sceneObjects;
      private BoundingBox boundingRectangle;
      private List<Line2D> gridLines = new List<Line2D>();
      private readonly float cellSize = Config.Instance.Rasterization.GridCellSize;
      private Cell[,] cells;
      private float minX, minY;
      private CachedRequest cachedRequest = null;

      public Grid(RawSceneObjectsList sceneObjects)
      {
         this.sceneObjects = sceneObjects;
         boundingRectangle = new BoundingBox();
      }


      public void Build()
      {
         using (Log.Instance.EnterTimingStage("Building grid"))
         {
            Log.Instance.Print("Processing vertices...");
            var highDetailedObjVertices = PreprocessObjects(sceneObjects.HighDetailedObjects);
            var lowDetailedObjVertices = PreprocessObjects(sceneObjects.LowDetailedObjects);

            Log.Instance.Print("Creating cells...");
            CreateCells();
            Log.Instance.Print(String.Format("Grid size: {0} rows, {1} columns, {2} cells", GridRows, GridColumns, GridRows * GridColumns));

            Log.Instance.Print("Rasterizing objects...");

            RasterizeObjects(highDetailedObjVertices, delegate(int row, int col)
               {
                  return cells[row, col].HighDetailedObjects;
               }
            );

            RasterizeObjects(lowDetailedObjVertices, delegate(int row, int col)
            {
               return cells[row, col].LowDetailedObjects;
            }
            );
         }
         sceneObjects = null; // чтобы освободить память
      }


      /// <summary>
      /// Преобразовывает координаты всех объектов в мировую систему координат,
      /// обновляет BoundingRectangle
      /// </summary>
      private List<ObjectVertices> PreprocessObjects(List<RawSceneObject> objects)
      {
         List<ObjectVertices> objVertices = new List<ObjectVertices>();
         for (var i = 0; i != objects.Count; ++i)
         {
            var curObj = new ObjectVertices() { Idx = i };

            foreach (var mesh in sceneObjects.HighDetailedObjects[i].Model.Meshes)
            {
               curObj.Vertices = new Vector3[mesh.Vertices.Count];
               for (var j = 0; j < curObj.Vertices.Length; ++j)
                  curObj.Vertices[j] = Vector3.Transform(mesh.Vertices[j], sceneObjects.HighDetailedObjects[i].WorldMatrix);

               objVertices.Add(curObj);

               var curBox = BoundingBox.CreateFromPoints(curObj.Vertices);
               boundingRectangle = BoundingBox.CreateMerged(boundingRectangle, curBox);
            }
         }
         return objVertices;
      }


      private void RasterizeObjects(List<ObjectVertices> objects, Func<int, int, List<int>> cellObjectsList)
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
               cellObjectsList(i, j).AddRange(cellsTmp[i, j]);

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


      /// <summary>
      /// Запрашивает список объектов, видимых из текущей позиции
      /// </summary>
      /// <param name="cameraPos">Текущая позиция камеры</param>
      /// <param name="highDetailedObjects">Список видимых высокодетализированных объектов</param>
      /// <param name="lowDetailedObjects">Список видимых низкодетализированных объектов</param>
      /// <returns></returns>
      public void GetVisibleObjects(Vector3 cameraPos, out List<int> highDetailedObjects, out List<int> lowDetailedObjects)
      {
         GetVisibleObjects(CellByPoint(cameraPos.X, cameraPos.Z), out highDetailedObjects, out lowDetailedObjects);
      }


      public void GetVisibleObjects(Point cellIdx, out List<int> highDetailedObjects, out List<int> lowDetailedObjects)
      {
         bool hasInCache = cachedRequest != null && cachedRequest.RequestedCell == cellIdx;
         if (!hasInCache)
         {
            var objectsFromCellsHD = new List<List<int>>(); // high-detailed objects
            var objectsFromCellsLD = new List<List<int>>(); // low-detailed objects

            var minRow = cellIdx.Y - 1;
            var maxRow = cellIdx.Y + 1;
            var minCol = cellIdx.X - 1;
            var maxCol = cellIdx.X + 1;

            if (minRow < 0) minRow = 0;
            if (maxRow >= GridRows) maxRow = GridRows - 1;
            if (minCol < 0) minCol = 0;
            if (maxCol >= GridColumns) maxCol = GridColumns - 1;

            // Добавляем HighDetailed-объекты из ячейки, в которой находится камера, и 8ми соседних ячеек
            for (var col = minCol; col <= maxCol; ++col)
               for (var row = minRow; row <= maxRow; ++row)
                  objectsFromCellsHD.Add(cells[row, col].HighDetailedObjects);

            // Добавляем LowDetailed-объекты из всех остальных ячеек
            for (var col = 0; col < GridColumns; ++col)
            {
              // if (col >= minCol && col <= maxCol)
              //    continue;

               for (var row = 0; row < GridRows; ++row)
               {
               //   if (row >= minRow && row <= maxRow)
               //      continue;

                  objectsFromCellsLD.Add(cells[row, col].LowDetailedObjects);
               }
            }

            var resultHD = Utils.MergeLists(objectsFromCellsHD);
            var resultLD = Utils.MergeLists(objectsFromCellsLD);
            cachedRequest = new CachedRequest(cellIdx, resultHD, resultLD);
         }
         highDetailedObjects = cachedRequest.HighDetailedResult;
         lowDetailedObjects = cachedRequest.LowDetailedResult;
      }
   }
}
