using System;
using System.Collections.Generic;
using GTAWorldRenderer.Logging;
using System.IO;

namespace GTAWorldRenderer.Scenes
{
   namespace Loaders
   {
      /// <summary>
      /// Загрузчик файлов .ide (Item Description)
      /// </summary>
      class IDEFileLoader
      {
         enum IDESection
         {
            OBJS, // описание статических и динамических объектов
            END,
         }

         private Log Logger = Log.Instance;
         private IDESection currentSection = IDESection.END;
         private string filePath;


         public IDEFileLoader(string filePath, GtaVersion gtaVersion)
         {
            this.filePath = filePath;
         }


         public IDictionary<int, SceneItemDefinition> Load()
         {
            var objects = new Dictionary<int, SceneItemDefinition>();

            using (Logger.EnterStage("Reading IDE file: " + filePath))
            {
               using (StreamReader fin = new StreamReader(filePath))
               {
                  string line;
                  while ((line = fin.ReadLine()) != null)
                  {
                     line = line.Trim();
                     if (line.Length == 0 || line.StartsWith("#"))
                        continue;

                     if (currentSection == IDESection.END)
                        ProcessNewSectionStart(line);
                     else
                     {
                        var obj = ProcessSectionItem(line);
                        if (obj != null)
                           objects[obj.Value.Key] = obj.Value.Value;
                     }
                  }
               }
               Logger.Print(String.Format("Loaded {0} scene object definitions from IDE file {1}", objects.Count, filePath));
            }

            return objects;
         }


         private void ProcessNewSectionStart(string line)
         {
            if (line.StartsWith("objs"))
               currentSection = IDESection.OBJS;
         }


         private KeyValuePair<int, SceneItemDefinition>? ProcessSectionItem(string line)
         {
            if (line.StartsWith("end"))
            {
               currentSection = IDESection.END;
               return null;
            }

            string[] toks = line.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (toks.Length < 5)
            {
               string msg = "Incorrect number of tokens in OBJS section: " + toks.Length.ToString() + ".";
               Log.Instance.Print(msg, MessageType.Error);
               throw new LoadingException(msg);
            }

            SceneItemDefinition obj = new SceneItemDefinition();
            int id = Int32.Parse(toks[0]);
            obj.Name = toks[1];
            obj.TextureFolder = toks[2];
            obj.DrawDistance = float.Parse(toks[4]);

            return new KeyValuePair<int, SceneItemDefinition>(id, obj);
         }

      }
   }
}
