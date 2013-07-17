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
		IManosLogger Log { get; }
		
		RouteHandler Route (string pattern, IModule module);
		RouteHandler Route (string pattern, ManosAction action);
		RouteHandler Route (string pattern, Waser.Routing.MatchType matchType, ManosAction action);
		RouteHandler Route (ManosAction action, params string [] patterns);
		RouteHandler Route (IModule module, params string [] patterns);
		
		RouteHandler Get (string pattern, IModule module);
		RouteHandler Get (string pattern, ManosAction action);
		RouteHandler Get (string pattern, Waser.Routing.MatchType matchType, ManosAction action);
		RouteHandler Get (ManosAction action, params string [] patterns);
		RouteHandler Get (IModule module, params string [] patterns);
		
	 	RouteHandler Put (string pattern, IModule module);
		RouteHandler Put (string pattern, ManosAction action);
		RouteHandler Put (string pattern, Waser.Routing.MatchType matchType, ManosAction action);
		RouteHandler Put (ManosAction action, params string [] patterns);
		RouteHandler Put (IModule module, params string [] patterns);
		
		RouteHandler Post (string pattern, IModule module);
		RouteHandler Post (string pattern, ManosAction action);
		RouteHandler Post (string pattern, Waser.Routing.MatchType matchType, ManosAction action);
		RouteHandler Post (ManosAction action, params string [] patterns);
		RouteHandler Post (IModule module, params string [] patterns);
		
		RouteHandler Delete (string pattern, IModule module);
		RouteHandler Delete (string pattern, ManosAction action);
		RouteHandler Delete (string pattern, Waser.Routing.MatchType matchType, ManosAction action);
		RouteHandler Delete (ManosAction action, params string [] patterns);
		RouteHandler Delete (IModule module, params string [] patterns);
		
		RouteHandler Head (string pattern, IModule module);
		RouteHandler Head (string pattern, ManosAction action);
		RouteHandler Head (string pattern, Waser.Routing.MatchType matchType, ManosAction action);
		RouteHandler Head (ManosAction action, params string [] patterns);
		RouteHandler Head (IModule module, params string [] patterns);
		
		RouteHandler Options (string pattern, IModule module);
		RouteHandler Options (string pattern, ManosAction action);
		RouteHandler Options (string pattern, Waser.Routing.MatchType matchType, ManosAction action);
		RouteHandler Options (ManosAction action, params string [] patterns);
		RouteHandler Options (IModule module, params string [] patterns);
		
		RouteHandler Trace (string pattern, IModule module);
		RouteHandler Trace (string pattern, ManosAction action);
		RouteHandler Trace (string pattern, Waser.Routing.MatchType matchType, ManosAction action);
		RouteHandler Trace (ManosAction action, params string [] patterns);
		RouteHandler Trace (IModule module, params string [] patterns);

		void StartInternal ();
	}
}
