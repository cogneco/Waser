using System;

namespace Waser.Demo
{
	class Program
	{
		public static void Main (string [] args)
		{
			Program.Run ();
		}
		static void Run ()
		{
			Waser.AppHost.ListenAt (new Waser.IO.IPEndPoint (Waser.IO.IPAddress.Any, 8080));
			Waser.AppHost.Start (new Demo.Application());
		}
	}
}
