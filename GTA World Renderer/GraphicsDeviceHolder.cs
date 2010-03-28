using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GTAWorldRenderer
{
   /// <summary>
   /// Хранит GraphicsDeviceManager. Один на всю программу.
   /// 
   /// Это удобно, так как GraphicsDevice нужен не только в Main при отрисовке, но и в некоторых Loader'ах
   /// (при создании IndexBuffer и VertexBuffer).
   /// </summary>
   static class GraphicsDeviceHolder
   {
      private static GraphicsDeviceManager deviceManager = null;
      public static GraphicsDeviceManager DeviceManager
      {
         get
         {
            return deviceManager;
         }

         set
         {
            deviceManager = value;
         }
      }

      public static GraphicsDevice Device { get; set; }


      /// <summary>
      /// Инициализирует поле Device
      /// Метод должен вызываться из Game.LoadContent.
      /// </summary>
      public static void InitDevice()
      {
         Device = deviceManager.GraphicsDevice;
      }
   }
}
