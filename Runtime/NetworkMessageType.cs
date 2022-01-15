using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SLIDDES.Networking.Transport
{
    /// <summary>
    /// The type a network message can be (uint is compared with this)
    /// </summary>
    public enum NetworkMessageType
    {
        /// <summary>
        /// When a client disconnects from the server
        /// </summary>
        CLIENT_DISCONNECT,
        /// <summary>
        /// Client tells server that he wants to form a connection
        /// </summary>
        HANDSHAKE_SERVER,
        /// <summary>
        /// Server response to client about connection
        /// </summary>
        HANDSHAKE_SERVER_RESPONSE,
        /// <summary>
        /// A message from the server
        /// </summary>
        SERVER_MESSAGE,

        // Custom messages
        /// <summary>
        /// Client wants to login into the server
        /// </summary>
        CLIENT_LOGIN_SERVER,
        /// <summary>
        /// Client pings server to see if it is online, server pings back client, client records response time
        /// </summary>
        PING_SERVER
    }
}