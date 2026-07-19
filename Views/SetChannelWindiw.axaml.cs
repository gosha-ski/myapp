using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using MyAvaloniaApp;

namespace MyAvaloniaApp.Views
{
    public partial class SetChannelWindow : Window
    {
        private readonly int _verificationId;
        private readonly int _instrumentId;

        // Конструктор принимает ID поверки и прибора, чтобы сохранить канал именно для них
        public SetChannelWindow(int verificationId, int instrumentId)
        {
            _verificationId = verificationId;
            _instrumentId = instrumentId;

            InitializeComponent();

            // По умолчанию ставим 1, как на скриншоте
            ChannelInput.Text = "1";
            ChannelInput.Focus();
        }

        private void OnOkClick(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ChannelInput.Text))
            {
                ShowError("Введите номер канала.");
                return;
            }

            if (!int.TryParse(ChannelInput.Text, out int channelValue) || channelValue <= 0)
            {
                ShowError("Номер канала должен быть положительным целым числом.");
                return;
            }

            try
            {
                Console.WriteLine($"SETTED {channelValue}");
                
                DbHelper.SetInstrumentChannel(_verificationId, _instrumentId, channelValue);

                this.Close(true); // Закрываем окно после успешного сохранения
            }
            catch (Exception ex)
            {
                ShowError($"Не удалось сохранить канал: {ex.Message}");
            }
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShowError(string message)
        {
            // Простой способ показать ошибку — через MessageBox
            //MessageBox.Show(this, message, "Ошибка", Avalonia.Controls.MessageBoxButton.OK, Avalonia.Controls.MessageBoxIcon.Error);
        }
    }
}
