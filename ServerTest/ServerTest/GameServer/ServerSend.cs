using System.Collections.Generic;
using GameServer.Packets;

namespace GameServer
{
    public class ServerSend
    {
        private static void SendTCPData(int toClient, Packet packet) {
            packet.ClosePacket();
            Server.clients[toClient].tcp.SendData(packet);
        }

        private static void SendDataToAll(Packet packet)
        {
            packet.ClosePacket();
            for (int i = 0; i < Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }

        private static void SendDataToAll(Packet packet, List<int> exceptClients)
        {
            packet.ClosePacket();
            for (int i = 0; i < Server.MaxPlayers; i++)
            {
                if (!exceptClients.Contains(i))
                {
                    Server.clients[i].tcp.SendData(packet);
                }
            }
        }

        public static void Welcome(int toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(true);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }
    }
}
