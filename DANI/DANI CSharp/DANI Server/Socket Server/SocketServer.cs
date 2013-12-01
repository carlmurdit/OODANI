using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Diagnostics;
using DANI_Server.Word_Processing;


namespace SocketServer
{
    class SocketServer
    {
        private cWordProcessor WordProcessor = null;
        private Thread listenThread;
        private TcpListener tcpListener = null;
        public int Port = 11000;

        public void Start(IPAddress ipAddress, cWordProcessor WordProcessor, int Port)
        {
            this.Port = Port;
            if (WordProcessor == null) {
                throw new Exception("WordProcessor not set.");}
            this.WordProcessor = WordProcessor;
            this.listenThread = new Thread(new ParameterizedThreadStart (ListenForClients));
            this.listenThread.Start(ipAddress);
        }

        volatile bool interrupted;
        public void Stop()
        {
            try {
                interrupted = true;
                tcpListener.Server.Close();
                listenThread.Abort();
            }
            catch (Exception e) {
                throw new Exception("Error stopping socket server. " + e.Message, e);
            }      
        }

        private void ListenForClients(object  ipAddress)
        {
            ListenForClients((IPAddress)ipAddress);
        }


        private void ListenForClients(IPAddress ipAddress)
        {

            tcpListener = new TcpListener(ipAddress, Port);
            tcpListener.Start();
            Console.WriteLine("Ready...");

            while (!interrupted)
            {

                //blocks until a client has connected to the server
                TcpClient client = tcpListener.AcceptTcpClient();

                //create a thread to handle communication with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch (Exception e)
                {
                    //a socket error has occured
                    Console.WriteLine("Read Failed.\n" + e.Message);
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    Console.WriteLine("Client has disconnected");
                    break;
                }

                //message has successfully been received
                ASCIIEncoding encoder = new ASCIIEncoding();
                string Input = (encoder.GetString(message, 0, bytesRead));
                Console.WriteLine("C: " + Input);
                Input = Input.Trim();
                try
                {
                    string reply = WordProcessor.Process(Input, true); //"You said: " + Input + "\n";
                    byte[] buffer = encoder.GetBytes(reply);
                    clientStream.Write(buffer, 0, buffer.Length);
                    clientStream.Flush();
                    Console.WriteLine("S: " + reply);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Reply Failed.\n" + e.Message);
                }
            }

            if (tcpClient.Connected)
                tcpClient.Close();
            Console.WriteLine("Disconnected the client.");
        }
    }
}
