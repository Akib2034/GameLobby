using System;
using System.ServiceModel;

namespace GameLobbyServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Game Lobby Server");

            ServiceHost host = null;
            try
            {
                // Create and configure the NetTcpBinding instance
                NetTcpBinding tcp = new NetTcpBinding
                {
                    TransferMode = TransferMode.Streamed, // Set to Streamed
                    MaxReceivedMessageSize = int.MaxValue, // Adjust as needed
                    MaxBufferSize = int.MaxValue, // Adjust as needed
                    MaxBufferPoolSize = 5242880 // Adjust as needed
                };

                // Initialize the ServiceHost with the type of the service
                host = new ServiceHost(typeof(Server));

                // Add the service endpoint with the configured binding
                host.AddServiceEndpoint(typeof(ServerInterface), tcp, "net.tcp://localhost:8200/DataService");

                // Open the host to start listening for incoming requests
                host.Open();
                Console.WriteLine("System Online");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (host != null && host.State == CommunicationState.Opened)
                {
                    host.Close();
                }
            }
        }
    }
}
