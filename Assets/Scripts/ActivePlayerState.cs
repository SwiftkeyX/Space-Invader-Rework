// Normal gameplay: reads input, moves, fires. Resets sprite to full alpha on enter.
public class ActivePlayerState : BasePlayerShipState
{
    public ActivePlayerState(PlayerShipContext ctx) : base(ctx) { }

    public override void OnEnter() => Ctx.SetSpriteAlpha(1f);

    public override void Tick(float dt)
    {
        var input = GameManager.Instance.Input;
        Ctx.PerformMove(input, dt);
        Ctx.Weapon?.HandleFire(input != null && input.FireHeld, Ctx.Stat, dt);
    }
}
