using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GTAWorldRenderer.Rendering
{


   /// <summary>
   /// Позволяет переключаться по хоткею ммежду разными рендерерами
   /// </summary>
   class RendererSwitcher : Renderer
   {

      private KeyboardState oldKeyboardState = Keyboard.GetState();
      private List<Renderer> renderers = new List<Renderer>();
      private int currentRenderer = 0;


      public RendererSwitcher(ContentManager content, Renderer firstRenderer)
         : base(content)
      {
         renderers.Add(firstRenderer);
      }


      public void AddRenderer(Renderer renderer)
      {
         renderers.Add(renderer);
      }


      public override void Update(GameTime gameTime)
      {
         KeyboardState kbdState = Keyboard.GetState();
         Func<Keys, bool> KeyPressed = key => kbdState.IsKeyDown(key) && !oldKeyboardState.IsKeyDown(key);

         if (KeyPressed(Keys.F5))
            currentRenderer = (currentRenderer + 1) % renderers.Count;

         oldKeyboardState = kbdState;

         renderers[currentRenderer].Update(gameTime);
      }


      public override void Draw(GameTime gameTime)
      {
         renderers[currentRenderer].Draw(gameTime);
      }


   }
}
