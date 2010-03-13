using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace GTAWorldRenderer.Scenes
{
   /// <summary>
   /// Представляет игровой мир
   /// </summary>
   class Scene
   {
      IEnumerable<Model3D> sceneModels;

      /// <summary>
      /// Загружает игровой мир GTA
      /// </summary>
      public void LoadScene()
      {
         SceneLoader sceneLoader = new SceneLoader();
         sceneModels = sceneLoader.LoadScene();
      }

      /// <summary>
      /// Отрисовывает сцену
      /// </summary>
      public void Draw(Effect effect)
      {
         foreach (var model in sceneModels)
            model.Draw(effect);
      }
   }
}
