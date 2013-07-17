using System;

namespace Waser.Demo
{
	public class Resource : 
		Manos.ManosModule 
	{
		string folder;
		public Resource ()
		{
			this.Get (".*", Manos.Routing.MatchType.Regex, this.Content);
			this.folder = System.IO.Path.GetFullPath ("resource");
		}
		public void Content (Manos.IManosContext context)
		{
			string path = context.Request.Path;
			Console.WriteLine(path);
			if (path.StartsWith ("/"))
				path = path.Substring (1);

			if (this.ValidFile (path)) 
			{
				context.Response.Headers.SetNormalizedHeader ("Content-Type", Manos.ManosMimeTypes.GetMimeType (path));
				context.Response.SendFile (path);
			} 
			else
				context.Response.StatusCode = 404;
			context.Response.End ();
		}
		bool ValidFile (string path)
		{
			bool result = false;
			try {
				string full = System.IO.Path.GetFullPath (path);
				result = full.StartsWith (folder) && System.IO.File.Exists (full);
			} catch {
				result = false;
			}
			return result;
		}
	}
}

