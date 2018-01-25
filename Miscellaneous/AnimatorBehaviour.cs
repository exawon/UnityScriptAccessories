using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class AnimatorStateAttribute : System.Attribute
{
	public int nameHash;

	public AnimatorStateAttribute(string name)
	{
		nameHash = Animator.StringToHash(name);
	}
}

public class AnimatorBehaviour : MonoBehaviour
{
	public class MethodMap : Dictionary<int, MethodInfo>
	{
		// nothing to do
	}

	static Dictionary<System.Type, MethodMap> methodMaps = new Dictionary<System.Type, MethodMap>();

	public static MethodMap GetMethodMap(System.Type thisType)
	{
		MethodMap methodMap;
		if (!methodMaps.TryGetValue(thisType, out methodMap))
		{
			methodMap = new MethodMap();

			var methods = thisType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			for (int i = 0; i < methods.Length; i++)
			{
				var attributes = methods[i].GetCustomAttributes(typeof(AnimatorStateAttribute), true) as AnimatorStateAttribute[];
				if (attributes.Length == 0)
				{
					continue;
				}

				if (!methods[i].ReturnType.Equals(typeof(IEnumerator)))
				{
					Debug.LogErrorFormat("{0}.cs({1}): error {2}: the method '{3}.{4}()' should be the type of coroutine.", thisType, 0, typeof(AnimatorStateAttribute).Name, thisType.Name, methods[i].Name);
					continue;
				}

				for (int j = 0; j < attributes.Length; j++)
				{
					int nameHash = attributes[j].nameHash;
					if (!methodMap.ContainsKey(nameHash))
					{
						methodMap.Add(nameHash, methods[i]);
					}
				}
			}

			if (methodMap.Count == 0)
			{
				Debug.LogWarningFormat("{0}.cs({1}): warning {2}: not found any coroutine.", thisType, 0, typeof(AnimatorStateAttribute).Name);
			}

			methodMaps.Add(thisType, methodMap);
		}

		return methodMap;
	}

	MethodMap methodMap;

	int[] currentStates;
	object[] param = new object[2];

	public Animator animator { get; private set; }

	protected virtual void Awake()
	{
		methodMap = GetMethodMap(GetType());
		
		animator = GetComponentInChildren<Animator>();
		animator.logWarnings = Debug.isDebugBuild;
		
		currentStates = new int[animator.layerCount];
		for (int i = 0; i < currentStates.Length; i++)
		{
			currentStates[i] = 0;
		}
	}

	void Update()
	{
		if (!animator.enabled)
		{
			return;
		}

		for (int i = 0; i < animator.layerCount; i++)
		{
			int newState = animator.GetCurrentAnimatorStateInfo(i).fullPathHash;
			if (currentStates[i] != newState)
			{
				int oldState = currentStates[i];
				currentStates[i] = newState;
				OnChangeState(oldState, newState);

				MethodInfo method;
				if (methodMap.TryGetValue(currentStates[i], out method))
				{
					param[0] = i;
					param[1] = currentStates[i];
					StartCoroutine(method.Invoke(this, param) as IEnumerator);

					break;
				}
			}
		}
	}

	protected virtual void OnChangeState(int oldState, int newState)
	{
		// nothing to do
	}

	public bool IsActiveState(int layer, int nameHash)
	{
		if (animator.enabled && animator.GetCurrentAnimatorStateInfo(layer).fullPathHash == nameHash)
		{
			return true;
		}
	
		return false;
	}

	public float GetNormalizedTime(int layer)
	{
		return animator.GetCurrentAnimatorStateInfo(layer).normalizedTime;
	}
}