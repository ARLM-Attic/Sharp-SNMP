﻿namespace Lextm.SharpSnmpLib.Agent
{
    /// <summary>
    /// Composed membership provider, who owns internal providers. If the request is authenticated by any of the internal providers, it is considered as authenticated.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class ComposedMembershipProvider : IMembershipProvider
    {
        private readonly IMembershipProvider[] _providers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComposedMembershipProvider"/> class.
        /// </summary>
        /// <param name="providers">The internal providers.</param>
        public ComposedMembershipProvider(IMembershipProvider[] providers)
        {
            _providers = providers;
        }

        /// <summary>
        /// Authenticates the request.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public bool AuthenticateRequest(ISnmpMessage message)
        {
            foreach (IMembershipProvider provider in _providers)
            {
                if (provider.AuthenticateRequest(message))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
