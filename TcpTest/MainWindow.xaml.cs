using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace TcpTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Socketクライアント
        TcpClient tClient = new TcpClient();

        public MainWindow()
        {
            Debug.WriteLine("Form1" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
            InitializeComponent();

            //接続OKイベント
            tClient.OnConnected += new TcpClient.ConnectedEventHandler(tClient_OnConnected);
            //接続断イベント
            tClient.OnDisconnected += new TcpClient.DisconnectedEventHandler(tClient_OnDisconnected);
            //データ受信イベント
            tClient.OnReceiveData += new TcpClient.ReceiveEventHandler(tClient_OnReceiveData);
        }

        /** 接続断イベント **/
        void tClient_OnDisconnected(object sender, EventArgs e)
        {
            Debug.WriteLine("tClient_OnDisconnected" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
            if (Dispatcher.CheckAccess())
                Dispatcher.Invoke(new DisconnectedDelegate(Disconnected), new object[] { sender, e });
        }
        delegate void DisconnectedDelegate(object sender, EventArgs e);
        private void Disconnected(object sender, EventArgs e)
        {
            //接続断処理
            Debug.WriteLine("Disconnected" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
        }


        /** 接続OKイベント **/
        void tClient_OnConnected(EventArgs e)
        {
            //接続OK処理
            Debug.WriteLine("tClient_OnConnected" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
        }

        /** 接続ボタンを押して接続処理 **/
        private void btn_Connect_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("btn_Connect_Click" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
            try
            {
                //接続先ホスト名
                string host = "127.0.0.1";
                //接続先ポート
                int port = int.Parse(portnum.Text);
                //接続処理
                // Connect to the remote endpoint.
                tClient.Connect(host, port);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /** データ受信イベント **/
        void tClient_OnReceiveData(object sender, string e)
        {
            Debug.WriteLine("tClient_OnReceiveData" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
            //別スレッドからくるのでInvokeを使用
            if (Dispatcher.CheckAccess())
                Dispatcher.Invoke(new ReceiveDelegate(ReceiveData), new object[] { sender, e });
        }
        delegate void ReceiveDelegate(object sender, string e);
        //データ受信処理
        private void ReceiveData(object sender, string e)
        {
            Debug.WriteLine("ReceiveData:" + e + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            //送信
            tClient.Send(textBox.Text);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Form1_FormClosing" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);
            if (!tClient.IsClosed)
                tClient.Close();

        }

        private void ServerStart_Click(object sender, RoutedEventArgs e)
        {
            Program.StartServer(portnum.Text);
        }

        private void SSend_Click(object sender, RoutedEventArgs e)
        {

            Program.Instance.Send(stextBox.Text);
        }
    }
}
