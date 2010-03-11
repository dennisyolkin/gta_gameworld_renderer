using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTAWorldRenderer.Logging;
using System.IO;

namespace GTAWorldRenderer.Scenes
{

   partial class SceneLoader
   {
      class LoadingException : ApplicationException
      {
         public LoadingException(string msg) : base(msg)
         {
         }
      }

      /// <summary>
      /// Пишет в текущий Stage лога текст ошибки и кидает исключение LoadingException
      /// </summary>
      /// <param name="msg"></param>
      private static void TerminateWithError(string msg)
      {
         Log.Instance.Print(msg, MessageType.Error);
         throw new LoadingException(msg);
      }


      enum GtaVersion
      {
         III, ViceCity, SanAndreas
      }


      private Log Logger = Log.Instance;
      private GtaVersion gtaVersion;

      private Dictionary<int, SceneObjectDefinition> objDefinitions = new Dictionary<int, SceneObjectDefinition>();
      private List<SceneObject> sceneObjects= new List<SceneObject>();

      public void LoadScene()
      {
         using (Logger.EnterStage("Loading scene"))
         {
            try
            {
               Logger.Print("Switching working directory to GTA folder");
               System.Environment.CurrentDirectory = Config.Instance.GTAFolderPath;

               DetermineGtaVersion();
               LoadDatFile("data/default.dat");
               LoadDatFile(GetVersionSpecificDatFile());

            } catch (Exception)
            {
               Logger.Print("Failed to load scene", MessageType.Error);
               throw;
            }
         }
      }


      private void DetermineGtaVersion()
      {
         Logger.Print("Determining GTA version...");
         if (File.Exists("gta3.exe"))
         {
            Logger.Print("... version is GTA III");
            gtaVersion = GtaVersion.III;
         }
         else if (File.Exists("gta-vc.exe"))
         {
            Logger.Print("... version is GTA Vice City");
            gtaVersion = GtaVersion.ViceCity;
         }
         else if (File.Exists("gta_sa.exe"))
         {
            Logger.Print("... version is GTA San Andreas");
            gtaVersion = GtaVersion.SanAndreas;
         }
         else
         {
            Logger.Print("Can not determine game version!", MessageType.Error);
            throw new LoadingException("Unknown or unsopported version of GTA.");
         }
      }


      private string GetVersionSpecificDatFile()
      {
         switch(gtaVersion)
         {
            case GtaVersion.III:
               return "data/gta3.dat";
            case GtaVersion.ViceCity:
               return "data/gta_vc.dat";
            case GtaVersion.SanAndreas:
               return "data/gra.dat";
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
                     // Device->getFileSystem()->registerFileArchive(inStr.subString(11, inStr.size() - 11), true, false); // " 11 = "TEXDICTION "
                     Logger.Print("TEXDICTION: not implemented yet", MessageType.Warning);
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
                        sceneObjects.Add(obj);
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
