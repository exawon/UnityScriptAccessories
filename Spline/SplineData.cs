using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SplinePoint
{
	[System.NonSerialized]
	public Spline track;

	public string name = "";

	public bool start
	{
		get
		{
			return (track.points[0] == this);
		}
	}

	[SerializeField]
	private Vector3 _localPosition;

	public Vector3 localPosition
	{
		get
		{
			return _localPosition;
		}

		set
		{
			_localPosition = value;
		}
	}

	public Vector3 position
	{
		get
		{
			return track.transform.TransformPoint(_localPosition);
		}

		set
		{
			localPosition = track.transform.InverseTransformPoint(value);
		}
	}

	[System.NonSerialized]
	public List<SplineWay> links = new List<SplineWay>();

	public bool IsEndPoint(SplineWay prevWay)
	{
		if (links.Count == 0)
		{
			return true;
		}
		else if (links.Count == 1 && links[0] == prevWay)
		{
			return true;
		}
		return false;
	}

	public SplineWay GetWayFor(SplinePoint point)
	{
		return links.Find(w => w.GetOtherPoint(this) == point);
	}

#if UNITY_EDITOR
	[HideInInspector]
	public bool selected;
#endif
}

[System.Serializable]
public class SplineWay
{
	[System.NonSerialized]
	public Spline track;

	public SplineHandle headHandle = new SplineHandle();
	public SplineHandle tailHandle = new SplineHandle();

	//[HideInInspector]
	public float length;

	public SplinePoint headPoint
	{
		get
		{
			return headHandle.point;
		}

		set
		{
			headHandle.point = value;
		}
	}

	public SplinePoint tailPoint
	{
		get
		{
			return tailHandle.point;
		}

		set
		{
			tailHandle.point = value;
		}
	}

	public bool Contains(SplinePoint point)
	{
		return (point == headPoint || point == tailPoint);
	}

	public SplineHandle GetHandle(SplinePoint point)
	{
		if (point != null)
		{
			if (point == headPoint)
			{
				return headHandle;
			}
			else if (point == tailPoint)
			{
				return tailHandle;
			}
		}
		return null;
	}

	public SplinePoint GetOtherPoint(SplinePoint point)
	{
		if (point != null)
		{
			if (point == headPoint)
			{
				return tailHandle.point;
			}
			else if (point == tailPoint)
			{
				return headHandle.point;
			}
		}
		return null;
	}

#if UNITY_EDITOR
	public Vector3[] ToArray()
	{
		var p = new Vector3[4];
		p[0] = headPoint.position;
		p[3] = tailPoint.position;
		p[1] = p[0] + headHandle.offset;
		p[2] = p[3] + tailHandle.offset;
		return p;
	}
#endif

	public Vector3 GetPosition(float t)
	{
		Vector3 p0 = headPoint.position;
		Vector3 p3 = tailPoint.position;
		Vector3 p1 = p0 + headHandle.offset;
		Vector3 p2 = p3 + tailHandle.offset;

		float u = 1.0f - t;
		return u * u * u * p0
			+ 3 * u * u * t * p1
			+ 3 * u * t * t * p2
			+ t * t * t * p3;
	}

	public Vector3 GetDirection(float t)
	{
		Vector3 p0 = headPoint.localPosition;
		Vector3 p3 = tailPoint.localPosition;
		Vector3 p1 = p0 + headHandle.offset;
		Vector3 p2 = p3 + tailHandle.offset;

		float u = 1.0f - t;
		return 3f * u * u * (p1 - p0)
			+ 6f * u * t * (p2 - p1)
			+ 3f * t * t * (p3 - p2);
	}

	public bool Get(SplinePoint point, float distance, out Vector3 position, out Vector3 direction)
	{
		position = Vector3.zero;
		direction = Vector3.zero;

		if (distance > length)
		{
			return false;
		}

		if (point == tailPoint)
		{
			distance = length - distance;
		}

		float t = distance / length;
		position = GetPosition(t);
		direction = GetDirection(t);

		if (point == tailPoint)
		{
			direction = -direction;
		}

		return true;
	}
}

[System.Serializable]
public class SplineHandle
{
	[System.NonSerialized]
	public SplineWay way;

	public int pointIndex;
	private SplinePoint _point;

	public SplinePoint point
	{
		get
		{
			return _point;
		}

		set
		{
			_point = value;
			pointIndex = way.track.points.FindIndex(p => p == _point);
		}
	}

	public Vector3 offset;
}


public static class SplineGizmos
{
	public static readonly Color handleColor = Color.magenta;
	public static readonly float pointSize = 0.5f;
	public static readonly float handleSize = 0.3f;
	public static readonly float lineWidth = 5.0f;
}