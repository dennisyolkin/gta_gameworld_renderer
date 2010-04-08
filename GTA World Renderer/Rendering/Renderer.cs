using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GTAWorldRenderer
{
   /// <summary>
   /// Интерфейс для объектов, поддерживающих обновление состояния и отрисовку
   /// </summary>
   abstract class Renderer
   {
      protected ContentManager Content;
      protected GraphicsDevice Device;

      /// <summary>
      /// Конструктор. Должен вызываться тогда, когда уже инициализирован ContentManager и Device
      /// </summary>
      public Renderer(ContentManager contentManager)
      {
         this.Content = contentManager;
         this.Device = GraphicsDeviceHolder.Device;
      }

      /// <summary>
      /// Обновление объекта
      /// </summary>
      public virtual void Update(GameTime gameTime)
      {
      }


      /// <summary>
      /// Отрисовка объекта
      /// </summary>
      public virtual void Draw(GameTime gameTime)
      {
      }
   }
}
