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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Waser.IO;
using Waser.Http;
using Waser.Caching;
using Waser.Logging;
using Libev;
using Waser.Threading;

namespace Waser
{
	/// <summary>
	/// The app runner. This is where the magic happens.
	/// </summary>
	public static class ApplicationHost
	{
		private static Application app;
		private static List<IPEndPoint> listenEndPoints = new List<IPEndPoint>();
		private static Dictionary<IPEndPoint, Tuple<string, string>> secureListenEndPoints = new Dictionary<IPEndPoint, Tuple<string, string>>();
		private static List<Server> servers = new List<Server>();
		private static ICache cache;
		private static ILogger log;
		private static List<IPipe> pipes;
		private static IO.Context context;
		static ApplicationHost()
		{
			context = IO.Context.Create();
		}
		public static Application App
		{
			get { return app; }	
		}
		public static ICache Cache
		{
			get
			{
				if (cache == null)
					cache = new InProcessCache();
				return cache;
			}
		}
		public static ILogger Log
		{
			get
			{
				if (log == null)
					log = new Waser.Logging.ConsoleLogger("Waser", LogLevel.Debug);
				return log;
			}
		}
		public static IO.Context Context
		{
			get { return context; }	
		}
		public static IList<IPipe> Pipes
		{
			get { return pipes; }
		}
		public static ICollection<IPEndPoint> ListenEndPoints
		{
			get
			{
				return listenEndPoints.AsReadOnly();
			}
		}
		public static void ListenAt(IPEndPoint endPoint)
		{
			if (endPoint == null)
				throw new ArgumentNullException("endPoint");
			
			if (listenEndPoints.Contains(endPoint) || secureListenEndPoints.ContainsKey(endPoint))
				throw new InvalidOperationException("Endpoint already registered");
			
			listenEndPoints.Add(endPoint);
		}
		public static void SecureListenAt(IPEndPoint endPoint, string cert, string key)
		{
			if (endPoint == null)
				throw new ArgumentNullException("endPoint");
			if (cert == null)
				throw new ArgumentNullException("cert");
			if (key == null)
				throw new ArgumentNullException("key");
			
			if (secureListenEndPoints.ContainsKey(endPoint) || listenEndPoints.Contains(endPoint))
				throw new InvalidOperationException("Endpoint already registered");
			
			secureListenEndPoints.Add(endPoint,
			                          Tuple.Create(cert, key));
		}
		public static void InitializeTLS(string priorities)
		{
#if !DISABLETLS
			manos_tls_global_init(priorities);
			RegenerateDHParams(1024);
#endif
		}
		public static void RegenerateDHParams(int bits)
		{
#if !DISABLETLS
			manos_tls_regenerate_dhparams(bits);
#endif
		}
		#if !DISABLETLS
		[DllImport ("libmanos", CallingConvention = CallingConvention.Cdecl)]
		private static extern int manos_tls_global_init(string priorities);
		[DllImport ("libmanos", CallingConvention = CallingConvention.Cdecl)]
		private static extern int manos_tls_regenerate_dhparams(int bits);

		#endif
		public static void Start(Application application)
		{
			if (application == null)
				throw new ArgumentNullException("application");
			
			app = application;

			app.StartInternal();

			foreach (var ep in listenEndPoints)
			{
				var server = new Server(Context, HandleTransaction, Context.CreateTcpServerSocket(ep.AddressFamily));
				server.Listen(ep.Address.ToString(), ep.Port);
				
				servers.Add(server);
			}
			foreach (var ep in secureListenEndPoints.Keys)
			{
//				var keypair = secureListenEndPoints [ep];
//				var socket = Context.CreateSecureSocket (keypair.Item1, keypair.Item2);
//				var server = new HttpServer (context, HandleTransaction, socket);
//				server.Listen (ep.Address.ToString (), ep.Port);
//				
//				servers.Add (server);
			}

			context.Start();
		}
		public static void Stop()
		{
			context.Stop();
		}
		public static void HandleTransaction(ITransaction con)
		{
			app.HandleTransaction(app, con);
		}
		public static void AddPipe(IPipe pipe)
		{
			if (pipes == null)
				pipes = new List<IPipe>();
			pipes.Add(pipe);
		}
		public static Timeout AddTimeout(TimeSpan timespan, IRepeatBehavior repeat, object data, TimeoutCallback callback)
		{
			return AddTimeout(timespan, timespan, repeat, data, callback);
		}
		public static Timeout AddTimeout(TimeSpan begin, TimeSpan timespan, IRepeatBehavior repeat, object data, TimeoutCallback callback)
		{
			Timeout t = new Timeout(begin, timespan, repeat, data, callback);
			
			ITimerWatcher timer = null;
			timer = context.CreateTimerWatcher(begin, timespan, delegate
			{
				t.Run(app);
				if (!t.ShouldContinueToRepeat())
				{
					t.Stop();
					timer.Dispose();
				}
			});

			timer.Start();

			return t;
		}
	}
}
