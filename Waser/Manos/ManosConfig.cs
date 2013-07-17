//
// ManosConfig - configuration management for Manos
// Author: Axel Callabed <axelc@github.com>
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

namespace Manos
{
	/// <summary>
	/// Manos config. The config file being loaded is in .ini format that
	/// contains of one or more sections, the main section is what this
	/// API uses. However if wanting to access different sections, one can
	/// go through the Source to access the nini IConfigSource.
	/// 
	/// The configs are loaded from $MANOS_CONFIG first, but if that variable
	/// is not set it will look for manos.config within the current directory.
	/// If neither are found, the config is left empty.
	/// 
	/// Here is an example config:
	/// 
	/// [manos]
	/// database_user = ahall
	/// database_password = temp123
	/// database_name = mydb
	/// database_hostname = localhost
	/// database_type = postgresql
	/// 
	/// Getting the db user is as simple as ManosConfig.GetString("database_user");
	/// </summary>
	public static class ManosConfig {
		private const string MAIN_SECTION = "manos";
		
		public static void Load ()
		{
			string source = Environment.GetEnvironmentVariable ("MANOS_CONFIG") ??
                    Path.Combine (Environment.CurrentDirectory, "manos.config");
			if (!File.Exists(source))
				return;

		}
		
    }
}

