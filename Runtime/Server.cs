using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using Unity.Networking.Transport.Utilities;

namespace SLIDDES.Networking.Transport
{
    public class Server : MonoBehaviour
    {
        public static Server Instance
        {
            get
            {
                return instance;
            }
            private set { }
        }
        /// <summary>
        /// The network driver of the server
        /// </summary>
        public static NetworkDriver Driver { get { return Instance.networkDriver; } }
        /// <summary>
        /// The network pipeline of the server
        /// </summary>
        public static NetworkPipeline Pipeline { get { return Instance.networkPipeline; } }
        /// <summary>
        /// The dictonary that handles the networkMessageType with the correspondent NetworkMessageHandler Method
        /// </summary>
        public static Dictionary<NetworkMessageType, NetworkMessageHandler> networkMessageHandlers;

        private static Server instance;

        [Header("Server")]
        [Tooltip("Handles how the Server behaves")]
        [SerializeField] private ServerHandler handler;
        [Tooltip("The port of the server")]
        [Range(0, 65535)]
        [SerializeField] private ushort port;
        [Tooltip("The amount of clients that can connect to this server")]
        [Range(0, 9999)]
        [SerializeField] private int maxConnections = 9999;

        private NetworkDriver networkDriver;
        private NetworkPipeline networkPipeline;

        /// <summary>
        /// Holds the connections to the server 
        /// </summary>
        private NativeList<NetworkConnection> networkConnections;

        private void OnDestroy()
        {
            // Clean up
            networkDriver.Dispose();
            networkConnections.Dispose();
        }

        private void Awake()
        {
            if(instance != null)
            {
                UnityEngine.Debug.LogWarning("[Server] Already active in scene! Make sure you only have 1 server component.");
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

            Create();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateConnections();
        }

        /// <summary>
        /// Create the server
        /// </summary>
        public static void Create()
        {
            // Create the network
            Instance.networkDriver = NetworkDriver.Create(new ReliableUtility.Parameters { WindowSize = 32 });
            Instance.networkPipeline = Instance.networkDriver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

            // Open listener on server port
            NetworkEndPoint endPoint = NetworkEndPoint.AnyIpv4;
            endPoint.Port = Instance.port;
            if(Instance.networkDriver.Bind(endPoint) != 0) UnityEngine.Debug.LogError("[Server] Failed to bind to port " + Instance.port);
            else Instance.networkDriver.Listen();

            Instance.networkConnections = new NativeList<NetworkConnection>(Instance.maxConnections, Allocator.Persistent);
            UnityEngine.Debug.Log("[Server] Created server on port: " + Instance.port);
        }

        /// <summary>
        /// Updates all the connections to the server (remove or add) and executes incoming recognized message types
        /// </summary>
        private void UpdateConnections()
        {
            // Dont execute when the network isnt created yet
            if(!networkDriver.IsCreated) return;

            // This is a jobified system, so we need to tell it to handle all its outstanding tasks first
            networkDriver.ScheduleUpdate().Complete();

            // Clean up connections, remove stale ones
            for(int i = 0; i < networkConnections.Length; i++)
            {
                if(!networkConnections[i].IsCreated)
                {
                    // Remove connection
                    handler.RemoveNetworkConnection(networkConnections[i]);
                    networkConnections.RemoveAtSwapBack(i);
                    // This little trick means we can alter the contents of the list without breaking/skipping instances
                    --i;
                }
            }

            // Accept new connections
            NetworkConnection c;
            while((c = networkDriver.Accept()) != default(NetworkConnection))
            {
                if(handler.AddNetworkConnection(c))
                {
                    networkConnections.Add(c);
                    UnityEngine.Debug.Log("[Server] Accepted a connection.");
                }
                else UnityEngine.Debug.Log("[Server] Rejected a connection.");
            }

            // Handle network messages
            DataStreamReader streamReader;
            for(int i = 0; i < networkConnections.Length; i++)
            {
                if(!networkConnections[i].IsCreated) continue;

                // Loop trough available events
                NetworkEvent.Type net;
                while((net = networkDriver.PopEventForConnection(networkConnections[i], out streamReader)) != NetworkEvent.Type.Empty)
                {
                    if(net == NetworkEvent.Type.Data)
                    {
                        // First UInt is always a message type (this is our first design choice)
                        NetworkMessageType msgType = (NetworkMessageType)streamReader.ReadUInt();

                        if(networkMessageHandlers.ContainsKey(msgType))
                        {
                            // Handle the message type
                            networkMessageHandlers[msgType].Invoke(this, networkConnections[i], streamReader);
                        }
                        else
                        {
                            // Didnt recognise message
                            UnityEngine.Debug.LogWarning("[Server] Unsupported message type received: " + msgType);
                        }
                    }
                }
            }
        }
    }
}