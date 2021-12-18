using System;

namespace GapBuffer
{
    public class BufferAccessException : Exception
    {
        private readonly int _position;
        private readonly string _size;

        public BufferAccessException(int position, int count)
        {
            _position = position;
            _size = count <= 1 ? $"size {count}" : $"0..{count - 1}";
        }

        public override string Message => $"Buffer access attempt at position {_position} in a buffer of {_size}.";
    }
}