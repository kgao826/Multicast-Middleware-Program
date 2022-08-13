using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;

namespace Middleware1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int msgcount;
        public int timeStamp = 1;
        const int myPort = 8082;
        public Dictionary<string, int> holdQueue = new();
        public MainWindow()
        {
            InitializeComponent();
            ReceiveMulticast();
            msgcount = 1; //Resets te message count
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            DoWork();
        }

        // This method sets up a socket for receiving messages from the Network
        private async void ReceiveMulticast()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];
            // Determine the IP address of localhost
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = null;
            int[] ports = { 8081, 8082, 8083, 8084, 8085, 8086 };
            foreach (IPAddress ip in ipHostInfo.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ip;
                    break;
                }
            }
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, myPort);
            // Create a TCP/IP socket for receiving message from the Network.
            TcpListener listener = new(localEndPoint);
            listener.Start();
            Dictionary<string, int> replyCount = new();
            Dictionary<string, int> deliveryQueue = new();
            string data = null;
            // Start listening for connections.
            while (true)
            {
                TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                data = null;
                while (true)
                {
                    bytes = new byte[1024];
                    NetworkStream readStream = tcpClient.GetStream();
                    int bytesRec = await readStream.ReadAsync(bytes, 0, 1024);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    int senderId = GetSenderID(data);
                    int msgNo = GetMsgNo(data);
                    string MsgArray = senderId + " " + msgNo;
                    System.Diagnostics.Debug.WriteLine(data);
                    if (!holdQueue.ContainsKey(MsgArray))
                    {
                        holdQueue.Add(MsgArray, 0);
                    }
                    if (data.Contains("<EOM>") && !data.Contains("largest"))
                    {
                        UpdateRecievedListBox(data);
                        Socket sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        sendSocket.Connect(ipAddress, ports[senderId]);
                        string messageFront = data.Substring(0, data.IndexOf("Middleware ") + 11);
                        string timestampMessage = messageFront + senderId + " at " + timeStamp;
                        byte[] message = Encoding.ASCII.GetBytes(timestampMessage);
                        sendSocket.Send(message);
                        sendSocket.Shutdown(SocketShutdown.Both);
                        sendSocket.Close();
                        holdQueue[MsgArray] = timeStamp;
                        timeStamp += 1;
                    }
                    else if (data.Contains("at"))
                    {
                        int recievedMsgTimestamp = GetTimestamp(data);
                        if (!replyCount.ContainsKey(MsgArray))
                        {
                            replyCount.Add(MsgArray, 0);
                        }
                        else
                        {
                            replyCount[MsgArray] = replyCount[MsgArray] + 1;
                            foreach (KeyValuePair<string, int> item in holdQueue)
                            {
                                if (item.Key == MsgArray)
                                {
                                    if (item.Value < recievedMsgTimestamp)
                                    {
                                        holdQueue[MsgArray] = recievedMsgTimestamp;
                                    }
                                }
                            }
                            if (replyCount[MsgArray] == 4)
                            {
                                for (int i = 1; i <= 5; i++)
                                {
                                    Socket sendSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                    sendSocket.Connect(ipAddress, ports[i]);
                                    string messageFront = data.Substring(0, data.IndexOf("Middleware ") + 11);
                                    string timestampMessage = messageFront + senderId + " largest " + holdQueue[MsgArray] + "<EOM>";
                                    byte[] message = Encoding.ASCII.GetBytes(timestampMessage);
                                    sendSocket.Send(message);
                                    sendSocket.Shutdown(SocketShutdown.Both);
                                    sendSocket.Close();
                                }
                            }
                        }

                    }
                    else if (data.Contains("largest"))
                    {
                        readyListBox.Items.Clear();
                        int largestTimeStamp = Int32.Parse(data.Substring(data.IndexOf("largest ") + 8, data.IndexOf("<EOM>") - (data.IndexOf("largest ") + 8)));
                        deliveryQueue.Add(MsgArray, largestTimeStamp); //Put back here!
                        int count = 0;
                        List<string[]> tempQueue = new();
                        string[] identifiers;
                        int mdlware;
                        while (count <= largestTimeStamp)
                        {
                            foreach (KeyValuePair<string, int> message in deliveryQueue)
                            {
                                identifiers = message.Key.Split(" ");
                                mdlware = Int32.Parse(identifiers[0]);
                                if (message.Value == count)
                                {
                                    tempQueue.Add(identifiers);
                                }
                            }
                            for (int i = 1; i <= 5; i++)
                            {
                                foreach (string[] item in tempQueue)
                                {
                                    if (Int32.Parse(item[0]) == i)
                                    {
                                        UpdateReadyListBox("Msg #" + item[1] + " from Middleware " + item[0] + " <EOM>");
                                    }
                                }
                            }
                            tempQueue = new();
                            count++;
                        }

                    }
                    break;
                }

            }
        }
        public int GetTimestamp(string message)
        {
            int timestamp = Int32.Parse(message.Substring(message.IndexOf("at ") + 3));
            return timestamp;
        }

        public int GetMsgNo(string message)
        {
            int msgNo = Int32.Parse(message.Substring(5, message.IndexOf(" from Middleware") - 5));
            return msgNo;
        }

        public int GetSenderID(string message)
        {
            int senderId = Int32.Parse(message.Substring(message.IndexOf("Middleware ") + 11, 1));
            return senderId;
        }
        // This method first sets up a task for receiving messages from the Network.
        // Then, it sends a multicast message to the Netwrok.
        public void DoWork()
        {
            // Send a multicast message to the Network
            try
            {
                // Find the IP address of localhost
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
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 8081);
                Socket sendSocket;
                try
                {
                    sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sendSocket.Connect(remoteEP);
                    string content = "Msg #" + msgcount + " from Middleware 1 <EOM>";
                    msgcount++;
                    byte[] message = Encoding.ASCII.GetBytes(content);
                    sendSocket.Send(message);
                    sentListBox.Items.Add(content);
                    sendSocket.Shutdown(SocketShutdown.Both);
                    sendSocket.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void SortHoldQueue()
        {

        }
        private void UpdateRecievedListBox(string data)
        {

            this.Dispatcher.Invoke((Action)(() =>
            {
                receivedListBox.Items.Add(data);
            }));

        }

        private void UpdateReadyListBox(string data)
        {

            this.Dispatcher.Invoke((Action)(() =>
            {
                readyListBox.Items.Add(data);
            }));

        }
    }
}
