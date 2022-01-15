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
    /// The class to inherit with your own ClientHandler version to handle what the client does
    /// </summary>
    public abstract class ClientHandler : MonoBehaviour
    {
        /// <summary>
        /// The dictonary that handles the networkMessageType with the correspondent NetworkMessageHandler Method
        /// </summary>
        /// <example>
        /// public override Dictionary<NetworkMessageType, NetworkMessageHandler> NetworkMessageHandlers { get { return networkMessageHandlers; }
        /// 
        /// private Dictionary<NetworkMessageType, NetworkMessageHandler> networkMessageHandlers = new Dictionary<NetworkMessageType, NetworkMessageHandler>
        /// {
        ///     { NetworkMessageType.HANDSHAKE_RESPONSE, HandleServerHandshake }
        /// };
        /// </example>
        public abstract Dictionary<NetworkMessageType, NetworkMessageHandler> NetworkMessageHandlers { get; }


        /// <summary>
        /// The message that gets send to the server for a handsake
        /// </summary>
        /// <param name="streamWriter">The DataStreamWriter reference</param>
        public abstract void HandsakeServerMessage(ref DataStreamWriter streamWriter);
    }
}
