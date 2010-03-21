using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GTAWorldRenderer.Rendering
{
   /// <summary>
   /// Представляет составной объект типа IRenderer. Позволяет инкапсулировать 
   /// набор побочных эффектов в главном. Делает прозрачным вызовы Update и Draw
   /// для главного и побочных объектов
   /// </summary>
   abstract class CompositeRenderer : Renderer
   {
      List<Renderer> subrenderers = new List<Renderer>();

      public CompositeRenderer(ContentManager contentManager)
         : base(contentManager)
      {
      }

      protected void AddSubRenderer(Renderer renderer)
      {
         subrenderers.Add(renderer);
      }

      void Initialize()
      {
         Initialize();
      }


      public override void Update(GameTime gameTime)
      {
         DoUpdate(gameTime);
         foreach (Renderer renderer in subrenderers)
            renderer.Update(gameTime);
      }


      public override void Draw(GameTime gameTime)
      {
         DoDraw(gameTime);
         foreach (Renderer renderer in subrenderers)
            renderer.Draw(gameTime);
      }


      public abstract void DoUpdate(GameTime gameTime);
      public abstract void DoDraw(GameTime gameTime);
   }
}
