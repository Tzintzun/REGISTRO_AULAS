using System;
using System.Collections.Generic;
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
    /// Lógica de interacción para ScreenServices.xaml
    /// </summary>
    public partial class ScreenServices : Window
    {
        public ScreenServices()
        {
            InitializeComponent();
        }
        private async void OnLoad(object sender, EventArgs eventArgs)
        {
            if (DataContext is ScreenServiceViewModel viewModel)
            {
                await viewModel.LoadBance();
                await viewModel.LoadServices();
                
            }

        }

        public void Configure()
        {
            if (DataContext is ScreenServiceViewModel viewModel)
            {
                viewModel.CloseWindowEvent += () =>
                {
                    Close();
                };
            }
        }
        private void OnClose(object sender, EventArgs eventArgs)
        {
            if (DataContext is ScreenServiceViewModel viewModel)
            {
                this.DialogResult =  viewModel._dialogResult;
            }
        }
    }

    

}
