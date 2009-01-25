﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Lextm.SharpSnmpLib
{
    /// <summary>
    /// TRAP v2 PDU.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pdu")]
    public class TrapV2Pdu : ISnmpPdu
    {
        private readonly IList<Variable> _variables;
        private readonly Integer32 _version;
        private readonly byte[] _raw;
        private readonly Sequence _varbindSection;
        private readonly TimeTicks _time;
        private readonly ObjectIdentifier _enterprise;
     
        /// <summary>
        /// Creates a <see cref="TrapV2Pdu"/> instance with all content.
        /// </summary>
        /// <param name="version">Version code</param>
        /// <param name="enterprise">Enterprise</param>
        /// <param name="time">Time stamp</param>
        /// <param name="variables">Variables</param>
        public TrapV2Pdu(Integer32 version, ObjectIdentifier enterprise, TimeTicks time, IList<Variable> variables)
        {
            _enterprise = enterprise;
            _version = version;
            _time = time;
            _variables = variables;
            IList<Variable> full = new List<Variable>(variables);
            full.Insert(0, new Variable(new uint[] { 1, 3, 6, 1, 2, 1, 1, 3, 0 }, _time));
            full.Insert(1, new Variable(new uint[] { 1, 3, 6, 1, 6, 3, 1, 1, 4, 1, 0 }, _enterprise));
            _varbindSection = Variable.Transform(full);
            _raw = ByteTool.ParseItems(_version, new Integer32(0), new Integer32(0), _varbindSection);
        }
     
        /// <summary>
        /// Creates a <see cref="TrapV2Pdu"/> instance from raw bytes.
        /// </summary>
        /// <param name="raw">Raw bytes</param>
        private TrapV2Pdu(byte[] raw): this(raw.Length, new MemoryStream(raw))
        {
           
        }

        #region ISnmpPdu Members
        /// <summary>
        /// Converts to message body.
        /// </summary>
        /// <param name="version">Prtocol version</param>
        /// <param name="community">Community name</param>
        /// <returns></returns>
        public ISnmpData ToMessageBody(VersionCode version, OctetString community)
        {
            return ByteTool.PackMessage(version, community, this);
        }
     
        /// <summary>
        /// Variables.
        /// </summary>
        public IList<Variable> Variables
        {
            get { return _variables; }
        }

        #endregion

        #region ISnmpData Members
        /// <summary>
        /// Type code.
        /// </summary>
        public SnmpType TypeCode
        {
            get { return SnmpType.TrapV2Pdu; }
        }

        private byte[] _bytes;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrapV2Pdu"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="stream">The stream.</param>
        public TrapV2Pdu(int length, Stream stream)
        {
            _version = (Integer32)DataFactory.CreateSnmpData(stream); // version number v2c
            Integer32 temp1 = (Integer32) DataFactory.CreateSnmpData(stream); // 0
            Integer32 temp2 = (Integer32) DataFactory.CreateSnmpData(stream); // 0
            _varbindSection = (Sequence)DataFactory.CreateSnmpData(stream);
            _variables = Variable.Transform(_varbindSection); // v[0] is timestamp. v[1] oid, v[2] value.
            _time = (TimeTicks)_variables[0].Data;
            _variables.RemoveAt(0);
            _enterprise = (ObjectIdentifier)_variables[0].Data;
            _variables.RemoveAt(0);
            _raw = ByteTool.ParseItems(_version, temp1, temp2, _varbindSection);
            Debug.Assert(length >= _raw.Length);
        }

        /// <summary>
        /// Converts to byte format.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            if (_bytes == null)
            {
                MemoryStream result = new MemoryStream();
                result.WriteByte((byte)TypeCode);
                ByteTool.WritePayloadLength(result, _raw.Length); // it seems that trap does not use this function
                result.Write(_raw, 0, _raw.Length);
                _bytes = result.ToArray();
            }
            
            return _bytes;
        }

        #endregion
        /// <summary>
        /// Enterprise.
        /// </summary>
        public ObjectIdentifier Enterprise
        {
            get { return _enterprise; }
        }
        
        /// <summary>
        /// Time stamp.
        /// </summary>
        [CLSCompliant(false)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TimeStamp")]
        public uint TimeStamp
        {
            get { return _time.ToUInt32(); }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="TrapV2Pdu"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "TRAP v2 PDU: enterprise: {0}; time stamp: {1}; variable count: {2}",
                _enterprise, 
                _time, 
                _variables.Count.ToString(CultureInfo.InvariantCulture));
        }
    }
}
