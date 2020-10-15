using System;
using System.Net.Sockets;

namespace GameServer
{
    using Packets;

    public class Client
    {
        public static int dataBufferSize = 64;

        public int id;
        public TCP tcp;

        public Client(int clientId)
        {
            id = clientId;
            tcp = new TCP(id);
        }

        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public bool Conected
            {
                get
                {
                    if (socket == null)
                    {
                        return false;
                    }
                    else
                    {
                        return socket.Connected;
                    }
                }
            }

            public TCP(int id)
            {
                this.id = id;
            }

            public void Connect(TcpClient socket)
            {
                this.socket = socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending data to the player {id} via TCP. Exception: /n{ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    // if socket wasn't closed
                    if (socket.Connected)
                    {
                        // I read the stream
                        int _byteLength = stream.EndRead(result);

                        // If stream didn't sent data, I'll procede to close;
                        // Maybe I should close this by getting a message
                        if (_byteLength <= 0)
                        {
                            Disconect();
                            return;
                        }

                        // else I create a auxiliar buffer
                        byte[] _data = new byte[_byteLength];
                        // copies the data received in the auxiliar buffer
                        Array.Copy(receiveBuffer, _data, _byteLength);
                        // handles the data, processing it depending on a group of rules
                        receivedData.Reset(HandleData(_data));
                        // continues listening the streams
                        stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                    }
                    else
                    {
                        stream.Dispose();
                        socket.Dispose();
                        stream = null;
                        socket = null;
                        Console.WriteLine($"TCPClient.ReceiveCallback() : A message has been received, but the socket is not conected anymore.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"TCPClient.ReceiveCallback() : Error receiving TCP data {ex}.");
                    Disconect();
                }
            }

            /// <summary>
            /// Manage the incoming data
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            private bool HandleData(byte[] data)
            {
                int packetLength = 0;

                receivedData.SetBytes(data);

                // Checks if at least it has 4 bytes to read.
                // This is because the first data the packet provides is
                // the length of the packet;
                if (receivedData.UnreadLength() >= 4)
                {
                    // Gets the packet lenght
                    packetLength = receivedData.ReadInt();

                    // checks if the packet has data checking the lenght
                    // if the length is 0 we return with true to erase receivedData
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                // while there's data tp read
                while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
                {
                    // we read the rest of the data, that is valid data, not verification data
                    byte[] packetBytes = receivedData.ReadBytes(packetLength);

                    GameServer.ThreadManager.ThreadManager.ExecuteOnMainThread(
                        () =>
                        {
                            // Create a new packet of data only and provide it to 
                            // the proper method.
                            using (Packet packet = new Packet(packetBytes))
                            {
                                //int packetId = packet.ReadInt();
                                Server.packetHandler[2](id, packet);
                            }
                        }
                    );

                    // Reset the length verification of the packet
                    packetLength = 0;
                    // Checks if at least it has 4 bytes to read.
                    // This is because the first data the packet provides is
                    // the length of the packet;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        // Gets the packet lenght
                        packetLength = receivedData.ReadInt();

                        // checks if the packet has data checking the lenght
                        // if the length is 0 we return with true to erase receivedData
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLength <= 1)
                {
                    return true;
                }

                return false;
            }

            public void Disconect()
            {
                Console.WriteLine($"Goodbye user number {id}, your are welcome for all te fish!. Thanks for the Paltas!.");
                socket.Close();
                stream.Close();
            }

            public void Dispose()
            {
                stream.Dispose();
                socket.Dispose();
                stream = null;
                socket = null;
            }
        }
    }
}
