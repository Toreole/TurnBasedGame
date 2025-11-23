using System.Collections.Generic;

public class CircularLinkedList<T>
{
    private Link<T> _start = null;

    private Link<T> _current = null;

    public bool IsEmpty => _start == null;

    public T GoNext()
    {
        _current = _current.Next;
        return _current.Item;
    }

    public class Link<T>
    {
        public T Item { get; private set; }

        public Link<T> Next { get; private set; }

        private Link(T item) {
            Item = item; 
        }

        public void InsertAfter(T item)
        {
            var other = new Link<T>(item);
            other.Next = this.Next;
            this.Next = other;
        }

        // InsertBefore funktioniert so nicht.

        public void Remove()
        {
            
        }
    }
}

