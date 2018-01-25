using UnityEngine;
using System.Collections;

public class FitObjectToCamera : MonoBehaviour
{
	public Camera _camera;
	public float distance;

	public bool fitWidth = true;
	public bool fitHeight = true;

	public Bounds bounds;
	public Vector2 offset;

	Bounds _cameraBounds;

	void OnValidate()
	{
		if (_camera == null)
		{
			_camera = Camera.main;
		}
	}

	void Update()
	{
		Bounds cameraBounds = new Bounds();
		cameraBounds.min = _camera.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, distance));
		cameraBounds.max = _camera.ViewportToWorldPoint(new Vector3(1.0f, 1.0f, distance));

		if (_cameraBounds != cameraBounds)
		{
			_cameraBounds = cameraBounds;
			FitToCamera();
		}
	}

	[ContextMenu("Fit")]
	void FitToCamera()
	{
		if (bounds.size.x * bounds.size.y == 0.0f)
		{
			transform.position = _camera.transform.position + (_camera.transform.forward * distance);
			return;
		}

		float scaleValue = float.MaxValue;
		if (fitWidth)
		{
			scaleValue = _cameraBounds.extents.x / bounds.extents.x;
		}
		if (fitHeight)
		{
			scaleValue = Mathf.Min(scaleValue, _cameraBounds.extents.y / bounds.extents.y);
		}
		transform.localScale = Vector3.one * scaleValue;

		Vector3 center = transform.TransformVector(GetCenter());
		transform.position = _camera.transform.position + (_camera.transform.forward * distance) - center;
	}

	Vector3 GetCenter()
	{
		Vector3 center = bounds.center;
		if (Screen.width < Screen.height)
		{
			center.y += offset.y;
		}
		else
		{
			center.x += offset.x;
		}
		return center;
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.magenta;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(GetCenter(), bounds.size);
	}
}
