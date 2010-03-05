using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GTAWorldRenderer.Scenes
{
   class SceneObject // from IPL files
   {
      public int Id{ get; set; }
      public string Name{ get; set; }
      public Vector3 Position{ get; set; }
      public Quaternion Rotation { get; set; }
      public Vector3 Scale { get; set; }
   }

   class SceneObjectDefinition // from IDE files
   {
      public string Name { get; set; }
      public string TextureFolder { get; set; }
      public float DrawDistance { get; set; }
   }
}
