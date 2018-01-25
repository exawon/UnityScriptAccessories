using UnityEngine;
using System.Xml;

public static class XmlExtension
{
	public static void AppendChildWithInnerText(this XmlElement xmlElement, string childName, object innerText)
	{
		var field = xmlElement.OwnerDocument.CreateElement(childName);
		if (innerText == null)
		{
			field.InnerText = "null";
		}
		else if (innerText is Object)
		{
			field.InnerText = (innerText as Object).name;
		}
		else
		{
			field.InnerText = innerText.ToString();
		}
		xmlElement.AppendChild(field);
	}
}