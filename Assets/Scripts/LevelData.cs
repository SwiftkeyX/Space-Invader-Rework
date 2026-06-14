using UnityEngine;

/// <summary>
/// Per-level tuned values for the 6-level curve (D5). Authored as ScriptableObject
/// assets — no level values are hardcoded. Source: game-vision difficulty table.
/// See LevelManager.md GDD.
/// </summary>
[CreateAssetMenu(menuName = "SpaceInvader/Level Data", fileName = "LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Formation")]
    public int rows = 4;
    public int columns = 8;
    public float horizontalSpacing = 1.2f;
    public float verticalSpacing = 1.0f;

    [Tooltip("HP per row, front row (index 0) first. If fewer entries than rows, the last value repeats.")]
    public int[] rowHealth = { 1 };

    public int pointsPerEnemy = 100;

    [Header("March")]
    public float baseMarchSpeed = 1.5f;
    [Tooltip("March speed when ~one enemy remains (formation speeds up as it thins).")]
    public float maxMarchSpeed = 4f;
    public float stepDownDistance = 0.5f;

    [Header("Fire")]
    public int activeShooters = 1;
    public float fireInterval = 1.5f;
    public float enemyBulletSpeed = 5f;
    public int enemyBulletDamage = 1;
    public float enemyBulletLifetime = 5f;
    public float enemyMuzzleOffset = 0.5f;

    /// <summary>HP for a given row index (0 = front), clamped to the authored array.</summary>
    public int HealthForRow(int row)
    {
        if (rowHealth == null || rowHealth.Length == 0) return 1;
        return rowHealth[Mathf.Clamp(row, 0, rowHealth.Length - 1)];
    }
}
