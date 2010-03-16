using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTAWorldRenderer.Logging;
using System.IO;
using GTAWorldRenderer.Scenes.ArchivesCommon;

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


      //private void TestUnpackTxd(string txdPath) // TODO :: remove
      //{
      //   IEnumerable<ArchiveEntry> entries;;
      //   TXDArchive archive = new TXDArchive(txdPath);
      //      entries = archive.Load();
         
      //   using (BinaryReader reader = new BinaryReader(new FileStream(txdPath, FileMode.Open)))
      //   {
      //      foreach (var entry in entries)
      //      {
      //         reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
      //         byte[] data = reader.ReadBytes(entry.Size);

      //         const string prefix = @"c:\home\tmp\root\";
      //         if (entry.Name.Contains('/'))
      //         {
      //            string dir = entry.Name.Substring(0, entry.Name.IndexOf('/'));
      //            if (!Directory.Exists(prefix + dir))
      //               Directory.CreateDirectory(prefix + dir);
      //         }
      //         string path = prefix + entry.Name;
      //         while (File.Exists(path))
      //            path += "_";

      //         using (FileStream fout = new FileStream(path, FileMode.CreateNew))
      //            fout.Write(data, 0, data.Length);
      //      }
      //   }
      //}


      //void TestUnpackImg(string img)
      //{
      //   using (Log.Instance.EnterStage("Unpacking IMG: " + img))
      //   {
      //      IMGArchive archive = new IMGArchive(img, GtaVersion.III);
      //      IEnumerable<ArchiveEntry> entries = archive.Load();

      //      const string prefix = @"c:\home\tmp\root\";

      //      using (BinaryReader reader = new BinaryReader(new FileStream(img, FileMode.Open)))
      //      {
      //         foreach (var entry in entries)
      //         {
      //            reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
      //            byte[] data = reader.ReadBytes(entry.Size);
      //            if (entry.Name.EndsWith(".txd"))
      //            {
      //               string path = prefix + @"\___txds\\" + entry.Name;
      //               using (FileStream fout = new FileStream(path, FileMode.Create))
      //                  fout.Write(data, 0, data.Length);
      //               TestUnpackTxd(path);
      //            }
      //            else
      //            {
      //               string path = prefix + entry.Name;
      //               using (FileStream fout = new FileStream(path, FileMode.Create))
      //                  fout.Write(data, 0, data.Length);
      //            }
      //         }
      //      }
      //   }
      //}


      //private void TestAllArchiveUnpacking() // TODO :: remove
      //{
      //   string[] txdArchives = { 
      //                                @"c:\Program Files\GTAIII\models\fonts.txd",
      //                                 @"c:\Program Files\GTAIII\models\frontend.txd",
      //                                 @"c:\Program Files\GTAIII\models\generic.txd",
      //                                 @"c:\Program Files\GTAIII\models\hud.txd",
      //                                 @"c:\Program Files\GTAIII\models\menu.txd",
      //                                 @"c:\Program Files\GTAIII\models\MISC.TXD",
      //                                 @"c:\Program Files\GTAIII\models\particle.txd",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC0.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC1.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC2.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC3.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC4.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC5.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC6.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC7.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC8.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC9.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC10.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC11.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC12.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC13.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC14.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC15.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC16.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC17.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC18.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC19.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC20.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC21.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC22.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC23.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC24.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\LOADSC25.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\mainsc1.txd",
      //                                 @"c:\Program Files\GTAIII\txd\mainsc2.txd",
      //                                 @"c:\Program Files\GTAIII\txd\NEWS.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\SPLASH1.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\SPLASH2.TXD",
      //                                 @"c:\Program Files\GTAIII\txd\SPLASH3.TXD",
      //                                };

      //   string[] imgArchives = {
      //                           @"c:\Program Files\GTAIII\anim\cuts.img",
      //                           @"c:\Program Files\GTAIII\models\gta3.img",
      //                           @"c:\Program Files\GTAIII\models\txd.img"
      //                        };

      //   using (Log.Instance.EnterStage("Unpacking all data"))
      //   {
      //      foreach (var path in txdArchives)
      //         TestUnpackTxd(path);

      //      foreach (var path in imgArchives)
      //         TestUnpackImg(path);
      //   }
      //}


      public IEnumerable<Model3D> LoadScene()
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

               var models = new List<Model3D>();
               var dffLoader = new DffLoader(@"c:\Program Files\GTAIII\models\Generic\arrow.DFF");
               Model3D model = dffLoader.Load();
               models.Add(model);

               //TestAllArchiveUnpacking();

               return models;

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
