using System;
using System.Collections.Generic;
using GTAWorldRenderer.Logging;
using System.IO;
using Microsoft.Xna.Framework;

namespace GTAWorldRenderer.Scenes.Loaders
{
   /// <summary>
   /// Парсит и заргужает данные из IPL (Item Placement) файла.
   /// В настоящий момент обрабатываются только секции INST, в которых задаётся расположение динамических
   /// и статических объектов  карты
   /// 
   /// http://gtamodding.ru/wiki/IPL - неофициальная спецификация IPL
   /// </summary>
   class IPLFileLoader
   {
      enum IPLSection
      {
         INST, // расположение динамических и статических объектов карты
         CULL, // зоны без дождя, зоны без воды, ТВ-экраны, карты отражений
         PICK, // This creates permanent weapon pickups. Rockstar only used this to create fire extinguisher pickups at fast food restaurants.
         END,
      }

      private static readonly int[] INST_REQUIRED_TOKENS = null;

      private Log Logger = Log.Instance;
      private IPLSection currentSection = IPLSection.END;
      private string filePath;
      private GtaVersion gtaVersion;

      static IPLFileLoader()
      {
         INST_REQUIRED_TOKENS = new int[4];
         INST_REQUIRED_TOKENS[(int)GtaVersion.Unknown] = 0;
         INST_REQUIRED_TOKENS[(int)GtaVersion.III] = 12;
         INST_REQUIRED_TOKENS[(int)GtaVersion.ViceCity] = 13;
         INST_REQUIRED_TOKENS[(int)GtaVersion.SanAndreas] = 11;
      }


      public IPLFileLoader(string filePath, GtaVersion gtaVersion)
      {
         this.filePath = filePath;
         this.gtaVersion = gtaVersion;
      }


      public IEnumerable<SceneItemPlacement> Load()
      {
         var objects = new List<SceneItemPlacement>();

         using (Logger.EnterStage("Reading IPL file: " + filePath))
         {
            using (StreamReader fin = new StreamReader(filePath))
            {
               string line;
               while ((line = fin.ReadLine()) != null)
               {
                  line = line.Trim();
                  if (line.Length == 0 || line.StartsWith("#"))
                     continue;

                  if (currentSection == IPLSection.END)
                     ProcessNewSectionStart(line);
                  else
                  {
                     var obj = ProcessSectionItem(line);
                     if (obj != null)
                        objects.Add(obj);
                  }
               }
            }
            Logger.Print(String.Format("Loaded {0} scene objects from IPL file {1}", objects.Count, filePath));
         }

         return objects;
      }


      private void ProcessNewSectionStart(string line)
      {
         //Trace.Assert(currentSection == IPLSection.END);
         if (line.StartsWith("inst"))
            currentSection = IPLSection.INST;
         else if (line.StartsWith("cull"))
            currentSection = IPLSection.CULL;
         else if (line.StartsWith("pick"))
            currentSection = IPLSection.PICK;
      }


      private SceneItemPlacement ProcessSectionItem(string line)
      {
         if (line.StartsWith("end"))
         {
            currentSection = IPLSection.END;
            return null;
         }
         if (currentSection != IPLSection.INST)
            return null;

         string[] toks = line.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

         if (toks.Length != INST_REQUIRED_TOKENS[(int)gtaVersion])
         {
            string msg = "Incorrect number of tokens in INST section: " + toks.Length.ToString() + ".";
            Log.Instance.Print(msg, MessageType.Error);
            throw new LoadingException(msg);
         }

         SceneItemPlacement obj = new SceneItemPlacement();
         obj.Id = Int32.Parse(toks[0]);
         obj.Name = toks[1];
         obj.Scale = Vector3.One;

         // y and z coords are exchanged because of different coordinate system !!!
         switch (gtaVersion) 
         {
            case GtaVersion.III:
               obj.Position = new Vector3(float.Parse(toks[2]), float.Parse(toks[4]), -float.Parse(toks[3]));
               obj.Scale = new Vector3(float.Parse(toks[5]), float.Parse(toks[6]), float.Parse(toks[7]));
               obj.Rotation = new Quaternion(float.Parse(toks[8]), float.Parse(toks[10]), -float.Parse(toks[9]), -float.Parse(toks[11]));
               break;

            case GtaVersion.ViceCity:
               obj.Position = new Vector3(float.Parse(toks[3]), float.Parse(toks[5]), -float.Parse(toks[4]));
               obj.Scale = new Vector3(float.Parse(toks[6]), float.Parse(toks[7]), float.Parse(toks[8]));
               obj.Rotation = new Quaternion(float.Parse(toks[9]), float.Parse(toks[11]), -float.Parse(toks[10]), -float.Parse(toks[12]));
               break;

            case GtaVersion.SanAndreas:
               obj.Position = new Vector3(float.Parse(toks[3]), float.Parse(toks[5]), -float.Parse(toks[4]));
               obj.Rotation = new Quaternion(float.Parse(toks[6]), float.Parse(toks[8]), -float.Parse(toks[7]), -float.Parse(toks[9]));
               // toks[10] -- LOD -- is temporary ignored
               break;
         }

         return obj;

      }
   }

}