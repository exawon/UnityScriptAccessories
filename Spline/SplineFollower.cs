using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplineFollower : MonoBehaviour
{
	public Spline track;
	public Vector3 offsetXY;
	public float moveSpeed;
	public float moveOnWay;

	public delegate SplineWay OnSplinePoint(SplinePoint point, SplineWay prevWay);
	public OnSplinePoint onSplinePoint;

	SplinePoint currPoint;
	SplineWay currWay;
	SplineWay prevWay;

	IEnumerator move;

	IEnumerator Move()
	{
		currPoint = track.GetStartPoint();
		currWay = null;
		prevWay = null;
		
		while (track != null)
		{
			if (onSplinePoint != null)
			{
				currWay = onSplinePoint.Invoke(currPoint, prevWay);
			}

			if (currWay == null)
			{
				currWay = currPoint.links.Find(w => w != prevWay);
				if (currWay == null)
				{
					break;
				}
			}

			while (moveOnWay < currWay.length)
			{
				yield return null;
				moveOnWay += moveSpeed * Time.deltaTime;
			}
			moveOnWay -= currWay.length;

			currPoint = currWay.GetOtherPoint(currPoint);
			prevWay = currWay;
			currWay = null;
		}
	}

	void Update()
	{
		if (track == null || move == null)
		{
			enabled = false;
			return;
		}

		if (!move.MoveNext())
		{
			Stop();
		}

		if (moveOnWay > 0f && currPoint != null && currWay != null)
		{
			Vector3 position;
			Vector3 direction;
			if (currWay.Get(currPoint, moveOnWay, out position, out direction))
			{
				transform.rotation = Quaternion.LookRotation(direction);
				transform.position = position + (transform.rotation * offsetXY);
			}
		}
	}

	public void Ready(Spline track, float moveSpeed, Vector3 offset)
	{
		Stop();

		this.track = track;
		this.moveSpeed = moveSpeed;
		this.offsetXY = offset.Z(0.0f);
		this.moveOnWay = offset.z;

		transform.position = track.GetStartPoint().position;

		move = Move();
	}

	public void Play(float moveSpeed)
	{
		this.moveSpeed = moveSpeed;
		enabled = true;
	}

	public void Stop()
	{
		enabled = false;
	}

	void OnDrawGizmos()
	{
		Vector3 position = transform.position - (transform.rotation * offsetXY);
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(position, 0.1f);
		Gizmos.color = Color.gray.A(0.5f);
		Gizmos.DrawLine(position, transform.position);
	}
}