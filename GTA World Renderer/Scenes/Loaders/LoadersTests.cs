using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTAWorldRenderer.Logging;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace GTAWorldRenderer.Scenes.Loaders
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

            var archive = new TXDArchive(txdPath);
            var textures = archive.Load();

            foreach (var entry in textures)
            {
               if (entry.Key.Contains('/')) // имя текстуры в TXD может иметь вид <имя TXD-файла>/<имя текстуры>.gtatexture
               {
                  string dir = entry.Key.Substring(0, entry.Key.LastIndexOf('/'));
                  if (!Directory.Exists(outputPathPrefix + dir))
                     Directory.CreateDirectory(outputPathPrefix + dir);
               }
               string path = outputPathPrefix + entry.Key;
               while (File.Exists(path))
               {
                  int sep = path.LastIndexOf('.');
                  path = path.Substring(0, sep) + "_" + path.Substring(sep);
               }

               entry.Value.Save(path, ImageFileFormat.Png);
            }

         }
         catch (Exception er)
         {
            Log.Instance.Print("Failed to unpack TXD. Exception occured: " + er.Message, MessageType.Error);
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
      /// <param name="gtaVersion">Версия игры</param>
      public static void UnpackImg(string imgPath, string outputPathPrefix, GtaVersion gtaVersion)
      {
         try
         {
         if (!outputPathPrefix.EndsWith(Path.DirectorySeparatorChar.ToString()))
            outputPathPrefix += Path.DirectorySeparatorChar;

         if (!Directory.Exists(outputPathPrefix + @"\___txds\\"))
            Directory.CreateDirectory(outputPathPrefix + @"\___txds\\");

         using (Log.Instance.EnterStage("Unpacking IMG: " + imgPath))
         {
            IMGArchive archive = new IMGArchive(imgPath, gtaVersion);
            IEnumerable<FileProxy> entries = archive.Load();
            foreach (var entry in entries)
            {
               byte[] data = entry.GetData();
               if (entry.Name.ToLower().EndsWith(".txd"))
               {
                  string path = outputPathPrefix + @"\___txds\\" + entry.Name;
                  if (!File.Exists(path))
                  {
                     using (FileStream fout = new FileStream(path, FileMode.Create))
                        fout.Write(data, 0, data.Length);
                     UnpackTxd(path, outputPathPrefix);
                  }
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
      /// <param name="gtaVersion">Версия игры</param>
      public static void UnpackAllArchivesInDirectory(string directoryPath, string outptuPathPrefix, GtaVersion gtaVersion)
      {
         foreach (var path in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
         {
            if (path.EndsWith(".img"))
               UnpackImg(path, outptuPathPrefix, gtaVersion);
            else if (path.EndsWith(".txd"))
               UnpackTxd(path, outptuPathPrefix);
         }
      }


      /// <summary>
      /// Распаковывает все текстуры (*.gtatexture) в directoryPath (и подкаталогах).
      /// Распакованные изображения сохраняются рядом в файл с таким же именем и одним из графических расширений.
      /// </summary>
      public static void UnpackAllTextures(string directoryPath)
      {
         int success = 0, fail = 0;
         using (Log.Instance.EnterStage("Unpacking all textures in directory " + directoryPath))
         {
            foreach (var file in Directory.GetFiles(directoryPath, "*.gtatexture", SearchOption.AllDirectories))
            {
               try
               {
                  using (Log.Instance.EnterStage("Unpacking texture: " + file.Substring(directoryPath.Length)))
                  {
                     GTATextureLoader textureLoader = new GTATextureLoader(new BinaryReader(new FileStream(file, FileMode.Open)));
                     Texture2D texture = textureLoader.Load();
                     texture.Save(file.Substring(0, file.LastIndexOf('.')) + ".png", ImageFileFormat.Png);

                     ++success;
                     Log.Instance.Print("success!");
                  }
               } 
               catch (Exception er)
               {
                  Log.Instance.Print("Failed to load texture. Exception: " + er.Message, MessageType.Error);
                  ++fail;
               }
            }
         }
         Log.Instance.Print(String.Format("Finished textures processing. Successes: {0}, failes: {1}", success, fail));
      }


      /// <summary>
      /// Пробует загрузить каждую найденную .dff в указанной папке
      /// </summary>
      /// <param name="directoryPath">Директория, в которой будет производиться рекурсивный поиск моделей .dff</param>
      public static void LoadAllModels(string directoryPath)
      {
         int success = 0, fail = 0;
         using (Log.Instance.EnterStage("Loading all .DFF models in directory: " + directoryPath))
         {
            foreach (var file in Directory.GetFiles(directoryPath, "*.dff", SearchOption.AllDirectories))
            {
               try
               {
                  using (Log.Instance.EnterStage("Processing file: " + file.Substring(directoryPath.Length)))
                  {
                     DffLoader dff = new DffLoader(file);
                     dff.Load();

                     ++success;
                     Log.Instance.Print("success!");
                  }
               } 
               catch (Exception er)
               {
                  Log.Instance.Print("Failed to load dff model. Exception: " + er.Message, MessageType.Error);
                  ++fail;
               }
            }
         }
         Log.Instance.Print(String.Format("Finished models processing. Successes: {0}, failes: {1}", success, fail));
      }

      //

   }

}
