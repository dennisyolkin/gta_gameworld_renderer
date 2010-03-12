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
using System.Threading;
using System.Globalization;
using GTAWorldRenderer.Rendering;

namespace GTAWorldRenderer
{
   public class Main : Microsoft.Xna.Framework.Game
   {
      GraphicsDeviceManager graphics;
      GraphicsDevice device;

      public Main()
      {
         Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
         
         // ����������� ���
         //Log.Instance.AddLogWriter(ConsoleLogWriter.Instance);
         Log.Instance.AddLogWriter(new FileLogWriter("log.log"));
         Scene scene = new Scene();

         // ��������� �����
         //scene.LoadScene();
         //Log.Instance.PrintStatistic();

         GC.Collect();

         // ����������� ����������� ����������
         graphics = new GraphicsDeviceManager(this);
         Content.RootDirectory = "Content";
      }

      private const float rotationSpeed = 0.3f;

      Camera camera;
      TextInfoPanel textInfoPanel;
      MouseState originalMouseState;

      VertexBuffer vertexBuffer; // TODO :: temporary!
      IndexBuffer indexBuffer; // TODO :: temporary!
      VertexDeclaration vertexDeclaration; // TODO :: temporary
      Effect effect;

      protected override void Initialize()
      {
         camera = new Camera();

         base.Initialize();
      }


      protected override void LoadContent()
      {
         device = graphics.GraphicsDevice;
         textInfoPanel = new TextInfoPanel(Content, device);
         textInfoPanel.Camera = camera;

         Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
         originalMouseState = Mouse.GetState();

         // TODO :: temporary
         Scenes.SceneLoader.DffLoader dff = new Scenes.SceneLoader.DffLoader(@"c:\Program Files\GTAIII\models\Generic\arrow.DFF");
         dff.Load();
         vertexBuffer = dff.GetVertexBuffer(device);
         indexBuffer = dff.GetIndexBuffer(device);
         vertexDeclaration = new VertexDeclaration(device, Scenes.SceneLoader.DffLoader.VertexPositionNormalFormat.VertexElements);
         effect = Content.Load<Effect>("effect");
      }


      protected override void Update(GameTime gameTime)
      {
         float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;

         ProcessMouse(gameTime, timeDifference);
         ProcessKeyboard(gameTime, timeDifference);


         base.Update(gameTime);
      }


      protected override void Draw(GameTime gameTime)
      {
         GraphicsDevice.Clear(Color.Black);

         textInfoPanel.Draw();

         // TODO :: temporary
         //////////////////////////////////////////////////////////////////////////
         effect.CurrentTechnique = effect.Techniques["Simple"];
         effect.Parameters["xWorld"].SetValue(Matrix.Identity);
         effect.Parameters["xView"].SetValue(camera.viewMatrix);
         effect.Parameters["xProjection"].SetValue(Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 200.0f));
         effect.Begin();
         foreach (var pass in effect.CurrentTechnique.Passes)
         {
            pass.Begin();
            device.VertexDeclaration = vertexDeclaration;
            device.Vertices[0].SetSource(vertexBuffer, 0, Scenes.SceneLoader.DffLoader.VertexPositionNormalFormat.SizeInBytes);
            device.Indices = indexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.SizeInBytes / Scenes.SceneLoader.DffLoader.VertexPositionNormalFormat.SizeInBytes, 0, indexBuffer.SizeInBytes / sizeof(short));
            pass.End();
         }
         effect.End();
         //////////////////////////////////////////////////////////////////////////

         base.Draw(gameTime);
      }


      private void ProcessMouse(GameTime gameTime, float amount)
      {
         MouseState currentMouseState = Mouse.GetState();
         if (currentMouseState != originalMouseState && currentMouseState.LeftButton == ButtonState.Pressed)
         {
            float xDifference = currentMouseState.X - originalMouseState.X;
            float yDifference = -currentMouseState.Y + originalMouseState.Y;
            float leftrightRot = rotationSpeed * xDifference * amount;
            float updownRot = rotationSpeed * yDifference * amount;
            Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
            camera.UpdateRotation(leftrightRot, updownRot);
         }
      }


      private void ProcessKeyboard(GameTime gameTime, float amount)
      {
         Vector3 moveVector = new Vector3(0, 0, 0);
         KeyboardState keyState = Keyboard.GetState();
         if (keyState.IsKeyDown(Keys.Up))
            moveVector += Vector3.Forward;
         if (keyState.IsKeyDown(Keys.Down))
            moveVector += Vector3.Backward;
         if (keyState.IsKeyDown(Keys.Right))
            moveVector += Vector3.Right;
         if (keyState.IsKeyDown(Keys.Left))
            moveVector += Vector3.Left;
         if (keyState.IsKeyDown(Keys.PageUp))
            moveVector += Vector3.Up;
         if (keyState.IsKeyDown(Keys.PageDown))
            moveVector += Vector3.Down;
         camera.UpdatePosition(moveVector * amount);
      }

   }
}
