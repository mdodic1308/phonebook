using System;
using System.Net;
using System.Net.Sockets;
using System.Data.SqlClient;
using System.Data;

namespace ServerApp
{
    class Server
    {
        const int createCode = 1;
        const int deleteCode = 2;
        const int searchCode = 3;
        const int showAllCode = 4;

        private static TcpListener server = null;
        public static void acceptClientRequest()
        {
            // Buffer for reading data
            Byte[] bytes = new Byte[256000];
            String data = null;

            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("Connected!");

            data = null;

            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();
			
            int i;

            // Loop to receive all the data sent by the client.
            try
            {         
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                    // Process the data sent by the client.
					string[] dataTxt = data.Split('^');
					string response=null;

                    int request = Int32.Parse(dataTxt[0]);

					if (request == createCode)   response = getFromDatabase("insert into \"Table\" (FirstName,LastName,PhoneNumber) values('" + dataTxt[1] + "','" + dataTxt[2] + "','" + dataTxt[3] + "')"); 
					if (request == deleteCode)   response = getFromDatabase("delete from \"Table\" where FirstName='"+dataTxt[1]+ "' and LastName='" + dataTxt[2] + "' and PhoneNumber='" + dataTxt[3] + "'");
					if (request == searchCode)	 response = getFromDatabase("select LastName as 'Last Name', FirstName as 'First Name', PhoneNumber as 'Phone Number' from \"Table\" where LastName like '" + dataTxt[1] + "%' order by 1,2");
					if (request == showAllCode)	 response = getFromDatabase("select LastName as 'Last Name',FirstName as 'First Name',PhoneNumber as 'Phone Number' from \"Table\" order by 1,2");

                    if (response == "") response = "empty";

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(response);

                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                }
            }
            catch (Exception) 
			{  
                Console.WriteLine("Connection closed.");
            }
            finally
            {
                // Shutdown and end connection
                client.Close();
            }
        }

        public static string getFromDatabase(string commandString)
        {
			try
			{
			var conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;" +
                               "AttachDbFilename=" + AppDomain.CurrentDomain.BaseDirectory + "Database1.mdf;" +
                               "Integrated Security=True");
            conn.Open();
            if (conn.State == ConnectionState.Open)
            {
                SqlDataAdapter sda = new SqlDataAdapter(commandString, conn);
                SqlCommand command = new SqlCommand(commandString);
                command.Connection = conn;
                SqlDataReader rdr = command.ExecuteReader();
                string s = "";
				while (rdr.Read())	
                {                              
                    s += rdr.GetString(0) + "^" + rdr.GetString(1) + "^" + rdr.GetString(2)+"^";
                }

                conn.Close();
                return s;
	
            }
            return "error2";
			}
			catch (Exception w)
			{
				Console.WriteLine("Error. Can not opet database. /n Esception:  {0}",w);
				return "error3";
			}
				
        }

        public static void Main()
        {
            try
            {				
                Int32 port = 9091;

                // Getting IPAddress
                IPAddress localAddr = null;
            
                IPAddress[] localIP = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (IPAddress address in localIP)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                       localAddr = address;
                    }
                }
				
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");
                    acceptClientRequest();
                }
            }
            catch (SocketException q)
            {
                Console.WriteLine("SocketException:  {0}", q);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }        
    }
}

