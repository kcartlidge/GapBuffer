using System;
using System.Collections.Generic;

namespace GapBuffer
{
    public class GapBuffer<T>
    {
        private const int DefaultCapacity = 10;

        private T[] _buffer;
        private int _gapEndPos;
        private int _gapStartPos;

        /// <summary>
        ///     A collection type implementing a gap buffer for improved
        ///     performance with localised updates.
        /// </summary>
        public GapBuffer()
        {
            _buffer = new T[DefaultCapacity];
            _gapEndPos = _buffer.Length;
        }

        /// <summary>Amount of physical space currently allocated.</summary>
        public int Capacity => _buffer.Length;

        /// <summary>Current size of the collection.</summary>
        public int Count => _buffer.Length - GapSize;

        /// <summary>The size of the movable gap.</summary>
        private int GapSize => _gapEndPos - _gapStartPos;

        /// <summary>The size of the movable gap.</summary>
        public int Length => Count;

        /// <summary>Get/set the entry at the specified position.</summary>
        public T this[int index]
        {
            get
            {
                BoundsCheck(index);
                return index >= _gapStartPos ? _buffer[index + GapSize] : _buffer[index];
            }
            set
            {
                BoundsCheck(index);
                if (index >= _gapStartPos) _buffer[index + GapSize] = value;
                else _buffer[index] = value;
            }
        }

        /// <summary>Add an item to the end of the collection.</summary>
        public void Add(T item)
        {
            Insert(Count, item);
        }

        /// <summary>Adds multiple items to the end of the collection.</summary>
        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items) Add(item);
        }

        /// <summary>Inserts an item into the collection.</summary>
        public void Insert(int index, T item)
        {
            if (index < 0 || index > Count) return;

            MoveGap(index);
            ResizeGap(1);

            _buffer[index] = item;
            _gapStartPos++;
        }

        /// <summary>Inserts multiple items into the collection.</summary>
        public void InsertRange(int index, IEnumerable<T> items)
        {
            var i = index;
            foreach (var item in items) Insert(i++, item);
        }

        /// <summary>Removes the item at the specified position.</summary>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count) return;

            MoveGap(index);
            _buffer[_gapEndPos] = default!; // Allow garbage collection.
            _gapEndPos++;
        }

        /// <summary>Removes multiple items at the specified position.</summary>
        public void RemoveRange(int index, int length)
        {
            if (length < 1) return;
            var idx = index + length - 1;
            for (var i = 0; i < length; i++) RemoveAt(idx--);
        }

        /// <summary>Clears the collection.</summary>
        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _gapStartPos = 0;
            _gapEndPos = _buffer.Length;
        }

        /// <summary>Sets the size the underlying storage.</summary>
        public void SetCapacity(int requestedCapacity)
        {
            if (requestedCapacity == _buffer.Length) return;
            if (requestedCapacity < Count) throw new BufferCapacityException(requestedCapacity, Count);
            if (requestedCapacity > 0)
            {
                var newBuffer = new T[requestedCapacity];
                var newGapEnd = newBuffer.Length - (_buffer.Length - _gapEndPos);

                Array.Copy(_buffer, 0, newBuffer, 0, _gapStartPos);
                Array.Copy(_buffer, _gapEndPos, newBuffer, newGapEnd, newBuffer.Length - newGapEnd);
                _buffer = newBuffer;
                _gapEndPos = newGapEnd;
            }
            else
            {
                _buffer = new T[DefaultCapacity];
                _gapStartPos = 0;
                _gapEndPos = _buffer.Length;
            }
        }

        /// <summary>Returns the position of the item, or -1 if not found.</summary>
        public int IndexOf(T item)
        {
            // Search before the gap.
            var foundAt = Array.IndexOf(_buffer, item, 0, _gapStartPos);
            if (foundAt > -1) return foundAt;

            // Still here? Search after the gap.
            foundAt = Array.IndexOf(_buffer, item, _gapEndPos, _buffer.Length - _gapEndPos);
            if (foundAt > -1) return foundAt - GapSize;

            // Still here? Then there's no match anywhere.
            return -1;
        }

        /// <summary>Repositions the gap in the buffer.</summary>
        private void MoveGap(int index)
        {
            if (index == _gapStartPos) return; // Doesn't need to move.
            if (GapSize == 0)
            {
                // Zero size gap.
                _gapStartPos = _gapEndPos = index;
                return;
            }

            if (index < _gapStartPos)
            {
                // Move the gap backwards.
                var offset = _gapStartPos - index;
                var sizeDiff = GapSize < offset ? GapSize : offset;
                Array.Copy(_buffer, index, _buffer, _gapEndPos - offset, offset);
                _gapStartPos -= offset;
                _gapEndPos -= offset;

                // Allow garbage collection.
                Array.Clear(_buffer, index, sizeDiff);
            }
            else
            {
                // Move the gap forwards.
                var count = index - _gapStartPos;
                var deltaIndex = index > _gapEndPos ? index : _gapEndPos;
                Array.Copy(_buffer, _gapEndPos,
                    _buffer, _gapStartPos, count);
                _gapStartPos += count;
                _gapEndPos += count;

                // Allow garbage collection.
                Array.Clear(_buffer, deltaIndex, _gapEndPos - deltaIndex);
            }
        }

        /// <summary>Resizes the gap in the buffer.</summary>
        private void ResizeGap(int requiredGapSize)
        {
            if (requiredGapSize <= GapSize) return;

            var newCapacity = (Count + requiredGapSize) * 2;
            if (newCapacity < DefaultCapacity) newCapacity = DefaultCapacity;
            SetCapacity(newCapacity);
        }

        /// <summary>Throws BufferAccessException if the index is out of bounds.</summary>
        private void BoundsCheck(int index)
        {
            if (index < 0 || index >= Count) throw new BufferAccessException(index, Count);
        }
    }
}