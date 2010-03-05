using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTAWorldRenderer.Scenes
{
   /// <summary>
   /// Представляет игровой мир
   /// </summary>
   class Scene
   {
      public void LoadScene()
      {
         SceneLoader sceneLoader = new SceneLoader();
         sceneLoader.LoadScene();
      }
   }
}
