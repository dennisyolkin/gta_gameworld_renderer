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
      class LoadersTests
      {

         /// <summary>
         /// Распаковывает TXD архив.
         /// В случае ошибки НЕ кидает дальше исключение, выводит ErrorMessage в лог.
         /// </summary>
         /// <param name="txdPath">TXD архив, который нужно распаковать</param>
         /// <param name="outputPathPrefix">Папка, в которую проихводится распаковка. Должна существовать!</param>
         public static void UnpackTxd(string txdPath, string outputPathPrefix)
         {
            try
            {
               if (!outputPathPrefix.EndsWith(Path.DirectorySeparatorChar.ToString()))
                  outputPathPrefix += Path.DirectorySeparatorChar;

               IEnumerable<ArchiveEntry> entries; ;
               TXDArchive archive = new TXDArchive(txdPath);
               entries = archive.Load();

               using (BinaryReader reader = new BinaryReader(new FileStream(txdPath, FileMode.Open)))
               {
                  foreach (var entry in entries)
                  {
                     reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                     byte[] data = reader.ReadBytes(entry.Size);

                     if (entry.Name.Contains('/')) // имя текстуры в TXD может иметь вид <имя TXD-файла>/<имя текстуры>.gtatexture
                     {
                        string dir = entry.Name.Substring(0, entry.Name.LastIndexOf('/'));
                        if (!Directory.Exists(outputPathPrefix + dir))
                           Directory.CreateDirectory(outputPathPrefix + dir);
                     }
                     string path = outputPathPrefix + entry.Name;
                     while (File.Exists(path))
                     {
                        int sep = path.LastIndexOf('.');
                        path = path.Substring(0, sep) + "_" + path.Substring(sep);
                     }

                     using (FileStream fout = new FileStream(path, FileMode.CreateNew))
                        fout.Write(data, 0, data.Length);
                  }
               }
            } catch (Exception er)
            {
               Log.Instance.Print("Failed to unpack TXD. Exception occured: " + er.ToString());
            }
         }

         /// <summary>
         /// Распаковывает IMG архив.
         /// В случае ошибки НЕ кидает дальше исключение, выводит ErrorMessage в лог.
         /// 
         /// Все распакованные TXD файлы сначала копируются в outputPathPrefix/___txds/,
         /// потом распаковываются в outputPathPrefix
         /// </summary>
         /// <param name="imgPath">Путь к IMG архиву</param>
         /// <param name="outputPathPrefix">Папка, в которую проихводится распаковка. Должна существовать!</param>
         public static void UnpackImg(string imgPath, string outputPathPrefix)
         {
            try
            {
            if (!outputPathPrefix.EndsWith(Path.DirectorySeparatorChar.ToString()))
               outputPathPrefix += Path.DirectorySeparatorChar;

            if (!Directory.Exists(outputPathPrefix + @"\___txds\\"))
               Directory.CreateDirectory(outputPathPrefix + @"\___txds\\");

            using (Log.Instance.EnterStage("Unpacking IMG: " + imgPath))
            {
               IMGArchive archive = new IMGArchive(imgPath, GtaVersion.III);
               IEnumerable<ArchiveEntry> entries = archive.Load();

               using (BinaryReader reader = new BinaryReader(new FileStream(imgPath, FileMode.Open)))
               {
                  foreach (var entry in entries)
                  {
                     reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                     byte[] data = reader.ReadBytes(entry.Size);
                     if (entry.Name.EndsWith(".txd"))
                     {
                        string path = outputPathPrefix + @"\___txds\\" + entry.Name;
                        using (FileStream fout = new FileStream(path, FileMode.Create))
                           fout.Write(data, 0, data.Length);
                        UnpackTxd(path, outputPathPrefix);
                     }
                     else
                     {
                        string path = outputPathPrefix + entry.Name;
                        using (FileStream fout = new FileStream(path, FileMode.Create))
                           fout.Write(data, 0, data.Length);
                     }
                  }
               }
            }
            }
            catch (Exception er)
            {
               Log.Instance.Print("Failed to unpack IMG. Exception occured: " + er.ToString());
            }
         }


         /// <summary>
         /// Распаковывает все найденные TXD и IMG архивы, найденные в directoryPath (поиск рекурсивный).
         /// </summary>
         /// <param name="directoryPath">Директория с исходными архивами (к примеру, папка с игрой GTA)</param>
         /// <param name="outptuPathPrefix">Папка, в которую проихводится распаковка. Должна существовать!</param>
         public static void UnpackAllArchivesInDirectory(string directoryPath, string outptuPathPrefix)
         {
            foreach (var path in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
               if (path.EndsWith(".img"))
                  UnpackImg(path, outptuPathPrefix);
               else if (path.EndsWith(".txd"))
                  UnpackTxd(path, outptuPathPrefix);
            }
         }



      }

   }
}
