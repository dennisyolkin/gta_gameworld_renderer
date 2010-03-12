using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GTAWorldRenderer.Rendering
{
   /// <summary>
   /// Работа с камерой
   /// </summary>
   class Camera
   {
      private Vector3 position = new Vector3(0, 0, 0);
      float leftRightRotation = 0.0f;
      float upDownRotation = 0.0f;
      Matrix viewMatrix = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);


      public void UpdateRotation(float leftRight, float upDown)
      {
         leftRightRotation += leftRight;
         upDownRotation += upDown;

         UpdateViewMatrix();
      }


      public void UpdatePosition(Vector3 moveVector)
      {
         Matrix cameraRotation = Matrix.CreateRotationX(upDownRotation) * Matrix.CreateRotationY(leftRightRotation);
         Vector3 rotatedVector = Vector3.Transform(moveVector, cameraRotation);
         position += rotatedVector;

         UpdateViewMatrix();
      }


      private void UpdateViewMatrix()
      {
         Matrix rotation = Matrix.CreateRotationX(upDownRotation) * Matrix.CreateRotationY(leftRightRotation);
         Vector3 target = Vector3.Transform(new Vector3(0, 0, -1), rotation) + position;
         Vector3 up = Vector3.Transform(Vector3.Up, rotation);

         viewMatrix = Matrix.CreateLookAt(position, target, up);
      }


      public Matrix GetViewMatrix()
      {
         return viewMatrix;
      }
   }
}
