using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class FitCameraToTarget : MonoBehaviour
{
	Camera _camera;

	int _sw;
	int _sh;

	public GameObject target;

	void Awake()
	{
		_camera = GetComponent<Camera>();
	}

	void Update()
	{
		int sw = Screen.width;
		int sh = Screen.height;
		if (_sw != sw || _sh != sh)
		{
			_sw = sw;
			_sh = sh;
			FitToTarget();
		}
	}

	[ContextMenu("Fit")]
	void FitToTarget()
	{
		var targetBounds = target.GetBounds();
		if (targetBounds.size.x * targetBounds.size.y == 0.0f)
		{
			return;
		}

		// 지정한 크기가 화면에 꼭 맞도록 camera z position 설정
		// http://docs.unity3d.com/kr/current/Manual/FrustumSizeAtDistance.html

		float height;
		if (_sw < _sh)
		{
			height = targetBounds.size.x * _sh / _sw;
		}
		else
		{
			height = targetBounds.size.y;
		}

		float tanFov = Mathf.Tan(0.5f * Mathf.Deg2Rad * _camera.fieldOfView);
		float distance = 0.5f * height / tanFov;

		Vector3 targetCenter = targetBounds.center;
		targetCenter.z = target.transform.position.z;
		_camera.transform.position = targetCenter - (_camera.transform.forward * distance);
		_camera.farClipPlane = Mathf.Ceil(distance);
	}
}