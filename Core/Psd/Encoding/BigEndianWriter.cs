using System;
using System.IO;

namespace DennokoWorks.Tool.PSDExporter.Core.Psd.Encoding
{
    /// <summary>
    /// ビッグエンディアンでストリームへ書き込む。長さプレフィックスは
    /// <see cref="BeginLength"/> で予約し、Dispose 時に実際の長さをバックパッチする。
    /// </summary>
    public sealed class BigEndianWriter
    {
        private readonly Stream _stream;
        private readonly byte[] _scratch = new byte[8];

        public BigEndianWriter(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            if (!stream.CanWrite) throw new ArgumentException("Stream must be writable.", nameof(stream));
            if (!stream.CanSeek) throw new ArgumentException("Stream must be seekable.", nameof(stream));
        }

        public long Position => _stream.Position;

        public void WriteByte(byte value) => _stream.WriteByte(value);

        public void WriteUInt16(ushort value)
        {
            _scratch[0] = (byte)(value >> 8);
            _scratch[1] = (byte)value;
            _stream.Write(_scratch, 0, 2);
        }

        public void WriteInt16(short value) => WriteUInt16(unchecked((ushort)value));

        public void WriteUInt32(uint value)
        {
            _scratch[0] = (byte)(value >> 24);
            _scratch[1] = (byte)(value >> 16);
            _scratch[2] = (byte)(value >> 8);
            _scratch[3] = (byte)value;
            _stream.Write(_scratch, 0, 4);
        }

        public void WriteInt32(int value) => WriteUInt32(unchecked((uint)value));

        public void WriteBytes(byte[] bytes) => WriteBytes(bytes, 0, bytes.Length);

        public void WriteBytes(byte[] bytes, int offset, int count)
        {
            if (count > 0) _stream.Write(bytes, offset, count);
        }

        public void WriteZeros(int count)
        {
            for (int i = 0; i < count; i++) _stream.WriteByte(0);
        }

        /// <summary>4 文字の ASCII シグネチャ/キー ("8BPS", "8BIM", "norm" など)。</summary>
        public void WriteSignature(string fourCC)
        {
            if (fourCC == null || fourCC.Length != 4)
                throw new ArgumentException("Signature must be 4 characters.", nameof(fourCC));

            for (int i = 0; i < 4; i++) _stream.WriteByte((byte)fourCC[i]);
        }

        /// <summary>
        /// 4 バイトの長さフィールドを予約する。スコープ終了時に、フィールド直後から現在位置までの
        /// バイト数を書き戻す。<paramref name="alignment"/> が 2 以上なら長さがその倍数になるよう
        /// 0 でパディングし、パディング分も長さに含める。
        /// </summary>
        public LengthScope BeginLength(int alignment = 1)
        {
            long lengthPosition = _stream.Position;
            WriteUInt32(0);
            return new LengthScope(this, lengthPosition, alignment);
        }

        private void EndLength(long lengthPosition, int alignment)
        {
            long start = lengthPosition + 4;
            long length = _stream.Position - start;

            if (alignment > 1)
            {
                long remainder = length % alignment;
                if (remainder != 0)
                {
                    int pad = (int)(alignment - remainder);
                    WriteZeros(pad);
                    length += pad;
                }
            }

            if (length > uint.MaxValue) throw new InvalidOperationException("Section is too large for PSD.");

            long end = _stream.Position;
            _stream.Position = lengthPosition;
            WriteUInt32((uint)length);
            _stream.Position = end;
        }

        public readonly struct LengthScope : IDisposable
        {
            private readonly BigEndianWriter _writer;
            private readonly long _lengthPosition;
            private readonly int _alignment;

            internal LengthScope(BigEndianWriter writer, long lengthPosition, int alignment)
            {
                _writer = writer;
                _lengthPosition = lengthPosition;
                _alignment = alignment;
            }

            public void Dispose() => _writer?.EndLength(_lengthPosition, _alignment);
        }
    }
}
