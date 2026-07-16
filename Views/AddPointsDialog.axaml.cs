using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace MyAvaloniaApp.Views
{
    public partial class AddPointsDialog : Window
    {
        // Результат, который мы вернем вызывающему коду
        public (double Start, double End, int Count) Result { get; private set; }
        public bool IsCancelled { get; private set; } = true; // По умолчанию считаем, что отменили
        public event Action? OnSaved;

        public AddPointsDialog()
        {
            InitializeComponent();
            InitializeLogic();
        }

        
        private void InitializeLogic()
        {
            // Привязка событий к кнопкам
            BtnOk.Click += OnOkClick;
            BtnCancel.Click += OnCancelClick;

            // Установка фокуса на первое поле при открытии
            this.Loaded += (s, e) => FirstPointInput.Focus();
        }

        private void OnOkClick(object? sender, RoutedEventArgs e)
        {
            if (TryParseInputs(out double start, out double end, out int count))
            {
                // Проверка логики: начало не может быть больше конца
                if (start > end)
                {
                    // Можно показать MessageBox, но для простоты просто не закрываем
                    FirstPointInput.Watermark = "Начало > Конец!";
                    FirstPointInput.Focus();
                    return;
                }

                Result = (start, end, count);
                IsCancelled = false;
                //OnSaved?.Invoke();
                this.Close();
            }
            else
            {
                // Если парсинг не удался (не число), подсветим первое поле
                FirstPointInput.Watermark = "Введите число!";
                FirstPointInput.Focus();
            }
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            IsCancelled = true;
            this.Close();
        }

        // Вспомогательный метод для безопасного получения чисел
        private bool TryParseInputs(out double start, out double end, out int count)
        {
            start = 0;
            end = 0;
            count = 0;

            // Используем InvariantCulture для точки или текущей культуры для запятой
            // Для инженерного софта часто лучше явно парсить с учетом культуры пользователя
            if (!double.TryParse(FirstPointInput.Text, out start)) return false;
            if (!double.TryParse(LastPointInput.Text, out end)) return false;
            if (!int.TryParse(CountInput.Value.ToString(), out count)) return false;

            return true;
        }
    }
}