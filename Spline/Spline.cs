using UnityEngine;
using System.Collections.Generic;

public class Spline : MonoBehaviour
{
	public List<SplinePoint> points = new List<SplinePoint>();
	public List<SplineWay> ways = new List<SplineWay>();

	public Color pointColor = Color.white;
	public Color wayColor = Color.black;

	void OnValidate()
	{
		if (points.Count == 0)
		{
			NewPoint(Vector3.zero);
		}

		if (wayColor == Color.black)
		{
			wayColor = ColorExtension.GetRandom();
		}

		ValidateRefer();
	}

	void Reset()
	{
		OnValidate();
	}

	void Awake()
	{
		ValidateRefer();
	}

	public void ValidateRefer()
	{
		foreach (SplinePoint p in points)
		{
			p.track = this;
			p.links.Clear();
		}

		foreach (SplineWay w in ways)
		{
			SetWay(w, points[w.headHandle.pointIndex], points[w.tailHandle.pointIndex]);
		}
	}

	public void ValidateIndex()
	{
		foreach (SplineWay w in ways)
		{
			w.headHandle.pointIndex = points.FindIndex(p => p == w.headPoint);
			w.tailHandle.pointIndex = points.FindIndex(p => p == w.tailPoint);
		}
	}

	public SplinePoint NewPoint(Vector3 position)
	{
		SplinePoint point = new SplinePoint();
		points.Add(point);

		point.track = this;
		point.position = position;
		return point;
	}

	public void DeletePoint(SplinePoint point)
	{
		if (point.start)
		{
			return;
		}

		if (points.Remove(point))
		{
			foreach (var w in point.links)
			{
				SplinePoint other = w.GetOtherPoint(point);
				other.links.Remove(w);
				ways.Remove(w);
			}
			point.links.Clear();
			ValidateIndex();
		}
	}

	public SplineWay NewWay(SplinePoint headPoint, SplinePoint tailPoint)
	{
		SplineWay way = new SplineWay();
		SetWay(way, headPoint, tailPoint);
		ways.Add(way);
		return way;
	}

	void SetWay(SplineWay way, SplinePoint headPoint, SplinePoint tailPoint)
	{
		way.track = this;
		way.headHandle.way = way;
		way.tailHandle.way = way;
		way.headPoint = headPoint;
		way.tailPoint = tailPoint;
		way.headPoint.links.Add(way);
		way.tailPoint.links.Add(way);
	}

	public void DeleteWay(SplineWay way)
	{
		way.headPoint.links.Remove(way);
		way.tailPoint.links.Remove(way);
		if (ways.Remove(way))
		{
			ValidateIndex();
		}
	}

	public SplinePoint GetStartPoint()
	{
		return points[0];
	}

	public SplineWay GetStartWay()
	{
		SplinePoint point = GetStartPoint();
		return (point != null && point.links.Count > 0) ? point.links[0] : null;
	}
}