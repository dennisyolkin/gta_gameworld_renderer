
using System.Collections.Generic;

namespace GTAWorldRenderer.Scenes
{
   /// <summary>
   /// Представляет содержимое игрового мира
   /// </summary>
   class Scene
   {
      public List<SceneObject> SceneObjects{ get; set; }


      public Scene()
      {
         SceneObjects = new List<SceneObject>();
      }
   }

}
