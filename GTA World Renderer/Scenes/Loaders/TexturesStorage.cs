using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using GTAWorldRenderer.Logging;

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
         public FileProxy FileProxy { get; private set; }
         public Texture2D Texture{ get; set; }

         public TextureEntry(FileProxy fileProxy)
         {
            this.FileProxy = fileProxy;
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
         AddTexturesArchive(new FileProxy(path));
      }

      /// <summary>
      /// Подгружает .txd-архив - источник текстур их массива байт. 
      /// </summary>
      public void AddTexturesArchive(FileProxy fileProxy)
      {
         loadedArchives.Add(Path.GetFileNameWithoutExtension(fileProxy.Name).ToLower());
         TXDArchive archive = new TXDArchive(fileProxy);
         foreach (var item in archive.Load())
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
            if (Config.Instance.Loading.ShowWarningsIfTextureNotFound)
               Log.Instance.Print(String.Format("Texture file {0} does not exists", fullPath), MessageType.Warning);
            ++MissedTextures;
            return null;
         }

         TextureEntry textureEntry = textures[fullPath];

         if (textureEntry.Texture != null)
            return textureEntry.Texture;

         Texture2D texture = new GTATextureLoader(textureEntry.FileProxy.GetData()).Load();
         textureEntry.Texture = texture;
         return texture;

      }
   }

}
