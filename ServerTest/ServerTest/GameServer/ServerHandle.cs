using System;
using GameServer.Packets;

namespace GameServer
{
    public class ServerHandle
    {
        public static void Request(int fromClient, Packet packet)
        {
            string request;
            Packet packetResponse;

            if (packet.UnreadLength() > 0)
            {
                request = packet.ReadString();

                packetResponse = new Packet();

                Console.WriteLine($"Client Id {fromClient} made the request {request}");

                if (request == "CHICHA")
                {
                    packetResponse.Write("LIMONADA");
                    packetResponse.ClosePacket();
                    Server.clients[fromClient].tcp.SendData(packetResponse);
                }
                else if (request == "PALTA")
                {
                    Console.WriteLine($"I like paltas. I'll store this one. Thanks {fromClient}");
                }
                else
                {
                    packetResponse.Write("I'M A VEGAN. CAN'T EAT THAT!!!.");
                    packetResponse.ClosePacket();
                    Server.clients[fromClient].tcp.SendData(packetResponse);
                }
            }

        }
    }
}
