using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Spline))]
public class SplineEditor : Editor
{
	Spline targetSpline;

	SplinePoint pickedPoint;
	SplineWay pickedWay;
	Vector3 pickedPosition;
	Vector3 pickedDirection;

	SplinePoint contextPoint;

	bool useMouseButton;

	List<SplinePoint> selectedPoints = new List<SplinePoint>();
	Quaternion rotationForSelection;
	Vector3 scaleForSelection;

	bool updated;

	static float GetGizmoAlpha(GizmoType type)
	{
		if ((type & GizmoType.NotInSelectionHierarchy) != 0)
		{
			return 0.5f;
		}
		else if ((type & GizmoType.Selected) != 0)
		{
			return 0.5f;
		}
		else if ((type & GizmoType.Active) != 0)
		{
			return 1.0f;
		}

		return 0.1f;
	}
	
	[DrawGizmo(GizmoType.Pickable | GizmoType.NotInSelectionHierarchy | GizmoType.Selected | GizmoType.Active)]
	static void DrawGizmoPoint(Spline t, GizmoType type)
	{
		float alpha = GetGizmoAlpha(type);
		foreach (SplinePoint p in t.points)
		{
			Gizmos.color = t.pointColor.A(alpha);
			if (p.start)
			{
				Gizmos.color = Color.green.A(alpha);
			}
			else if (p.links.Count < 2)
			{
				Gizmos.color = Color.red.A(alpha);
			}
			Gizmos.DrawSphere(p.position, SplineGizmos.pointSize * 0.5f);
		}
	}

	[DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Selected | GizmoType.Active)]
	static void DrawGizmoWay(Spline t, GizmoType type)
	{
		float alpha = GetGizmoAlpha(type);
		Gizmos.color = t.wayColor.A(alpha);
		foreach (SplineWay w in t.ways)
		{
			Vector3[] p = w.ToArray();
			Handles.DrawBezier(p[0], p[3], p[1], p[2], t.wayColor.A(alpha), null, SplineGizmos.lineWidth);
		}
	}

	[DrawGizmo(GizmoType.Active)]
	static void DrawGizmoWayScales(Spline t, GizmoType type)
	{
		float alpha = GetGizmoAlpha(type);
		Gizmos.color = Color.white.A(alpha);
		foreach (SplineWay w in t.ways)
		{
			for (float distance = 0.0f; distance < w.length; distance += 1.0f)
			{
				Gizmos.DrawSphere(w.GetPosition(distance / w.length), SplineGizmos.pointSize * 0.2f);
			}
		}
	}

    void OnEnable()
	{
		targetSpline = target as Spline;

		Undo.undoRedoPerformed += OnUndoRedo;
    }

    void OnDisable()
	{
		ClearSelection();
	}

	void OnUndoRedo()
	{
		targetSpline.ValidateRefer();

		selectedPoints.Clear();
		foreach (var p in targetSpline.points)
		{
			if (p.selected)
			{
				selectedPoints.Add(p);
			}
		}

		rotationForSelection = Quaternion.identity;
		scaleForSelection = Vector3.one;
	}

	static Vector3 ScreenToWorldPoint(Vector3 position, Vector3 pivot)
	{
		float z = Camera.current.WorldToScreenPoint(pivot).z;
		position = new Vector3(position.x, Camera.current.pixelRect.height - position.y, z);
		return Camera.current.ScreenToWorldPoint(position);
	}

	bool UpdatePicking()
	{
		pickedWay = null;

		pickedPoint = targetSpline.points.Find((p) => (HandleUtility.DistanceToCircle(p.position, SplineGizmos.pointSize) == 0f));
		if (pickedPoint != null)
		{
			pickedPosition = pickedPoint.position;
			return true;
		}

		const int precision = 10;
		foreach (SplineWay w in targetSpline.ways)
		{
			Vector3 from = w.headPoint.position;
			for (int i = 0; i < precision; i++)
			{
				Vector3 to = w.GetPosition((float)(i + 1) / precision);
				if (HandleUtility.DistanceToLine(from, to) < SplineGizmos.lineWidth)
				{
					pickedWay = w;

					Vector3 position = ScreenToWorldPoint(Event.current.mousePosition, to);
					pickedPosition = HandleUtility.ProjectPointLine(position, from, to);

					if (Vector3.SqrMagnitude(from - pickedPosition) < Vector3.SqrMagnitude(to - pickedPosition))
					{
						pickedDirection = w.GetDirection((float)i / precision);
					}
					else
					{
						pickedDirection = w.GetDirection((float)(i + 1) / precision);
					}
					break;
				}
				from = to;
			}
		}

		if (pickedWay != null)
		{
			return true;
		}

		SplinePoint selectedPoint = (selectedPoints.Count == 1) ? selectedPoints[0] : null;
		if (selectedPoint != null)
		{
			pickedPosition = ScreenToWorldPoint(Event.current.mousePosition, selectedPoint.position);
		}

		return false;
	}

	void ClearSelection()
	{
		UpdateSelection(null, false);
	}

	void UpdateSelection(SplinePoint point, bool selected)
	{
		Undo.RecordObject(targetSpline, "SplineEditor Change Selection");

		bool control = Event.current != null && Event.current.control;
		if (point == null || !control)
		{
			selectedPoints.ForEach(p => p.selected = false);
			selectedPoints.Clear();
		}

		if (point != null)
		{
			point.selected = selected;
			if (point.selected)
			{
				selectedPoints.Add(point);
			}
			else
			{
				selectedPoints.Remove(point);
			}
		}

		rotationForSelection = Quaternion.identity;
		scaleForSelection = Vector3.one;
		updated = true;
	}

	Vector3 GetCenterOfPointSelection()
	{
		Vector3 center = Vector3.zero;
		foreach (SplinePoint p in selectedPoints)
		{
			center += p.position;
		}
		center /= selectedPoints.Count;
		return center;
	}

	void EventProc()
	{
		switch (Event.current.type)
		{
		case EventType.KeyDown:
			if (Event.current.keyCode == KeyCode.Escape)
			{
				ClearSelection();
			}
			break;

		case EventType.MouseDown:
			if (Event.current.button == 1 && UpdatePicking())
			{
				contextPoint = pickedPoint;

				Event.current.Use();
				useMouseButton = true;
			}
			break;

		case EventType.MouseUp:
			if (useMouseButton)
			{
				Event.current.Use();
				useMouseButton = false;

				if (Event.current.button == 1 && UpdatePicking())
				{
					if (contextPoint == pickedPoint)
					{
						ShowContextMenu();
					}
					contextPoint = null;
				}
			}
			break;
		}
	}

	void ShowContextMenu()
	{
		var menu = new GenericMenu();
		SplinePoint selectedPoint = (selectedPoints.Count == 1) ? selectedPoints[0] : null;
		if (pickedPoint != null)
		{
			if (pickedPoint.start)
			{
				menu.AddSeparator("Start Point");
				menu.AddSeparator("");
			}
			pickedWay = pickedPoint.GetWayFor(selectedPoint);

			GenericMenu.MenuFunction onDeletePoint = null;
			if (!pickedPoint.start && pickedPoint.links.Count <= 2)
			{
				onDeletePoint = delegate { DeletePoint(pickedPoint); };
			}
			menu.AddItem(new GUIContent("Delete Point"), false, onDeletePoint);
		}
		else if (pickedWay != null)
		{
			GenericMenu.MenuFunction onMakeLinear = delegate { MakeLinear(pickedWay); };
			menu.AddItem(new GUIContent("Make Linear"), false, onMakeLinear);

			GenericMenu.MenuFunction onUnlinkWay = delegate { UnlinkWay(pickedWay); };
			menu.AddItem(new GUIContent("Unlink Way"), false, onUnlinkWay);
		}

		menu.ShowAsContext();
	}

	void OnSceneGUI()
	{
		if (targetSpline.points.Count == 0 || targetSpline.points[0].track == null) return;

		EventProc();

		foreach (var p in targetSpline.points)
		{
			ControlPoint(p);
		}

		bool tool = false;
		switch (Tools.current)
		{
		case Tool.View: tool = ToolNew(); break;
		case Tool.Move: tool = ToolMove(); break;
		case Tool.Rotate: tool = ToolRotate(); break;
		case Tool.Scale: tool = ToolScale(); break;
		}

		Tools.hidden = tool;

		if (updated)
		{
			updated = false;
			UpdateWayLength();
			EditorUtility.SetDirty(targetSpline);
		}
	}

	void ControlPoint(SplinePoint point)
	{
		if (point.selected)
		{
			Handles.color = Color.yellow;
		}
		else
		{
			Handles.color = targetSpline.pointColor;
			if (point.start)
			{
				Handles.color = Color.green;
			}
			else if (point.links.Count < 2)
			{
				Handles.color = Color.red;
			}
		}

		EditorGUI.BeginChangeCheck();
		if (Handles.Button(point.position, Quaternion.identity, SplineGizmos.pointSize, 0f, Handles.SphereCap))
		{
			UpdateSelection(point, !point.selected);
		}

		if (Tools.current == Tool.Move)
		{
			foreach (var p in selectedPoints)
			{
				foreach (var w in p.links)
				{
					ControlHandle(p, w);
				}
			}
		}

		if (!string.IsNullOrEmpty(point.name))
		{
			Handles.Label(point.position, point.name);
		}
	}

	void ControlHandle(SplinePoint point, SplineWay way)
	{
		SplineHandle handle = way.GetHandle(point);
		Vector3 position = handle.offset + point.position;
		Handles.color = SplineGizmos.handleColor;
		Handles.DrawLine(position, point.position);

		EditorGUI.BeginChangeCheck();
		position = Handles.FreeMoveHandle(position, Quaternion.identity, SplineGizmos.handleSize, Vector3.zero, Handles.CubeCap);

		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(targetSpline, "SplineEditor Move Handle");
			handle.offset = position - point.position;
			UpdateHandles(point, way);
			updated = true;
		}
	}

	bool ToolNew()
	{
		if (Event.current.button == 1)
		{
			return false;
		}

		if (Event.current.type == EventType.MouseMove)
		{
			UpdatePicking();
		}

		Handles.color = Color.magenta;

		SplinePoint selectedPoint = (selectedPoints.Count == 1) ? selectedPoints[0] : null;
		if (selectedPoint != null && selectedPoint.GetWayFor(pickedPoint) == null && pickedWay == null)
		{
			Handles.DrawDottedLine(selectedPoint.position, pickedPosition, SplineGizmos.lineWidth);
		}

		SplinePoint newPoint = null;
		if (pickedWay != null)
		{
			if (Handles.Button(pickedPosition, Quaternion.identity, SplineGizmos.pointSize, 1f, Handles.SphereCap))
			{
				Undo.RecordObject(targetSpline, "SplineEditor ToolNew");

				newPoint = InsertPoint(pickedWay, pickedPosition);
			}
		}
		else if (selectedPoint != null)
		{
			if (Handles.Button(pickedPosition, Quaternion.identity, SplineGizmos.pointSize, 1f, Handles.SphereCap))
			{
				Undo.RecordObject(targetSpline, "SplineEditor ToolNew");

				if (pickedPoint == null)
				{
					newPoint = AddPoint(selectedPoint, pickedPosition);
				}
				else if (selectedPoint.GetWayFor(pickedPoint) == null)
				{
					LinkWay(selectedPoint, pickedPoint);
				}
			}
		}

		if (newPoint != null)
		{
			ClearSelection();
			UpdateSelection(newPoint, true);
		}
		return true;
	}

	bool ToolMove()
	{
		if (selectedPoints.Count == 0)
		{
			return false;
		}

		EditorGUI.BeginChangeCheck();
		Vector3 center = GetCenterOfPointSelection();
		Vector3 position = Handles.PositionHandle(center, targetSpline.transform.rotation);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(targetSpline, "SplineEditor ToolMove");
			Vector3 diff = position - center;
			foreach (var p in selectedPoints)
			{
				p.position += diff;
			}
		}
		return true;
	}

	bool ToolRotate()
	{
		if (selectedPoints.Count < 2)
		{
			return false;
		}

		EditorGUI.BeginChangeCheck();
		Vector3 center = GetCenterOfPointSelection();
		Quaternion rotation = Handles.RotationHandle(rotationForSelection, center);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(targetSpline, "SplineEditor ToolRotate");
			Quaternion diff = Quaternion.Inverse(rotationForSelection) * rotation;
			foreach (var p in selectedPoints)
			{
				Vector3 position = p.position - center;
				p.position = diff * position + center;
			}
			rotationForSelection = rotation;
		}
		return true;
	}

	bool ToolScale()
	{
		if (selectedPoints.Count < 2)
		{
			return false;
		}

		EditorGUI.BeginChangeCheck();
		Vector3 center = GetCenterOfPointSelection();
		Vector3 scale = Handles.ScaleHandle(scaleForSelection, center, targetSpline.transform.rotation, HandleUtility.GetHandleSize(center));
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(targetSpline, "SplineEditor ToolScale");
			Vector3 diff = new Vector3(1f / scaleForSelection.x * scale.x, 1f / scaleForSelection.y * scale.y, 1f / scaleForSelection.z * scale.z);
			foreach (var p in selectedPoints)
			{
				Vector3 position = p.position - center;
				p.position = new Vector3(position.x * diff.x, position.y * diff.y, position.z * diff.z) + center;
			}
			scaleForSelection = scale;
		}
		return true;
	}

    public override void OnInspectorGUI()
    {
		DrawDefaultInspector();

		if (GUILayout.Button("SplineEditor Change Way Color"))
		{
			Undo.RecordObject(targetSpline, "Change Way Color");
			targetSpline.wayColor = ColorExtension.GetRandom();
		}
    }

	SplinePoint AddPoint(SplinePoint point, Vector3 position)
	{
		Undo.RecordObject(targetSpline, "SplineEditor AddPoint");

		SplinePoint newPoint = targetSpline.NewPoint(position);
		SplineWay newWay = targetSpline.NewWay(point, newPoint);
		Vector3 direction = position - point.position;
		newWay.headHandle.offset = direction * 0.2f;
		newWay.tailHandle.offset = -direction * 0.2f;
		UpdateHandles(point, newWay);
		updated = true;
		return newPoint;
	}

	SplinePoint InsertPoint(SplineWay way, Vector3 position)
	{	
		Undo.RecordObject(targetSpline, "SplineEditor InsertPoint");

		SplinePoint newPoint = targetSpline.NewPoint(position);
		SplineWay newWay = targetSpline.NewWay(way.headPoint, newPoint);
		newWay.headHandle.offset = way.headHandle.offset;
		newWay.tailHandle.offset = -pickedDirection.normalized;

		newWay = targetSpline.NewWay(newPoint, way.tailPoint);
		newWay.headHandle.offset = pickedDirection.normalized;
		newWay.tailHandle.offset = way.tailHandle.offset;

		targetSpline.DeleteWay(way);
		updated = true;
		return newPoint;
	}

	void DeletePoint(SplinePoint point)
    {
		Undo.RecordObject(targetSpline, "SplineEditor DeletePoint");

		if (point.links.Count == 2)
		{
			SplineWay headWay = point.links[0];
			SplineWay tailWay = point.links[1];
			SplinePoint headPoint = headWay.GetOtherPoint(point);
			SplinePoint tailPoint = tailWay.GetOtherPoint(point);
			SplineWay newWay = targetSpline.NewWay(headPoint, tailPoint);
			newWay.headHandle = headWay.GetHandle(headPoint);
			newWay.tailHandle = tailWay.GetHandle(tailPoint);
		}

		targetSpline.DeletePoint(point);
		selectedPoints.Remove(point);
		updated = true;
    }

	SplineWay LinkWay(SplinePoint from, SplinePoint to)
	{
		Undo.RecordObject(targetSpline, "SplineEditor LinkWay");

		Vector3 direction = to.position - from.position;
		SplineWay newWay = targetSpline.NewWay(from, to);
		newWay.headHandle.offset = direction.normalized;
		newWay.tailHandle.offset = -direction.normalized;
		updated = true;
		return newWay;
	}

	void UnlinkWay(SplineWay way)
	{
		Undo.RecordObject(targetSpline, "SplineEditor UnlinkWay");

		targetSpline.DeleteWay(way);
		updated = true;
	}

	void MakeLinear(SplineWay way)
	{
		Undo.RecordObject(targetSpline, "SplineEditor MakeLinear");

		Vector2 direction = way.tailPoint.position - way.headPoint.position;
		way.headHandle.offset = direction.normalized;
		way.tailHandle.offset = -direction.normalized;
		updated = true;
	}

	void UpdateHandles(SplinePoint point, SplineWay align)
	{
		if (Event.current == null || Event.current.control)
		{
			return;
		}

		SplineHandle handle = align.GetHandle(point);
		foreach (SplineWay w in point.links)
		{
			SplineHandle h = w.GetHandle(point);
			if (h.way != align)
			{
				float sign = (Vector3.Dot(h.offset, handle.offset) > 0f) ? 1f : -1f;
				float length = (Event.current.shift) ? handle.offset.magnitude : h.offset.magnitude;
				h.offset = sign * length * handle.offset.normalized;
			}
		}
	}

	void UpdateWayLength()
	{
		foreach (var way in targetSpline.ways)
		{
			way.length = 0.0f;
			Vector3 from = way.headPoint.position;
			int i = 0;
			while (i < 100)
			{
				Vector3 to = way.GetPosition(++i / 100f);
				way.length += Vector3.Distance(from, to);
				from = to;
			}
		}
	}

	[MenuItem("GameObject/Create Other/Spline")]
    public static void CreatSpline()
    {
		GameObject go = new GameObject();

		if (Selection.activeGameObject != null)
		{
			go.transform.parent = Selection.activeGameObject.transform;
			go.transform.SetLocalPosition(0, 0, 0);
			go.transform.SetLocalScale(1, 1, 1);
		}

		go.SetNumberedName<Spline>("Spline ");
		go.AddComponent<Spline>();

		Selection.activeGameObject = go;
    }
}