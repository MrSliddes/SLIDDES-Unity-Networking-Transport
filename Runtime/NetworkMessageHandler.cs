using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using Unity.Networking.Transport.Utilities;

namespace SLIDDES.Networking.Transport
{
    /// <summary>
    /// For handleing a type of message
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="connection"></param>
    /// <param name="dataStreamReader"></param>
    public delegate void NetworkMessageHandler(object handler, NetworkConnection connection, DataStreamReader dataStreamReader);
}