
using System.Collections.Generic;
using GTAWorldRenderer.Scenes.Rasterization;

namespace GTAWorldRenderer.Scenes
{
   /// <summary>
   /// Представляет содержимое игрового мира
   /// </summary>
   class Scene
   {
      public List<CompiledSceneObject> SceneObjects{ get; set; }
      public Grid Grid { get; set; }

      public Scene()
      {
         SceneObjects = new List<CompiledSceneObject>();
      }
   }

}
