using Engine.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace XmlUtilities
{
	public static class XmlUtils
	{
		public static object GetAttributeValue(XElement node, string attributeName, Type type)
		{
			object attributeValue = GetAttributeValue(node, attributeName, type, null);
			if (attributeValue != null)
			{
				return attributeValue;
			}
			throw new Exception($"Required XML attribute \"{attributeName}\" not found in node \"{node.Name}\".");
		}

		public static object GetAttributeValue(XElement node, string attributeName, Type type, object defaultValue)
		{
			XAttribute xAttribute = node.Attribute(attributeName);
			if (xAttribute != null)
			{
				try
				{
					return HumanReadableConverter.ConvertFromString(type, xAttribute.Value);
				}
				catch (Exception)
				{
					return defaultValue;
				}
			}
			return defaultValue;
		}

		public static T GetAttributeValue<T>(XElement node, string attributeName)
		{
			return (T)GetAttributeValue(node, attributeName, typeof(T));
		}

		public static T GetAttributeValue<T>(XElement node, string attributeName, T defaultValue)
		{
			return (T)GetAttributeValue(node, attributeName, typeof(T), defaultValue);
		}

		public static void SetAttributeValue(XElement node, string attributeName, object value)
		{
			string value2 = HumanReadableConverter.ConvertToString(value);
			XAttribute xAttribute = node.Attribute(attributeName);
			if (xAttribute != null)
			{
				xAttribute.Value = value2;
			}
			else
			{
				node.Add(new XAttribute(attributeName, value2));
			}
		}

		public static XElement FindChildElement(XElement node, string elementName, bool throwIfNotFound)
		{
			XElement xElement = node.Elements(elementName).FirstOrDefault();
			if (xElement != null)
			{
				return xElement;
			}
			if (throwIfNotFound)
			{
				throw new Exception($"Required XML element \"{elementName}\" not found in node \"{node.Name}\".");
			}
			return null;
		}

		public static XElement AddElement(XElement parentNode, string name)
		{
			XElement xElement = new XElement(name);
			parentNode.Add(xElement);
			return xElement;
		}

		public static XElement LoadXmlFromTextReader(TextReader textReader, bool throwOnError)
		{
			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.CheckCharacters = false;
			xmlReaderSettings.IgnoreComments = true;
			xmlReaderSettings.IgnoreProcessingInstructions = true;
			using (XmlReader reader = XmlReader.Create(textReader, xmlReaderSettings))
			{
				return XElement.Load(reader, LoadOptions.None);
			}
		}

		public static XElement LoadXmlFromStream(Stream stream, Encoding encoding, bool throwOnError)
		{
			using (StreamReader textReader = (encoding != null) ? new StreamReader(stream, encoding) : new StreamReader(stream, detectEncodingFromByteOrderMarks: true))
			{
				return LoadXmlFromTextReader(textReader, throwOnError);
			}
		}

		public static XElement LoadXmlFromString(string data, bool throwOnError)
		{
			using (StringReader textReader = new StringReader(data))
			{
				return LoadXmlFromTextReader(textReader, throwOnError);
			}
		}

		public static void SaveXmlToTextWriter(XElement node, TextWriter textWriter, bool throwOnError)
		{
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.OmitXmlDeclaration = true;
			xmlWriterSettings.Indent = true;
			xmlWriterSettings.Encoding = Encoding.UTF8;
			xmlWriterSettings.CloseOutput = true;
			using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, xmlWriterSettings))
			{
				node.Save(xmlWriter);
				xmlWriter.Flush();
			}
		}

		public static void SaveXmlToStream(XElement node, Stream stream, Encoding encoding, bool throwOnError)
		{
			using (TextWriter textWriter = (encoding != null) ? new StreamWriter(stream, encoding) : new StreamWriter(stream))
			{
				SaveXmlToTextWriter(node, textWriter, throwOnError);
			}
		}

		public static string SaveXmlToString(XElement node, bool throwOnError)
		{
			using (StringWriter stringWriter = new StringWriter())
			{
				SaveXmlToTextWriter(node, stringWriter, throwOnError);
				return stringWriter.ToString();
			}
		}
	}
}
