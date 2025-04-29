using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AulasSiencb2.ViewModel;

namespace AulasSiencb2.Views
{
    /// <summary>
    /// Lógica de interacción para ScreenLogOut.xaml
    /// </summary>
    public partial class ScreenLogOut : Window
    {
        public ScreenLogOut()
        {
            InitializeComponent();
        }

        private async void OnLoad(object sender, EventArgs eventArgs)
        {
            if (DataContext is ScreenLogOutViewmodel viewModel) {

              await viewModel.LoadOptions();
            }

        }

        public void Configure()
        {
            Debug.WriteLine(this.DataContext.GetType().Name);
            if (DataContext is ScreenLogOutViewmodel viewModel)
            {
                
                viewModel.CloseWindowEvent += () =>
                {
                    Debug.WriteLine("Cerrando Salida");
                    Close();
                };
            }

            
        }

        private void OnCloseWindow(object sender, EventArgs eventArgs)
        {
            if (DataContext is ScreenLogOutViewmodel viewModel)
            {
                
                this.DialogResult = viewModel._dialogResult;
            }
        }
    }
}
