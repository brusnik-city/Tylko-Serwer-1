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

namespace Tylko_Serwer_1
{
    public class KittyClient
    {
        public Socket _socket;
        public string _nick;

        public KittyClient(string nick, Socket socket)
        {
            _nick = nick;
            _socket = socket;
        }
    }
}
