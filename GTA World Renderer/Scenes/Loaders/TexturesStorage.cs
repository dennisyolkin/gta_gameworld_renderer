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

         /*
          * TODO ::
          * Из-за того, что в некоторых TXD архивах некорректно установлены размеры секций, в ViceCity
          * падает загрузка некоторых архивов. Это можно обойти, если при чтении TXD архива не ориентироваться
          * на размер секции, а сразу считатывать текстуру (в текстуре размер данных правильно установлен).
          * 
          * http://github.com/dennisyolkin/gta_gameworld_renderer/issues/issue/14
          */
         IEnumerable<FileProxy> archiveItems;
         try
         {
            archiveItems = archive.Load();
         } catch (Exception er)
         {
            Log.Instance.Print("Failed to load TXD archive. " + er.Message, MessageType.Error);
            return;
         }

         foreach (var item in archiveItems)
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


      /// <summary>
      /// Вычисляет количество памяти, занимаемое всеми загруженными текстурами
      /// </summary>
      /// <returns></returns>
      public int GetMemoryUsed()
      {
         int totalSize = 0;
         foreach (var item in textures)
         {
            Texture2D texture = item.Value.Texture;
            if (texture == null)
               continue;
            totalSize += texture.Height * texture.Width * 4; // считаем, что каждый пиксель в видеопамяти будет занимать 4 байта
         }
         return totalSize;
      }


   }

}
