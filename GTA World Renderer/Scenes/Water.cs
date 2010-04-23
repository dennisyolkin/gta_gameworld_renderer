using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTAWorldRenderer.Scenes.Loaders;
using System.IO;
using GTAWorldRenderer.Logging;

namespace GTAWorldRenderer.Scenes
{
   class WaterQuad
   {
      public float Level { get; private set; }
      public float Xmin { get; private set; }
      public float Xmax { get; private set; }
      public float Ymin { get; private set; }
      public float Ymax { get; private set; }


      /// <summary>
      /// Конструирует WaterQuad по строчке-описанию из файла water.dat
      /// </summary>
      public WaterQuad(string line)
      {
         var toks = line.Split(new char[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
         if (toks.Length != 5)
            Utils.TerminateWithError("WaterSystem: incorrect tokens count in line describing water quad");
         Level = float.Parse(toks[0]);
         Xmin = float.Parse(toks[1]);
         Ymin = float.Parse(toks[2]);
         Xmax = float.Parse(toks[3]);
         Ymax = float.Parse(toks[4]);
      }
   }


   class Water
   {
      public List<WaterQuad> WaterQuads { get; set; }


      public Water(GtaVersion gtaVersion)
      {
         if (gtaVersion == GtaVersion.SanAndreas)
            throw new NotImplementedException("Water System for GTA San Andreas have not been implemented yet");

         WaterQuads = new List<WaterQuad>();

         using (var reader = new StreamReader(Config.Instance.GTAFolderPath + "data\\water.dat"))
         {
            while (!reader.EndOfStream)
            {
               var line = reader.ReadLine();
               if (line.Length == 0 || line.StartsWith(";"))
                  continue;
               if (line.StartsWith("*")) // * is mark of EOF
                  break;
               WaterQuads.Add(new WaterQuad(line));
            }
         }

         Log.Instance.Print("Water quads: " + WaterQuads.Count);
      }
   }
}
