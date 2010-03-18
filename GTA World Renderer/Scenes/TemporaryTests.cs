using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTAWorldRenderer.Logging;
using GTAWorldRenderer.Scenes.ArchivesCommon;
using System.IO;

namespace GTAWorldRenderer.Scenes
{
   partial class SceneLoader
   {
      /// <summary>
      /// Различные тесты.
      /// Нужны только временно, на этапе отладки.
      /// Потом либо будут приведены в порядок, либо нафиг удалены
      /// </summary>
      class TemporaryTests
      {

         public static void TestUnpackTxd(string txdPath) // TODO :: remove
         {
            IEnumerable<ArchiveEntry> entries; ;
            TXDArchive archive = new TXDArchive(txdPath);
            entries = archive.Load();

            using (BinaryReader reader = new BinaryReader(new FileStream(txdPath, FileMode.Open)))
            {
               foreach (var entry in entries)
               {
                  reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                  byte[] data = reader.ReadBytes(entry.Size);

                  const string prefix = @"c:\home\tmp\root\";
                  if (entry.Name.Contains('/'))
                  {
                     string dir = entry.Name.Substring(0, entry.Name.IndexOf('/'));
                     if (!Directory.Exists(prefix + dir))
                        Directory.CreateDirectory(prefix + dir);
                  }
                  string path = prefix + entry.Name;
                  while (File.Exists(path))
                     path += "_";

                  using (FileStream fout = new FileStream(path, FileMode.CreateNew))
                     fout.Write(data, 0, data.Length);
               }
            }
         }


         public static void TestUnpackImg(string img)
         {
            using (Log.Instance.EnterStage("Unpacking IMG: " + img))
            {
               IMGArchive archive = new IMGArchive(img, GtaVersion.III);
               IEnumerable<ArchiveEntry> entries = archive.Load();

               const string prefix = @"c:\home\tmp\root\";

               using (BinaryReader reader = new BinaryReader(new FileStream(img, FileMode.Open)))
               {
                  foreach (var entry in entries)
                  {
                     reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                     byte[] data = reader.ReadBytes(entry.Size);
                     if (entry.Name.EndsWith(".txd"))
                     {
                        string path = prefix + @"\___txds\\" + entry.Name;
                        using (FileStream fout = new FileStream(path, FileMode.Create))
                           fout.Write(data, 0, data.Length);
                        TestUnpackTxd(path);
                     }
                     else
                     {
                        string path = prefix + entry.Name;
                        using (FileStream fout = new FileStream(path, FileMode.Create))
                           fout.Write(data, 0, data.Length);
                     }
                  }
               }
            }
         }


         public static void TestAllArchiveUnpacking() // TODO :: remove
         {
            string[] txdArchives = { 
                                      @"c:\Program Files\GTAIII\models\fonts.txd",
                                       @"c:\Program Files\GTAIII\models\frontend.txd",
                                       @"c:\Program Files\GTAIII\models\generic.txd",
                                       @"c:\Program Files\GTAIII\models\hud.txd",
                                       @"c:\Program Files\GTAIII\models\menu.txd",
                                       @"c:\Program Files\GTAIII\models\MISC.TXD",
                                       @"c:\Program Files\GTAIII\models\particle.txd",
                                       @"c:\Program Files\GTAIII\txd\LOADSC0.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC1.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC2.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC3.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC4.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC5.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC6.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC7.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC8.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC9.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC10.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC11.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC12.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC13.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC14.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC15.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC16.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC17.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC18.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC19.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC20.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC21.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC22.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC23.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC24.TXD",
                                       @"c:\Program Files\GTAIII\txd\LOADSC25.TXD",
                                       @"c:\Program Files\GTAIII\txd\mainsc1.txd",
                                       @"c:\Program Files\GTAIII\txd\mainsc2.txd",
                                       @"c:\Program Files\GTAIII\txd\NEWS.TXD",
                                       @"c:\Program Files\GTAIII\txd\SPLASH1.TXD",
                                       @"c:\Program Files\GTAIII\txd\SPLASH2.TXD",
                                       @"c:\Program Files\GTAIII\txd\SPLASH3.TXD",
                                      };

            string[] imgArchives = {
                                 @"c:\Program Files\GTAIII\anim\cuts.img",
                                 @"c:\Program Files\GTAIII\models\gta3.img",
                                 @"c:\Program Files\GTAIII\models\txd.img"
                              };

            using (Log.Instance.EnterStage("Unpacking all data"))
            {
               foreach (var path in txdArchives)
                  TestUnpackTxd(path);

               foreach (var path in imgArchives)
                  TestUnpackImg(path);
            }
         }

      }

   }
}
