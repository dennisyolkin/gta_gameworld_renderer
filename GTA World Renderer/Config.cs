using System;
using GTAWorldRenderer.Logging;
using System.IO;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;

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
                  using (Logger.EnterStage("Parsing " + ConfigFilePath + " file"))
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

      [DataContract(Namespace = "gta-gameworld-renderer")]
      public class LoadingParams
      {
         private int _sceneObjectsAmountLimit;

         [DataMember(IsRequired = true, Order = 1)]
         public bool ShowWarningsIfTextureNotFound { get; private set; }

         [DataMember(IsRequired = true, Order = 2)]
         public int SceneObjectsAmountLimit
         {
            get { return _sceneObjectsAmountLimit; } 

            private set
            {
               _sceneObjectsAmountLimit = value == -1? int.MaxValue : value;
            }
         }

         [DataMember(IsRequired = true, Order = 3)]
         public bool DetailedLogOutput { get; private set; }

      }


      [DataContract(Namespace = "gta-gameworld-renderer")]
      public class RenderingParams
      {
         [DataMember(IsRequired = true, Order = 1)]
         public bool FullScreen { get; private set; }

         [DataMember(IsRequired = true, Order = 2)]
         public float NearClippingDistance { get; private set; }

         [DataMember(IsRequired = true, Order = 3)]
         public float FarClippingDistance { get; private set; }
      }


      [DataContract(Namespace = "gta-gameworld-renderer")]
      public class RasterizationParams
      {
         [DataMember(IsRequired = true, Order = 1)]
         public float GridCellSize { get; private set; }
      }


      [DataContract(Name = "ConfigData", Namespace = "gta-gameworld-renderer")]
      public class ConfigData
      {
         private string _gtaFolderPath;

         [DataMember(IsRequired = true, Order = 1)]
         public string GTAFolderPath
         {
            get { return _gtaFolderPath; }

            private set
            {
               _gtaFolderPath = value;
               if (!_gtaFolderPath.EndsWith("/") && !_gtaFolderPath.EndsWith("\\"))
                  _gtaFolderPath += "\\";
            }
         }


         [DataMember(IsRequired = true, Order = 2)]
         public LoadingParams Loading { get; private set; }

         [DataMember(IsRequired = true, Order = 3)]
         public RenderingParams Rendering { get; private set; }

         [DataMember(IsRequired = true, Order = 4)]
         public RasterizationParams Rasterization { get; private set; }
      }


      /// <summary>
      /// Считывает содержимое конфигурационного файла. Предполагается, что файл точно существует и корректен.
      /// </summary>
      private static void ReadConfig()
      {
         var dsr = new DataContractSerializer(typeof(ConfigData));
         configData = (ConfigData)dsr.ReadObject(new FileStream(ConfigFilePath, FileMode.Open));

         Logger.Print("GTA Folder: " + configData.GTAFolderPath);
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
         using(XmlReader reader = XmlReader.Create(xml_path, settings))
            while (reader.Read());
      }

   }
}
