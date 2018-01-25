using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class CameraExtension
{
	public static float GetNearFarDistance(this Camera camera)
	{
		return camera.farClipPlane - camera.nearClipPlane;
	}

	public static float GetNearFarAspect(this Camera camera)
	{
		return camera.nearClipPlane / camera.farClipPlane;
	}

	public static Vector3 GetPositionInCameraSpace(this Camera camera, Vector3 position, float depth, Camera from = null)
	{
		if (from == null)
		{
			from = camera;
		}
		return camera.ViewportToWorldPoint(from.WorldToViewportPoint(position).Z(depth));
	}
}