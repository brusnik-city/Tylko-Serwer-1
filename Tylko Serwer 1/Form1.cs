using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Media;

namespace Tylko_Serwer_1
{
    public partial class SerwerForm : Form
    {

        private Socket _serwerSocket, _clientSocket;
        private byte[] _buffer;
        public  Socket tmp;
        List<KittyClient> kitty = new List<KittyClient>();
        public SerwerForm()
        {
            InitializeComponent();
            StartServer();
        }
       
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private void SerwerForm_Load(object sender, EventArgs e)
        {

        }

        private void StartServer()
        {
            try
            {
                _serwerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _serwerSocket.Bind(new IPEndPoint(IPAddress.Parse("192.168.1.5"), 8001));
                _serwerSocket.Listen(10);
               // _serwerSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
                _serwerSocket.BeginAccept(new AsyncCallback(AcceptCallback), _serwerSocket);
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();
            try
            {
                _clientSocket = _serwerSocket.EndAccept(ar);
                
                _buffer = new byte[_clientSocket.ReceiveBufferSize];
                _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), _clientSocket);
                
                _serwerSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
                AppendToTextBox("Client has connected");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {

            try
            {
                int received = _clientSocket.EndReceive(ar);
                if (received == 0)
                {
                    return;
                }
                Array.Resize(ref _buffer, received);
                string text = Encoding.ASCII.GetString(_buffer);
                Array.Resize(ref _buffer, _clientSocket.ReceiveBufferSize);

                string [] w =text.Split(' ');
                string first = w[0];
                string second = w[1];
                string last="a";
                int q = text.Length;
                int s = first.Length + second.Length+1;
                
                
                if ((q) > (s))
                {
                     last = w[2];
                }
                if (first == "NowyUczestnikDane")
                    SaveToFile(text, @"C:\Users\bru\Desktop\Tylko Serwer 1\Dane_osobowe.txt");
                if (first == "NowyUczestnikKomunikatora")
                    SaveToFile(text, @"C:\Users\bru\Desktop\Tylko Serwer 1\Nick_haslo.txt");
                if (second == "DodajNowegoZnajomegoProsze")
                {
                    string[] liness = System.IO.File.ReadAllLines(@"C:\Users\bru\Desktop\Tylko Serwer 1\Dane_osobowe.txt");
                    int j = 0;
                    bool tmp1 = true;
                    foreach (string line in liness)
                    {
                        if (line.Trim().StartsWith("NowyUczestnikDane"))
                        {
                            if (first == liness[j + 1])
                            {
                                SendMessage(liness[j + 2]);
                                tmp1 = false;
                                break;
                            }

                            else j = j + 3;
                        }
                    }
                    if (tmp1)
                        SendMessage("Blad");
                }
                if (last == "SprawdzamyLogowanieTeraz")
                {
                    string[] lines = System.IO.File.ReadAllLines(@"C:\Users\bru\Desktop\Tylko Serwer 1\Nick_haslo.txt");
                    bool tmp = true;
                    int i = 0;
                    foreach (string line in lines)
                    {
                        if (line.Trim().StartsWith("NowyUczestnikKomunikatora"))
                        {
                            if (first+second == (lines[i + 1] + lines[i + 2]))
                            {
                                SendMessage("PoprawneLogowanieSuper");
                                bool exist = kitty.Exists(x => x._nick == first);
                                if (!exist)
                                 kitty.Add(new KittyClient(first, _clientSocket));
                                else
                                {
                                    KittyClient tmp1 = kitty.Find(x => x._nick == first);
                                    tmp1._socket = _clientSocket; 
                                }
                                
                                //foreach(KittyClient k in kitty)
                                //{
                                //    textBox1.Invoke(new Action(() => textBox1.Text = k._nick + " " + k._socket));
 
                                //}
                                tmp = false;
                                break;
                            }
                            else i = i + 3;
                        }
                    }
                    if (tmp)
                        SendMessage("NiepoprawneLogowanieSuper");

                }
                if (first == "WysylamNickDoSerwera")
                {
                    bool exist = kitty.Exists(x => x._nick == second);
                    if (exist)
                    {
                        KittyClient tmp1 = kitty.Find(x => x._nick == second);
                         text=text.Remove(0, ((first.Length + second.Length)+2));
                      
                        byte[] buffer1 = Encoding.ASCII.GetBytes(text);
                        try
                        {
                            // IPAddress address = ((IPEndPoint)tmp1._socket.RemoteEndPoint).Address;
                            //  _serwerSocket.BeginConnect(new IPEndPoint(address, 8001), new AsyncCallback(nu), _serwerSocket);
                              tmp1._socket.BeginSend(buffer1, 0, buffer1.Length, 0, new AsyncCallback(SendCallback), _serwerSocket);
                           // Program p = new Program();
                           
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        _clientSocket.BeginSend(buffer1, 0, buffer1.Length, 0, new AsyncCallback(SendCallback), _serwerSocket);
                        
                    }
                    else SendMessage("BladNieMaTakiegoNicku Ok");

                }
                
                _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void playSimpleSound()
        {
            SoundPlayer simpleSound = new SoundPlayer(@"C:\Users\bru\Desktop\Tylko Serwer 1\Miau.wav");
            simpleSound.Play();
        }
        
        private static void SendCallback (IAsyncResult ar)
        {

            try
            {
                Socket _clientsocket = (Socket)ar.AsyncState;
                _clientsocket.EndSend(ar);
              //  _clientsocket.Shutdown(SocketShutdown.Both);
                _clientsocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void SendMessage(string text)
        {
            byte[] buffer1 = Encoding.ASCII.GetBytes(text);
            _clientSocket.BeginSend(buffer1, 0, buffer1.Length, 0, new AsyncCallback(SendCallback), _serwerSocket);
        }
        

       
        private void SaveToFile(string text,string path)
        {
            
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(text, Encoding.Default);

            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void AppendToTextBox(string text)
        {
            MethodInvoker invoker = new MethodInvoker(delegate
            {
                textBox1.Text += "\r\n" + "\r\n" + text;
            });
            this.Invoke(invoker);
            
        }
    }
}
