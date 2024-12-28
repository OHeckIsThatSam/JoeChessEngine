using Chess_Bot.Core;

namespace JoeChessEngine.Testing;

public class MoveTreeNode
{
    public List<MoveTreeNode> Children = new();
    public MoveTreeNode Parent;

    public Board Position;

    public MoveTreeNode(Board position)
    {
        this.Position = position;
    }

    public void Add(MoveTreeNode node)
    {
        this.Children.Add(node);
        node.Parent = this;
    }

    public int Count() => Children.Count;
}
