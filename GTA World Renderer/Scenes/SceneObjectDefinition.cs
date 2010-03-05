using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GTAWorldRenderer.Scenes
{
   /// <summary>
   /// Определяет объект сцены (его позицию, масштаб, поворт)
   /// Данные берутся из *.ipl файлов
   /// </summary>
   class SceneObject // from IPL files
   {
      public int Id{ get; set; }
      public string Name{ get; set; }
      public Vector3 Position{ get; set; }
      public Quaternion Rotation { get; set; }
      public Vector3 Scale { get; set; }
   }


   /// <summary>
   /// Определяет описание объекта (текстуру и растояние отрисовки).
   /// Данные берутся из *.ipl файлов
   /// </summary>
   class SceneObjectDefinition 
   {
      public string Name { get; set; }
      public string TextureFolder { get; set; }
      public float DrawDistance { get; set; }
   }
}
