using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using GTAWorldRenderer.Logging;
using GTAWorldRenderer.Scenes.ArchivesCommon;

namespace GTAWorldRenderer.Scenes.Loaders
{

   /// <summary>
   /// Хранит загруженные текстуры.
   /// Загружает текстуры "лениво", т.е. только по первому требованию.
   /// Загруженные текстуры всегда остаются в памяти.
   /// Реализует паттерн Синглтон.
   /// </summary>
   class TexturesStorage
   {
      class TextureEntry
      {
         public ArchiveEntry ArchiveEntry { get; private set; }
         public Texture2D Texture{ get; set; }

         public TextureEntry(ArchiveEntry archiveEntry)
         {
            this.ArchiveEntry = archiveEntry;
         }
      }

      private static TexturesStorage me = new TexturesStorage();
      private Dictionary<string, TextureEntry> textures = new Dictionary<string, TextureEntry>();
      HashSet<string> loadedArchives = new HashSet<string>();

      /// <summary>
      /// Количество текстур, которые ,были запрошены, но не были найдены
      /// </summary>
      public int MissedTextures{ get; private set; }

      public static TexturesStorage Instance
      {
         get{ return me; }
      }


      /// <summary>
      /// Подгружает .txd-архив - источник текстур
      /// </summary>
      public void AddTexturesArchive(string path)
      {
         loadedArchives.Add(Path.GetFileNameWithoutExtension(path).ToLower());
         TXDArchive archive = new TXDArchive(path);
         UpdateArchiveItems(archive.Load());
      }

      /// <summary>
      /// Подгружает .txd-архив - источник текстур их массива байт. 
      /// Непосредственное чтение из файла не производится!
      /// </summary>
      /// <param name="data">Массив байт - содержимое архива</param>
      /// <param name="txdFileName">Имя файла - TXD архива</param>
      /// <param name="sourceArchiveFilePath">Исходный файл, в котором содержится TXD архив.</param>
      /// /// <param name="offsetInFile">Смещение в исходном файле, по которому начинается TXD-архив</param>
      public void AddTexturesArchive(byte[] data, string txdFileName, string sourceArchiveFilePath, int offsetInFile)
      {
         loadedArchives.Add(Path.GetFileNameWithoutExtension(txdFileName).ToLower());
         TXDArchive archive = new TXDArchive(data, txdFileName, sourceArchiveFilePath, offsetInFile);
         UpdateArchiveItems(archive.Load());
      }


      private void UpdateArchiveItems(IEnumerable<ArchiveEntry> items)
      {
         foreach (var item in items)
            textures[item.Name.ToLower()] = new TextureEntry(item);
      }


      /// <summary>
      /// Возвращает текстуру по запрошенному именем.
      /// Если текстура уже была загружена, будет возвращена ссылка на существующую текстуру.
      /// Если нет, то текстура будет загружена и созранена в кеше.
      /// </summary>
      public Texture2D GetTexture(string textureName, string textureFolder)
      {
         textureFolder = textureFolder.ToLower();
         textureName = textureName.ToLower();

         string fullPath = String.Format(@"{0}/{1}.gtatexture", textureFolder, textureName);

         if (!loadedArchives.Contains(textureFolder))
            Utils.TerminateWithError(String.Format("Requested TXD archive {0} was not loaded to Starage. ", textureFolder));

         if (!textures.ContainsKey(fullPath))
         {
            /*
             * Судя по всему, это нормальная ситуация, когда некоторых текстур не существует.
             */
            if (Config.Instance.ShowWarningsIfTextureNotFound)
               Log.Instance.Print(String.Format("Texture file {0} does not exists", fullPath), MessageType.Warning);
            ++MissedTextures;
            return null;
         }

         TextureEntry textureEntry = textures[fullPath];

         if (textureEntry.Texture != null)
            return textureEntry.Texture;

         using (BinaryReader reader = new BinaryReader(new FileStream(textureEntry.ArchiveEntry.ArchiveFilePath, FileMode.Open)))
         {
            reader.BaseStream.Seek(textureEntry.ArchiveEntry.Offset, SeekOrigin.Begin);
            byte[] data = reader.ReadBytes(textureEntry.ArchiveEntry.Size);
            Texture2D texture = new GTATextureLoader(data).Load();
            textureEntry.Texture = texture;
            return texture;
         }

      }
   }

}
