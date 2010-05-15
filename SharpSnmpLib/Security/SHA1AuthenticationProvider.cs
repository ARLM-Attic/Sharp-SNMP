﻿// SHA-1 authentication provider.
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

using System;
using System.IO;
using System.Security.Cryptography;
using Lextm.SharpSnmpLib.Messaging;

namespace Lextm.SharpSnmpLib.Security
{
    /// <summary>
    /// Authentication provider using SHA-1.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SHA", Justification = "definition")]
    public class SHA1AuthenticationProvider : IAuthenticationProvider
    {
        private readonly byte[] _password;
        private const int DigestLength = 12;

        /// <summary>
        /// Initializes a new instance of the <see cref="SHA1AuthenticationProvider"/> class.
        /// </summary>
        /// <param name="phrase">The phrase.</param>
        public SHA1AuthenticationProvider(OctetString phrase)
        {
            if (phrase == null)
            {
                throw new ArgumentNullException("phrase");
            }
            
            _password = phrase.GetRaw();
        }

        #region IAuthenticationProvider Members
        /// <summary>
        /// Passwords to key.
        /// </summary>
        /// <param name="password">The user password.</param>
        /// <param name="engineId">The engine ID.</param>
        /// <returns></returns>
        public byte[] PasswordToKey(byte[] password, byte[] engineId)
        {
            // key length has to be at least 8 bytes long (RFC3414)
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            
            if (password.Length < 8)
            {
                throw new ArgumentException("Secret key is too short. Must be >= 8. Current: " + password.Length, "password");
            }
            
            if (engineId == null)
            {
                throw new ArgumentNullException("engineId");
            }            
            
            using (SHA1 sha = new SHA1CryptoServiceProvider())
            {
                int passwordIndex = 0;
                int count = 0;
                /* Use while loop until we've done 1 Megabyte */
                byte[] sourceBuffer = new byte[1048576];
                byte[] buf = new byte[64];
                while (count < 1048576)
                {
                    for (int i = 0; i < 64; ++i)
                    {
                        // Take the next octet of the password, wrapping
                        // to the beginning of the password as necessary.
                        buf[i] = password[passwordIndex++ % password.Length];
                    }
                    
                    Array.Copy(buf, 0, sourceBuffer, count, buf.Length);
                    count += 64;
                }

                byte[] digest = sha.ComputeHash(sourceBuffer);

                using (MemoryStream buffer = new MemoryStream())
                {
                    buffer.Write(digest, 0, digest.Length);
                    buffer.Write(engineId, 0, engineId.Length);
                    buffer.Write(digest, 0, digest.Length);
                    return sha.ComputeHash(buffer.ToArray());
                }
            }
        }

        /// <summary>
        /// Gets the clean digest.
        /// </summary>
        /// <value>The clean digest.</value>
        public OctetString CleanDigest
        {
            get { return new OctetString(new byte[DigestLength]); }
        }

        /// <summary>
        /// Computes the hash.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public OctetString ComputeHash(ISnmpMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            
            byte[] key = PasswordToKey(_password, message.Parameters.EngineId.GetRaw());
            using (HMACSHA1 sha1 = new HMACSHA1(key))
            {
                byte[] buffer = message.ToBytes();
                byte[] hash = sha1.ComputeHash(buffer);
                sha1.Clear();
                byte[] result = new byte[DigestLength];
                Array.Copy(hash, result, result.Length);
                return new OctetString(result);
            }
        }

        #endregion

        public override string ToString()
        {
            return "SHA-1 authentication provider";
        }
    }
}
