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
using GTAWorldRenderer.Scenes;

namespace GTAWorldRenderer
{
   public class Main : Microsoft.Xna.Framework.Game
   {
      GraphicsDeviceManager graphics;

      public Main()
      {
         ConsoleLogger.Instance.SetMessagesTypeToOutput((int)MessageType.Info | (int)MessageType.Error);
         Scene scene = new Scene();
         scene.LoadScene();
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
