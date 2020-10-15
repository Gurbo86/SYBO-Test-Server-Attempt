using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using GameServer.Packets;

namespace GameServer
{
    public class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public delegate void PacketHandler(int fromClient, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandler;

        private static TcpListener tcpListener;

        public static void Start(int maxPlayers, int port)
        {
            MaxPlayers = maxPlayers;
            Port = port;

            Console.WriteLine("Starting server ...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Server Started on port {port}");
        }

        /// <summary>
        /// Handles any conecction requests that arrives from the listener
        /// </summary>
        /// <param name="result"></param>
        public static void TCPConnectCallback(IAsyncResult result)
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(result);
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            bool assinged = false;
            int i = -1;

            // search through the available mirror clients to establish a conection
            while (!assinged && (i <= MaxPlayers - 1))
            {
                i++;

                if ((clients[i].tcp.socket != null) && (!clients[i].tcp.Conected))
                {
                    clients[i].tcp.Dispose();
                }

                // If it finds a mirror client that has no socket
                if (clients[i].tcp.socket == null)
                {
                    // Provides the mirror client with the socket that arrived
                    clients[i].tcp.Connect(client);
                    assinged = true;
                }
            }

            // If wasn't able to assign any mirror client
            if (assinged)
            {
                Console.WriteLine($"{client.Client.RemoteEndPoint} has been assigned to index {i}");
            }
            else
            {
                // Close the incoming conection
                client.Close();
                Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
                // Continue listening
            }
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        }

        public static void InitializeServerData()
        {
            // Creates the mirror clients based on the amount of maxPlayers.
            for (int i = 0; i <= MaxPlayers - 1; i++)
            {
                // Each mirror client is given an id. This id will help to
                // send messages to the proper client.
                clients.Add(i, new Client(i));
            }

            // this will be the handler that has the response methods.
            // this is specific for this server.
            packetHandler = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.request, ServerHandle.Request}
            };

            Console.WriteLine($"Initialized packets.");
        }
    }
}