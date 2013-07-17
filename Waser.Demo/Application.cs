using System;

namespace Waser.Demo
{
	public class Application : 
		Waser.ManosApp
	{
		public Application ()
		{
			this.Route ("/resource", new Resource ());
		}
	}
}

