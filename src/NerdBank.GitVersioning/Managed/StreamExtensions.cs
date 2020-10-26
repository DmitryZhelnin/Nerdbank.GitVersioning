﻿using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;

namespace NerdBank.GitVersioning.Managed
{
    /// <summary>
    /// Provides extension methods for the <see cref="Stream"/> class.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Reads data from a <see cref="Stream"/>, and asserts the amount of data read equals the size of the buffer.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream"/> from which to read data.
        /// </param>
        /// <param name="buffer">
        /// A buffer into which to store the data read.
        /// </param>
        public static void ReadAll(this Stream stream, Span<byte> buffer)
        {
            int read = stream.Read(buffer);
            Debug.Assert(read == buffer.Length);
        }

        /// <summary>
        /// Reads an variable-length integer off a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        /// The stream off which to read the variable-length integer.
        /// </param>
        /// <returns>
        /// The requested value.
        /// </returns>
        public static int ReadMbsInt(this Stream stream)
        {
            int value = 0;
            int currentBit = 0;
            int read;

            while (true)
            {
                read = stream.ReadByte();
                value |= (read & 0b_0111_1111) << currentBit;
                currentBit += 7;

                if (read < 128)
                {
                    break;
                }
            }

            return value;
        }

#if NETSTANDARD
        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by
        /// the number of bytes read.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream"/> from which to read the data.
        /// </param>
        /// <param name="span">
        /// A region of memory. When this method returns, the contents of this region are replaced by the bytes
        /// read from the current source.
        /// </param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes allocated
        /// in the buffer if that many bytes are not currently available, or zero (0) if the end of the stream
        /// has been reached.
        /// </returns>
        public static int Read(this Stream stream, Span<byte> span)
        {
            byte[] buffer = null;

            try
            {
                buffer = ArrayPool<byte>.Shared.Rent(span.Length);
                int read = stream.Read(buffer, 0, span.Length);

                buffer.AsSpan(0, read).CopyTo(span);
                return read;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream
        /// by the number of bytes written.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream"/> to which to write the data.
        /// </param>
        /// <param name="span">
        /// A region of memory. This method copies the contents of this region to the current stream.
        /// </param>
        public static void Write(this Stream stream, Span<byte> span)
        {
            byte[] buffer = null;

            try
            {
                buffer = ArrayPool<byte>.Shared.Rent(span.Length);
                span.CopyTo(buffer.AsSpan(0, span.Length));

                stream.Write(buffer, 0, span.Length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
#endif
    }
}
