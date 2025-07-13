using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

public class UdpMovementSender : MonoBehaviour
{
    public string playerId = "player1";

    private Socket socket;
    private IPEndPoint serverEndPoint;

    void Awake()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4000);
    }

    void OnDestroy()
    {
        if (socket != null)
        {
            socket.Close();
            socket = null;
        }
    }


    public void SendMovement(Vector3 delta)
    {
        if (socket == null)
            return;

        byte[] packet = BuildPacket(delta);
        Debug.Log("Sending UDP packet: " + BitConverter.ToString(packet));
        socket.SendTo(packet, serverEndPoint);
    }

    byte[] BuildPacket(Vector3 delta)
    {
        byte[] idBytes = Encoding.UTF8.GetBytes(playerId ?? string.Empty);
        if (idBytes.Length > 255)
            throw new InvalidOperationException("playerId too long");

        byte[] buffer = new byte[1 + idBytes.Length + 2 + 4 * 3];
        int offset = 0;

        buffer[offset++] = (byte)idBytes.Length;
        Array.Copy(idBytes, 0, buffer, offset, idBytes.Length);
        offset += idBytes.Length;

        WriteShort(buffer, ref offset, 1); // opcode for movement
        WriteFloat(buffer, ref offset, delta.x);
        WriteFloat(buffer, ref offset, delta.y);
        WriteFloat(buffer, ref offset, delta.z);

        return buffer;
    }

    void WriteShort(byte[] buffer, ref int offset, short value)
    {
        buffer[offset++] = (byte)((value >> 8) & 0xFF);
        buffer[offset++] = (byte)(value & 0xFF);
    }

    void WriteFloat(byte[] buffer, ref int offset, float value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        Array.Copy(bytes, 0, buffer, offset, 4);
        offset += 4;
    }

}
