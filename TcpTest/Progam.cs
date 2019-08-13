using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpTest
{
    public class Program
    {
        static public Program Instance { get; set; }
        public static void StartServer(string portnum)
        {
            try
            {
                Console.WriteLine("Main ThreadID:" + Thread.CurrentThread.ManagedThreadId);
                Program prog = new Program(portnum);
                prog.init();
                Instance = prog;
            }
            catch (Exception e)
            {
                
            }
        }

        private static ManualResetEvent SocketEvent = new ManualResetEvent(false);

        private IPEndPoint ipEndPoint;

        private Socket sock;

        private Thread tMain;
        //送受信文字列エンコード
        private Encoding enc = Encoding.UTF8;

        Program(String port)
        {
            Console.WriteLine("Program ThreadID:" + Thread.CurrentThread.ManagedThreadId);
            //IPAddress myIP = Dns.GetHostByName(Dns.GetHostName()).AddressList[0];
            IPAddress myIP = IPAddress.Parse("127.0.0.1");
            ipEndPoint = new IPEndPoint(myIP, Int32.Parse(port));
        }

        void init()
        {
            Console.WriteLine("init ThreadID:" + Thread.CurrentThread.ManagedThreadId);
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(ipEndPoint);
            sock.Listen(10);
            Console.WriteLine("サーバー起動中・・・");
            tMain = new Thread(new ThreadStart(Round));
            tMain.Start();
        }

        void Round()
        {
            Console.WriteLine("Round ThreadID:" + Thread.CurrentThread.ManagedThreadId);
            while (true)
            {
                SocketEvent.Reset();
                sock.BeginAccept(new AsyncCallback(OnConnectRequest), sock);
                SocketEvent.WaitOne();
            }
        }

        //★
        Socket Gl_socket;
        void OnConnectRequest(IAsyncResult ar)
        {
            Console.WriteLine("OnConnectRequest ThreadID:" + Thread.CurrentThread.ManagedThreadId);
            SocketEvent.Set();
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            //★
            Gl_socket = handler;
            Console.WriteLine(handler.RemoteEndPoint.ToString() + " joined");
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(ReadCallback), state);
        }

        void ReadCallback(IAsyncResult ar)
        {
            Console.WriteLine("ReadCallback ThreadID:" + Thread.CurrentThread.ManagedThreadId);
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            int ReadSize = handler.EndReceive(ar);
            if (ReadSize < 1)
            {
                Console.WriteLine(handler.RemoteEndPoint.ToString() + " disconnected");
                return;
            }
            byte[] bb = new byte[ReadSize];
            Array.Copy(state.buffer, bb, ReadSize);
            string msg = System.Text.Encoding.UTF8.GetString(bb);
            Console.WriteLine(msg);
            handler.BeginSend(bb, 0, bb.Length, 0, new AsyncCallback(WriteCallback), state);
        }

        void WriteCallback(IAsyncResult ar)
        {
            Console.WriteLine("WriteCallback ThreadID:" + Thread.CurrentThread.ManagedThreadId);
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            handler.EndSend(ar);
            Console.WriteLine("送信完了");
            handler.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(ReadCallback), state);
        }

        void disConnect()
        {
            Console.WriteLine("disConnect ThreadID:" + Thread.CurrentThread.ManagedThreadId);
            sock.Close();
        }
        /// <summary>
        /// ★メッセージを送信する
        /// </summary>
        /// <param name="str"></param>
        public void Send(string str)
        {
            Debug.WriteLine("Send" + " ThreadID:" + Thread.CurrentThread.ManagedThreadId);

            //if (!IsClosed)
            //{
                //文字列をBYTE配列に変換
                byte[] sendBytes = enc.GetBytes(str + "\r\n");
            //lock (syncLock)
            //{
            //送信
            //mySocket.Send(sendBytes);
            //sock.Send(sendBytes);
            Gl_socket.Send(sendBytes);
            //sock.BeginSend(sendBytes,);
            //}
            //}
        }

    }

    class StateObject
    {
        public Socket workSocket { get; set; }
        public const int BUFFER_SIZE = 1024;
        internal byte[] buffer = new byte[BUFFER_SIZE];
    }
}
