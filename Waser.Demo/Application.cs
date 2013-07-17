using System;

namespace Waser.Demo
{
	public class Application : 
		Waser.Application
	{
		public Application ()
		{
			this.Route ("/resource", new Resource ());
		}
	}
}

