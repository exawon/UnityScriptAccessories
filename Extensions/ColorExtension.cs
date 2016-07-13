using UnityEngine;

public static class ColorExtension
{
	public static Color R(this Color c, float r)
	{
		c.r = r;
		return c;
	}

	public static Color G(this Color c, float g)
	{
		c.g = g;
		return c;
	}

	public static Color B(this Color c, float b)
	{
		c.b = b;
		return c;
	}

	public static Color A(this Color c, float a)
	{
		c.a = a;
		return c;
	}
}
