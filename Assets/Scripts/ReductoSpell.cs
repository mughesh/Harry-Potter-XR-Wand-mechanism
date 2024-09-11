using UnityEngine;

public class ReductoSpell : BaseSpell
{
    public float projectileSpeed = 10f;
    public float maxDistance = 100f;

    public override void OnSelect(Transform spawnPoint)
    {
        SpawnEquipVFX(spawnPoint);
    }

    public override void OnDeselect()
    {
        RemoveEquipVFX();
    }

    public override void OnCast(Transform spawnPoint)
    {
        RemoveEquipVFX();
        GameObject projectile = Instantiate(castVFX, spawnPoint.position, spawnPoint.rotation);
        SpellProjectile spellProjectile = projectile.AddComponent<SpellProjectile>();
        spellProjectile.Initialize(spawnPoint, projectileSpeed, maxDistance);
        spellProjectile.OnProjectileHit += OnHit;
    }

    public override void OnRelease()
    {
        // Implement any release behavior if needed
    }

    public override void OnHit(RaycastHit hit)
    {
        SpawnVFX(hitVFX, hit.point, Quaternion.LookRotation(hit.normal));
        // Implement any additional hit effects (e.g., setting objects on fire)
    }
}