using UnityEngine;

// Post-hit invulnerability: moves, fires, blinks sprite, counts down on unscaled time.
// Transitions back to NormalState when the timer expires.
public class InvulnPlayerState : BasePlayerShipState
{
    private float _timer;
    private readonly float _duration;

    public InvulnPlayerState(PlayerShipContext ctx, float duration) : base(ctx)
    {
        _duration = duration;
    }

    public override void OnEnter() => _timer = _duration;

    public override void Tick(float dt)
    {
        _timer -= Time.unscaledDeltaTime;
        var input = GameManager.Instance.Input;
        Ctx.PerformMove(input, dt);
        Ctx.Weapon?.HandleFire(input != null && input.FireHeld, Ctx.Stat, dt);
        Ctx.SetSpriteAlpha(UnityEngine.Mathf.PingPong(Time.unscaledTime * 8f, 1f) * 0.7f + 0.3f);
        if (_timer <= 0f)
            Ctx.TransitionTo(Ctx.NormalState);
    }
}
