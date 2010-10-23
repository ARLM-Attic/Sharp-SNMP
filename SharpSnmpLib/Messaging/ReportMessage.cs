// REPORT message type.
// Copyright (C) 2008-2010 Malcolm Crowe, Lex Li, and other contributors.
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

/*
 * Created by SharpDevelop.
 * User: lextm
 * Date: 2008/8/3
 * Time: 15:36
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Lextm.SharpSnmpLib.Security;

namespace Lextm.SharpSnmpLib.Messaging
{
    /// <summary>
    /// REPORT message.
    /// </summary>
    public class ReportMessage : ISnmpMessage
    {
        private readonly SecurityParameters _parameters;
        private readonly Header _header;
        private readonly IPrivacyProvider _privacy = DefaultPrivacyProvider.DefaultPair;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportMessage"/> class.
        /// </summary>
        /// <param name="version">The version code.</param>
        /// <param name="header">The header.</param>
        /// <param name="parameters">The security parameters.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="privacy">The privacy provider.</param>
        public ReportMessage(VersionCode version, Header header, SecurityParameters parameters, Scope scope, IPrivacyProvider privacy)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("scope");
            }
            
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }
            
            if (privacy == null)
            {
                throw new ArgumentNullException("privacy");
            }
            
            if (version != VersionCode.V3)
            {
                throw new ArgumentException("only v3 is supported", "version");
            }

            Version = version;
            _header = header;
            _parameters = parameters;
            Scope = scope;
            _privacy = privacy;
        }

        /// <summary>
        /// Security parameters.
        /// </summary>
        public SecurityParameters Parameters
        {
            get { return _parameters; }
        }
        
/*
        /// <summary>
        /// Variables.
        /// </summary>
        public IList<Variable> Variables
        {
            get { return Scope.Pdu.Variables; }
        }
*/

/*
        /// <summary>
        /// Sends this <see cref="ReportMessage"/> and handles the response from agent.
        /// </summary>
        /// <param name="timeout">The time-out value, in milliseconds. The default value is 0, which indicates an infinite time-out period. Specifying -1 also indicates an infinite time-out period.</param>
        /// <param name="receiver">Agent.</param>
        /// <returns></returns>
        public ISnmpMessage GetResponse(int timeout, IPEndPoint receiver)
        {
            if (receiver == null)
            {
                throw new ArgumentNullException("receiver");
            }

            using (Socket socket = SnmpMessageExtension.GetSocket(receiver))
            {
                return GetResponse(timeout, receiver, socket);
            }
        }
*/

/*
        /// <summary>
        /// Sends this <see cref="ReportMessage"/> and handles the response from agent.
        /// </summary>
        /// <param name="timeout">The time-out value, in milliseconds. The default value is 0, which indicates an infinite time-out period. Specifying -1 also indicates an infinite time-out period.</param>
        /// <param name="receiver">Agent.</param>
        /// <param name="socket">The socket.</param>
        /// <returns></returns>
        public ISnmpMessage GetResponse(int timeout, IPEndPoint receiver, Socket socket)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            
            if (receiver == null)
            {
                throw new ArgumentNullException("receiver");
            }
            
            return MessageFactory.GetResponse(receiver, ToBytes(), MessageId, timeout, new UserRegistry(), socket);
        }
*/

        /// <summary>
        /// Gets the request ID.
        /// </summary>
        /// <value>The request ID.</value>
        public int RequestId
        {
            get { return Scope.Pdu.RequestId.ToInt32(); }
        }
        
        /// <summary>
        /// Gets the message ID.
        /// </summary>
        /// <value>The message ID.</value>
        /// <remarks>For v3, message ID is different from request ID. For v1 and v2c, they are the same.</remarks>
        public int MessageId
        {
            get
            {
                return _header.MessageId;
            }
        }

        /// <summary>
        /// Gets the scope.
        /// </summary>
        /// <value>The scope.</value>
        public Scope Scope { get; private set; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        public VersionCode Version { get; private set; }

        /// <summary>
        /// Converts to byte format.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            return SnmpMessageExtension.PackMessage(Version, Privacy, _header, _parameters, Scope).ToBytes();
        }

        /// <summary>
        /// PDU.
        /// </summary>
        public ISnmpPdu Pdu
        {
            get { return Scope.Pdu; }
        }

        /// <summary>
        /// Gets the privacy provider.
        /// </summary>
        /// <value>The privacy provider.</value>
        public IPrivacyProvider Privacy
        {
            get { return _privacy; }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="ReportMessage"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "REPORT request message: version: " + Version + "; " + _parameters.UserName + "; " + Scope.Pdu;
        }
    }
}
