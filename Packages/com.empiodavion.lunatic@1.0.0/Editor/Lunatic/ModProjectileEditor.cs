using UnityEditor;

[CustomEditor(typeof(ModProjectile), true)]
public class ModProjectileEditor : ModBaseEditor
{
	protected SerializedProperty pl;
	protected SerializedProperty speedIncrease;
	protected SerializedProperty force;

	public override SerializedProperty LastProperty => force;

	private void OnEnable()
	{
		pl = serializedObject.FindProperty("Pl");
		speedIncrease = serializedObject.FindProperty("speed_increase");
		force = serializedObject.FindProperty("force");
	}

	public override void DrawGUI()
	{
		EditorTools.DrawHelpProperty(pl, "If the projectile is launched on spawn.");

		UnityEngine.GUI.enabled = pl.boolValue;

		EditorTools.DrawHelpProperty(speedIncrease, "Multiplies the projectile velocity by this amount every 0.1 seconds if launched.");
		EditorTools.DrawHelpProperty(force, "The initial launch force on spawn.");

		UnityEngine.GUI.enabled = true;
	}
}
