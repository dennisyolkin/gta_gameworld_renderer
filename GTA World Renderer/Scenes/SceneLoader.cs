using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTAWorldRenderer.Logging;
using System.IO;

namespace GTAWorldRenderer.Scenes
{

   class SceneLoader
   {
      class LoadingException : ApplicationException
      {
         public LoadingException(string msg) : base(msg)
         {
         }
      }

      enum GtaVersion
      {
         III, ViceCity, SanAndreas
      }


      enum DataFileType
      {
         DAT, IPL, IDE,
      }


      private Log Logger = Log.Instance;
      private GtaVersion gtaVersion;


      public void LoadScene()
      {
         using (Logger.EnterStage("Loading scene"))
         {
            try
            {
               Logger.Print("Switching working directory to GTA folder");
               System.Environment.CurrentDirectory = Config.Instance.GTAFolderPath;

               DetermineGtaVersion();
               LoadDataFile("data/default.dat", DataFileType.DAT);
               LoadDataFile(GetVersionSpecificDataFile(), DataFileType.DAT);

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
            throw new LoadingException("Unknown or unsopported version of GTA");
         }
      }


      private string GetVersionSpecificDataFile()
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
               string msg = "Unsopported GTA version: " + gtaVersion.ToString();
               Logger.Print(msg, MessageType.Error);
               throw new LoadingException(msg);
         }
      }


      private void LoadDataFile(string path, DataFileType type)
      {
         using (Logger.EnterStage("Reading file: " + path))
         {
            using (StreamReader fin = new StreamReader(path))
            {
               string line;
               while ((line = fin.ReadLine()) != null)
               {
                  line = line.Trim();
                  if (line.Length == 0 || line.StartsWith("#"))
                     continue;
                  if (type == DataFileType.DAT)
                     processDatLine(line);
                  else if (type == DataFileType.IPL)
                     processIplLine(line);
                  else if (type == DataFileType.IDE)
                     processIdeLine(line);
                  else
                     Logger.Print("Unsupported data file type: " + type.ToString(), MessageType.Warning);
               }
            }
         }
      }


      void processDatLine(string line)
      {
         if (line.StartsWith("TEXDICTION"))
         {
            // Device->getFileSystem()->registerFileArchive(inStr.subString(11, inStr.size() - 11), true, false); // " 11 = "TEXDICTION "
            Logger.Print("TEXDICTION: not implemented yet", MessageType.Warning);
         }
         else if (line.StartsWith("IDE"))
         {
            LoadDataFile(line.Substring(4), DataFileType.IDE);
         } else if (line.StartsWith("IPL"))
         {
            LoadDataFile(line.Substring(4), DataFileType.IPL);
         }
         else if (line.StartsWith("SPLASH") || line.StartsWith("COLFILE") || line.StartsWith("MAPZONE") || line.StartsWith("MODELFILE"))
         {
            // Ignoring this commands
         } else
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


      void processIplLine(string line)
      {
         Logger.Print("processIplLine: not implemented yet!", MessageType.Warning);
      }


      void processIdeLine(string line)
      {
         Logger.Print("processIdeLine: not implemented yet!", MessageType.Warning);
      }


   }
}
