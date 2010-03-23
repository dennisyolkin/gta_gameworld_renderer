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
      public  Vector3 Position { get; private set; }
      public float LeftRightRotation { get; private set; }
      public float UpDownRotation { get; private set; }
      public Matrix ViewMatrix { get; private set; }

      private Matrix rotationMatrix;

      public Camera()
      {
         Position = Vector3.Zero;
         LeftRightRotation = MathHelper.Pi;
         UpDownRotation = 0.0f;
         rotationMatrix = Matrix.CreateRotationY(LeftRightRotation);
         UpdateViewMatrix();
      }


      public void UpdateRotation(float leftRight, float upDown)
      {
         LeftRightRotation += leftRight;
         UpDownRotation += upDown;
         UpdateViewMatrix();
      }


      public void UpdatePosition(Vector3 moveVector)
      {
         //Matrix cameraRotation = rotationMatrix;
         //cameraRotation *= Matrix.CreateRotationX(UpDownRotation) * Matrix.CreateRotationY(LeftRightRotation);
         Matrix cameraRotation = Matrix.CreateRotationX(UpDownRotation) * Matrix.CreateRotationY(LeftRightRotation);
         Vector3 rotatedVector = Vector3.Transform(moveVector, cameraRotation);
         Position += rotatedVector;

         UpdateViewMatrix();
      }


      public void FixRotation()
      {
         //GTAWorldRenderer.Logging.Log.Instance.Print("Fix");
         //Matrix newRotation = Matrix.CreateRotationX(UpDownRotation) * Matrix.CreateRotationY(LeftRightRotation);
         //rotationMatrix *= newRotation;

         //UpDownRotation = .0f;
         //LeftRightRotation = .0f;

         //UpdateViewMatrix();
      }


      private void UpdateViewMatrix()
      {
         //Matrix rotation = rotationMatrix;
         //rotation *= Matrix.CreateRotationX(UpDownRotation) * Matrix.CreateRotationY(LeftRightRotation);
         Matrix rotation = Matrix.CreateRotationX(UpDownRotation) * Matrix.CreateRotationY(LeftRightRotation);
         Vector3 target = Vector3.Transform(Vector3.Forward, rotation) + Position;
         Vector3 up = Vector3.Transform(Vector3.Up, rotation);

         ViewMatrix = Matrix.CreateLookAt(Position, target, up);
      }


   }
}

