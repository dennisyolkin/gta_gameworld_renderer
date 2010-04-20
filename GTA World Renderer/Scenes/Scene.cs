
using System.Collections.Generic;
using GTAWorldRenderer.Scenes.Rasterization;

namespace GTAWorldRenderer.Scenes
{
   /// <summary>
   /// Представляет содержимое игрового мира
   /// </summary>
   class Scene
   {
      /// <summary>
      /// Список высокодетализированных объектов сцены
      /// </summary>
      public List<CompiledSceneObject> HighDetailedObjects{ get; set; }


      /// <summary>
      /// Список низкодетализированных объектов сцены
      /// </summary>
      public List<CompiledSceneObject> LowDetailedObjects { get; set; }

      /// <summary>
      /// Регулярная сетка, по которой растеризованы все объекты.
      /// Позволяет по позиции камеры на сцене получить список высокодетализированных
      /// и низкодетализированных объектов сцены, которые нужно отрисовать
      /// </summary>
      public Grid Grid { get; set; }

      /// <summary>
      /// Индекс, с которого начинаются тени. Тени идут всегда в конце списка.
      /// Тени учитываются только в HighDetailed!
      /// </summary>
      public int ShadowsStartIdx { get; set; }



      public Scene()
      {
         HighDetailedObjects = new List<CompiledSceneObject>();
         LowDetailedObjects = new List<CompiledSceneObject>();
      }
   }

}
