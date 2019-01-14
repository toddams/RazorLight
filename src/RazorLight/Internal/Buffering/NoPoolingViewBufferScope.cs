using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace RazorLight.Internal
{
    /// <summary>
    /// A <see cref="IViewBufferScope"/> that uses pooled memory.
    /// </summary>
    public class NoPoolingViewBufferScope : IViewBufferScope, IDisposable
    {
        public static readonly int MinimumSize = 16;
        private List<ViewBufferValue[]> _allBuffers;

        public NoPoolingViewBufferScope()
        {
            _allBuffers = new List<ViewBufferValue[]>(); //8 by default
        }

        /// <summary>
        /// Fake one to do not change implementation of other classes
        /// </summary>
        public NoPoolingViewBufferScope(params object [] placeholder)
        {
            _allBuffers = new List<ViewBufferValue[]>(); //8 by default
        }

        /// <inheritdoc />
        public ViewBufferValue[] GetPage(int pageSize)
        {
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            ViewBufferValue [] segment = new ViewBufferValue[Math.Max(pageSize, MinimumSize)];
            _allBuffers.Add(segment);
            return segment;
        }

        /// <inheritdoc />
        public void ReturnSegment(ViewBufferValue[] segment)
        {
            //not doing anything. This type is passed by reference and clearing it here affect rendeing process
        }

        /// <inheritdoc />
        public PagedBufferedTextWriter CreateWriter(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            return new PagedBufferedTextWriter(ArrayPool<char>.Create(), writer);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _allBuffers.Clear();
        }
    }
}
