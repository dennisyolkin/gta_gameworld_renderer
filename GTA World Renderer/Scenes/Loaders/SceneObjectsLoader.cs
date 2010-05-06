using System;
using System.Collections.Generic;
using GTAWorldRenderer.Logging;
using Microsoft.Xna.Framework;
using System.IO;

namespace GTAWorldRenderer.Scenes.Loaders
{

   /// <summary>
   /// Загружает всю необходимую для построения сцены информацию из файлов GTA, а именно:
   /// <list>
   /// <item>Модели</item>
   /// <item>Текстуры</item>
   /// <item>Положение всех моделей сцены</item>
   /// </list>
   /// Загруженные модели хранятся в виде списков вершин, индексов (а НЕ IndexBuffer и VertexBuffer), текстур, фреймов и т.д.
   /// </summary>
   class SceneObjectsLoader
   {

      class ModelEntry
      {
         public FileProxy FileProxy { get; private set; }
         public ModelData Model { get; set; }

         public ModelEntry(FileProxy fileProxy)
         {
            this.FileProxy = fileProxy;
         }
      }

      public SceneObjectsLoader(GtaVersion gtaVersion)
      {
         this.gtaVersion = gtaVersion;
      }

      private static Log Logger = Log.Instance;
      private GtaVersion gtaVersion;
      private Dictionary<int, SceneItemDefinition> objDefinitions = new Dictionary<int, SceneItemDefinition>();
      private List<SceneItemPlacement> objPlacements = new List<SceneItemPlacement>();
      private Dictionary<string, ModelEntry> loadedModels = new Dictionary<string, ModelEntry>();


      /// <summary>
      /// Препроцессит объекты сцены.
      /// Проверяет существование модели, отбрасывает "ночные" (и некоторые другие ненужные) модели,
      /// а также сортирует объекты так, чтобы модели с полупрозрачностью шли после непрозрачных
      /// </summary>
      private void PreprocessIpls()
      {
         var filteredObjPlacements = new List<SceneItemPlacement>();
         foreach (var obj in objPlacements)
         {
            if (filteredObjPlacements.Count >= Config.Instance.Loading.SceneObjectsAmountLimit)
            {
               Logger.Print("Limit for maximum number of objects to load is exceeded", MessageType.Warning);
               break;
            }

            // Игнорируем "ночной" вариант модели
            if (obj.Name.EndsWith("_nt"))
               continue;

            // в GTA Vice City такие модели дублируют те части сцены, которы и так представлены в LowDetailed.
            // из-за них z-fighting и падение fps
            if (obj.Name.StartsWith("islandlod"))
               continue;

            if (!loadedModels.ContainsKey(obj.Name))
               Utils.TerminateWithError("Model " + obj.Name + " is not loaded because it is not found");

            filteredObjPlacements.Add(obj);
         }

         // За счёт сортировки с учётом флагов в IDE добиваемся того, что объекты с альфой (в частности, тени) будут отрисовываться
         // после непрозрачных объектов
         filteredObjPlacements.Sort(
            delegate(SceneItemPlacement a, SceneItemPlacement b)
            {
               SceneItemDefinition aIde, bIde;
               objDefinitions.TryGetValue(a.Id, out aIde);
               objDefinitions.TryGetValue(b.Id, out bIde);

               if (aIde == null && bIde == null)
                  return a.Id.CompareTo(b.Id);

               if (aIde == null) return -1;
               if (bIde == null) return 1;

               bool aHasShadInName = a.Name.Contains("shad");
               bool bHasShadInName = b.Name.Contains("shad");

               if (aIde.Flags == bIde.Flags && aHasShadInName == bHasShadInName)
                  return a.Id.CompareTo(b.Id);

               bool aAlpha1 = (aIde.Flags & IdeFlags.AlphaTransparency1) != IdeFlags.None;
               bool aAlpha2 = (aIde.Flags & IdeFlags.AlphaTransparency2) != IdeFlags.None;
               bool bAlpha1 = (bIde.Flags & IdeFlags.AlphaTransparency1) != IdeFlags.None;
               bool bAlpha2 = (bIde.Flags & IdeFlags.AlphaTransparency2) != IdeFlags.None;
               bool aShadow = (aIde.Flags & IdeFlags.Shadows) != IdeFlags.None;
               bool bShadow = (bIde.Flags & IdeFlags.Shadows) != IdeFlags.None;

               if (aShadow && bShadow)
               {
                  if (!aHasShadInName && bHasShadInName) return -1;
                  if (aHasShadInName && !bHasShadInName) return 1;
               }

               if (!aShadow && bShadow) return -1;
               if (aShadow && !bShadow) return 1;
               if (!aAlpha1 && bAlpha1) return -1;
               if (aAlpha1 && !bAlpha1) return 1;
               if (!aAlpha2 && bAlpha2) return -1;
               if (aAlpha2 && !bAlpha2) return 1;

               return aIde.Flags.CompareTo(bIde.Flags);
            }
         );

         objPlacements = filteredObjPlacements;
      }


      private bool isShadow(SceneItemPlacement obj)
      {
         SceneItemDefinition ide = null;
         if (!objDefinitions.TryGetValue(obj.Id, out ide))
            return false;
         return ((ide.Flags & IdeFlags.Shadows) != IdeFlags.None) && obj.Name.Contains("shad");
      }


      public RawSceneObjectsList LoadScene()
      {
         using (Logger.EnterTimingStage("Loading all data from GTA"))
         {
            MessagesFilter? oldMessagesFilter = null;
            if (!Config.Instance.Loading.DetailedLogOutput)
            {
               oldMessagesFilter = Log.Instance.MessagesToOutput;
               Log.Instance.MessagesToOutput = MessagesFilter.Warning | MessagesFilter.Error;
            }
            try
            {
               string mainImgPath = Config.Instance.GTAFolderPath + "models/gta3.img";
               var loadedArchiveEntries = LoadMainImgArchive(mainImgPath);
               foreach (var item in loadedArchiveEntries)
                  loadedModels[item.Name.Substring(0, item.Name.Length - 4)] = new ModelEntry(item); // берём название модели без расширения .dff

               LoadDatFile("data/default.dat");
               LoadDatFile(GetVersionSpecificDatFile(gtaVersion));

               var sceneLowDetailed = new List<RawSceneObject>();
               var sceneHighDetailed = new List<RawSceneObject>();
               int missedIDEs = 0;

               PreprocessIpls();

               int shadowMinIdx = int.MaxValue;

               foreach (var obj in objPlacements)
               {
                  bool lowDetailedObj = obj.Name.StartsWith("lod");

                  string textureFolder = null;
                  var modelEntry = loadedModels[obj.Name];

                  if (modelEntry.Model == null)
                  {
                     if (objDefinitions.ContainsKey(obj.Id))
                     {
                        textureFolder = objDefinitions[obj.Id].TextureFolder;
                     }
                     else
                        ++missedIDEs;

                     var modelData = new DffLoader(modelEntry.FileProxy.GetData(), modelEntry.FileProxy.Name, textureFolder).Load();

                     // ignore object that has no texture coordinates
                     bool no_texture_coords = false;
                     foreach (var mesh in modelData.Meshes)
                        if (mesh.TextureCoords == null)
                        {
                           no_texture_coords = true;
                           break;
                        }
                     if (no_texture_coords)
                        continue;

                     modelEntry.Model = modelData;
                  }

                  // нам не нужны модели без значимой геометрии
                  if (modelEntry.Model.Meshes.Count == 0)
                     continue;

                  Matrix matrix = Matrix.CreateScale(obj.Scale) * Matrix.CreateFromQuaternion(obj.Rotation) * Matrix.CreateTranslation(obj.Position);

                  var objToAdd = new RawSceneObject(modelEntry.Model, matrix);
                  if (lowDetailedObj)
                     sceneLowDetailed.Add(objToAdd);
                  else
                  {
                     sceneHighDetailed.Add(objToAdd);
                     if (shadowMinIdx == int.MaxValue && isShadow(obj))
                        shadowMinIdx = sceneHighDetailed.Count - 1;
                  }
               }

               if (missedIDEs != 0)
                  Logger.Print(String.Format("Missed IDE(s): {0}", missedIDEs), MessageType.Warning);
               else
                  Logger.Print("No IDE was missed!");

               if (TexturesStorage.Instance.MissedTextures != 0)
                  Logger.Print(String.Format("Missed textures(s): {0}", TexturesStorage.Instance.MissedTextures), MessageType.Warning);
               else
                  Logger.Print("No texture was missed!");

               if (oldMessagesFilter != null)
                  Log.Instance.MessagesToOutput = oldMessagesFilter.Value;

               Logger.Print("Scene loaded!");
               Logger.PrintStatistic();
               Logger.Print(String.Format("Objects located on scene: {0} high-detailed, {1} low-detailed", sceneHighDetailed.Count, sceneLowDetailed.Count));
               Logger.Flush();

               var result = new RawSceneObjectsList(sceneHighDetailed, sceneLowDetailed);
               result.ShadowsStartIdx = shadowMinIdx;
               return result;

            }
            catch (Exception er)
            {
               Logger.Print("Failed to load scene! " + er, MessageType.Error);
               Logger.PrintStatistic();
               throw;
            }
            finally
            {
               if (oldMessagesFilter != null)
                  Log.Instance.MessagesToOutput = oldMessagesFilter.Value;
            }
         }
      }


      private List<FileProxy> LoadMainImgArchive(string path)
      {
         IMGArchive archive = new IMGArchive(path, gtaVersion);
         var items = archive.Load();
         var result = new List<FileProxy>();

         foreach (var item in items)
         {
            if (item.Name.EndsWith(".dff"))
               result.Add(item);
            else if (item.Name.EndsWith(".txd"))
               TexturesStorage.Instance.AddTexturesArchive(item);
         }

         return result;
      }


      private static string GetVersionSpecificDatFile(GtaVersion gtaVersion)
      {
         switch (gtaVersion)
         {
            case GtaVersion.III:
               return "data/gta3.dat";
            case GtaVersion.ViceCity:
               return "data/gta_vc.dat";
            case GtaVersion.SanAndreas:
               return "data/gta.dat";
            default:
               string msg = "Unsopported GTA version: " + gtaVersion.ToString() + ".";
               Logger.Print(msg, MessageType.Error);
               throw new LoadingException(msg);
         }
      }


      private void LoadDatFile(string path)
      {
         using (Logger.EnterStage("Reading DAT file: " + path))
         {
            using (StreamReader fin = new StreamReader(path))
            {
               string line;
               while ((line = fin.ReadLine()) != null)
               {
                  line = line.Trim();
                  if (line.Length == 0 || line.StartsWith("#"))
                     continue;

                  if (line.StartsWith("TEXDICTION"))
                  {
                     TexturesStorage.Instance.AddTexturesArchive(line.Substring("TEXDICTION ".Length));
                  }
                  else if (line.StartsWith("IDE"))
                  {
                     string fileName = line.Substring(4);
                     var objs = new IDEFileLoader(fileName, gtaVersion).Load();
                     foreach (var obj in objs)
                        objDefinitions.Add(obj.Key, obj.Value);
                  }
                  else if (line.StartsWith("IPL"))
                  {
                     string fileName = line.Substring(4);
                     var objs = new IPLFileLoader(fileName, gtaVersion).Load();
                     foreach (var obj in objs)
                        objPlacements.Add(obj);
                  } else if (line.StartsWith("IMG")) // in SanAndreas only
                  {
                     // Ignoring it, there is no interesting information for us
                  }
                  else if (line.StartsWith("SPLASH") || line.StartsWith("COLFILE") || line.StartsWith("MAPZONE") || line.StartsWith("MODELFILE"))
                  {
                     // Ignoring this commands
                  }
                  else
                  {
                     int sep_idx = line.IndexOf(' ');
                     if (sep_idx == -1)
                        sep_idx = line.IndexOf('\t');
                     if (sep_idx == -1)
                        sep_idx = line.Length;
                     string command = line.Substring(0, sep_idx);
                     Logger.Print("Unsupported command in DAT file: " + command, MessageType.Error);
                  }

               }
            }
         }
      }


   }



}
