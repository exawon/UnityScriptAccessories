public static class XmlNodeExtension
{
	public static XmlNode GetNode(this XmlDocument xml, string tag, int index = 0)
	{
		return xml.GetElementsByTagName(tag)[index];
	}

	public static string GetString(this XmlNode node, string name)
	{
		return node.Attributes.GetNamedItem(name).InnerText;
	}

	public static int GetInt(this XmlNode node, string name)
	{
		return int.Parse(GetString(node, name));
	}

	public static float GetFloat(this XmlNode node, string name)
	{
		return float.Parse(GetString(node, name));
	}
}