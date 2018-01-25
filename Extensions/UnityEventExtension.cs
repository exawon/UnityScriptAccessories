using UnityEngine;
using UnityEngine.Events;

public static class UnityEventExtension
{
	public static void AddBoolPersistentListener(this UnityEventBase unityEvent, UnityAction<bool> call, bool argument)
	{
#if UNITY_EDITOR
		int index = FindPersistentListener(unityEvent, call.Target, call.Method.Name);
		if (index < 0)
		{
			UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(unityEvent, call, argument);
		}
		else
		{
			UnityEditor.Events.UnityEventTools.RegisterBoolPersistentListener(unityEvent, index, call, argument);
		}
#else
		throw new System.NotSupportedException();
#endif
	}

	public static void AddFloatPersistentListener(this UnityEventBase unityEvent, UnityAction<float> call, float argument)
	{
#if UNITY_EDITOR
		int index = FindPersistentListener(unityEvent, call.Target, call.Method.Name);
		if (index < 0)
		{
			UnityEditor.Events.UnityEventTools.AddFloatPersistentListener(unityEvent, call, argument);
		}
		else
		{
			UnityEditor.Events.UnityEventTools.RegisterFloatPersistentListener(unityEvent, index, call, argument);
		}
#else
		throw new System.NotSupportedException();
#endif
	}

	public static void AddIntPersistentListener(this UnityEventBase unityEvent, UnityAction<int> call, int argument)
	{
#if UNITY_EDITOR
		int index = FindPersistentListener(unityEvent, call.Target, call.Method.Name);
		if (index < 0)
		{
			UnityEditor.Events.UnityEventTools.AddIntPersistentListener(unityEvent, call, argument);
		}
		else
		{
			UnityEditor.Events.UnityEventTools.RegisterIntPersistentListener(unityEvent, index, call, argument);
		}
#else
		throw new System.NotSupportedException();
#endif
	}

	public static void AddObjectPersistentListener<T>(this UnityEventBase unityEvent, UnityAction<T> call, T argument) where T : UnityEngine.Object
	{
#if UNITY_EDITOR
		int index = FindPersistentListener(unityEvent, call.Target, call.Method.Name);
		if (index < 0)
		{
			UnityEditor.Events.UnityEventTools.AddObjectPersistentListener(unityEvent, call, argument);
		}
		else
		{
			UnityEditor.Events.UnityEventTools.RegisterObjectPersistentListener(unityEvent, index, call, argument);
		}
#else
		throw new System.NotSupportedException();
#endif
	}

	public static void AddPersistentListener<T0, T1, T2, T3>(this UnityEvent<T0, T1, T2, T3> unityEvent, UnityAction<T0, T1, T2, T3> call)
	{
#if UNITY_EDITOR
		int index = FindPersistentListener(unityEvent, call.Target, call.Method.Name);
		if (index < 0)
		{
			UnityEditor.Events.UnityEventTools.AddPersistentListener(unityEvent, call);
		}
		else
		{
			UnityEditor.Events.UnityEventTools.RegisterPersistentListener(unityEvent, index, call);
		}
#else
		throw new System.NotSupportedException();
#endif
	}

	public static void AddPersistentListener<T0, T1, T2>(this UnityEvent<T0, T1, T2> unityEvent, UnityAction<T0, T1, T2> call)
	{
#if UNITY_EDITOR
		int index = FindPersistentListener(unityEvent, call.Target, call.Method.Name);
		if (index < 0)
		{
			UnityEditor.Events.UnityEventTools.AddPersistentListener(unityEvent, call);
		}
		else
		{
			UnityEditor.Events.UnityEventTools.RegisterPersistentListener(unityEvent, index, call);
		}
#else
		throw new System.NotSupportedException();
#endif
	}

	public static void AddPersistentListener<T0, T1>(this UnityEvent<T0, T1> unityEvent, UnityAction<T0, T1> call)
	{
#if UNITY_EDITOR
		int index = FindPersistentListener(unityEvent, call.Target, call.Method.Name);
		if (index < 0)
		{
			UnityEditor.Events.UnityEventTools.AddPersistentListener(unityEvent, call);
		}
		else
		{
			UnityEditor.Events.UnityEventTools.RegisterPersistentListener(unityEvent, index, call);
		}
#else
		throw new System.NotSupportedException();
#endif
	}

	public static void AddPersistentListener<T0>(this UnityEvent<T0> unityEvent, UnityAction<T0> call)
	{
#if UNITY_EDITOR
		int index = FindPersistentListener(unityEvent, call.Target, call.Method.Name);
		if (index < 0)
		{
			UnityEditor.Events.UnityEventTools.AddPersistentListener(unityEvent, call);
		}
		else
		{
			UnityEditor.Events.UnityEventTools.RegisterPersistentListener(unityEvent, index, call);
		}
#else
		throw new System.NotSupportedException();
#endif
	}

	public static void AddPersistentListener(this UnityEvent unityEvent, UnityAction call)
	{
#if UNITY_EDITOR
		int index = FindPersistentListener(unityEvent, call.Target, call.Method.Name);
		if (index < 0)
		{
			UnityEditor.Events.UnityEventTools.AddPersistentListener(unityEvent, call);
		}
		else
		{
			UnityEditor.Events.UnityEventTools.RegisterPersistentListener(unityEvent, index, call);
		}
#else
		throw new System.NotSupportedException();
#endif
	}

	public static void AddStringPersistentListener(this UnityEventBase unityEvent, UnityAction<string> call, string argument)
	{
#if UNITY_EDITOR
		int index = FindPersistentListener(unityEvent, call.Target, call.Method.Name);
		if (index < 0)
		{
			UnityEditor.Events.UnityEventTools.AddStringPersistentListener(unityEvent, call, argument);
		}
		else
		{
			UnityEditor.Events.UnityEventTools.RegisterStringPersistentListener(unityEvent, index, call, argument);
		}
#else
		throw new System.NotSupportedException();
#endif
	}

	public static void AddVoidPersistentListener(this UnityEventBase unityEvent, UnityAction call)
	{
#if UNITY_EDITOR
		int index = FindPersistentListener(unityEvent, call.Target, call.Method.Name);
		if (index < 0)
		{
			UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(unityEvent, call);
		}
		else
		{
			UnityEditor.Events.UnityEventTools.RegisterVoidPersistentListener(unityEvent, index, call);
		}
#else
		throw new System.NotSupportedException();
#endif
	}

	static int FindPersistentListener(this UnityEventBase unityEvent, object callTarget, string callMethodName)
	{
#if UNITY_EDITOR
		while (unityEvent.RemoveInvalidListener())
		{
			// nothing to do
		}

		int eventCount = unityEvent.GetPersistentEventCount();
		for (int i = 0; i < eventCount; i++)
		{
			if (unityEvent.GetPersistentTarget(i) == callTarget as Object)
			{
				if (unityEvent.GetPersistentMethodName(i) == callMethodName)
				{
					return i;
				}
			}
		}
		return -1;
#else
		throw new System.NotSupportedException();
#endif
	}

	static bool RemoveInvalidListener(this UnityEventBase unityEvent)
	{
#if UNITY_EDITOR
		int eventCount = unityEvent.GetPersistentEventCount();
		for (int i = 0; i < eventCount; i++)
		{
			if (string.IsNullOrEmpty(unityEvent.GetPersistentMethodName(i)))
			{
				UnityEditor.Events.UnityEventTools.RemovePersistentListener(unityEvent, i);
				return true;
			}
		}
		return false;
#else
		throw new System.NotSupportedException();
#endif
	}
}