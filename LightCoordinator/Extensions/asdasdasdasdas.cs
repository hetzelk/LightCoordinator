
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
class Program
{
    public void asd(string[] args)
    {
        Boolean done = false;
        Boolean exception_thrown = false;

        Socket sending_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
        ProtocolType.Udp);

        IPAddress send_to_address = IPAddress.Parse("192.168.2.255");

        IPEndPoint sending_end_point = new IPEndPoint(send_to_address, 11000);

        Console.WriteLine("Enter text to broadcast via UDP.");
        Console.WriteLine("Enter a blank line to exit the program.");
        while (!done)
        {
            Console.WriteLine("Enter text to send, blank line to quit");
            string text_to_send = Console.ReadLine();
            if (text_to_send.Length == 0)
            {
                done = true;
            }
            else
            {
                byte[] send_buffer = Encoding.ASCII.GetBytes(text_to_send);

                // Remind the user of where this is going.
                Console.WriteLine("sending to address: {0} port: {1}",
                sending_end_point.Address,
                sending_end_point.Port);
                try
                {
                    sending_socket.SendTo(send_buffer, sending_end_point);
                }
                catch (Exception send_exception)
                {
                    exception_thrown = true;
                    Console.WriteLine(" Exception {0}", send_exception.Message);
                }
                if (exception_thrown == false)
                {
                    Console.WriteLine("Message has been sent to the broadcast address");
                }
                else
                {
                    exception_thrown = false;
                    Console.WriteLine("The exception indicates the message was not sent.");
                }
            }
        } // end of while (!done)
    } // end of main()
} // end of class Program
