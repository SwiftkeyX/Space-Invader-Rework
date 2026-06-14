using UnityEngine;

// Handles invulnerability blink and timer. When invuln expires, restores full alpha.
// Timer uses unscaledDeltaTime so it survives hit-stop (timeScale = 0).
public class InvulnOverlayAction : BTNode
{
    private readonly PlayerShipContext _ctx;

    public InvulnOverlayAction(PlayerShipContext ctx) => _ctx = ctx;

    public override BTStatus Tick(float dt)
    {
        if (!_ctx.IsInvulnerable)
        {
            SetAlpha(1f);
            return BTStatus.Success;
        }

        _ctx.InvulnTimer -= Time.unscaledDeltaTime;
        if (_ctx.InvulnTimer <= 0f)
        {
            _ctx.IsInvulnerable = false;
            SetAlpha(1f);
            return BTStatus.Success;
        }

        SetAlpha(Mathf.PingPong(Time.unscaledTime * 8f, 1f) * 0.7f + 0.3f);
        return BTStatus.Running;
    }

    private void SetAlpha(float alpha)
    {
        if (_ctx.Sprite != null)
            _ctx.Sprite.color = new Color(1f, 1f, 1f, alpha);
    }
}
