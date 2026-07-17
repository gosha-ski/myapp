using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Linq;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.Views
{
    public partial class NewVerificationWindow : Window
    {
        // Храним экземпляры контролов, чтобы данные не сбрасывались при переходах
        private readonly List<Control> _stepViews = new();

        public List<InstrumentModel> SelectedInstruments { get; set; } = new();
        public List<TemplateModel> SelectedTemplates { get; set; } = new();

        private int _currentIndex = 0;

        public NewVerificationWindow()
        {
            InitializeComponent();
            InitializeSteps();
        }

        private void InitializeSteps()
        {
            _stepViews.Add(new VerificationInfoView(this)); 
            _stepViews.Add(new DevicesView(this));
            _stepViews.Add(new InputTemplateView(this));
            _stepViews.Add(new OuterTemplateView(this));
            _stepViews.Add(new ProbingView(this));
            _stepViews.Add(new LoaderRangeView(this));
            _stepViews.Add(new VerificationOperationsView(this));
            

            var stepNames = new List<string>
            {
                "Сведения о поверке",
                "Поверяемые приборы",
                "Входной эталон",
                "Выходной эталон",
                "Опробация",
                "Ряд нагружения",
                "Операции поверки"
            };

            StepsTree.ItemsSource = stepNames;
            
            // Сразу показываем первый шаг
            ShowStep(0);
        }

        // Метод, который просто ставит нужный контрол в область контента
        private void ShowStep(int index)
        {
            if (index < 0 || index >= _stepViews.Count) return;

            _currentIndex = index;
            StepContent.Content = _stepViews[index];
            if (index == 4 )
            {
                ProbingView step = _stepViews[index] as ProbingView;
                step.InitializeWithInstruments();
            }

            // Синхронизируем выделение в дереве (чтобы галочка тоже прыгнула)
            var names = StepsTree.ItemsSource?.Cast<string>().ToList();
            if (names != null && index < names.Count)
            {
                StepsTree.SelectedItem = names[index];
            }

            UpdateNavigationButtons();
        }

        private void UpdateNavigationButtons()
        {
            BtnPrev.IsEnabled = _currentIndex > 0;
            BtnNext.IsEnabled = _currentIndex < _stepViews.Count - 1;
        }

        // Кнопки "Назад/Вперед" (оставляем их, они просто вызывают ShowStep)
        private void OnPrevClick(object? sender, RoutedEventArgs e)
        {
            if (_currentIndex > 0)
                ShowStep(_currentIndex - 1);
        }

        private void OnNextClick(object? sender, RoutedEventArgs e)
        {
            if (_currentIndex < _stepViews.Count - 1)
                ShowStep(_currentIndex + 1);
        }

        // ГЛАВНОЕ: обработка клика по дереву
        private void OnStepSelected(object? sender, SelectionChangedEventArgs e)
        {
            var selected = StepsTree.SelectedItem as string;
            if (selected == null) return;

            var names = StepsTree.ItemsSource?.Cast<string>().ToList();
            if (names == null) return;

            int index = names.IndexOf(selected);
            
            // Если нашли индекс и он отличается от текущего — переключаем
            if (index >= 0 && index != _currentIndex)
            {
                ShowStep(index);
            }
        }
    }
}