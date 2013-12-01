using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DANI_Client
{
    class Program
    {

        static void Main(string[] args)
        {
            SocketClient client = null;
            string input;

            while (true)
            {
                try
                {
                    Console.WriteLine("First start the DANI Server.");
                    //Get IP
                    Console.WriteLine("Enter its IP Address or press Enter to accept the default (192.168.3.11):");
                    input = Console.ReadLine();
                    if (input == "") {
                        input = "192.168.3.11";
                        Console.WriteLine(input);
                    } else if (input.ToLower() == "exit") {
                        return; }
                    string IP = input;
                    //Get Port
                    Console.WriteLine("Enter its Port Number or press Enter to accept the default (11000):");
                    input = Console.ReadLine();
                    if (input == "") {
                        input = "11000";
                        Console.WriteLine(input);
                    }
                    else if (input.ToLower() == "exit") {
                        return;
                    }
                    int Port = int.Parse(input);
                    //instanciate client
                    client = new SocketClient(IP, Port);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: "+e.Message);
                }
            }

            Console.WriteLine("Ask DANI:");
            input = Console.ReadLine();
            while (input.ToLower() != "exit")
            {
                client.StartClient(input);
                input = Console.ReadLine();
            } 

        }
    }
}
