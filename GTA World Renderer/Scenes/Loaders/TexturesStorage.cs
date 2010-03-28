using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using GTAWorldRenderer.Logging;

namespace GTAWorldRenderer.Scenes
{
   namespace Loaders
   {

      /// <summary>
      /// Хранит загруженные текстуры.
      /// Загружает текстуры "лениво", т.е. только по первому требованию.
      /// Загруженные текстуры всегда остаются в памяти.
      /// Реализует паттерн Синглтон.
      /// </summary>
      class TexturesStorage
      {
         private static TexturesStorage me = new TexturesStorage();
         private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

         /// <summary>
         /// Количество текстур, которые ,были запрошены, но не были найдены
         /// </summary>
         public int MissedTextures{ get; private set; }

         public static TexturesStorage Instance
         {
            get{ return me; }
         }


         public Texture2D GetTexture(string textureName, string textureFolder)
         {
            // TODO :: temporary code with absolute paths!!!
            string fullPath = String.Format(@"c:\home\tmp\root\{0}\{1}.gtatexture", textureFolder, textureName);

            if (textures.ContainsKey(fullPath))
               return textures[fullPath];

            if (!File.Exists(fullPath))
            {
               /*
                * Судя по всему, это нормальная ситуация, когда некоторых текстур не существует.
                */
               if (Config.Instance.ShowWarningsIfTextureNotFound)
                  Log.Instance.Print(String.Format("Texture file {0} does not exists", fullPath), MessageType.Warning);
               ++MissedTextures;
               return null;
            }

            Texture2D texture = new GTATextureLoader(new BinaryReader(new FileStream(fullPath, FileMode.Open))).Load();
            textures[fullPath] = texture;
            return texture;
         }
      }


   }
}
