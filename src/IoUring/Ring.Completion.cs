using System.Runtime.CompilerServices;

namespace IoUring
{
    public sealed partial class Ring
    {
        private readonly RingCompletions? _ringCompletions;

        /// <summary>
        /// Provides access to an allocation-free enumerator over the currently available Completion Queue Events.
        /// </summary>
        public RingCompletions Completions
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ringCompletions!;
        }

        /// <summary>
        /// Checks whether a Completion Queue Event is available.
        /// </summary>
        /// <param name="result">The data from the observed Completion Queue Event if any</param>
        /// <returns>Whether a Completion Queue Event was observed</returns>
        /// <exception cref="ErrnoException">If a syscall failed</exception>
        /// <exception cref="CompletionQueueOverflowException">If an overflow in the Completion Queue occurred</exception>
        public bool TryRead(out Completion result)
        {
            if (_cq.TryRead(out result))
            {
                DecrementOperationsInFlight();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reads, blocking if required, a Completion Queue Event.
        /// </summary>
        /// <returns>The read Completion Queue Event</returns>
        /// <exception cref="ErrnoException">If a syscall failed</exception>
        /// <exception cref="CompletionQueueOverflowException">If an overflow in the Completion Queue occurred</exception>
        public Completion Read()
        {
            var completion = _cq.Read(_ringFd.DangerousGetHandle().ToInt32());
            DecrementOperationsInFlight();
            return completion;
        }

        internal bool TryReadEnumerator(out Completion result)
        {
            if (_cq.TryReadNoUpdate(out result))
            {
                DecrementOperationsInFlight();
                return true;
            }

            return false;
        }

        internal void UpdateReadHeadPostEnumeration() => _cq.UpdateHead();
    }
}