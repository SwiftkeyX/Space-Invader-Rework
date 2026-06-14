using UnityEngine;

/// <summary>
/// Thin BT coordinator for the player ship. Owns the behavior tree root;
/// Update() is a guard + _btRoot.Tick(dt) only. PlayerShipStat handles
/// IDamageable, numeric stats, and invuln state. See PlayerShip.md GDD.
/// </summary>
[RequireComponent(typeof(PlayerShipStat))]
public class PlayerShipContext : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Weapon weapon;
    [SerializeField] private PowerUpSystem powerUpSystem;

    public PlayerShipStat Stat   { get; private set; }
    public Weapon         Weapon => weapon;
    public SpriteRenderer Sprite { get; private set; }

    private BTNode _btRoot;

    private void Awake()
    {
        Stat   = GetComponent<PlayerShipStat>();
        Sprite = GetComponentInChildren<SpriteRenderer>();
        _btRoot = new BTParallel(
            new MoveAction(this),
            new FireAction(this),
            new InvulnOverlayAction(this));
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            Stat.OnPlayerDeath += GameManager.Instance.HandlePlayerDeath;
        if (powerUpSystem != null)
            powerUpSystem.OnPowerUpChosen += ApplyPowerUp;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            Stat.OnPlayerDeath -= GameManager.Instance.HandlePlayerDeath;
        if (powerUpSystem != null)
            powerUpSystem.OnPowerUpChosen -= ApplyPowerUp;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Running) return;
        _btRoot.Tick(Time.deltaTime);
    }

    private void ApplyPowerUp(PowerUpData data)
    {
        switch (data.effect)
        {
            case PowerUpEffect.ExtraLife:   GameManager.Instance?.AddLife(1); break;
            case PowerUpEffect.RapidFire:   Stat.ModifyFireCooldown(data.magnitude); break;
            case PowerUpEffect.MultiShot:   Stat.ModifyMultiShot((int)data.magnitude); break;
            case PowerUpEffect.PowerShot:   Stat.ModifyProjectileDamage((int)data.magnitude); break;
            case PowerUpEffect.Swift:       Stat.ModifyMoveSpeed(data.magnitude); break;
            case PowerUpEffect.BulletSpeed: Stat.ModifyProjectileSpeed(data.magnitude); break;
        }
        weapon?.ApplyPowerUp(data);
        Debug.Log($"[PlayerShipContext] Applied {data.displayName} — effect={data.effect} mag={data.magnitude}");
    }
}
