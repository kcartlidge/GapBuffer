using System;

namespace GapBuffer
{
    public class BufferCapacityException : Exception
    {
        private readonly int _requested;
        private readonly int _count;

        public BufferCapacityException(int requested, int count)
        {
            _requested = requested;
            _count = count;
        }

        public override string Message => $"Buffer capacity of {_requested} was requested yet content has a length of {_count}.";
    }
}