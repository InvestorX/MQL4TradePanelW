using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MQL4TradePanelW
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        [DllImport("MemMap.dll", CharSet = CharSet.Ansi)]
        private extern static IntPtr GetMemString(String tag);

        [DllImport("MemMap.dll", CharSet = CharSet.Ansi)]
        private extern static IntPtr SetMemString(string tag, string msg);


        private void lots_PreviewMouseDown(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("PreviewMouseDown lotsがクリックされた気がする");
            //popupLots.IsOpen = !popupLots.IsOpen;

        }

        private void lots_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Trace.WriteLine("PreviewMouseUP lotsがクリックされた気がする");
            popupLots.IsOpen = !popupLots.IsOpen;

        }

        private void sellBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void popupBtn_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine(((Button)sender).Content.ToString());
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            popupLots.IsOpen = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TimerCallback timerDelegate = new TimerCallback(requestMTStatus);
            Timer timer = new Timer(timerDelegate, null, 0, 200);
        }

        private void requestMTStatus(object state)
        {
            try
            {
                GetMemString("MT2EXE");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }
    }
}
