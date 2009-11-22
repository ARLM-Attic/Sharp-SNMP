﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;

namespace Lextm.SharpSnmpLib
{
    /// <summary>
    /// ObjectIdentifier type.
    /// </summary>
    #if (!CF)
    [TypeConverter(typeof(ObjectIdentifierConverter))]
    #endif
    [Serializable]
    public sealed class ObjectIdentifier : 
        ISnmpData, IEquatable<ObjectIdentifier>, IComparable<ObjectIdentifier>, IComparable
    {
        private readonly uint[] _oid;
        #if (CF)
        [NonSerialized]
        #endif
        private int _hashcode;

        #region Constructor

        /// <summary>
        /// Creates an <see cref="ObjectIdentifier"/> instance from textual ID.
        /// </summary>
        /// <param name="text">String in this format, "*.*.*.*".</param>
        public ObjectIdentifier(string text)
            : this(Convert(text))
        {
        }

        /// <summary>
        /// Creates an <see cref="ObjectIdentifier"/> instance from numerical ID.
        /// </summary>
        /// <param name="id">OID <see cref="uint"/> array</param>
        [CLSCompliant(false)]
        public ObjectIdentifier(uint[] id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            if (id.Length < 2)
            {
                throw new ArgumentException("The length of the shortest identifier is two", "id");
            }

            if (id[0] > 2)
            {
                throw new ArgumentException("The first sub-identifier must be 0, 1, or 2.", "id");
            }

            if (id[1] > 39)
            {
                throw new ArgumentException("The second sub-identifier must be less than 40", "id");
            }

            _oid = id;
        }

        /// <summary>
        /// Creates an <see cref="ObjectIdentifier"/> instance from raw bytes.
        /// </summary>
        /// <param name="raw">Raw bytes</param>
        internal ObjectIdentifier(byte[] raw)
            : this(raw.Length, new MemoryStream(raw))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIdentifier"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="stream">The stream.</param>
        public ObjectIdentifier(int length, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            byte[] raw = new byte[length];
            stream.Read(raw, 0, length);
            if (length == 0)
            {
                throw new ArgumentException("length cannot be 0", "length");
            }

            List<uint> result = new List<uint>();
            result.Add((uint)(raw[0] / 40));
            result.Add((uint)(raw[0] % 40));
            uint buffer = 0;
            for (int i = 1; i < raw.Length; i++)
            {
                if ((raw[i] & 0x80) == 0)
                {
                    result.Add(raw[i] + (buffer << 7));
                    buffer = 0;
                }
                else
                {
                    buffer <<= 7;
                    buffer += (uint)(raw[i] & 0x7F);
                }
            }

            _oid = result.ToArray();
        }

        #endregion Constructor

        /// <summary>
        /// Convers to numerical ID.
        /// </summary>
        /// <returns></returns>
        [CLSCompliant(false)]
        public uint[] ToNumerical()
        {
            return _oid;
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings:
        /// Value
        /// Meaning
        /// Less than zero
        /// This object is less than the <paramref name="other"/> parameter.
        /// Zero
        /// This object is equal to <paramref name="other"/>.
        /// Greater than zero
        /// This object is greater than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(ObjectIdentifier other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            int shortest = (_oid.Length < other._oid.Length) ? _oid.Length : other._oid.Length;
            for (int i = 0; i < shortest; i++)
            {
                if (_oid[i] > other._oid[i])
                {
                    return 1;
                }
                
                if (_oid[i] < other._oid[i])
                {
                    return -1;
                }

                continue;
            }

            return _oid.Length - other._oid.Length;
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this <see cref="ObjectIdentifier"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Convert(_oid);
        }

        /// <summary>
        /// Converts uint array to dotted <see cref="String"/>.
        /// </summary>
        /// <param name="numerical"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public static string Convert(uint[] numerical)
        {
            if (numerical == null)
            {
                throw new ArgumentNullException("numerical");
            }

            StringBuilder result = new StringBuilder();
            for (int k = 0; k < numerical.Length; k++)
            {
                result.Append(".").Append(numerical[k].ToString(CultureInfo.InvariantCulture));
            }

            return result.ToString();
        }

        /// <summary>
        /// Converts dotted <see cref="String"/> to uint array.
        /// </summary>
        /// <param name="dotted">Dotted string.</param>
        /// <returns>uint array.</returns>
        [CLSCompliant(false)]
        public static uint[] Convert(string dotted)
        {
            if (dotted == null)
            {
                throw new ArgumentNullException("dotted");
            }

            string[] parts = dotted.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            uint[] result = new uint[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                result[i] = uint.Parse(parts[i], CultureInfo.InvariantCulture);
            }

            return result;
        }

        /// <summary>
        /// Converts to byte format.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use AppendBytesTo instead.")]
        public byte[] ToBytes()
        {
            using (MemoryStream result = new MemoryStream())
            {
                AppendBytesTo(result);
                return result.ToArray();
            }
        }

        /// <summary>
        /// Appends the bytes to <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void AppendBytesTo(Stream stream)
        {
            // TODO: improve here.
            List<byte> temp = new List<byte>();
            byte first = (byte)((40 * _oid[0]) + _oid[1]);
            temp.Add(first);
            for (int i = 2; i < _oid.Length; i++)
            {
                temp.AddRange(ConvertToBytes(_oid[i]));
            }

            ByteTool.AppendBytes(stream, TypeCode, temp.ToArray());
        }

        private static IEnumerable<byte> ConvertToBytes(uint subIdentifier)
        {
            List<byte> result = new List<byte>();
            result.Add((byte)(subIdentifier & 0x7F));
            while ((subIdentifier = subIdentifier >> 7) > 0)
            {
                result.Add((byte)((subIdentifier & 0x7F) | 0x80));
            }

            result.Reverse();
            return result;
        }

        /// <summary>
        /// Type code.
        /// </summary>
        public SnmpType TypeCode
        {
            get
            {
                return SnmpType.ObjectIdentifier;
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="ObjectIdentifier"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="ObjectIdentifier"/>. </param>
        /// <returns><value>true</value> if the specified <see cref="Object"/> is equal to the current <see cref="ObjectIdentifier"/>; otherwise, <value>false</value>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(this, obj as ObjectIdentifier);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><value>true</value> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <value>false</value>.
        /// </returns>
        public bool Equals(ObjectIdentifier other)
        {
            return Equals(this, other);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see cref="ObjectIdentifier"/>.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                if (_hashcode == 0)
                {
                    int hash = 0;
                    for (int i = _oid.Length - 1; i >= 0; i--)
                    {                        
                        hash ^= (int)_oid[i];                        
                    }
                    
                    _hashcode = hash != 0 ? hash : 1;    // Very unlikely that hash=0, but I prefer to foresee the case.
                }
            }
            
            return _hashcode;
        }

        public int CompareTo(object obj)
        {
            return CompareTo((ObjectIdentifier)obj);
        }

        /// <summary>
        /// The equality operator.
        /// </summary>
        /// <param name="left">Left <see cref="ObjectIdentifier"/> object</param>
        /// <param name="right">Right <see cref="ObjectIdentifier"/> object</param>
        /// <returns>
        /// Returns <c>true</c> if the values of its operands are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(ObjectIdentifier left, ObjectIdentifier right)
        {
            return Equals(left, right);
        }
        
        /// <summary>
        /// The inequality operator.
        /// </summary>
        /// <param name="left">Left <see cref="ObjectIdentifier"/> object</param>
        /// <param name="right">Right <see cref="ObjectIdentifier"/> object</param>
        /// <returns>
        /// Returns <c>true</c> if the values of its operands are not equal, <c>false</c> otherwise.</returns>
        public static bool operator !=(ObjectIdentifier left, ObjectIdentifier right)
        {
            return !(left == right);
        }
        
        /// <summary>
        /// The comparison.
        /// </summary>
        /// <param name="left">Left <see cref="ObjectIdentifier"/> object</param>
        /// <param name="right">Right <see cref="ObjectIdentifier"/> object</param>
        /// <returns>
        /// Returns <c>true</c> if the values of its operands are not equal, <c>false</c> otherwise.</returns>
        public static bool Equals(ObjectIdentifier left, ObjectIdentifier right)
        {
            object lo = left;
            object ro = right;
            if (lo == ro)
            {
                return true;
            }

            if (lo == null || ro == null)
            {
                return false;
            }

            return left.CompareTo(right) == 0;
            //return ByteTool.CompareArray(left._oid, right._oid);
        }
    }
}