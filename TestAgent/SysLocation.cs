﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Lextm.SharpSnmpLib.Agent
{
    internal class SysLocation : IScalarObject
    {
        private static readonly ObjectIdentifier _id = new ObjectIdentifier("1.3.6.1.2.1.1.6.0");
        private OctetString _location = OctetString.Empty;
        
        public ISnmpData Get()
        {
            return OctetString.Empty;
        }

        public void Set(ISnmpData data)
        {
            if (data.TypeCode != SnmpType.OctetString)
            {
                throw new ArgumentException("data");
            }
            
            _location = (OctetString)data;
        }

        public ObjectIdentifier Id
        {
            get
            {
                return _id;
            }
        }
    }
}
