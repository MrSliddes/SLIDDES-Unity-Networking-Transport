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
    /// The client framework class to communicate with a server
    /// </summary>
    public class Client : MonoBehaviour
    {
        public static Client Instance
        {
            get
            {                
                return instance;
            }
            private set { }
        }

        /// <summary>
        /// The network connection of the client
        /// </summary>
        public static NetworkConnection Connection { get { return Instance.networkConnection; } }
        /// <summary>
        /// The network driver of the client
        /// </summary>
        public static NetworkDriver Driver { get { return Instance.networkDriver; } }
        /// <summary>
        /// The ClientHandler of this Client
        /// </summary>
        public static ClientHandler Handler { get { return Instance.handler; } }
        /// <summary>
        /// If the client is connected to the server
        /// </summary>
        public static bool IsConnected 
        { 
            get 
            {
                bool b = Driver.IsCreated && Connection.IsCreated && Connection.GetState(Driver) == NetworkConnection.State.Connected;
                if(!b) UnityEngine.Debug.Log("[Client] No connection");
                return b; 
            } 
        }
        /// <summary>
        /// The dictonary that handles the networkMessageType with the correspondent NetworkMessageHandler Method
        /// </summary>
        public static Dictionary<NetworkMessageType, NetworkMessageHandler> networkMessageHandlers;
        
        private static Client instance;

        [Header("Client")]
        [Tooltip("Handles how the Client behaves")]
        [SerializeField] private ClientHandler handler;
        [Tooltip("The IP address of the server to connect to")]
        [SerializeField] private string serverIP;
        [Range(0, 65535)]
        [Tooltip("The port number of the server to connect to")]
        [SerializeField] private ushort port;
        [Header("Debug")]
        [Tooltip("Should the client try to connect to the server when playmode starts")]
        [SerializeField] private bool makeConnectionAtPlay;

        private NetworkDriver networkDriver;
        private NetworkConnection networkConnection;

        private void OnDestroy()
        {
            // Clean up
            if(networkDriver.IsCreated)
            {
                Disconnect();
                networkDriver.Dispose();
            }
        }

        private void Awake()
        {
            if(instance != null)
            {
                UnityEngine.Debug.LogWarning("[Client] Already active in scene! Make sure you only have 1 client component.");
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            // Set the networkMessageHandles to that of the handler
            networkMessageHandlers = handler.NetworkMessageHandlers;

            if(makeConnectionAtPlay) Connect();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateConnection();
        }

        /// <summary>
        /// Connect to the server
        /// </summary>
        public static void Connect()
        {
            // Check server ip
            if(string.IsNullOrEmpty(Instance.serverIP))
            {
                UnityEngine.Debug.LogError("[Client] Server IP not set!");
                return;
            }

            // Create the networks
            Instance.networkDriver = NetworkDriver.Create();
            Instance.networkConnection = default(NetworkConnection);

            NetworkEndPoint endPoint = NetworkEndPoint.Parse(Instance.serverIP, Instance.port);
            Instance.networkConnection = Instance.networkDriver.Connect(endPoint);
            UnityEngine.Debug.Log("[Client] Started client connection.");
        }

        /// <summary>
        /// Disconnect the client from the server if there is a connection
        /// </summary>
        public static void Disconnect()
        {
            if(!Instance.networkDriver.IsCreated) return;

            // If there is a connection to the server tell the server we left
            if(Instance.networkConnection.GetState(Instance.networkDriver) == NetworkConnection.State.Connected)
            {
                DataStreamWriter streamWriter;
                int result = Instance.networkDriver.BeginSend(NetworkPipeline.Null, Instance.networkConnection, out streamWriter);
                if(result == 0)
                {
                    // No problems, send message to server
                    streamWriter.WriteUInt((uint)NetworkMessageType.CLIENT_DISCONNECT);
                    Instance.networkDriver.EndSend(streamWriter);
                    // Force it to send it
                    Instance.networkDriver.ScheduleUpdate().Complete();
                }
                else
                {
                    Debug.LogError(string.Format("[Client] Could not write message to driver: {0}", result));
                }
            }
        }

        /// <summary>
        /// Update the clients connection
        /// </summary>
        private void UpdateConnection()
        {
            // Dont execute when the network isnt created yet
            if(!networkDriver.IsCreated) return;

            networkDriver.ScheduleUpdate().Complete();

            // Wait until the connection is created
            if(!networkConnection.IsCreated)
            {
                return;
            }

            // Communicate with the server
            DataStreamReader streamReader;
            NetworkEvent.Type net;

            // Go through all events that need to be popped (ignoring empty events)
            while((net = networkConnection.PopEvent(networkDriver, out streamReader)) != NetworkEvent.Type.Empty)
            {
                switch(net)
                {
                    case NetworkEvent.Type.Data:
                        // Received data from server
                        // Read the uint and check if networkMessageHandlers contains it
                        NetworkMessageType msgType = (NetworkMessageType)streamReader.ReadUInt();
                        if(networkMessageHandlers.ContainsKey(msgType))
                        {
                            // Handle the message
                            networkMessageHandlers[msgType].Invoke(this, networkConnection, streamReader);
                        }
                        else
                        {
                            // Didn't recognise message
                            UnityEngine.Debug.LogWarning("[Client] Unsupported message type received: " + msgType);
                        }
                        break;
                    case NetworkEvent.Type.Connect:
                        // Connection with the server, send a HANDSHAKE_SERVER
                        DataStreamWriter streamWriter;
                        int result = networkDriver.BeginSend(NetworkPipeline.Null, networkConnection, out streamWriter);
                        if(result == 0)
                        {
                            // No problems, send message to server
                            streamWriter.WriteUInt((uint)NetworkMessageType.HANDSHAKE_SERVER);
                            // Data to send with
                            handler.HandsakeServerMessage(ref streamWriter);
                            // Send
                            networkDriver.EndSend(streamWriter);
                        }
                        else
                        {
                            UnityEngine.Debug.LogError(string.Format("[Client] Could not write message to driver: {0}", result));
                        }
                        break;
                    case NetworkEvent.Type.Disconnect:
                        // Client disconnected from server
                        UnityEngine.Debug.Log("[Client] Disconnected from server.");
                        networkConnection = default(NetworkConnection); // Reset
                        break;
                    default:
                        break;
                }
            }
        }
    }
}