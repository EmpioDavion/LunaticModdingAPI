using UnityEngine;

public class ModDamageTrigger : Damage_Trigger
{
	// "constant" refers to if the hit player/object has stayed
	// inside the collision box of the projectile (example: standing in fire)

	public string spawnOnHit;
	public bool spawnOnConstant;

	private void SpawnHit(Transform hit, bool constant)
	{
		if ((!constant || spawnOnConstant) && !string.IsNullOrEmpty(spawnOnHit))
		{
			GameObject go = Resources.Load<GameObject>(spawnOnHit);

			if (go != null)
			{
				Transform tr = transform;
				Transform parent = arrow ? hit : null;

				go = Instantiate(go, tr.position, tr.rotation, parent);

				Lunatic.RemoveCloneSuffix(go);
			}
			else
				Debug.LogWarning($"Failed to spawn {spawnOnHit} on hit by {name}");
		}
	}

	public void HitPlayer(Player_Control_scr player, bool constant)
	{
		SpawnHit(player.transform, constant);
		OnHitPlayer(player, constant);
	}

	public void HitObject(OBJ_HEALTH obj, bool constant)
	{
		SpawnHit(obj.transform, constant);
		OnHitObject(obj, constant);
	}

	public void HitPhysical(Transform obj)
	{
		OnHitPhysical(obj);
	}	

	// the projectile hit a player
	protected virtual void OnHitPlayer(Player_Control_scr player, bool constant) { }

	// the projectile hit an enemy or breakable object (example: barrel)
    protected virtual void OnHitObject(OBJ_HEALTH obj, bool constant) { }

	// the projectile hit a non-water object and physical is set to true
	// meaning the projectile will be destroyed
    protected virtual void OnHitPhysical(Transform obj) { }
}
