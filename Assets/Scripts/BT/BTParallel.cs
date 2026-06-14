// Composite: ticks every child every frame. Always returns Running.
public class BTParallel : BTNode
{
    private readonly BTNode[] _children;

    public BTParallel(params BTNode[] children) => _children = children;

    public override BTStatus Tick(float dt)
    {
        foreach (var child in _children)
            child.Tick(dt);
        return BTStatus.Running;
    }
}
