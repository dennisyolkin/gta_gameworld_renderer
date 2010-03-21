using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GTAWorldRenderer
{
   /// <summary>
   /// Интерфейс для объектов, поддерживающих инициализацию, обновление состояния и отрисовку
   /// </summary>
   interface IRenderer
   {
      /// <summary>
      /// Инициализация объекта. На этом этапе Device уже инициализирован!
      /// </summary>
      void Initialize();


      /// <summary>
      /// Обновление объекта
      /// </summary>
      void Update(GameTime gameTime);


      /// <summary>
      /// Отрисовка объекта
      /// </summary>
      void Draw(GameTime gameTime);
   }
}
