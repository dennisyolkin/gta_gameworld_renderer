using System;
using GTAWorldRenderer.Logging;
using System.IO;
using System.Xml.Schema;
using System.Xml;

namespace GTAWorldRenderer
{
   /// <summary>
   /// Предоставляет параметры приложения, считанные из xml-файлов конфигурации
   /// (основной - config.xml)
   /// </summary>
   class Config
   {
      public class ConfigInitializationFailedException : ApplicationException
      {
         public ConfigInitializationFailedException(string msg)
            : base(msg)
         {
         }

         public ConfigInitializationFailedException(string msg, Exception innerException)
            : base(msg, innerException)
         {
         }
      }

      private static Config instance = new Config();
      private Log Logger = Log.Instance;

      public static Config Instance
      {
         get { return instance; }
      }

      // TODO :: сеттеры в *Params сейчас публичные, и это плохо.
      // нужно сделать так, чтобы записывать в них можно было только из методов класса Config

      public struct GlobalParams
      {
         public string GTAFolderPath { set; get; }
      }

      public struct LoadingParams
      {
         public bool ShowWarningsIfTextureNotFound { set; get; }
         public bool LowDetailedScene { set; get; }
         public bool DetailedLogOutput { set; get; }
         public int SceneObjectsAmountLimit { set; get; }
      }

      public struct RenderingParams
      {
         public bool FullScreenMode { set; get; }
         public float NearClippingDistance { set; get; }
         public float FarClippingDistance { set; get; }
      }


      public GlobalParams Global;
      public LoadingParams Loading;
      public RenderingParams Rendering;

      public Config()
      {
         using (Logger.EnterStage("Reading config file"))
         {
            Logger.Print("Working directory: " + System.Environment.CurrentDirectory);
            using (Logger.EnterStage("Parsing config.xml file..."))
            {
               if (!File.Exists("config.xml"))
                  TerminateWithError("File not found: config.xml");
               if (!File.Exists("config.xsd"))
                  TerminateWithError("File not found: config.xsd");

               Logger.Print("Validating file...");
               try
               {
                  ValidateXml("config.xml", "config.xsd");
               } catch (Exception er)
               {
                  TerminateWithError("Content of config.xml is not valid! Validation exception details: " + er.Message, er);
               }

               ReadConfig();
            }
         }
         Logger.Print("Application configuration was successfully loaded");
      }

      /// <summary>
      /// Считывает содержимое конфигурационного файла. Предполагается, что файл точно существует и корректен.
      /// </summary>
      private void ReadConfig()
      {
         XmlDocument doc = new XmlDocument();
         doc.Load("config.xml");
         XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
         nsmgr.AddNamespace("ns", "gta-gameworld-renderer");

         Global.GTAFolderPath = doc.SelectSingleNode("/ns:GlobalConfig/ns:GtaFolder", nsmgr).InnerText;
         if (!Global.GTAFolderPath.EndsWith("/") && !Global.GTAFolderPath.EndsWith("\\"))
            Global.GTAFolderPath += "\\";
         Logger.Print("GTA Folder: " + Global.GTAFolderPath);

         Loading.ShowWarningsIfTextureNotFound = Boolean.Parse(
            doc.SelectSingleNode("/ns:GlobalConfig/ns:LoadingParams/ns:ShowWarningsIfTextureNotFound", nsmgr).InnerText);

         Loading.SceneObjectsAmountLimit = int.Parse(doc.SelectSingleNode("/ns:GlobalConfig/ns:LoadingParams/ns:SceneObjectsAmountLimit", nsmgr).InnerText);
         if (Loading.SceneObjectsAmountLimit == -1)
            Loading.SceneObjectsAmountLimit = int.MaxValue;

         Loading.LowDetailedScene = Boolean.Parse(
            doc.SelectSingleNode("/ns:GlobalConfig/ns:LoadingParams/ns:LowDetailedScene", nsmgr).InnerText);

         Loading.DetailedLogOutput = Boolean.Parse(
            doc.SelectSingleNode("/ns:GlobalConfig/ns:LoadingParams/ns:DetailedLogOutput", nsmgr).InnerText);

         Rendering.FullScreenMode = Boolean.Parse(
            doc.SelectSingleNode("/ns:GlobalConfig/ns:RenderingParams/ns:FullScreen", nsmgr).InnerText);

         Rendering.NearClippingDistance = float.Parse(doc.SelectSingleNode("/ns:GlobalConfig/ns:RenderingParams/ns:NearClippingDist", nsmgr).InnerText);
         Rendering.FarClippingDistance = float.Parse(doc.SelectSingleNode("/ns:GlobalConfig/ns:RenderingParams/ns:FarClippingDist", nsmgr).InnerText);
      }


      void TerminateWithError(string msg, Exception innerException)
      {
         Log.Instance.Print(msg, MessageType.Error);
         throw new ConfigInitializationFailedException(msg, innerException);
      }


      void TerminateWithError(string error_msg)
      {
         TerminateWithError(error_msg, null);
      }


      /// <summary>
      /// Проверяет соответствие XML файла схеме.
      /// Кидает исключение типа XmlSchemaCalidationFailed, если XML файл не соответствует схеме
      /// </summary>
      private void ValidateXml(string xml_path, string schema_path)
      {
         XmlSchemaSet sc = new XmlSchemaSet();
         sc.Add("gta-gameworld-renderer", schema_path);
         XmlReaderSettings settings = new XmlReaderSettings();
         settings.ValidationType = ValidationType.Schema;
         settings.Schemas = sc;
         XmlReader reader = XmlReader.Create(xml_path, settings);
         while (reader.Read()) ;
      }

   }
}
