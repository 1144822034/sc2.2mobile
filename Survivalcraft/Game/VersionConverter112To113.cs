using Engine;
using System.IO;
using System.Xml.Linq;
using XmlUtilities;

namespace Game
{
	public class VersionConverter112To113 : VersionConverter
	{
		public override string SourceVersion => "1.12";

		public override string TargetVersion => "1.13";

		public override void ConvertProjectXml(XElement projectNode)
		{
			XmlUtils.SetAttributeValue(projectNode, "Version", TargetVersion);
			ProcessNode(projectNode);
		}

		public override void ConvertWorld(string directoryName)
		{
			string path = Storage.CombinePaths(directoryName, "Project.xml");
			XElement xElement;
			using (Stream stream = Storage.OpenFile(path, OpenFileMode.Read))
			{
				xElement = XmlUtils.LoadXmlFromStream(stream, null, throwOnError: true);
			}
			ConvertProjectXml(xElement);
			using (Stream stream2 = Storage.OpenFile(path, OpenFileMode.Create))
			{
				XmlUtils.SaveXmlToStream(xElement, stream2, null, throwOnError: true);
			}
		}

		public void ProcessNode(XElement node)
		{
			foreach (XAttribute item in node.Attributes())
			{
				ProcessAttribute(item);
			}
			foreach (XElement item2 in node.Elements())
			{
				ProcessNode(item2);
			}
		}

		public void ProcessAttribute(XAttribute attribute)
		{
			if (attribute.Name == "Value" && attribute.Value == "Dangerous")
			{
				attribute.Value = "Normal";
			}
		}
	}
}
