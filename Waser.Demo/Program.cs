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
			Manos.AppHost.ListenAt (new Manos.IO.IPEndPoint (Manos.IO.IPAddress.Any, 8080));
			Manos.AppHost.Start (new Demo.Application());
		}
	}
}
