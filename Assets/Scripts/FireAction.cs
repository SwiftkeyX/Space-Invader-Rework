// Delegates fire intent to the weapon each tick.
public class FireAction : BTNode
{
    private readonly PlayerShipContext _ctx;

    public FireAction(PlayerShipContext ctx) => _ctx = ctx;

    public override BTStatus Tick(float dt)
    {
        var input = GameManager.Instance?.Input;
        _ctx.Weapon?.HandleFire(input != null && input.FireHeld, _ctx.Stat, dt);
        return BTStatus.Success;
    }
}
