using System;

namespace Waser.Demo
{
	public class Application : 
		Manos.ManosApp
	{
		public Application ()
		{
			this.Route ("/resource", new Resource ());
		}
	}
}

