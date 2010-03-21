using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GTAWorldRenderer.Rendering
{
   /// <summary>
   /// Представляет составной объект типа IRenderer. Позволяет инкапсулировать 
   /// набор побочных эффектов в главном. Делает прозрачным вызовы Update и Draw
   /// для главного и побочных объектов
   /// </summary>
   abstract class CompositeRenderer : IRenderer
   {
      List<IRenderer> subrenderers = new List<IRenderer>();


      protected void AddSubRenderer(IRenderer renderer)
      {
         subrenderers.Add(renderer);
      }

      void IRenderer.Initialize()
      {
         Initialize();
      }


      void IRenderer.Update(GameTime gameTime)
      {
         Update(gameTime);
         foreach (IRenderer renderer in subrenderers)
            renderer.Update(gameTime);
      }


      void IRenderer.Draw(GameTime gameTime)
      {
         Draw(gameTime);
         foreach (IRenderer renderer in subrenderers)
            renderer.Draw(gameTime);
      }


      public abstract void Initialize();
      public abstract void Update(GameTime gameTime);
      public abstract void Draw(GameTime gameTime);
   }
}
