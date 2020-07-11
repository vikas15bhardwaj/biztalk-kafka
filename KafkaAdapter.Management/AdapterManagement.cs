
using System;
using System.IO;
using System.Xml;
using Microsoft.BizTalk.Adapter.Framework;
using AdapterFramework;
using System.Windows.Forms;

namespace KafkaAdapter.Management
{
	/// <summary>
	/// Summary description for AdapterManagement.
	/// </summary>
	public class AdapterManagement :
		AdapterManagementBase,
		IAdapterConfig,
		IAdapterConfigValidation 
	{
		public AdapterManagement()
		{
		}

		//  IAdapterConfig
		public string GetConfigSchema (ConfigType type)
		{
			switch (type)
			{
				case ConfigType.TransmitHandler:
                    return GetSchemaFromResource("KafkaAdapter.Management.TransmitHandler.xsd");

				case ConfigType.TransmitLocation:
                    return GetSchemaFromResource("KafkaAdapter.Management.TransmitLocation.xsd");

				case ConfigType.ReceiveHandler:
                    return GetSchemaFromResource("KafkaAdapter.Management.ReceiveHandler.xsd");
				
				case ConfigType.ReceiveLocation:
                    return GetSchemaFromResource("KafkaAdapter.Management.ReceiveLocation.xsd");
			}

			return String.Empty;
		}

		// used to get the WSDL file name for the selected WSDL.
		public string [] GetServiceDescription(string [] wsdls)
		{
			string []result = null;
			return result;
		}

		//used to acquire externally referenced xsd's
		public Result GetSchema( string xsdLocation, string xsdNamespace, out string xsdFileName)
		{
			xsdFileName = string.Empty;
			return Result.Continue;
		}

		public string ValidateConfiguration(ConfigType type, string config)
		{
			try
			{
				switch (type)
				{
					case ConfigType.TransmitHandler:
						return config;

					case ConfigType.TransmitLocation:
						return ValidateTransmitLocation(config);
					
					case ConfigType.ReceiveHandler:
						return config;
					
					case ConfigType.ReceiveLocation:
						return ValidateLocation(config);
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
			return String.Empty;
		}
        private string ValidateLocation(string config)
        {
            XmlDocument configDOM = new XmlDocument();
            configDOM.LoadXml(config);
            XmlNode uri = configDOM.SelectSingleNode("Config/uri");
            uri.InnerText = AddUriNodeForReceiveLocation(configDOM);

            return configDOM.OuterXml;
        }
        private string ValidateTransmitLocation (string config)
		{
			XmlDocument configDOM = new XmlDocument();
			configDOM.LoadXml(config);
            XmlNode uri = configDOM.SelectSingleNode("Config/uri");
            uri.InnerText = AddUriNode(configDOM);

            return configDOM.OuterXml;
		}

		private string AddUriNode(XmlDocument configDOM)
		{
            // get the fields from the configuration XML
			XmlNode connection = configDOM.SelectSingleNode("Config/connection");
            XmlNode topic = configDOM.SelectSingleNode("Config/topic");
            if ((connection == null || connection.InnerText == String.Empty)
                || (topic == null || topic.InnerText == String.Empty))
			{
                throw new Exception("One of more required property is not specified, check Broker Server, Topic");
			}

            return $"kafka://{connection.InnerText}/{topic.InnerText}";

		}

        private string AddUriNodeForReceiveLocation(XmlDocument configDOM)
        {
            // get the fields from the configuration XML
            XmlNode connection = configDOM.SelectSingleNode("Config/connection");
            XmlNode topic = configDOM.SelectSingleNode("Config/topic");
            XmlNode groupId = configDOM.SelectSingleNode("Config/groupId");
            XmlNode partitionKey = configDOM.SelectSingleNode("Config/partitionKey");
            XmlNode partitionId = configDOM.SelectSingleNode("Config/partitionId");
            XmlNode offset = configDOM.SelectSingleNode("Config/offset");

            if ((connection == null || connection.InnerText == String.Empty)
                || (topic == null || topic.InnerText == String.Empty)
                || (groupId == null || groupId.InnerText == String.Empty)
                || !ValidatePartitionOffset(partitionId, partitionKey, offset))
            {
                throw new Exception("One of more required property is not specified, check Broker Server, Topic, GroupId Or (partition Ids/parition Keys and offset) combination is not valid. See properties documentation for details.");
            }

            return $"kafka://{connection.InnerText}/{topic.InnerText}/{groupId.InnerText}";

        }

        private bool ValidatePartitionOffset(XmlNode partitionId, XmlNode partitionKey, XmlNode offset)
        {
            bool validCount = true;
            bool validOffset = true;
            bool validPartition = true;

            int partitionCount = 0;
            if (partitionId != null && !String.IsNullOrEmpty(partitionId.InnerText))
            {
                var partitions = partitionId.InnerText.Split(',');
                partitionCount = partitions.Length;
                validPartition = ValidatePartition(partitions);
            }
            else if (partitionKey != null && !String.IsNullOrEmpty(partitionKey.InnerText))
            {
                partitionCount = partitionKey.InnerText.Split(',').Length;
            }

            if (offset != null && !String.IsNullOrEmpty(offset.InnerText))
            {
                var offsets = offset.InnerText.Split(',');
                validOffset = ValidateOffset(offsets);
                var offsetCount = offsets.Length;
                validCount =  offsetCount == partitionCount;
            }

            return validOffset && validPartition && validCount;
        }

        private bool ValidatePartition(string[] array)
        {
            foreach (var value in array)
            {
                int parsedValue;
                if (Int32.TryParse(value, out parsedValue))
                    continue;
                else
                {
                    return false;
                }
            }

            return true;
        }
        private bool ValidateOffset(string[] array)
        {
            foreach (var value in array)
            {
                long parsedValue;
                if (Int64.TryParse(value, out parsedValue))
                    continue;
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
