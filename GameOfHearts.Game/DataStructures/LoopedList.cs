using System.Diagnostics;

namespace GameOfHearts.Game.DataStructures;

public sealed class LoopedList<T>
{
    private LoopedListNode<T>? _current;
    
    public int Count { get; private set; }
    public T? Selected { get => _current is not null ? _current.Item : default; }
    public int SelectedId { get => _current is not null ? _current.Id : -1; }

    public LoopedList()
    {
        Count = 0;
        _current = null;
    }

    public void Add(T value)
    {
        if (_current is null)
        {
            // Create initial node with id of the current count (0) and provided value
            // then increment count
            _current = new(Count++, value);
            return;
        }

        // *Current node is not neccessarily the 0-index (0-id) node.
        LoopedListNode<T> last = _current;
        while (last.Next.Id != 0)
        {
            // if successor's id is 0, then this is the last node; finish loop
            if (last.Next.Id == 0)
                break;

            last = last.Next;
        }

        // last node's old successor is new node's successor
        LoopedListNode<T> next = new(Count++, value, last.Next);
        // last node's new successor is the new node
        last.Next = next;
    }

    public void MoveNext()
    {
        if (_current is null)
        { 
            return; 
        }

        _current = _current.Next;
    }

    public void MoveTo(int id)
    {
        if (_current is null)
        {
            return;
        }

        while (_current.Id != id)
        {
            MoveNext();
        }
    }

    public void Remove(int id)
    {
        if (id >= Count || _current is null)
        {
            throw new IndexOutOfRangeException();
        }
        
        LoopedListNode<T> selected = _current;
        int counter = 0; // counter included to check for infinite loops that should not occur
        // find the node that precedes the one to be deleted
        while (true)
        {
            if (selected.Next.Id == id)
                break;

            if (++counter > Count)
                throw new UnreachableException($"The provided id {id} was not found.");

            selected = selected.Next;
        }

        // jump the node to be deleted
        selected.Next = selected.Next.Next;

        // decrement id's on following nodes
        selected = selected.Next;
        while (selected.Id != 0)
        {
            selected.Id--;
            selected = selected.Next;
        }
    }

    public T[] ToArray()
    {
        if (Count == 0 || _current is null)
        {
            return [];
        }

        int initialId = _current.Id;
        T[] array = new T[Count];

        // start at index 0 to add all in order
        MoveTo(0);
        for (int i = 0; i <= Count - 1; i++)
        {
            array[i] = _current.Item;
            MoveNext();
        }
        // return to initial index to not break anything elsewhere
        MoveTo(initialId);

        return array;
    }
}
