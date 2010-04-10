
using System.Collections.Generic;
using GTAWorldRenderer.Scenes.Rasterization;

namespace GTAWorldRenderer.Scenes
{
   /// <summary>
   /// Представляет содержимое игрового мира
   /// </summary>
   class Scene
   {
      public List<CompiledSceneObject> HighDetailedObjects{ get; set; }
      public List<CompiledSceneObject> LowDetailedObjects { get; set; }
      public Grid Grid { get; set; }

      public Scene()
      {
         HighDetailedObjects = new List<CompiledSceneObject>();
         LowDetailedObjects = new List<CompiledSceneObject>();
      }
   }

}
