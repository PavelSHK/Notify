using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Notify.Controls
{
    public sealed partial class HeaderControl : UserControl
    {
        MainPage mainPage;
        public HeaderControl()
        {
            this.InitializeComponent();
            this.DataContext = this;
            mainPage = new MainPage();
        }

        public delegate void ValueChangedEventHandler(object sender, EventArgs e);

        public event ValueChangedEventHandler ValueChanged;

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(HeaderControl), null);

        public string Label
        {
            get { return GetValue(LabelProperty) as string; }
            set { SetValue(LabelProperty, value); }
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ValueChanged(this, EventArgs.Empty);
        }

       
    }
}
