using GTAWorldRenderer.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace GTAWorldRenderer.Rendering
{
   class SceneRenderer3D : CompositeRenderer
   {
      private const float rotationSpeed = 0.3f;
      private const float slowMoveSpeed = 50.0f;
      private const float fastMoveSpeed = 500.0f;

      private const float SlopeScaleDepthBiasForShadows = -5f;

      public Scene SceneContent { get; set; }

      Camera camera;
      InfoPanelFor3Dview textInfoPanel;
      Effect effect;
      Matrix projectionMatrix;
      bool wireframeMode = false;
      DepthStencilBuffer secondDepthBuffer; // используется для сохранения карты глубин воды

      private Renderer waterRenderer;
      private Renderer skyRenderer;

      KeyboardState oldKeyboardState = Keyboard.GetState();
      MouseState originalMouseState;
      bool usingMouse = true;


      /*
       * TODO :: 
       * в C# 4.0 появятся параметры по умолчанию.
       * Заменить набор конструкторов и метод Construct на единый конструктор с параметрами по умолчанию.
       */

      public SceneRenderer3D(ContentManager contentManager)
         : base(contentManager)
      {
         Initialize();
      }


      public SceneRenderer3D(ContentManager contentManager, Scene scene)
         : base(contentManager)
      {
         SceneContent = scene;
         Initialize();
      }


      public void Initialize()
      {
         camera = new Camera();

         textInfoPanel = new InfoPanelFor3Dview(Content);
         textInfoPanel.Camera = camera;
         AddSubRenderer(textInfoPanel);

         projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Device.Viewport.AspectRatio, 
            Config.Instance.Rendering.NearClippingDistance, Config.Instance.Rendering.FarClippingDistance);

         effect = Content.Load<Effect>("effect");

         waterRenderer = new WaterRenderer(Content, SceneContent, effect);
         skyRenderer = new SkyRenderer(Content, camera);
         secondDepthBuffer = new DepthStencilBuffer(Device, Device.PresentationParameters.BackBufferWidth, Device.PresentationParameters.BackBufferHeight, Device.DepthStencilBuffer.Format);

         Mouse.SetPosition(Device.Viewport.Width / 2, Device.Viewport.Height / 2);
         originalMouseState = Mouse.GetState();
      }


      public override void DoUpdate(GameTime gameTime)
      {
         float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;

         if (Config.Instance.Rendering.ShowWater)
            waterRenderer.Update(gameTime);

         if (Config.Instance.Rendering.ShowSky)
            skyRenderer.Update(gameTime);

         ProcessMouse(timeDifference);
         ProcessKeyboard(timeDifference);
      }


      private void ProcessMouse(float timeDifference)
      {
         if (!usingMouse)
            return;

         MouseState currentMouseState = Mouse.GetState();
         if (currentMouseState.LeftButton == ButtonState.Pressed && currentMouseState != originalMouseState)
         {
            float xDifference = -currentMouseState.X + originalMouseState.X;
            float yDifference = -currentMouseState.Y + originalMouseState.Y;
            float leftrightRot = rotationSpeed * xDifference * timeDifference;
            float updownRot = rotationSpeed * yDifference * timeDifference;
            camera.UpdateRotation(leftrightRot, updownRot);
         }
         Mouse.SetPosition(Device.Viewport.Width / 2, Device.Viewport.Height / 2);
      }


      private void ProcessKeyboard(float timeDifference)
      {
         Vector3 moveVector = new Vector3(0, 0, 0);
         KeyboardState keyState = Keyboard.GetState();

         Func<Keys, bool> KeyDown = key => keyState.IsKeyDown(key);
         Func<Keys, bool> KeyPressed = key => keyState.IsKeyDown(key) && !oldKeyboardState.IsKeyDown(key);

         // Перемещение камеры
         if (KeyDown(Keys.Up))
            moveVector += Vector3.Forward;
         if (KeyDown(Keys.Down))
            moveVector += Vector3.Backward;
         if (KeyDown(Keys.Right))
            moveVector += Vector3.Right;
         if (KeyDown(Keys.Left))
            moveVector += Vector3.Left;
         if (KeyDown(Keys.PageUp))
            moveVector += Vector3.Up;
         if (KeyDown(Keys.PageDown))
            moveVector += Vector3.Down;

         // Ускоренное перемещениеи камеры
         bool fast = keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift);

         if (KeyPressed(Keys.Escape))
         {
            usingMouse = !usingMouse;
            if (usingMouse)
               Mouse.SetPosition(Device.Viewport.Width / 2, Device.Viewport.Height / 2);
         }

         if (KeyPressed(Keys.W))
            wireframeMode = !wireframeMode;

         oldKeyboardState = keyState;

         camera.UpdatePosition(moveVector * timeDifference * (fast? fastMoveSpeed : slowMoveSpeed));
      }


      public override void DoDraw(GameTime gameTime)
      {
         if (SceneContent == null)
            return;

         if (wireframeMode)
            Device.RenderState.FillMode = FillMode.WireFrame;

         effect.Parameters["xView"].SetValue(camera.ViewMatrix);
         effect.Parameters["xProjection"].SetValue(projectionMatrix);
         
         List<int> visibleLowDetailedObjs, visibleHighDetailedObjs;
         SceneContent.Grid.GetVisibleObjects(camera.Position, out visibleHighDetailedObjs, out visibleLowDetailedObjs);
         textInfoPanel.Data["Objects to draw (HD)"] = String.Format("{0} of {1}", visibleHighDetailedObjs.Count, SceneContent.HighDetailedObjects.Count);
         textInfoPanel.Data["Objects to draw (LD)"] = String.Format("{0} of {1}", visibleLowDetailedObjs.Count, SceneContent.LowDetailedObjects.Count);

         // ---  отрисовка  ---

         // рисуем небо
         if (Config.Instance.Rendering.ShowSky)
            skyRenderer.Draw(gameTime);
         else
            Device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);

         DepthStencilBuffer oldDepthBuffer = null;

         Device.RenderState.DepthBufferEnable = true;

         // рисуем воду
         if (Config.Instance.Rendering.ShowWater)
         {
            waterRenderer.Draw(gameTime);
            oldDepthBuffer = Device.DepthStencilBuffer;
            Device.DepthStencilBuffer = secondDepthBuffer;
            Device.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            waterRenderer.Draw(gameTime);
         }

         // настраиваем RenderState
         Device.RenderState.AlphaBlendEnable = true;
         Device.RenderState.AlphaTestEnable = true;
         Device.RenderState.AlphaFunction = CompareFunction.Greater;
         Device.RenderState.AlphaBlendOperation = BlendFunction.Add;
         Device.RenderState.SourceBlend = Blend.SourceAlpha;
         Device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
         Device.RenderState.SeparateAlphaBlendEnabled = false;

         // TODO :: а надо ли оно?
         //Device.RenderState.FogColor = Color.WhiteSmoke;
         //Device.RenderState.FogStart = 50.0f;
         //Device.RenderState.FogEnd = 5000.0f;
         //Device.RenderState.FogTableMode = FogMode.Linear;
         //Device.RenderState.FogVertexMode = FogMode.None;
         //Device.RenderState.FogDensity = 1.0f;
         //Device.RenderState.FogEnable = true;


         // Рисуем низкодетализированный вариант нужного куска сцены
         foreach (var objIdx in visibleLowDetailedObjs)
         {
            var obj = SceneContent.LowDetailedObjects[objIdx];
            obj.Model.Draw(effect, obj.WorldMatrix, true);
         }

         // очищаем DepthBuffer от low-detailed объектов
         if (Config.Instance.Rendering.ShowWater)
            Device.DepthStencilBuffer = oldDepthBuffer;
         else
            Device.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

         // находим индекс, начиная с которого идут тени
         int shadowsStartIdx = visibleHighDetailedObjs.BinarySearch(SceneContent.ShadowsStartIdx);
         if (shadowsStartIdx < 0)
            shadowsStartIdx = ~shadowsStartIdx;

         // Рисуем высокодетализированный вариант нужного куска сцены  (без теней)
         for (var i = 0; i < shadowsStartIdx; ++i)
         {
            var obj = SceneContent.HighDetailedObjects[visibleHighDetailedObjs[i]];
            obj.Model.Draw(effect, obj.WorldMatrix, true);
         }

         // выключаем запись в z-буффер
         Device.RenderState.DepthBufferWriteEnable = false;

         Device.RenderState.SlopeScaleDepthBias = SlopeScaleDepthBiasForShadows;

         // Рисуем тени
         for (var i = shadowsStartIdx; i < visibleHighDetailedObjs.Count; ++i)
         {
            var obj = SceneContent.HighDetailedObjects[visibleHighDetailedObjs[i]];
            obj.Model.Draw(effect, obj.WorldMatrix, true);
         }

         //Device.RenderState.DepthBias = 0;
         Device.RenderState.SlopeScaleDepthBias = 0;

         // включаем запись в z-буффер
         Device.RenderState.DepthBufferWriteEnable = true;

         // выключаем WireFrame (он мог быть включен)
         Device.RenderState.FillMode = FillMode.Solid;
      }
   }
}
