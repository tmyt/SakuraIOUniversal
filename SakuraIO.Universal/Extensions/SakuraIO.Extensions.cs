using System.Threading.Tasks;

namespace SakuraIO.Extensions
{
    public static class SakuraIOExtensions
    {
        public static async Task WaitForConnectionAsync(this SakuraIO sakuraio)
        {
            while (sakuraio.GetConnectionStatus() != 0x80)
            {
                await Task.Delay(1000);
            }
        }
    }
}
