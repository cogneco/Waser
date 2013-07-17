//
// Based on http_parser.java: http://github.com/a2800276/http-parser.java
// which is based on http_parser: http://github.com/ry/http-parser
//
//
// Copyright 2009,2010 Ryan Dahl <ry@tinyclouds.org>
// Copyright (C) 2010 Tim Becker
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
using System.Text;
using System.Collections.Generic;


namespace Waser.Http {
	
	public enum Method {
		Error = -1,
		
		Delete,
		Get,
		Head,
		Post,
		Put,
		Connect,
		Options,
		Trace,
		Copy,
		Lock,
		MakeCollection,
		Move,
		PropertyFind,
		PropertyPatch,
		Unlock,
		Report,
		MakeActivity,
		Checkout,
		Merge,
	}

	public static class MethodBytes {

		private static object lock_obj = new object ();
		private static Dictionary<Method,byte[]> methods = new Dictionary<Method,byte[]> ();

		static MethodBytes ()
		{
			lock (lock_obj) {
				foreach (Method m in Enum.GetValues (typeof (Method))) {
					Initialize (m);
				}
			}
		}

		public static void Initialize (Method method)
		{
			byte[] result;
			switch (method) {
			default:
			case Method.Error: result = Encoding.ASCII.GetBytes ("ERROR"); break;
			case Method.Delete: result = Encoding.ASCII.GetBytes ("DELETE"); break;
			case Method.Get: result = Encoding.ASCII.GetBytes ("GET"); break;
			case Method.Head: result = Encoding.ASCII.GetBytes ("HEAD"); break;
			case Method.Post: result = Encoding.ASCII.GetBytes ("POST"); break;
			case Method.Put: result = Encoding.ASCII.GetBytes ("PUT"); break;
			case Method.Connect: result = Encoding.ASCII.GetBytes ("CONNECT"); break;
			case Method.Options: result = Encoding.ASCII.GetBytes ("OPTIONS"); break;
			case Method.Trace: result = Encoding.ASCII.GetBytes ("TRACE"); break;
			case Method.Copy: result = Encoding.ASCII.GetBytes ("COPY"); break;
			case Method.Lock: result = Encoding.ASCII.GetBytes ("LOCK"); break;
			case Method.MakeCollection: result = Encoding.ASCII.GetBytes ("MKCOL"); break;
			case Method.Move: result = Encoding.ASCII.GetBytes ("MOVE"); break;
			case Method.PropertyFind: result = Encoding.ASCII.GetBytes ("PROPFIND"); break;
			case Method.PropertyPatch: result = Encoding.ASCII.GetBytes ("PROPPATCH"); break;
			case Method.Unlock: result = Encoding.ASCII.GetBytes ("UNLOCK"); break;
			case Method.Report: result = Encoding.ASCII.GetBytes ("REPORT"); break;
			case Method.MakeActivity: result = Encoding.ASCII.GetBytes ("MKACTIVITY"); break;
			case Method.Checkout: result = Encoding.ASCII.GetBytes ("CHECKOUT"); break;
			case Method.Merge: result = Encoding.ASCII.GetBytes ("MERGE"); break;
			}
			methods [method] = result;
		}

		// TODO: This is good enough for now, but we shouldn't be allocing
		public static byte [] GetBytes (Method method)
		{
			byte [] bytes;
			if (!methods.TryGetValue (method, out bytes))
				return null;
			return bytes;
		}

	}
}

