using System.Net.Sockets;

namespace ExperimentConfigSidecar.Services
{

    public class StartupService
    {
        public async Task WaitForStartup(int appPort)
        {
            while (true)
            {
                try
                {
                    using TcpClient client = new();
                    await client.ConnectAsync("localhost", appPort);
                    break;
                }
                catch
                {
                    await Task.Delay(100);
                }
            }
        }
    }

}