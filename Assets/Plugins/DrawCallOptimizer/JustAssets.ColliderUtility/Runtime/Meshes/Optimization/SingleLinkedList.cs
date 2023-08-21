using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace JustAssets.ColliderUtilityRuntime.Optimization
{
    [DebuggerDisplay("Count {Count}")]
    public class SingleLinkedList<T> : IReadOnlyCollection<T>
    {

        [DebuggerDisplay("{Value} -> {Next.Value}")]
        public class LinkNode<TNode>
        {
            public TNode Value { get; }
            public LinkNode<TNode> Next { get; set; }

            public LinkNode(TNode data)
            {
                Value = data;
                Next = null;
            }
        }

        public LinkNode<T> First { get; set; }
        public LinkNode<T> Last { get; set; }

        public SingleLinkedList(List<T> initialEntries = null)
        {
            First = null;
            Last = null;

            if (initialEntries != null)
                foreach (var initialEntry in initialEntries)
                {
                    AddAtEnd(initialEntry);
                }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            if (First == null)
            {
                builder.Append("Empty linked list");
                return builder.ToString();
            }

            var temp = First;
            //iterating linked list elements
            while (temp != null)
            {
                builder.Append($" {temp.Value} ->");
                // Visit to next node
                temp = temp.Next;
            }

            builder.Append(" NULL");

            return builder.ToString();
        }

        // Add new node at end of linked list 
        public void AddAtEnd(T data)
        {
            var node = new LinkNode<T>(data);
            if (First == null)
            {
                First = node;
            }
            else
            {
                // Append the node at last position
                Last.Next = node;
            }

            Last = node;
        }

        // Display linked list element

        public IEnumerator<T> GetEnumerator()
        {
            var current = First;

            while (current != null)
            {
                yield return current.Value;
                current = current.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => this.Count();

        public LinkNode<T> Find(T searchItem, out LinkNode<T> previous)
        {
            previous = null;
            var current = First;

            while (current != null)
            {
                if (Equals(current.Value, searchItem))
                    return current;

                previous = current;
                current = current.Next;
            }

            return null;
        }

        public void Clear()
        {
            First = null;
            Last = null;
        }

        public void Remove(T itemToRemove)
        {
            LinkNode<T> previous = null;
            var current = First;
            int count = -1;
            while (current != null)
            {
                count++;
                if (Equals(current.Value, itemToRemove))
                {
                    if (previous == null)
                        First = current.Next;
                    else
                        previous.Next = current.Next;
                    
                    if (current.Next == null)
                        Last = previous;
                }

                previous = current;
                current = current.Next;

                if (count > 10000)
                    throw new Exception();
            }
        }

        public void Remove(LinkNode<T> itemToRemove)
        {
            Remove(itemToRemove.Value);
        }

        public void AddAfter(LinkNode<T> nodeBase, LinkNode<T> toInsert)
        {
            toInsert.Next = null;

            //var oldPrevious = nodeBase.Previous;
            var oldNext = nodeBase.Next;

            nodeBase.Next = toInsert;
            if (oldNext != null)
            {
            }
            else
                Last = toInsert;
        }

        public void InsertAt(LinkNode<T> insertAt, [NotNull] LinkNode<T> from, [NotNull] LinkNode<T> to)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (to == null)
                throw new ArgumentNullException(nameof(to));

            var oldNext = insertAt == null ? First : insertAt.Next;

            if (insertAt != null) 
                insertAt.Next = from;
            else
                First = from;

            to.Next = oldNext;
            
            if (oldNext != null)
            {
            }
            else
                Last = to;
        }

        public void DeleteIf(Func<T, bool> isToBeDeleted)
        {
            var current = First;
            LinkNode<T> previous = null;
            while (current != null)
            {
                if (isToBeDeleted(current.Value))
                {
                    if (previous != null)
                        previous.Next = current.Next;
                    else
                        First = current.Next;

                    current = current.Next;
                    continue;
                }

                previous = current;
                current = current?.Next;
            }
        }
    }
}