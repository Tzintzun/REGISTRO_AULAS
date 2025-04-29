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
using AulasSiencb2.Helpers;
using Serilog;

namespace AulasSiencb2.Views
{
    /// <summary>
    /// Lógica de interacción para InputPasswordDialog.xaml
    /// </summary>
    public partial class InputPasswordDialog : Window
    {

        private bool authenticated = false;
        public InputPasswordDialog()
        {
            InitializeComponent();
        }

        
        public void CancelPassword(object sender, RoutedEventArgs e)
        {
            this.authenticated = false;
            this.Close();
        }

        public void LookPassword(object sender, RoutedEventArgs e)
        {
            if (SegurityUtils.VerifyPasswordAdmin(_inputPassword.Password))
            {
                this.authenticated = true;
                this.Close();
            }
            else
            {
                _message.Foreground = Brushes.Red;
                _message.Content = "Contraseña incorrecta";
            }
            
        }

        public void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                this.DialogResult = this.authenticated;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Log.Error($"{ex.Message} - {ex.StackTrace}");

            }
        }
    }
}
