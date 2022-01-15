using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using Unity.Networking.Transport.Utilities;

namespace SLIDDES.Networking.Transport
{
    /// <summary>
    /// The class to inherit with your own ServerHandler version to handle what the server does
    /// </summary>
    public abstract class ServerHandler : MonoBehaviour
    {
        /// <summary>
        /// The dictonary that handles the networkMessageType with the correspondent NetworkMessageHandler Method
        /// </summary>
        /// <example>
        /// public override Dictionary<NetworkMessageType, NetworkMessageHandler> NetworkMessageHandlers { get { return networkMessageHandlers; }
        /// 
        /// private Dictionary<NetworkMessageType, NetworkMessageHandler> networkMessageHandlers = new Dictionary<NetworkMessageType, NetworkMessageHandler>
        /// {
        ///     { NetworkMessageType.HANDSHAKE_RESPONSE, HandleClientHandshake }
        /// };
        /// </example>
        public abstract Dictionary<NetworkMessageType, NetworkMessageHandler> NetworkMessageHandlers { get; }

        /// <summary>
        /// For when the server adds a network connection
        /// </summary>
        /// <param name="networkConnection"></param>
        /// <returns>True to add the network, false to reject it</returns>
        public abstract bool AddNetworkConnection(NetworkConnection connection);

        /// <summary>
        /// For when the server removes a network connection
        /// </summary>
        /// <param name="networkConnection">The connection that gets removed</param>
        public abstract void RemoveNetworkConnection(NetworkConnection connection);
    }
}
