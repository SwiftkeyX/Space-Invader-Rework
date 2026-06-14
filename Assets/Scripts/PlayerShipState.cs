// Abstract base for all PlayerShip behavioural states.
// Concrete states (ActivePlayerState, InvulnPlayerState) inherit and implement Tick().
public abstract class BasePlayerShipState
{
    protected PlayerShipContext Ctx { get; }
    protected BasePlayerShipState(PlayerShipContext ctx) => Ctx = ctx;
    public virtual void OnEnter() { }
    public abstract void Tick(float dt);
}
