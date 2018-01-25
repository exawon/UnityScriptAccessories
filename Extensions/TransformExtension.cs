using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class TransformExtension
{
	public static void ChangeLayerRecursively(this Transform transform, string layerName)
	{
		transform.ChangeLayerRecursively(LayerMask.NameToLayer(layerName));
	}

	public static void ChangeLayerRecursively(this Transform transform, int layer)
	{
		transform.gameObject.layer = layer;
		
		foreach (Transform child in transform)
		{
			child.ChangeLayerRecursively(layer);
		}
	}

	public static Transform GetNextSibling(this Transform transform)
	{
		if (!transform.parent)
		{
			return null;
		}

		int index = transform.GetSiblingIndex() + 1;

		if (index >= transform.parent.childCount)
		{
			return null;
		}

		return transform.parent.GetChild(index);
	}

	public static T GetNextSibling<T>(this Transform transform) where T : Component
	{
		Transform sibling = GetNextSibling(transform);

		if (sibling)
		{
			return sibling.GetComponent<T>();
		}
		else
		{
			return null;
		}
	}

	public static Transform GetPreviousSibling(this Transform transform)
	{
		if (!transform.parent)
		{
			return null;
		}

		int index = transform.GetSiblingIndex() - 1;

		if (index < 0)
		{
			return null;
		}

		return transform.parent.GetChild(index);
	}

	public static T GetPreviousSibling<T>(this Transform transform) where T : Component
	{
		Transform sibling = GetPreviousSibling(transform);

		if (sibling)
		{
			return sibling.GetComponent<T>();
		}
		else
		{
			return null;
		}
	}

	public static Transform[] FindChildrenWithTag(this Transform transform, string tag)
	{
		List<Transform> children = new List<Transform>();

		foreach (Transform child in transform.GetComponentsInChildren<Transform>(true))
		{
			if (child.CompareTag(tag))
			{
				children.Add(child);
			}
		}

		return children.ToArray();
	}

	public static Transform[] FindChildrenByName(this Transform transform, string name)
	{
		List<Transform> children = new List<Transform>();

		foreach (Transform child in transform.GetComponentsInChildren<Transform>(true))
		{
			if (child.name == name)
			{
				children.Add(child);
			}
		}

		return children.ToArray();
	}
}
