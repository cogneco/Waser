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

using Waser.Http;

namespace Waser {
	
	/// <summary>
	/// Static lookups for each of the http verbs, and then an array with all of them.
	/// </summary>
	/// <remarks>
	/// ATT: not sure what the purpose of these are, couldn't one just use enum with the "Flags" attribute? 
	/// Maybe this is a special performance thing?
	/// </remarks>
	public static class HttpMethods {

		public static readonly Method [] GetMethods = new Method [] { Method.Get };
		public static readonly Method [] HeadMethods = new Method [] { Method.Head };
		public static readonly Method [] PostMethods = new Method [] { Method.Post };
		public static readonly Method [] PutMethods = new Method [] { Method.Put };
		public static readonly Method [] DeleteMethods = new Method [] { Method.Delete };
		public static readonly Method [] TraceMethods = new Method [] { Method.Trace };
		public static readonly Method [] OptionsMethods = new Method [] { Method.Options };
		
		public static readonly Method [] RouteMethods = new Method [] { Method.Get,
											Method.Put,
											Method.Post,
											Method.Head,
											Method.Delete,
											Method.Trace,
											Method.Options };

	}
}

