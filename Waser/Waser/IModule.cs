//
// Copyright (C) 2011 Antony Pinchbeck (antony@componentx.co.uk)
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
using Waser.Routing;
using Waser.Caching;
using Waser.Logging;

namespace Waser
{
	public interface IModule 
	{
		RouteHandler Routes { get; }
		ICache Cache { get; }
		ILogger Log { get; }
		
		RouteHandler Route (string pattern, IModule module);
		RouteHandler Route (string pattern, Routing.Action action);
		RouteHandler Route (string pattern, Waser.Routing.MatchType matchType, Routing.Action action);
		RouteHandler Route (Routing.Action action, params string [] patterns);
		RouteHandler Route (IModule module, params string [] patterns);
		
		RouteHandler Get (string pattern, IModule module);
		RouteHandler Get (string pattern, Routing.Action action);
		RouteHandler Get (string pattern, Waser.Routing.MatchType matchType, Routing.Action action);
		RouteHandler Get (Routing.Action action, params string [] patterns);
		RouteHandler Get (IModule module, params string [] patterns);
		
	 	RouteHandler Put (string pattern, IModule module);
		RouteHandler Put (string pattern, Routing.Action action);
		RouteHandler Put (string pattern, Waser.Routing.MatchType matchType, Routing.Action action);
		RouteHandler Put (Routing.Action action, params string [] patterns);
		RouteHandler Put (IModule module, params string [] patterns);
		
		RouteHandler Post (string pattern, IModule module);
		RouteHandler Post (string pattern, Routing.Action action);
		RouteHandler Post (string pattern, Waser.Routing.MatchType matchType, Routing.Action action);
		RouteHandler Post (Routing.Action action, params string [] patterns);
		RouteHandler Post (IModule module, params string [] patterns);
		
		RouteHandler Delete (string pattern, IModule module);
		RouteHandler Delete (string pattern, Routing.Action action);
		RouteHandler Delete (string pattern, Waser.Routing.MatchType matchType, Routing.Action action);
		RouteHandler Delete (Routing.Action action, params string [] patterns);
		RouteHandler Delete (IModule module, params string [] patterns);
		
		RouteHandler Head (string pattern, IModule module);
		RouteHandler Head (string pattern, Routing.Action action);
		RouteHandler Head (string pattern, Waser.Routing.MatchType matchType, Routing.Action action);
		RouteHandler Head (Routing.Action action, params string [] patterns);
		RouteHandler Head (IModule module, params string [] patterns);
		
		RouteHandler Options (string pattern, IModule module);
		RouteHandler Options (string pattern, Routing.Action action);
		RouteHandler Options (string pattern, Waser.Routing.MatchType matchType, Routing.Action action);
		RouteHandler Options (Routing.Action action, params string [] patterns);
		RouteHandler Options (IModule module, params string [] patterns);
		
		RouteHandler Trace (string pattern, IModule module);
		RouteHandler Trace (string pattern, Routing.Action action);
		RouteHandler Trace (string pattern, Waser.Routing.MatchType matchType, Routing.Action action);
		RouteHandler Trace (Routing.Action action, params string [] patterns);
		RouteHandler Trace (IModule module, params string [] patterns);

		void StartInternal ();
	}
}
