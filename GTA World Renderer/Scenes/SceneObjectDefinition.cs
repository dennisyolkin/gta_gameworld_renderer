using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GTAWorldRenderer.Scenes
{
   class SceneObjectDefinition
   {
      public int Id{ get; set; }
      public string Name{ get; set; }
      public Vector3 Position{ get; set; }
      public Quaternion Rotation { get; set; }
      public Vector3 Scale { get; set; }
   }
}
