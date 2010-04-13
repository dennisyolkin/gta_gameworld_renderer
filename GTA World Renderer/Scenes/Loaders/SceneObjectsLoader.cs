using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

      private static Log Logger = Log.Instance;
      private GtaVersion gtaVersion;
      private Dictionary<int, SceneItemDefinition> objDefinitions = new Dictionary<int, SceneItemDefinition>();
      private List<SceneItemPlacement> objPlacements = new List<SceneItemPlacement>();

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
               Logger.Print("Switching working directory to GTA folder");
               System.Environment.CurrentDirectory = Config.Instance.GTAFolderPath;

               gtaVersion = GetGtaVersion();

               var loadedModels = new Dictionary<string, ModelEntry>();

               string mainImgPath = Config.Instance.GTAFolderPath + "models/gta3.img";
               var loadedArchiveEntries = LoadMainImgArchive(mainImgPath);
               foreach (var item in loadedArchiveEntries)
                  loadedModels[item.Name.Substring(0, item.Name.Length - 4)] = new ModelEntry(item); // берём название модели без расширения .dff

               LoadDatFile("data/default.dat");
               LoadDatFile(GetVersionSpecificDatFile(gtaVersion));

               var sceneLowDetailed = new List<RawSceneObject>();
               var sceneHighDetailed = new List<RawSceneObject>();
               int missedIDEs = 0;

               foreach (var obj in objPlacements)
               {
                  if (sceneLowDetailed.Count + sceneHighDetailed.Count >= Config.Instance.Loading.SceneObjectsAmountLimit)
                  {
                     Logger.Print("Limit for maximum number of objects to load is exceeded", MessageType.Warning);
                     break;
                  }

                  bool lowDetailedObj = obj.Name.StartsWith("lod");

                  if (!loadedModels.ContainsKey(obj.Name))
                     Utils.TerminateWithError("Model " + obj.Name + " is not loaded because it is not found");

                  string textureFolder = null;
                  var modelEntry = loadedModels[obj.Name];
                  if (modelEntry.Model == null)
                  {
                     if (objDefinitions.ContainsKey(obj.Id))
                        textureFolder = objDefinitions[obj.Id].TextureFolder;
                     else
                        ++missedIDEs;

                     var modelData = new DffLoader(modelEntry.FileProxy.GetData(), modelEntry.FileProxy.Name, textureFolder).Load();

                     // - - - - - - - - - - -- - - - - - - - - - -- - - - - - - - - - -- - - - - - - - - - -- - - - - - - - - - -
                     // TODO :: workaround situation when object has no texture coodinates. FIX IT!!!!
                     bool fail = false;
                     foreach (var mesh in modelData.Meshes)
                        if (mesh.TextureCoords == null)
                        {
                           fail = true;
                           break;
                        }
                     if (fail)
                     {
                        Logger.Print("FIXME: Object has no texture coordinates and was IGNORED.", MessageType.Warning);
                        continue;
                     }
                     // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

                     modelEntry.Model = modelData;
                  }

                  Matrix matrix = Matrix.CreateScale(obj.Scale) * Matrix.CreateFromQuaternion(obj.Rotation) * Matrix.CreateTranslation(obj.Position);

                  var objToAdd = new RawSceneObject(modelEntry.Model, matrix);
                  if (lowDetailedObj)
                     sceneLowDetailed.Add(objToAdd);
                  else
                     sceneHighDetailed.Add(objToAdd);
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

               return new RawSceneObjectsList(sceneHighDetailed, sceneLowDetailed);

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


      private static GtaVersion GetGtaVersion()
      {
         Logger.Print("Determining GTA version...");
         if (File.Exists("gta3.exe"))
         {
            Logger.Print("... version is GTA III");
            return GtaVersion.III;
         }
         else if (File.Exists("gta-vc.exe"))
         {
            Logger.Print("... version is GTA Vice City");
            return GtaVersion.ViceCity;
         }
         else if (File.Exists("gta_sa.exe"))
         {
            Logger.Print("... version is GTA San Andreas");
            return GtaVersion.SanAndreas;
         }
         else
         {
            Utils.TerminateWithError("Unknown or unsopported version of GTA.");
         }
         return GtaVersion.Unknown;
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
