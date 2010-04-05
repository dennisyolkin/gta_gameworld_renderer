using System;
using GTAWorldRenderer.Logging;
using System.IO;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Serialization;

namespace GTAWorldRenderer
{
   /// <summary>
   /// Предоставляет параметры приложения, считанные из xml-файлов конфигурации
   /// (основной - config.xml)
   /// </summary>
   public class Config
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

      private const string ConfigFilePath = "config.xml";
      private const string SchemaFilePath = "config.xsd";

      private static ConfigData configData;
      private static Log Logger = Log.Instance;

      public static ConfigData Instance
      {
         get 
         {
            if (configData == null)
            {
               using (Logger.EnterStage("Reading config file"))
               {
                  Logger.Print("Working directory: " + System.Environment.CurrentDirectory);
                  using (Logger.EnterStage("Parsing " + ConfigFilePath + " file..."))
                  {
                     if (!File.Exists(ConfigFilePath))
                        TerminateWithError("File not found: " + ConfigFilePath);
                     if (!File.Exists(SchemaFilePath))
                        TerminateWithError("File not found: " + SchemaFilePath);

                     Logger.Print("Validating file...");
                     try
                     {
                        ValidateXml(ConfigFilePath, SchemaFilePath);
                     } catch (Exception er)
                     {
                        TerminateWithError(String.Format("Content of {0} is not valid! Validation exception details: {1}" ,ConfigFilePath, er.Message), er);
                     }

                     ReadConfig();
                  }
               }
               Logger.Print("Application configuration was successfully loaded");
            }
            return configData;
         }
      }

      // TODO :: сеттеры в *Params сейчас публичные, и это плохо.
      // нужно сделать так, чтобы записывать в них можно было только из методов класса Config


      public struct LoadingParams
      {
         public bool ShowWarningsIfTextureNotFound { set; get; }
         public bool LowDetailedScene { set; get; }
         public bool DetailedLogOutput { set; get; }
         public int SceneObjectsAmountLimit { set; get; }
      }

      public struct RenderingParams
      {
         public bool FullScreen { set; get; }
         public float NearClippingDistance { set; get; }
         public float FarClippingDistance { set; get; }
      }

      [XmlRoot(Namespace = "gta-gameworld-renderer")]
      public class ConfigData
      {
         public string GTAFolderPath { set; get; }
         public LoadingParams Loading;
         public RenderingParams Rendering;
      }


      /// <summary>
      /// Считывает содержимое конфигурационного файла. Предполагается, что файл точно существует и корректен.
      /// </summary>
      private static void ReadConfig()
      {
         configData = new ConfigData();
         XmlSerializer dsr = new XmlSerializer(configData.GetType());
         configData = (ConfigData)dsr.Deserialize(new StreamReader(ConfigFilePath));

         if (!configData.GTAFolderPath.EndsWith("/") && !configData.GTAFolderPath.EndsWith("\\"))
            configData.GTAFolderPath += "\\";
         Logger.Print("GTA Folder: " + configData.GTAFolderPath);

         if (configData.Loading.SceneObjectsAmountLimit == -1)
            configData.Loading.SceneObjectsAmountLimit = int.MaxValue;

      }


      private static void TerminateWithError(string msg, Exception innerException)
      {
         Log.Instance.Print(msg, MessageType.Error);
         throw new ConfigInitializationFailedException(msg, innerException);
      }


      private static void TerminateWithError(string error_msg)
      {
         TerminateWithError(error_msg, null);
      }


      /// <summary>
      /// Проверяет соответствие XML файла схеме.
      /// Кидает исключение типа XmlSchemaCalidationFailed, если XML файл не соответствует схеме
      /// </summary>
      private static void ValidateXml(string xml_path, string schema_path)
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
