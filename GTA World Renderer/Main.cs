using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using GTAWorldRenderer.Logging;

namespace GTAWorldRenderer
{
   public class Main : Microsoft.Xna.Framework.Game
   {
      GraphicsDeviceManager graphics;

      public Main()
      {

         using (ConsoleLogger.Instance.EnterStage("stage 1"))
         {
            ConsoleLogger.Instance.Print("vasya");
            ConsoleLogger.Instance.Print("petya");
            ConsoleLogger.Instance.Print("masha", MessageType.Warning);
            using (ConsoleLogger.Instance.EnterStage("stage 2"))
            {
               ConsoleLogger.Instance.Print("vova", MessageType.Error);
            }
         }
         ConsoleLogger.Instance.Print("dasha");
         ConsoleLogger.Instance.PrintStatistic();

         graphics = new GraphicsDeviceManager(this);
         Content.RootDirectory = "Content";
      }

      
      protected override void Initialize()
      {
         base.Initialize();
      }


      protected override void LoadContent()
      {
      }


      protected override void Update(GameTime gameTime)
      {
         base.Update(gameTime);
      }


      protected override void Draw(GameTime gameTime)
      {
         GraphicsDevice.Clear(Color.CornflowerBlue);
         base.Draw(gameTime);
      }
   }
}
