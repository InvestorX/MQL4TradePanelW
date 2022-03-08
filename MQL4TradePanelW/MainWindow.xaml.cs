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
        MemoryMappedFile share_mem;
        MemoryMappedFile shareMemOrder;
        String pullTAG = "mtexe";
        String pushTAG = "exemt";
        bool orderFlag;
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
            //Trace.WriteLine("PreviewMouseDown lotsがクリックされた気がする");
            //popupLots.IsOpen = !popupLots.IsOpen;

        }

        private void lots_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //Trace.WriteLine("PreviewMouseUP lotsがクリックされた気がする");
            popupLots.IsOpen = !popupLots.IsOpen;

        }

        private void buyBtn_Click(object sender, RoutedEventArgs e)
        {
            int tp;
            int sl;
            double lot;
            tp = (bool)(tpCbx.IsChecked) ? int.Parse(tpTbx.Text) : 0;
            sl = (bool)(slCbx.IsChecked) ? int.Parse(slTbx.Text) : 0;
            lot = double.Parse(lots.Text);
            MemoryMappedViewAccessor accessor = shareMemOrder.CreateViewAccessor();
            // buyは0
            // Write data to shared memory
            string str = "0," + lot + ",,," + sl + "," + tp + ",PANEL," + (DateTime.Now).Millisecond;
            //Trace.WriteLine(str);
            char[] data = str.ToCharArray();
            accessor.Write(0, data.Length);
            accessor.WriteArray<char>(0, data, 0, data.Length);

            // Dispose accessor
            accessor.Dispose();

            //setMemorryString(pushTAG + ".order", str);
            orderFlag = true;
        }

        private void sellBtn_Click(object sender, RoutedEventArgs e)
        {
            //int RequestOrder(string mySymbol,int orderSB,double lot,double value,double sp,double SL,double TP,string comment,int mgno,int orderLimit,color orderColor)
            //Trace.WriteLine("SELL button");
            int tp;
            int sl;
            double lot;
            tp = (bool)(tpCbx.IsChecked) ? int.Parse(tpTbx.Text) : 0;
            sl = (bool)(slCbx.IsChecked) ? int.Parse(slTbx.Text) : 0;
            lot = double.Parse(lots.Text);
            MemoryMappedViewAccessor accessor = shareMemOrder.CreateViewAccessor();
            // sellは1
            // Write data to shared memory
            string str = "1," + lot + ",,," + sl + "," + tp + ",PANEL,"+(DateTime.Now).Millisecond+",";
            //Trace.WriteLine(str);
            char[] data = str.ToCharArray();
            accessor.Write(0, data.Length);
            accessor.WriteArray<char>(0, data, 0, data.Length);

            // Dispose accessor
            accessor.Dispose();

            //setMemorryString(pushTAG + ".order", str);
            orderFlag = true;
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
            orderFlag = false;
            mt4Status.Text = "未連携";
            Trace.WriteLine("mtexe:" + getMemorryString(pullTAG) + ":");
            Trace.WriteLine(getMemorryString(pullTAG+".array"));
            String getSymbol = getMemorryString("mtexe").Trim().Replace("\0", "");

            share_mem = MemoryMappedFile.CreateNew(pushTAG+".symbol", 256, MemoryMappedFileAccess.ReadWriteExecute);

            shareMemOrder = MemoryMappedFile.CreateOrOpen(pushTAG+".order", 256, MemoryMappedFileAccess.ReadWriteExecute);


            if (getSymbol != null && getSymbol.Length > 0)
            {
                timerDelegate = new TimerCallback(requestMTStatus);
                timer = new Timer(timerDelegate, null, 0, 200);
                statusBorder.Background = Brushes.LightGreen;
                mt4Status.Text = "連携済み";
            }
            if (getMemorryString(pullTAG+ ".array") != null && getMemorryString(pullTAG +".array").Length > 0)
            {
                int selectedItem = 0;
                int i = 0;
                String[] sep = { "," };
                String[] f = getMemorryString(pullTAG+".array").Split(sep, StringSplitOptions.RemoveEmptyEntries);
                Trace.WriteLine("\r\nMT4通貨:" + getSymbol);
                foreach (String str in f)
                {
                    if (str.IndexOf(getSymbol) > -1)
                    {
                        selectedItem = i;
                    }
                    else
                    {
                        i++;
                    }
                    comboBox.Items.Add(str);
                }

                //Trace.WriteLine("MT4側の選択されている通貨は？" + selectedItem);
                comboBox.SelectedItem = selectedItem;
                comboBox.Text = getSymbol;
            }
        }

        private void requestMTStatus(object state)
        {

            this.Dispatcher.Invoke(() =>
            {
                try
                {
                    //
                    String orderEnable = getMemorryString(pullTAG + ".order");
                    orderEnable = orderEnable.Replace("\0", "");
                    if (orderEnable.Length > 0)
                    {
                        if (orderFlag)
                        {
                            Trace.WriteLine("o:" + orderEnable);
                            MemoryMappedViewAccessor accessor = shareMemOrder.CreateViewAccessor();
                            // sellは1
                            // Write data to shared memory
                            string orderStr = "";
                            char[] data = orderStr.ToCharArray();
                            accessor.Write(0, data.Length);
                            accessor.WriteArray<char>(0, data, 0, data.Length);

                            // Dispose accessor
                            accessor.Dispose();
                            orderFlag = !orderFlag;
                        }
                    }


                    String val = getMemorryString(pullTAG+".value");
                    //Trace.WriteLine(val);
                    textBlock.Text = val;
                    String[] sep = { "," };
                    String[] sep2 = { "." };
                    String[] str = val.Split(sep, StringSplitOptions.RemoveEmptyEntries);

                    //Trace.WriteLine(str[0]+","+str[1]);
                    String[] bid = str[0].Split(sep2, StringSplitOptions.RemoveEmptyEntries);
                    String[] ask = str[1].Split(sep2, StringSplitOptions.RemoveEmptyEntries);

                    int digits = int.Parse(str[2].Replace("\0", ""));
                    digits = digits - 3 < 0 ? 0 : digits - 3;
                    int index = str[0].IndexOf(".");

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
                    this.selltextBlock.Inlines.Add(bid[0] + "." + bid[1].Substring(0, digits));
                    run = new Run();
                    run.Text = bid[1].Substring(digits, 2);
                    run.FontSize = 24;
                    this.selltextBlock.Inlines.Add(run);
                    //int qr = digits - 2;
                    this.selltextBlock.Inlines.Add(bid[1].Substring(digits + 2, 1));
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

                    this.buytextBlock.Inlines.Add(ask[0] + "." + ask[1].Substring(0, digits));
                    run2 = new Run();
                    run2.Text = ask[1].Substring(digits, 2);
                    run2.FontSize = 24;
                    this.buytextBlock.Inlines.Add(run2);
                    this.buytextBlock.Inlines.Add(ask[1].Substring(digits + 2, 1));
                    buyBtn.Content = buytextBlock;

                    spreadTbk.Text = "";
                    Run run3 = new Run();
                    run3.Text = "Spread\n" + str[3];
                    run3.FontSize = 18;
                    this.spreadTbk.Inlines.Add(run3);
                    this.spreadTbk.TextAlignment = TextAlignment.Center;



                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }

            });

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
            MemoryMappedFile share_mem2 = MemoryMappedFile.CreateOrOpen(TAG, 256);
            MemoryMappedViewAccessor accessor = share_mem2.CreateViewAccessor();

            // Write data to shared memory
            string str = msg;
            char[] data = str.ToCharArray();
            accessor.Write(0, data.Length);
            accessor.WriteArray<char>(0, data, 0, data.Length);

            // Dispose accessor
            accessor.Dispose();
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            popupLots.IsOpen = false;
            String msg = ((ComboBox)sender).SelectedItem.ToString().Trim();
            Trace.WriteLine(msg);
            //setMemorryString("exemt.symbol", msg);


            MemoryMappedViewAccessor accessor = share_mem.CreateViewAccessor();

            // Write data to shared memory
            char[] data = msg.ToCharArray();
            accessor.Write(0, data.Length);
            accessor.WriteArray<char>(0, data, 0, data.Length);

            // Dispose accessor
            accessor.Dispose();
            //SetMemString("exemt.symbol", ((ComboBox)sender).SelectedItem.ToString().Trim());
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

            Trace.WriteLine(getMemorryString("exemt.symbol"));
        }

        private void gamenButton_Click(object sender, RoutedEventArgs e)
        {
            this.mainWindow.Topmost = !this.mainWindow.Topmost;
            if (this.mainWindow.Topmost)
            {
                //this.gamenButton.Background = Brushes.Red;
                fillR.Fill = Brushes.Red;
            }
            else
            {
                //Color b = Color.FromArgb(0xFF, 0xBB, 0xBB, 0xBB);


                // this.gamenButton.Background = new SolidColorBrush(b);
                fillR.Fill = Brushes.White;
            }
        }

        private void closeAllBtn_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
