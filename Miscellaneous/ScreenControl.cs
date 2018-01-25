using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class ScreenControl : MonoBehaviour
{
	public static bool resolutionChanged
	{
		get;
		private set;
	}

	public static bool cameraMatrixChanged
	{
		get;
		private set;
	}

	public static bool isPortrait
	{
		get;
		private set;
	}

	static int _sw;
	static int _sh;

	public static void Reset()
	{
		resolutionChanged = true;
		cameraMatrixChanged = true;
		_sw = 0;
		_sh = 0;
	}

	public enum Axis
	{
		Horizontal,
		Vertical,
	}

	[System.Serializable]
	public struct AxisValue
	{
		public Axis axis;
		public float value;

		public AxisValue(Axis axis, float value)
		{
			this.axis = axis;
			this.value = value;
		}
	}

	public AxisValue portraitFov = new AxisValue(Axis.Vertical, 0.0f);
	public AxisValue landscapeFov = new AxisValue(Axis.Vertical, 0.0f);

	[SerializeField]
	Camera _camera;

	void OnValidate()
	{
		_camera = GetComponent<Camera>();

		if (portraitFov.value == 0)
		{
			portraitFov.value = _camera.fieldOfView;
		}

		if (landscapeFov.value == 0)
		{
			landscapeFov.value = _camera.fieldOfView;
		}
	}

	void Awake()
	{
		Reset();
	}

	void Update()
	{
		int sw = Screen.width;
		int sh = Screen.height;

		cameraMatrixChanged = resolutionChanged;

		resolutionChanged = (sw != _sw || sh != _sh);
		if (resolutionChanged)
		{
			_sw = sw;
			_sh = sh;

			AxisValue fov;

			isPortrait = (_sw < _sh);
			if (isPortrait)
			{
				fov = portraitFov;
			}
			else
			{
				fov = landscapeFov;
			}

			if (fov.axis == Axis.Horizontal)
			{
				_camera.fieldOfView = fov.value * _sh / _sw;
			}
			else
			{
				_camera.fieldOfView = fov.value;
			}
		}
	}
}