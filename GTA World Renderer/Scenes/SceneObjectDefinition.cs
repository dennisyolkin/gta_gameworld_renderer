using Microsoft.Xna.Framework;
using GTAWorldRenderer.Scenes.Loaders;
using System.Collections.Generic;
using System;

namespace GTAWorldRenderer.Scenes
{
   /// <summary>
   /// Определяет объект сцены (его позицию, масштаб, поворт)
   /// Данные берутся из *.ipl файлов
   /// </summary>
   class SceneItemPlacement
   {
      public int Id{ get; set; }
      public string Name{ get; set; }
      public Vector3 Position{ get; set; }
      public Quaternion Rotation { get; set; }
      public Vector3 Scale { get; set; }
   }

   [Flags]
   enum IdeFlags // http://gtamodding.ru/wiki/IDE#.D0.A4.D0.BB.D0.B0.D0.B3.D0.B8_.D0.9E.D0.B1.D1.8A.D0.B5.D0.BA.D1.82.D0.BE.D0.B2
   {
      None, // general object
      WetEffect = 1 << 0,
      TimeObject = 1 << 1,
      AlphaTransparency1 = 1 << 2,
      AlphaTransparency2 = 1 << 3,
      TimeObjectFlag = 1 << 4,
      InteriorObject = 1 << 5,
      Shadows = 1 << 6,
      CollisionModelOff = 1 << 7,
      DrawDistanceOff = 1 << 8,
      BreakableGlass = 1 << 9,
      BreakableGlassWithCracks = 1 << 10,
      GarageDoors = 1 << 11,
      TwoClumpObject = 1 << 12,
      SmallFlora = 1 << 13,
      StandardFlora = 1 << 14,
      TimeCyclePoleShadow = 1 << 15,
      Explosive = 1 << 16,
      ScmFlag = 1 << 17,
      // Unknown = 1 << 18,
      // Unknown = 1 << 19,
      Graffity = 1 << 20,
      FaceCullingOff = 1 << 21,
   }


   /// <summary>
   /// Определяет описание объекта (текстуру и растояние отрисовки).
   /// Данные берутся из *.ide файлов
   /// </summary>
   class SceneItemDefinition
   {
      public string Name { get; set; }
      public string TextureFolder { get; set; }
      public float DrawDistance { get; set; }
      public IdeFlags Flags { get; set; }
   }


   /// <summary>
   /// Описание объекта на построенной сцене
   /// </summary>
   class RawSceneObject
   {
      public Matrix WorldMatrix { get; private set; }
      public ModelData Model { get; private set; }

      public RawSceneObject(ModelData model, Matrix worldMatrix)
      {
         Model = model;
         WorldMatrix = worldMatrix;
      }
   }

   /// <summary>
   /// Описание объекта на построенной сцене
   /// </summary>
   class CompiledSceneObject
   {
      public Matrix WorldMatrix { get; private set; }
      public Model3D Model { get; private set; }

      public CompiledSceneObject(Model3D model, Matrix worldMatrix)
      {
         Model = model;
         WorldMatrix = worldMatrix;
      }
   }


   class RawSceneObjectsList
   {
      public List<RawSceneObject> HighDetailedObjects { get; private set; }
      public List<RawSceneObject> LowDetailedObjects { get; private set; }

      public RawSceneObjectsList(List<RawSceneObject> highDetailedObjects, List<RawSceneObject> lowDetailedObjects)
      {
         this.HighDetailedObjects = highDetailedObjects;
         this.LowDetailedObjects = lowDetailedObjects;
      }
   }

}
