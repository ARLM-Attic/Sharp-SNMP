﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Lextm.SharpSnmpLib
{
    /// <summary>
    /// Counter64 type.
    /// </summary>
    public class Counter64 : ISnmpData, IEquatable<Counter64>
    {
        private ulong _count;
        
        /// <summary>
        /// Creates a <see cref="Counter64"/> instance from raw bytes.
        /// </summary>
        /// <param name="raw"></param>
        public Counter64(byte[] raw)
        {
            if (raw == null)
            {
                throw new ArgumentNullException("raw");
            }
            
            if (raw.Length == 0 || raw.Length > 9)
            {
                throw new ArgumentException("byte length must between 1 and 9");
            }
            
            if (raw.Length == 9 && raw[0] != 0)
            {
                throw new ArgumentException("if byte length is 5, then first byte must be empty");
            }
            
            List<byte> list = new List<byte>(raw);
            list.Reverse();
            while (list.Count > 8)
            {
                list.RemoveAt(list.Count - 1);
            }
            
            while (list.Count < 8)
            {
                list.Add(0);
            }
            
            _count = BitConverter.ToUInt64(list.ToArray(), 0);
        }
        
        /// <summary>
        /// Creates a <see cref="Counter64"/> with a specific <see cref="UInt64"/>.
        /// </summary>
        /// <param name="value">Value</param>
        [CLSCompliant(false)]
        public Counter64(ulong value)
        {
            _count = value;
        }

        #region ISnmpData Members
        /// <summary>
        /// Type code.
        /// </summary>
        public SnmpType TypeCode
        {
            get { return SnmpType.Counter64; }
        }
        
        /// <summary>
        /// Converts to byte format.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            return ByteTool.ToBytes(TypeCode, GetRaw());
        }

        #endregion
        /// <summary>
        /// Returns a <see cref="UInt64"/> that represents a <see cref="Counter64"/>.
        /// </summary>
        /// <returns></returns>
        [CLSCompliant(false)]
        public ulong ToUInt64()
        {
            return _count;
        }
        
        /// <summary>
        /// Returns a <see cref="String"/> that represents this <see cref="Counter64"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToUInt64().ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets that raw bytes.
        /// </summary>
        /// <returns></returns>
        internal byte[] GetRaw()
        {
            return ByteTool.GetRawBytes(BitConverter.GetBytes(_count), false);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><value>true</value> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <value>false</value>.
        /// </returns>
        public bool Equals(Counter64 other)
        {
            if (other == null)
            {
                return false;    
            }
            
            return ToUInt64() == other.ToUInt64();
        }
        
        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="Counter64"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Counter64"/>. </param>
        /// <returns><value>true</value> if the specified <see cref="Object"/> is equal to the current <see cref="Counter64"/>; otherwise, <value>false</value>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            
            if (GetType() != obj.GetType())
            {
                return false;
            }
            
            return Equals((Counter64)obj);
        }
        
        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see cref="Counter64"/>.</returns>
        public override int GetHashCode()
        {
            return ToUInt64().GetHashCode();
        }
        
        /// <summary>
        /// The equality operator.
        /// </summary>
        /// <param name="left">Left <see cref="Counter64"/> object</param>
        /// <param name="right">Right <see cref="Counter64"/> object</param>
        /// <returns>
        /// Returns <c>true</c> if the values of its operands are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(Counter64 left, Counter64 right)
        {
            if (left == null)
            {
                return right == null;    
            }
            
            return left.Equals(right);
        }
        
        /// <summary>
        /// The inequality operator.
        /// </summary>
        /// <param name="left">Left <see cref="Counter64"/> object</param>
        /// <param name="right">Right <see cref="Counter64"/> object</param>
        /// <returns>
        /// Returns <c>true</c> if the values of its operands are not equal, <c>false</c> otherwise.</returns>
        public static bool operator !=(Counter64 left, Counter64 right)
        {
            return !(left == right);
        }
    }
}