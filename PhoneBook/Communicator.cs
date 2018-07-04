using System;
using System.Net.Sockets;

namespace PhoneBook
{
    class Communicator
    {
        private static TcpClient client;
        private static NetworkStream stream;

        public static void connecToServer(string ipAddres)
        {
            try
            {
                Int32 port = 9091;
                client = new TcpClient(ipAddres, port);
                stream = client.GetStream();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        public static string sendRequest(string request)
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(request);

            // Send the message to the connected TcpServer. 
            stream.Write(data, 0, data.Length);

            // Receive the TcpServer.response.

            // Buffer to store the response bytes.
            data = new Byte[256000];
            // String to store the response ASCII representation.
            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

            return responseData;
        }

        public static void close()
        {
            // Close everything.
            stream.Close();
        }
    }
    
}
