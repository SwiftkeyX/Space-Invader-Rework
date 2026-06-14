// Runtime conditions for the player ship: invulnerability window tracking.
// Distinct from PlayerShipStat, which holds numeric values (speed, damage, etc.).
public class PlayerShipState
{
    private float _invulnTimer;
    private readonly float _invulnDuration;

    public bool IsInvulnerable { get; private set; }

    public PlayerShipState(float invulnDuration)
    {
        _invulnDuration = invulnDuration;
    }

    /// <summary>Tick the i-frame timer. Must be called with unscaled delta time to survive timeScale = 0 (hit-stop).</summary>
    public void Tick(float unscaledDt)
    {
        if (!IsInvulnerable) return;
        _invulnTimer -= unscaledDt;
        if (_invulnTimer <= 0f)
            IsInvulnerable = false;
    }

    /// <summary>Returns true if the hit was blocked by i-frames (no life cost). Starts i-frames on an unblocked hit.</summary>
    public bool TakeHit()
    {
        if (IsInvulnerable) return true;
        IsInvulnerable = true;
        _invulnTimer = _invulnDuration;
        return false;
    }
}
