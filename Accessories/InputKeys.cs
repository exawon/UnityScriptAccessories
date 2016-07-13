using UnityEngine;
using System.Collections.Generic;

public class InputKeys
{
	public static bool alt
	{
		get
		{
			return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
		}
	}

	public static bool control
	{
		get
		{
			return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		}
	}

	public static bool shift
	{
		get
		{
			return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		}
	}

	static Dictionary<KeyCode, float> keyTime = new Dictionary<KeyCode, float>();

	public static bool GetRepeat(KeyCode key, float interval)
	{
		if (Input.GetKey(key))
		{
			float t = 0.0f;
			if (keyTime.TryGetValue(key, out t))
			{
				t += interval;
				if (t < Time.realtimeSinceStartup)
				{
					keyTime[key] = t;
					return true;
				}
			}
			else
			{
				keyTime.Add(key, Time.realtimeSinceStartup);
				return true;
			}
		}
		else
		{
			keyTime.Remove(key);
		}
		return false;
	}
}
