using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
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

        TimerCallback timerDelegate;
        Timer timer;
        public MainWindow()
        {
            InitializeComponent();
        }

        [DllImport("MM0423.dll", CharSet = CharSet.Ansi)]
        private extern static IntPtr GetMemString(String tag);

        [DllImport("MM0423.dll", CharSet = CharSet.Ansi)]
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
            mt4Status.Text = "未連携";
            Trace.WriteLine(getMemorryString(@"mtexe"));
            Trace.WriteLine(getMemorryString(@"mtexe.array"));
            if (getMemorryString(@"mtexe") != null)
            {
                timerDelegate = new TimerCallback(requestMTStatus);
                timer = new Timer(timerDelegate, null, 0, 300);
                statusBorder.Background = Brushes.LightGreen;
                mt4Status.Text = "連携済み";
            }
            if (getMemorryString(@"mtexe.array") != null)
            {
                String[] sep = { "," };
                String[] f = getMemorryString(@"mtexe.array").Split(sep, StringSplitOptions.RemoveEmptyEntries);
                foreach (String str in f)
                {
                    comboBox.Items.Add(str);
                }
            }
        }

        private void requestMTStatus(object state)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    String val = getMemorryString(@"mtexe.value");
                    //Trace.WriteLine(val);

                    String[] sep = { "," };
                    String[] sep2 = { "." };
                    String[] str = val.Split(sep, StringSplitOptions.RemoveEmptyEntries);

                    //Trace.WriteLine(str[0]+","+str[1]);
                    String[] bid = str[0].Split(sep2, StringSplitOptions.RemoveEmptyEntries);
                    String[] ask = str[1].Split(sep2, StringSplitOptions.RemoveEmptyEntries);

                    selltextBlock.Text = "";
                    Underline underline = new Underline();
                    Run run = new Run();
                    run.Text = "SELL\n";
                    run.Foreground = Brushes.White;
                    run.FontSize = 16;
                    underline.Inlines.Add(run);
                    underline.Foreground = Brushes.White;

                    this.selltextBlock.Inlines.Add(underline);
                    selltextBlock.TextAlignment = TextAlignment.Right;

                    this.selltextBlock.Inlines.Add(bid[0] + ".");
                    run = new Run();
                    run.Text = bid[1].Substring(0, 2);
                    run.FontSize = 24;
                    this.selltextBlock.Inlines.Add(run);
                    this.selltextBlock.Inlines.Add(bid[1].Substring(2, int.Parse(str[2])-2));
                    sellBtn.Content = selltextBlock;

                    //buyの処理
                    buytextBlock.Text = "";
                    Underline underline2 = new Underline();
                    Run run2 = new Run();
                    run2.Text = "BUY\n";
                    run2.Foreground = Brushes.White;
                    run2.FontSize = 16;
                    underline2.Inlines.Add(run2);
                    underline2.Foreground = Brushes.White;

                    this.buytextBlock.Inlines.Add(underline2);
                    buytextBlock.TextAlignment = TextAlignment.Left;

                    this.buytextBlock.Inlines.Add(ask[0] + ".");
                    run2 = new Run();
                    run2.Text = ask[1].Substring(0, 2);
                    run2.FontSize = 24;
                    this.buytextBlock.Inlines.Add(run2);
                    this.buytextBlock.Inlines.Add(ask[1].Substring(2, int.Parse(str[2]) - 2));
                    buyBtn.Content = buytextBlock;


                });
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }

        private String getMemorryString(String TAG)
        {
            String ans = "";
            try
            {

                MemoryMappedFile mmf = MemoryMappedFile.OpenExisting(TAG);
                MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);

                char[] data = new char[256];
                int readsize = accessor.ReadArray<char>(0, data, 0, 256);

                ans += new string(data);

                accessor.Dispose();
                mmf.Dispose();

            }
            catch (Exception ex)
            {
                Trace.WriteLine(TAG + "\r\n" + ex.StackTrace);
            }

            return ans;
        }

        private void setMemorryString(String TAG, String msg)
        {
            // Open shared memory
            MemoryMappedFile share_mem = MemoryMappedFile.CreateNew(TAG, 1024);
            MemoryMappedViewAccessor accessor = share_mem.CreateViewAccessor();

            // Write data to shared memory
            string str = msg;
            char[] data = str.ToCharArray();
            accessor.Write(0, data.Length);
            accessor.WriteArray<char>(sizeof(int), data, 0, data.Length);

            // Dispose accessor
            accessor.Dispose();
        }

    }
}
