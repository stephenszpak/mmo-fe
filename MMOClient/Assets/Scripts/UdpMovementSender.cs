using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class UdpMovementSender : MonoBehaviour
{
    public string playerId = "player1";
    public float deltaDistance = 1f;

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

    void Update()
    {
        Vector3 delta = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            delta += Vector3.forward * deltaDistance;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            delta += Vector3.back * deltaDistance;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            delta += Vector3.left * deltaDistance;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            delta += Vector3.right * deltaDistance;

        if (delta != Vector3.zero)
            SendMovement(delta);
    }

    public void SendMovement(Vector3 delta)
    {
        if (socket == null)
            return;

        byte[] packet = BuildPacket(delta);
        socket.SendTo(packet, serverEndPoint);
    }

    byte[] BuildPacket(Vector3 delta)
    {
        byte[] buffer = new byte[4 + 2 + 4 * 3];
        int offset = 0;

        WriteInt(buffer, ref offset, GetStableHash(playerId));
        WriteShort(buffer, ref offset, 1); // opcode for movement
        WriteFloat(buffer, ref offset, delta.x);
        WriteFloat(buffer, ref offset, delta.y);
        WriteFloat(buffer, ref offset, delta.z);

        return buffer;
    }

    void WriteInt(byte[] buffer, ref int offset, int value)
    {
        buffer[offset++] = (byte)((value >> 24) & 0xFF);
        buffer[offset++] = (byte)((value >> 16) & 0xFF);
        buffer[offset++] = (byte)((value >> 8) & 0xFF);
        buffer[offset++] = (byte)(value & 0xFF);
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

    public static int GetStableHash(string str)
    {
        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length; i++)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[++i];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
}
