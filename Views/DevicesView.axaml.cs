using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;

namespace MyAvaloniaApp.Views
{
    public partial class DevicesView : UserControl
    {
        public ObservableCollection<Device> Devices { get; } = new();

        public DevicesView()
        {
            InitializeComponent();
            this.DataContext = this; // Привязываем данные к самому контролу

            // Добавляем тестовые данные, как на скриншоте
            Devices.Add(new Device { ChannelNumber = "---", Name = "метран 120", SerialNumber = "1234" });
            Devices.Add(new Device { ChannelNumber = "---", Name = "метран 120", SerialNumber = "1234" });
        }

        private void OnAddClick(object? sender, RoutedEventArgs e)
        {
            Devices.Add(new Device { ChannelNumber = "---", Name = "метран 120", SerialNumber = "1234" });
        }

        private void OnRemoveClick(object? sender, RoutedEventArgs e)
        {
            if (Devices.Count > 0)
                Devices.RemoveAt(Devices.Count - 1);
        }

        private void OnSetChannelClick(object? sender, RoutedEventArgs e)
        {
            // Логика установки номера канала (можно добавить диалог ввода)
        }
    }

    public class Device
    {
        public string? ChannelNumber { get; set; }
        public string? Name { get; set; }
        public string? SerialNumber { get; set; }
    }
}