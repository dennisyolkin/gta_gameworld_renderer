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


      public Camera()
      {
         Position = Vector3.Zero;
         LeftRightRotation = 0.0f;
         UpDownRotation = 0.0f;
         ViewMatrix = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);
      }


      public void UpdateRotation(float leftRight, float upDown)
      {
         LeftRightRotation += leftRight;
         UpDownRotation += upDown;

         UpdateViewMatrix();
      }


      public void UpdatePosition(Vector3 moveVector)
      {
         Matrix cameraRotation = Matrix.CreateRotationX(UpDownRotation) * Matrix.CreateRotationY(LeftRightRotation);
         Vector3 rotatedVector = Vector3.Transform(moveVector, cameraRotation);
         Position += rotatedVector;

         UpdateViewMatrix();
      }


      private void UpdateViewMatrix()
      {
         Matrix rotation = Matrix.CreateRotationX(UpDownRotation) * Matrix.CreateRotationY(LeftRightRotation);
         Vector3 target = Vector3.Transform(new Vector3(0, 0, -1), rotation) + Position;
         Vector3 up = Vector3.Transform(Vector3.Up, rotation);

         ViewMatrix = Matrix.CreateLookAt(Position, target, up);
      }


   }
}
