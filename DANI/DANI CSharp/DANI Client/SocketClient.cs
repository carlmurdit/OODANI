using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DANI_Client
{

    public class SocketClient
    {

        private IPAddress ServerIP = null;
        private int Port = 0;

        public SocketClient(string ServerIP, int Port)
        {
            if (IPAddress.TryParse(ServerIP, out this.ServerIP))
                this.Port = Port;
            else
                throw new Exception (String.Format("IP Address {0} is invalid.", ServerIP));
        }


        public void StartClient(string msg)
        {
            // Data buffer for incoming data.
            byte[] bytes = new byte[1024];

            // Connect to a remote device.
            try
            {

                // Establish the remote endpoint for the socket.
                IPEndPoint remoteEP = new IPEndPoint(ServerIP, Port);

                // Create a TCP/IP  socket.
                Socket sender = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    // Encode the data string into a byte array.
                    byte[] msgbytes = Encoding.ASCII.GetBytes(msg);

                    // Send the data through the socket.
                    int bytesSent = sender.Send(msgbytes);

                    // Receive the response from the remote device.
                    int bytesRec = sender.Receive(bytes);
                    Console.WriteLine("DANI: {0}",
                        Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    // Release the socket.
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

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

    }
}
