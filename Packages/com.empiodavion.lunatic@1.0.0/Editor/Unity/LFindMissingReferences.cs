using UnityEditor;
using UnityEngine;

public class LFindMissingReferences
{
	[MenuItem("Assets/Find Missing References")]
	public static void Find()
	{
		GameObject[] gameObjects = Object.FindObjectsOfType<GameObject>(true);

		foreach (GameObject gameObject in gameObjects)
		{
			Component[] components = gameObject.GetComponents<Component>();

			foreach (Component com in components)
			{
				if (com == null)
					Debug.LogError("Missing script on " + GetPath(gameObject), gameObject);
				else
				{
					SerializedObject so = new SerializedObject(com);
					SerializedProperty sp = so.GetIterator();

					while (sp.NextVisible(true))
					{
						if (sp.propertyType == SerializedPropertyType.ObjectReference &&
							sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
						{
							Debug.LogError($"Missing script on {GetPath(gameObject)}.{sp.propertyPath}", gameObject);
						}
					}
				}
			}
		}
	}

	private static string GetPath(GameObject go)
	{
		Transform tr = go.transform;

		System.Text.StringBuilder path = new System.Text.StringBuilder(go.name);

		tr = tr.parent;

		while (tr != null)
		{
			path.Insert(0, $"{tr.name}/");

			tr = tr.parent;
		}

		return path.ToString();
	}
}
