using UnityEngine;

// Reads horizontal input each tick and moves the ship within its stat bounds.
public class MoveAction : BTNode
{
    private readonly PlayerShipContext _ctx;

    public MoveAction(PlayerShipContext ctx) => _ctx = ctx;

    public override BTStatus Tick(float dt)
    {
        var input = GameManager.Instance?.Input;
        float axis = input != null ? input.MoveAxis : 0f;
        var pos = _ctx.transform.position;
        pos.x = Mathf.Clamp(pos.x + axis * _ctx.Stat.MoveSpeed * dt, _ctx.Stat.LeftBound, _ctx.Stat.RightBound);
        _ctx.transform.position = pos;
        return BTStatus.Success;
    }
}
