using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFrustumGizmos : MonoBehaviour
{
	#if UNITY_EDITOR
	public Color color = new Color(1.0f, 1.0f, 1.0f, 0.2f);

	void OnEnable()
	{
	}

	void OnDrawGizmos()
	{
		for (var t = transform; t != null; t = t.parent)
		{
			if (UnityEditor.Selection.Contains(t))
			{
				return;
			}
		}

		var camera = GetComponent<Camera>();
		if (!camera || !enabled)
		{
			return;
		}

		Gizmos.color = color;
		Gizmos.matrix = Matrix4x4.TRS(camera.transform.position, camera.transform.rotation, Vector3.one);

		if (camera.orthographic)
		{
			float center = (camera.farClipPlane + camera.nearClipPlane) * 0.5f;
			float spread = camera.farClipPlane - camera.nearClipPlane;
			float size = camera.orthographicSize * 2.0f;
			Gizmos.DrawWireCube(new Vector3(0.0f, 0.0f, center), new Vector3(size * camera.aspect, size / camera.aspect, spread));
		}
		else
		{
			Gizmos.DrawFrustum(Vector3.zero, camera.fieldOfView, camera.farClipPlane, camera.nearClipPlane, camera.aspect);
		}
	}
	#endif
}