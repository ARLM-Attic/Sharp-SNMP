﻿using Lextm.SharpSnmpLib.Messaging;

namespace Lextm.SharpSnmpLib.Pipeline
{
    /// <summary>
    /// A placeholder.
    /// </summary>
    internal class NullMessageHandler : IMessageHandler
    {
        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="store">The object store.</param>
        /// <returns></returns>
        public ResponseData Handle(SnmpContext message, ObjectStore store)
        {
            return new ResponseData(null, ErrorCode.NoError, 0);
        }
    }
}
