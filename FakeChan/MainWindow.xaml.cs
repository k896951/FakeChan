using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Net.Sockets;
using System.IO;

namespace FakeChan
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        WCFClient WcfClient;
        Dictionary<int, string> AvatorNameList;
        Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>> AvatorParamList;
        IPListenPoint ListenEndPoint;
        TcpListener Listener;
        Action BGTask;
        bool KeepListen;
        int SelectedCid;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                WcfClient = new WCFClient();
                AvatorNameList = WcfClient.AvatorList2().ToDictionary(k => k.Key, v => string.Format(@"{0} : {1}({2})", v.Key, v.Value["name"], v.Value["prod"]));
                AvatorParamList = AvatorNameList.ToDictionary(k => k.Key, v => WcfClient.GetDefaultParams2(v.Key));

                ListenEndPoint = new IPListenPoint();
                ListenEndPoint.Address = IPAddress.Parse("127.0.0.1");
                ListenEndPoint.PortNum = 50001;
            }
            catch(Exception e1)
            {
                MessageBox.Show(e1.Message, "Sorry1");
                Application.Current.Shutdown();
            }

            Binding myBinding = new Binding("PortNum");
            myBinding.Source = ListenEndPoint;
            TextBoxListenPort.SetBinding(TextBox.TextProperty, myBinding);

            ComboBoxAvator.ItemsSource = null;
            if (AvatorNameList.Count != 0)
            {
                ComboBoxAvator.ItemsSource = AvatorNameList;
                ComboBoxAvator.SelectedIndex = 0;
                ComboBoxAvator.IsEnabled = true;
            }
            else
            {
                ComboBoxAvator.IsEnabled = false;
            }
        }

        private void ComboBoxAvator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int cid = Convert.ToInt32(ComboBoxAvator.SelectedValue);

            SelectedCid = cid;

            WrapPanelParams1.Children.Clear();
            WrapPanelParams2.Children.Clear();

            ReSetupParams(cid, AvatorParamList[cid]["effect"],  WrapPanelParams1.Children);
            ReSetupParams(cid, AvatorParamList[cid]["emotion"], WrapPanelParams2.Children);
        }

        private void ReSetupParams(int cid, Dictionary<string, Dictionary<string, decimal>> @params, UIElementCollection panel)
        {
            foreach(var param in @params)
            {
                StackPanel sp = new StackPanel();
                Label lb = new Label();
                TextBlock tb = new TextBlock();

                sp.Orientation = Orientation.Vertical;
                sp.Margin = new Thickness(2, 0, 2, 2);
                sp.Background = new SolidColorBrush(Color.FromArgb(0xff, 253, 252, 227));

                // パラメタ名
                lb.HorizontalAlignment = HorizontalAlignment.Left;
                lb.Content = param.Key;

                // スライダー
                Slider sl = new Slider();
                sl.Name = param.Key;
                sl.Width = 100;
                sl.Minimum = Convert.ToDouble(param.Value["min"]);
                sl.Maximum = Convert.ToDouble(param.Value["max"]);
                sl.SelectionStart = Convert.ToDouble(param.Value["min"]);
                sl.SelectionEnd = Convert.ToDouble(param.Value["max"]);
                sl.Value = Convert.ToDouble(param.Value["value"]);
                sl.TickFrequency = Convert.ToDouble(param.Value["step"]);
                sl.LargeChange = Convert.ToDouble(param.Value["step"]);
                sl.SmallChange = Convert.ToDouble(param.Value["step"]);
                sl.IsSnapToTickEnabled = true;

                sl.ValueChanged += (sender, args) => {
                    param.Value["value"] = Convert.ToDecimal(sl.Value);
                };

                // 数値（表示）
                Binding myBinding = new Binding("Value");
                myBinding.Source = sl;
                tb.SetBinding(TextBlock.TextProperty, myBinding);
                tb.HorizontalAlignment = HorizontalAlignment.Center;

                sp.Children.Add(lb);
                sp.Children.Add(sl);
                sp.Children.Add(tb);

                panel.Add(sp);
            }
        }

        private void TextBoxListenPort_LostFocus(object sender, RoutedEventArgs e)
        {
            ButtonStart.IsEnabled = false;

            if (TextBoxListenPort.Text != "")
            {
                int p;

                if (Int32.TryParse(TextBoxListenPort.Text, out p))
                {
                    if ((p > 1023) && (p < 65536))
                    {
                        ButtonStart.IsEnabled = true;
                    }
                }
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonStart.IsEnabled = false;
                TextBoxListenPort.IsEnabled = false;
                Listener = new TcpListener(ListenEndPoint.Address, ListenEndPoint.PortNum);
                KeepListen = true;
                SetupBGTask();
                Task.Run(BGTask);
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Sorry2");
                ButtonStart.IsEnabled = true;
                ButtonStop.IsEnabled = false;
            }

            ButtonStop.IsEnabled = true;
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            KeepListen = false;
            Listener.Stop();
            ButtonStart.IsEnabled = true;
            ButtonStop.IsEnabled = false;
            TextBoxListenPort.IsEnabled = true;
        }

        private void SetupBGTask()
        {
            // パケット（プログラムでは先頭5項目をスキップ）
            //   Int16  iCommand = 0x0001;
            //   Int16  iSpeed = -1;
            //   Int16  iTone = -1;
            //   Int16  iVolume = -1;
            //   Int16  iVoice = 1;
            //   byte   bCode = 0;
            //   Int32  iLength = bMessage.Length;
            //   byte[] bMessage;

            BGTask = (()=>{

                int SkipSize = 2 * 5;

                while (KeepListen) // とりあえずの待ち受け構造
                {
                    try
                    {
                        Listener.Start();
                        TcpClient client = Listener.AcceptTcpClient();
                        byte bCode;
                        Int32 iLength;
                        byte[] iLengthBuff;
                        string TalkText = "";

                        using (NetworkStream ns = client.GetStream())
                        {
                            using (BinaryReader br = new BinaryReader(ns))
                            {
                                br.ReadBytes(SkipSize);

                                bCode = br.ReadByte();
                                iLengthBuff = br.ReadBytes(4);
                                iLength = BitConverter.ToInt32(iLengthBuff, 0); // うーん…

                                byte[] TalkTextBuff = new byte[iLength];

                                TalkTextBuff = br.ReadBytes(iLength);

                                switch (bCode)
                                {
                                    case 0: // UTF8
                                        TalkText = System.Text.Encoding.UTF8.GetString(TalkTextBuff, 0, iLength);
                                        break;

                                    case 2: // CP932
                                        TalkText = System.Text.Encoding.GetEncoding(932).GetString(TalkTextBuff, 0, iLength);
                                        break;

                                    case 1: // 暫定で書いた
                                        TalkText = System.Text.Encoding.Unicode.GetString(TalkTextBuff, 0, iLength);
                                        break;
                                }
                            }
                        }

                        Dispatcher.Invoke(() => {
                            Dictionary<string, decimal> effects = AvatorParamList[SelectedCid]["effect"].ToDictionary(k => k.Key, v => v.Value["value"]);
                            Dictionary<string, decimal> emotions = AvatorParamList[SelectedCid]["emotion"].ToDictionary(k => k.Key, v => v.Value["value"]);

                            WcfClient.Talk(SelectedCid, TalkText, "", effects, emotions);
                        });
                    }
                    catch (Exception)
                    {
                        //Dispatcher.Invoke(() =>
                        //{
                        //    MessageBox.Show(e.Message, "sorry3");
                        //});
                    }
                }
            });
        }

    }
}
