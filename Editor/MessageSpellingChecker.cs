using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Reflection;

public class MessageSpellingChecker : AssetPostprocessor
{
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		if (importedAssets.Length < 64)
		{
			string importedScripts = "";
			foreach (string asset in importedAssets)
			{
				if (Path.GetExtension(asset) == ".cs")
				{
					importedScripts += Path.GetFileNameWithoutExtension(asset) + " ";
				}
			}
			EditorPrefs.SetString("importedScripts", importedScripts);
		}
	}

	static string[] messagesOfEditorWindow =
	{
		"OnDestroy",
		"OnFocus",
		"OnGUI",
		"OnHierarchyChange",
		"OnInspectorUpdate",
		"OnLostFocus",
		"OnProjectChange",
		"OnSelectionChange",
		"Update",
	};

	static string[] messagesOfMonoBehaviour =
	{
		"Awake",
		"FixedUpdate",
		"LateUpdate",
		"OnAnimatorIK",
		"OnAnimatorMove",
		"OnApplicationFocus",
		"OnApplicationPause",
		"OnApplicationQuit",
		"OnAudioFilterRead",
		"OnBecameInvisible",
		"OnBecameVisible",
		"OnCollisionEnter",
		"OnCollisionEnter2D",
		"OnCollisionExit",
		"OnCollisionExit2D",
		"OnCollisionStay",
		"OnCollisionStay2D",
		"OnConnectedToServer",
		"OnControllerColliderHit",
		"OnDestroy",
		"OnDisable",
		"OnDisconnectedFromServer",
		"OnDrawGizmos",
		"OnDrawGizmosSelected",
		"OnEnable",
		"OnFailedToConnect",
		"OnFailedToConnectToMasterServer",
		"OnGUI",
		"OnJointBreak",
		"OnLevelWasLoaded",
		"OnMasterServerEvent",
		"OnMouseDown",
		"OnMouseDrag",
		"OnMouseEnter",
		"OnMouseExit",
		"OnMouseOver",
		"OnMouseUp",
		"OnMouseUpAsButton",
		"OnNetworkInstantiate",
		"OnParticleCollision",
		"OnPlayerConnected",
		"OnPlayerDisconnected",
		"OnPostRender",
		"OnPreCull",
		"OnPreRender",
		"OnRenderImage",
		"OnRenderObject",
		"OnSerializeNetworkView",
		"OnServerInitialized",
		"OnTransformChildrenChanged",
		"OnTransformParentChanged",
		"OnTriggerEnter",
		"OnTriggerEnter2D",
		"OnTriggerExit",
		"OnTriggerExit2D",
		"OnTriggerStay",
		"OnTriggerStay2D",
		"OnValidate",
		"OnWillRenderObject",
		"Reset",
		"Start",
		"Update",
	};

	[DidReloadScripts]
	static void OnReloadScripts()
	{
		var importedScripts = EditorPrefs.GetString("importedScripts");
		foreach (string script in importedScripts.Split(' '))
		{
			var type = GetTypeByName(script);
			if (type == null)
			{
				// nothing to do
			}
			else if (type.IsSubclassOf(typeof(EditorWindow)))
			{
				CheckMessageSpelling(type, messagesOfEditorWindow);
			}
			else if (type.IsSubclassOf(typeof(MonoBehaviour)))
			{
				CheckMessageSpelling(type, messagesOfMonoBehaviour);
			}
		}
	}

	public static System.Type GetTypeByName(string name)
	{
		foreach (Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
		{
			foreach (System.Type type in assembly.GetTypes())
			{
				if (type.Name == name)
				{
					return type;
				}
			}
		}

		return null;
	}

	static void CheckMessageSpelling(System.Type type, string[] messages)
	{
		foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
		{
			int wdMin = int.MaxValue;
			string messageClosest = "";
			foreach (var message in messages)
			{
				int wd = GetWordDistance(method.Name, message);
				if (wdMin > wd)
				{
					wdMin = wd;
					if (wdMin == 0)
					{
						break;
					}

					messageClosest = message;
				}
			}

			if (wdMin == 1 || wdMin == 2)
			{
				Debug.LogWarningFormat("{0}: Is '<color=yellow>{1}</color>()' misspelling of '<color=lime>{2}</color>()' in class '{3}'?", typeof(MessageSpellingChecker), method.Name, messageClosest, type.Name);
			}
		}
	}

	static int GetWordDistance(string lhs, string rhs)
	{
		int l = lhs.Length;
		int r = rhs.Length;
		var d = new int[l + 1, r + 1];

		for (int i = 0; i <= l; d[i, 0] = i++)
		{
		}

		for (int j = 0; j <= r; d[0, j] = j++)
		{
		}

		for (int i = 1; i <= l; i++)
		{
			for (int j = 1; j <= r; j++)
			{
				int cost = (rhs[j - 1] == lhs[i - 1]) ? 0 : 1;
				d[i, j] = Mathf.Min(Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
			}
		}
		return d[l, r];
	}
}