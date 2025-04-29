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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AulasSiencb2.Views
{
    /// <summary>
    /// Lógica de interacción para UserInfo.xaml
    /// </summary>
    public partial class UserInfo : UserControl
    {
        public UserInfo()
        {
            InitializeComponent();
            this.Loaded += OnUserControlLoaded;
        }
        private void OnUserControlLoaded(object sender, RoutedEventArgs e)
        {
            var fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var slideInAnimation = new DoubleAnimation
            {
                From = 50, // Comienza desde 50 píxeles abajo
                To = 0,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            this.BeginAnimation(UserControl.OpacityProperty, fadeInAnimation);
            Transform.BeginAnimation(TranslateTransform.YProperty, slideInAnimation);
        }

        private void onUserControlUnloaded(object sender, RoutedEventArgs e)
        {
            var fadeInAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var slideInAnimation = new DoubleAnimation
            {
                From = 0, // Comienza desde 50 píxeles abajo
                To = 50,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            this.BeginAnimation(UserControl.OpacityProperty, fadeInAnimation);
            Transform.BeginAnimation(TranslateTransform.YProperty, slideInAnimation);
        }
    }
}
