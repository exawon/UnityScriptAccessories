using UnityEngine;
 
public static class VectorExtension
{
	public static Vector3 X(this Vector3 v, float x)
	{
		v.x = x;
		return v;
	}

	public static Vector3 Y(this Vector3 v, float y)
	{
		v.y = y;
		return v;
	}

	public static Vector3 Z(this Vector3 v, float z)
	{
		v.z = z;
		return v;
	}
}