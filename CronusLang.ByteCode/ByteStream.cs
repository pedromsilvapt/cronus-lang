using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.ByteCode
{
    /// <summary>
    /// TODO Transform this into a list of lists, where each sub-list is preallocated memory that grows exponentially
    ///      This way, we avoid inserting one element causing the whole byte stream having to be fully copied
    /// TODO Maybe make this a sub class of IO? Or acept an underlying IO object instead of a List of bytes?
    /// TODO Use an optional internal static byte buffer to pool byte arrays instead of allocating them on the heap
    ///      enable it for the stack, and "reset" it before each opcode instruction
    /// </summary>
    public class ByteStream
    {
        protected List<byte> _bytecode;

        public int _cursor = 0;

        public IReadOnlyList<byte> Buffer => _bytecode;

        /// <summary>
        /// The current Cursor position of this Byte Stream
        /// </summary>
        public int Cursor { get => _cursor; set => _cursor = value; }

        /// <summary>
        /// Total number of bytes held by this byte stream
        /// </summary>
        public int Count => _bytecode.Count;

        /// <summary>
        /// Indicates if there are no more bytes to be read from this byte stream
        /// </summary>
        public bool EOF => Cursor >= Count;

        /// <summary>
        /// Create an byte stream starting with an existing list
        /// </summary>
        /// <param name="bytecode"></param>
        public ByteStream(List<byte> bytecode)
        {
            _bytecode = bytecode;
        }

        /// <summary>
        /// Create an byte stream from scratch
        /// </summary>
        public ByteStream()
        {
            _bytecode = new List<byte>();
        }

        public void Compact()
        {
            if (_bytecode.Count > Cursor)
            {
                _bytecode.RemoveRange(Cursor, _bytecode.Count - Cursor);
            }
            _bytecode.TrimExcess();
        }

        #region Write

        public void Write(params byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                if (_cursor + i < _bytecode.Count)
                {
                    _bytecode[_cursor + i] = bytes[i];
                }
                else
                {
                    _bytecode.Add(bytes[i]);
                }
            }

            _cursor += bytes.Length;
        }

        public void Write(OpCode code)
        {
            Write((byte)code);
        }

        public void Write(int number)
        {
            Write(BitConverter.GetBytes(number));
        }
        
        public void Write(bool boolean)
        {
            Write(boolean ? 1 : 0);
        }

        public void Write(uint number)
        {
            Write(BitConverter.GetBytes(number));
        }

        public void Write(string? str)
        {
            if (str == null)
            {
                Write(0);
            }
            else
            {
                var bytes = Encoding.UTF8.GetBytes(str);
                Write(bytes.Length);

                if (bytes.Length > 0)
                {
                    Write(bytes);
                }
            }
        }

        #endregion

        #region Read

        public byte Read()
        {
            byte value = _bytecode[_cursor];
            _cursor += 1;
            return value;
        }

        public byte[] Read(int size)
        {
            byte[] result = new byte[size];
            Read(result);
            return result;
        }

        public int Read(Span<byte> buffer)
        {
            int length = Math.Min(buffer.Length, _bytecode.Count - _cursor);
            for (int i = 0; i < length; i++)
            {
                buffer[i] = _bytecode[_cursor + i];
            }
            _cursor += length;
            return length;
        }

        public OpCode ReadOpCode()
        {
            return (OpCode)Read();
        }

        public bool ReadBool()
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            Read(buffer);
            return BitConverter.ToInt32(buffer) != 0;
        }

        public int ReadInt()
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            Read(buffer);
            return BitConverter.ToInt32(buffer);
        }

        public uint ReadUInt()
        {
            Span<byte> buffer = stackalloc byte[sizeof(uint)];
            Read(buffer);
            return BitConverter.ToUInt32(buffer);
        }

        public string? ReadString()
        {
            var byteSize = ReadInt();

            if (byteSize == 0)
            {
                return null;
            }

            return Encoding.UTF8.GetString(Read(byteSize));
        }

        #endregion

        #region Pop

        public byte Pop()
        {
            _cursor -= 1;
            return _bytecode[_cursor];
        }

        public byte[] Pop(int size)
        {
            byte[] result = new byte[size];
            Pop(result);
            return result;
        }

        public int Pop(Span<byte> buffer)
        {
            int length = Math.Min(buffer.Length, _cursor);
            _cursor -= length;
            for (int i = 0; i < length; i++)
            {
                buffer[i] = _bytecode[_cursor + i];
            }
            return length;
        }

        public bool PopBool()
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            Pop(buffer);
            return BitConverter.ToInt32(buffer) != 0;
        }

        public int PopInt()
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            Pop(buffer);
            return BitConverter.ToInt32(buffer);
        }

        public uint PopUInt()
        {
            Span<byte> buffer = stackalloc byte[sizeof(uint)];
            Pop(buffer);
            return BitConverter.ToUInt32(buffer);
        }

        #endregion
    }
}
