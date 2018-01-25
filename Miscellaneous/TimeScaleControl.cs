using UnityEngine;

public class TimeScaleControl : MonoBehaviour
{
	void Awake()
	{
		if (Debug.isDebugBuild)
		{
			float timeScale = PlayerPrefs.GetFloat("timeScale");
			if (timeScale == 0.0f)
			{
				timeScale = 1.0f;
			}
			ChangeTimeScale(timeScale);
		}
		else
		{
			enabled = false;
		}
	}

	void Update()
	{
		if (Input.anyKey)
		{
			for (int i = 0; i < 10; i++)
			{
				if (Input.GetKeyDown(KeyCode.Keypad0 + i) || Input.GetKeyDown(KeyCode.Alpha0 + i))
				{
					if (InputKeys.shift)
					{
						ChangeTimeScale(i * 0.1f);
					}
					else
					{
						ChangeTimeScale(i);
					}
				}
			}

			if (Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Equals))
			{
				ChangeTimeScale(Time.timeScale + Time.unscaledDeltaTime);
			}
			else if (Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.Minus))
			{
				ChangeTimeScale(Time.timeScale - Time.unscaledDeltaTime);
			}
		}
	}

	void ChangeTimeScale(float timeScale)
	{
		timeScale = Mathf.Clamp(timeScale, 0.0f, 9.0f);

		Time.timeScale = timeScale;
		PlayerPrefs.SetFloat("timeScale", timeScale);
		Debug.LogFormat("timeScale={0}", timeScale);
	}
}