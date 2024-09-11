using UnityEngine;

public abstract class BaseSpell : MonoBehaviour
{
    public string spellName;
    public string description;
    public GameObject equipVFX;
    public GameObject castVFX;
    public GameObject hitVFX;
    public Material spellMaterial;

    protected GameObject activeEquipVFX;

    public abstract void OnSelect(Transform spawnPoint);
    public abstract void OnDeselect();
    public abstract void OnCast(Transform spawnPoint);
    public abstract void OnRelease();
    public abstract void OnHit(RaycastHit hit);

    protected void SpawnEquipVFX(Transform spawnPoint)
    {
        if (equipVFX != null)
        {
            activeEquipVFX = Instantiate(equipVFX, spawnPoint.position, spawnPoint.rotation, spawnPoint);
        }
    }

    protected void RemoveEquipVFX()
    {
        if (activeEquipVFX != null)
        {
            Destroy(activeEquipVFX);
            activeEquipVFX = null;
        }
    }

    protected void SpawnVFX(GameObject vfxPrefab, Vector3 position, Quaternion rotation)
    {
        if (vfxPrefab != null)
        {
            Instantiate(vfxPrefab, position, rotation);
        }
    }
}