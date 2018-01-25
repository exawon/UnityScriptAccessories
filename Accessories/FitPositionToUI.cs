using UnityEngine;
using System.Collections;

public class FitPositionToUI : MonoBehaviour
{
	public Transform UITransform;

	float z = 0f;

	void Start()
	{
		z = transform.position.z;

		SetPosition();
	}

	void Update()
	{
		SetPosition();
	}

	void SetPosition()
	{
		if (UITransform)
		{
			transform.position = Camera.main.GetPositionInCameraSpace(UITransform.position, z);
			transform.localRotation = UITransform.localRotation;
			transform.localScale = UITransform.localScale;
		}
	}
}