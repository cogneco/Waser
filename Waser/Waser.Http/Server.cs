//
// Copyright (C) 2010 Jackson Harper (jackson@manosdemono.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//


using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Libev;

using Waser.IO;

namespace Waser.Http
{

    public delegate void ConnectionCallback(ITransaction transaction);

    public class Server : IDisposable
    {

        public static readonly string ServerVersion;

        private ConnectionCallback callback;
        ITcpServerSocket socket;
        private bool closeOnEnd;

        static Server()
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            ServerVersion = "Waser/" + v.ToString();
        }

        public Server(IO.Context context, ConnectionCallback callback, ITcpServerSocket socket, bool closeOnEnd = false)
        {
            this.callback = callback;
            this.socket = socket;
            this.closeOnEnd = closeOnEnd;
			this.Context = context;
        }

        public IO.Context Context
        {
			get;
			private set;
        }

        public void Listen(string host, int port)
        {
            socket.Bind(new IPEndPoint(IPAddress.Parse (host), port));
			socket.Listen (128, ConnectionAccepted);
        }

        public void Dispose()
        {
            if (socket != null) {
                socket.Dispose();
                socket = null;
            }
        }

        public void RunTransaction(Transaction trans)
        {
            trans.Run();
        }

        private void ConnectionAccepted(ITcpSocket socket)
        {
            Transaction.BeginTransaction(this, socket, callback, closeOnEnd);
        }
    }
}


