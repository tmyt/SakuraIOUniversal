using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SakuraIO.Extensions;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace SakuraIO.Sample
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int WAKE_OUT = 27;

        private SakuraIO_I2C sakuraio = new SakuraIO_I2C();
        private Dictionary<int, int> requests = new Dictionary<int, int>();

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;
        }

        private void PinOnValueChanged()
        {
            if (sakuraio.GetRxQueueLength(out _, out var queued) != Error.None) return;
            if (queued == 0) return;
            for (var i = 0; i < queued; ++i)
            {
                sakuraio.DequeueRx(out var ch, out _, out var value, out var offset);
                var now = DateTime.Now.AddMilliseconds(-(long)offset);
                Log($"[{now}] {ch}: {value.Select(x => x.ToString("X2")).JoinToString(" ")}");
                requests[ch] = BitConverter.ToInt32(value, 0);
            }

            Process();
        }

        private void Process()
        {
            if (requests.Count != 2) return;
            sakuraio.EnqueueTx(0, requests[0] + requests[1]);
            sakuraio.Send();
            requests.Clear();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await sakuraio.OpenAsync("I2C1");
            Log("[System]: Waiting for connection");
            await sakuraio.WaitForConnectionAsync();
            Log("[System]: Connection established");
            RegisterGPIO();
            PinOnValueChanged();
        }

        private void RegisterGPIO()
        {
            var gpio = GpioController.GetDefault();
            var pin = gpio.OpenPin(WAKE_OUT);
            pin.SetDriveMode(GpioPinDriveMode.InputPullDown);
            pin.ValueChanged += (_, __) =>
            {
                if (pin.Read() != GpioPinValue.High) return;
                Dispatcher.RunIdleAsync(x => PinOnValueChanged());
            };
        }

        private void Log(string message)
        {
            list.Items.Insert(0, message);
        }
    }

    static class A
    {
        public static string JoinToString(this IEnumerable<string> that, string separator)
        {
            return string.Join(separator, that);
        }
    }
}
