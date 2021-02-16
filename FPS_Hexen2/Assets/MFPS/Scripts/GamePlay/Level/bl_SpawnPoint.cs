using UnityEngine;
#if UNITY_EDITOR
using MFPSEditor;
#endif

public class bl_SpawnPoint : MonoBehaviour {

    public Team m_Team = Team.All;
    public float SpawnSpace = 3f;

    public static implicit operator Transform(bl_SpawnPoint d) => d.transform;

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        bl_SpawnPointManager.Instance.AddSpawnPoint(this);
    }

    /// <summary>
    /// 
    /// </summary>
    public void GetSpawnPosition(out Vector3 position, out Quaternion Rotation)
    {
        Vector3 s = Random.insideUnitSphere * SpawnSpace;
        position = transform.position + new Vector3(s.x, 0.55f, s.z);
        Rotation = transform.rotation;
    }

#if UNITY_EDITOR
    DomeGizmo _gizmo = null;
    void OnDrawGizmosSelected()
    {
        Draw();
    }

    private void OnDrawGizmos()
    {
        if (bl_SpawnPointManager.Instance == null || !bl_SpawnPointManager.Instance.drawSpawnPoints) return;
        Draw();
    }

    void Draw()
    {
        if (bl_SpawnPointManager.Instance == null) return;

        float h = 180;
        if (_gizmo == null || _gizmo.horizon != h)
        {
            _gizmo = new DomeGizmo(h);
        }

        Color c = (m_Team == Team.Team2) ? bl_GameData.Instance.Team2Color : bl_GameData.Instance.Team1Color;
        if (m_Team == Team.All) { c = Color.white; }
        Gizmos.color = c;
        _gizmo.Draw(transform, c, SpawnSpace);
        if (bl_SpawnPointManager.Instance.SpawnPointPlayerGizmo != null)
        {
            Gizmos.DrawWireMesh(bl_SpawnPointManager.Instance.SpawnPointPlayerGizmo, transform.position, transform.rotation, Vector3.one * 2.75f);
        }
        Gizmos.DrawLine(base.transform.position + ((base.transform.forward * this.SpawnSpace)), base.transform.position + (((base.transform.forward * 2f) * this.SpawnSpace)));
    }
#endif
}
