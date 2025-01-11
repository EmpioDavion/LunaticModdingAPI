public class BaseMaterial : UnityEngine.ScriptableObject
{
	[UnityEngine.HideInInspector]
	public int count = 1;

	protected static T Convert<T>(string materialName) where T : BaseMaterial
	{
		if (string.IsNullOrEmpty(materialName))
			return null;

		T inst = CreateInstance<T>();

		if (Lunatic.EndsWithNumbers(materialName, 2))
		{
			inst.count = int.Parse(materialName.Substring(materialName.Length - 2));
			inst.name = materialName.Substring(0, materialName.Length - 2);
		}
		else if (Lunatic.EndsWithNumbers(materialName, 1))
		{
			inst.count = int.Parse(materialName.Substring(materialName.Length - 1));
			inst.name = materialName.Substring(0, materialName.Length - 1);
		}
		else
			inst.name = materialName;

		return inst;
	}

	public static explicit operator BaseMaterial(string materialName) =>
		Convert<BaseMaterial>(materialName);
}
