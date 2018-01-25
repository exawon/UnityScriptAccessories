using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(ReadOnlyInInspectorAttribute))]
public class ReadOnlyPropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
	{
		GUI.enabled = false;
		EditorGUI.PropertyField(position, prop, label);
		GUI.enabled = true;
	}
}
#endif

public class ReadOnlyInInspectorAttribute : PropertyAttribute
{
	public ReadOnlyInInspectorAttribute()
	{
		// nothing to do
	}
}
