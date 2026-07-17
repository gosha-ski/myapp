using Avalonia.Controls;
using Avalonia.Interactivity;      
using MyAvaloniaApp; 
using System.Collections.Generic;
using System;
using MyAvaloniaApp.Models;
using System.Threading.Tasks;
using Avalonia.VisualTree;

namespace MyAvaloniaApp.Views
{
    public partial class VerificationInfoView : UserControl
    {
        private readonly NewVerificationWindow _ownerWindow;

        public VerificationInfoView(Window ownerWindow)
        {
            _ownerWindow = (NewVerificationWindow)ownerWindow;
            Console.WriteLine($"_ownerWindow.VerificationId: {_ownerWindow.VerificationId}");
            InitializeComponent();
            LoadInspector();
        }

        // Метод для валидации данных перед переходом
        public bool Validate()
        {
            // Здесь можно добавить проверку полей
            return true; 
        }

        public void LoadInspector() 
        {
            Console.WriteLine("LoadInspector");
            InspectorModel? inspector = DbHelper.GetInspectorByVerificationId(_ownerWindow.VerificationId);
            //Console.WriteLine($"{inspector.FirstName} {inspector.MiddleName} {inspector.LastName}");
        
            if (inspector != null)
            {
                string fullname = $"{inspector.FirstName} {inspector.MiddleName} {inspector.LastName}";
                TbSelectInspector.Text = fullname;
                //Console.WriteLine($"{inspector.FirstName} {inspector.MiddleName} {inspector.LastName}");
            }
        }

        public async void BtnAddInspectorInVerificationStepClicked(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("DADADAA");
            var dialog = new SelectInspectorListWindow();
            await dialog.ShowDialog(_ownerWindow);
            if (dialog.SelectedInspector != null)
            {
                InspectorModel inspector = dialog.SelectedInspector;
                string fullname = $"{inspector.FirstName} {inspector.MiddleName} {inspector.LastName}";
                DbHelper.SetInspectorForVerification(_ownerWindow.VerificationId, inspector.Id);
                TbSelectInspector.Text = fullname;
            }

        }
    }
}