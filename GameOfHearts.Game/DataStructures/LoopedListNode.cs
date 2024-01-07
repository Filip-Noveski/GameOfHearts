namespace GameOfHearts.Game.DataStructures;

internal sealed class LoopedListNode<T>
{
    public int Id { get; set; }
    public T Item { get; set; }
    public LoopedListNode<T> Next { get; set; }

    /// <summary>
    /// Full parameter constructor, sets the provided id, value and succesor node.
    /// </summary>
    /// <param name="id">The Id of the node.</param>
    /// <param name="item">The value to be stored.</param>
    /// <param name="next">The successor of this node.</param>
    public LoopedListNode(int id, T item, LoopedListNode<T> next)
    {
        Id = id;
        Item = item;
        Next = next;
    }

    /// <summary>
    /// Constructor that sets the id and value, and references itself
    /// as its successor. Should be used when it is the only entry in the list.
    /// </summary>
    /// <param name="id">The id of the node.</param>
    /// <param name="item">The value to be stored.</param>
    public LoopedListNode(int id, T item)
    {
        Id = id;
        Item = item;
        Next = this;
    }
}
