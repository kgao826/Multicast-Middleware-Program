using System;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// Class for handling a message sent by a user.
// The class simulates the delay in message transmission.
// A message sent by an user is broacast to all the users in the system.
// The broadcast is carried out by a point-to-point send to each of the users.
// The time at which the message is sent to an user is determined randomly.
public class MessageHandler
{
    // Maximum delay for a message
    const int maxSleepTime = 20000;

    // The number of ports for receiving the message
    const int numPorts = 5;

    // Ports for receiving the message
    int[] ports = { 8082, 8083, 8084, 8085, 8086 };

    // Channel for receiving the message from the original sender of the message
    NetworkStream readStream;

    // The amount of delays before a message is sent to the corresponding users (i.e. ports)
    // ports and sleepTime are two correspoding arrays. That is, sleepTime[i] is the delay time for port ports[i].
    int[] sleepTime = new int[numPorts];

    byte[] bytes;
    string data;

    public MessageHandler(TcpClient tcpClient) { readStream = tcpClient.GetStream(); }

    // This method carries out the work of a MessageHandler object
    // First, the MessageHandler reads the multicast message from the sender.
    // Then, the messages is broadcast to all the middleware.
    public async void DoWork()
    {
        // Obtains the message from the sender.
        await readMsg();

        // Broadcast the message.
        sendMsg();
    }


    // The method for receiving the message from the original sender of the message
    private async Task readMsg()
    {
        // Read the message from the original sender
        while (true)
        {
            bytes = new byte[1024];
            int bytesRec = await readStream.ReadAsync(bytes, 0, 1024);

            // Convert received bytes to a string.
            data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

            // Each message is ended with "<EOM>".
            // Check whether the end of the message is reached
            if (data.IndexOf("<EOM>") > -1)
            {
                break;
            }
        }

        // Set the delay for sending the message to each of the middleware.
        Random delayTime = new Random();
        for (int i = 0; i < numPorts; i++)
            sleepTime[i] = delayTime.Next(maxSleepTime);

        Console.Write("Message received: {0}", data);
        for (int i = 0; i < numPorts; i++)
        {
            Console.WriteLine("delay time for {1} is {0}", sleepTime[i], ports[i]);
        }
    }

    // Send the message to each of the users after a delay
    private void sendMsg()
    {
        // Sort the users (i.e. ports) according to their delay time
        Array.Sort(sleepTime, ports);

        // Broadcast the multicast message using point-to-point communication.
        for (int i = 0; i < numPorts; i++)
        {
            // delay the sending of a message
            if (i == 0)
                Thread.Sleep(sleepTime[i]);
            else
                Thread.Sleep(sleepTime[i] - sleepTime[i - 1]);

            byte[] msg = Encoding.ASCII.GetBytes(data);
            Socket sendSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Find out the IP address of the localhost
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = null;
            foreach (IPAddress ip in ipHostInfo.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ip;
                    break;
                }
            }
            // Determine the address of the user (i.e. the receiver of the messsage
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, ports[i]);

            // Connect to the user and send the message
            sendSocket.Connect(remoteEP);
            sendSocket.Send(msg);
            sendSocket.Shutdown(SocketShutdown.Both);
            sendSocket.Close();
        }
    }
}

public class Network
{
    public string data = null;

    // The Network object creates a MessageHandler object for handling each message sent by the middleware.
    public async void DoWork()
    {
        // Data buffer for incoming data.
        byte[] bytes = new Byte[1024];

        // Determine the IP address of the localhost
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = null;
        foreach (IPAddress ip in ipHostInfo.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                ipAddress = ip;
                break;
            }
        }
        // The socket of the Network is at port 8081
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8081);

        // Create a TCP/IP socket for receiving incoming multicast message.
        TcpListener listener = new TcpListener(localEndPoint);
        listener.Start(100);

        try
        {
            while (true)
            {
                Console.WriteLine("Waiting for a connection...");

                // Program is suspended while waiting for an incoming connection.
                TcpClient tcpClient = await listener.AcceptTcpClientAsync();

                // A MessageHandler is created for processing a connection request.
                MessageHandler worker = new MessageHandler(tcpClient);
                worker.DoWork();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public static int Main(String[] args)
    {
        Network s = new Network();
        s.DoWork();
        Console.WriteLine("\nPress ENTER to terminate...");
        Console.ReadLine();
        return 0;
    }
}