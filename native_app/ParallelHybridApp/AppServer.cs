using Newtonsoft.Json;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace ParallelHybridApp
{

    public partial class AppServer : Form
    {
        public List<String> log_ary = new List<string>();
        public static AppServer frm;
        public List<WebSocketSession> session_ary = new List<WebSocketSession>();

        SuperWebSocket.WebSocketServer server;
        SuperWebSocket.WebSocketServer server_ssl;

        public AppServer()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            frm = this;

            var server_config = new SuperSocket.SocketBase.Config.ServerConfig()
            {
                Port = 80,
                Ip = "127.0.0.1",
                MaxConnectionNumber = 100,
                Mode = SuperSocket.SocketBase.SocketMode.Tcp,
                Name = "SuperWebSocket Sample Server",
                MaxRequestLength = 1024 * 1024 * 10,
            };

            setup_server(ref server, server_config);

            var server_config_ssl = new SuperSocket.SocketBase.Config.ServerConfig()
            {
                Port = 443,
                Ip = "127.0.0.1",
                MaxConnectionNumber = 100,
                Mode = SuperSocket.SocketBase.SocketMode.Tcp,
                Name = "SuperWebSocket Sample Server",
                MaxRequestLength = 1024 * 1024 * 10,
                Security = "tls",
                Certificate = new SuperSocket.SocketBase.Config.CertificateConfig
                {
                    FilePath = @"test.pfx",
                    Password = "test"
                }
            };

            setup_server(ref server_ssl, server_config_ssl);


        }

        private void setup_server(ref WebSocketServer server, SuperSocket.SocketBase.Config.ServerConfig serverConfig)
        {
            var rootConfig = new SuperSocket.SocketBase.Config.RootConfig();

            server = new SuperWebSocket.WebSocketServer();

            //サーバーオブジェクト作成＆初期化
            server.Setup(rootConfig, serverConfig);

            //イベントハンドラの設定
            //接続
            server.NewSessionConnected += HandleServerNewSessionConnected;
            //メッセージ受信
            server.NewMessageReceived += HandleServerNewMessageReceived;
            //切断        
            server.SessionClosed += HandleServerSessionClosed;

            //サーバー起動
            server.Start();

        }
            

        //接続
        static void HandleServerNewSessionConnected(SuperWebSocket.WebSocketSession session)
        {
            frm.Invoke((MethodInvoker)delegate () {

                frm.session_ary.Add(session);
                frm.add_log(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), "接続");

            });
            
        }

        //メッセージ受信
        static void HandleServerNewMessageReceived(SuperWebSocket.WebSocketSession session,
                                                    string e)
        {
            frm.Invoke((MethodInvoker)delegate ()
            {
                MessageData recv = JsonConvert.DeserializeObject<MessageData>(e);

                switch (recv.command)
                {
                    case "add_message_to_app":

                        frm.add_log(recv.time, "受信: " + recv.message);

                        break;
                }

            });

        }

        //切断
        static void HandleServerSessionClosed(SuperWebSocket.WebSocketSession session,
                                                    SuperSocket.SocketBase.CloseReason e)
        {
            if(frm != null)
            {
                frm.Invoke((MethodInvoker)delegate () {
                    frm.add_log(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), "切断");
                });
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            frm = null;

            server.Stop();
            server_ssl.Stop();
        }
        
        public void add_log(string time, String log)
        {
            log = "[" + time + "] " + log + "\r\n";
            this.txtMessage.AppendText(log);
        }

        //メッセージ送信
        private void send_message_to_sessions(string message)
        {
            foreach(var session in session_ary)
            {
                MessageData send = new MessageData();

                send.command = "add_message_to_browser";
                send.message = message;
                send.time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                string send_str = JsonConvert.SerializeObject(send);

                session.Send(send_str);

                add_log(send.time, "送信:" + message);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            send_message_to_sessions(this.txtSendMessage.Text);
        }
    }
}
