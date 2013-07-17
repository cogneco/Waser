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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

using Waser.Http;
using Waser.Routing;
using Waser.Caching;
using Waser.Template;
using Waser.Logging;


namespace Waser {
	
	/// <summary>
	/// A pre-packaged set of routes/actions that can be registered in the constructor of a ManoApp-derived class.
	/// </summary>
	public class Module : IModule {

		private RouteHandler routes = new RouteHandler ();

		public Module ()
		{
			//StartInternal ();
		}

		public RouteHandler Routes {
			get { return routes; }
		}

		public ICache Cache {
			get {
				return ApplicationHost.Cache;	
			}
		}

		public ILogger Log {
			get {
				return ApplicationHost.Log;
			}
		}

		public virtual string Get404Response()
		{
			return default404;
		}

		public virtual string Get500Response()
		{
			return default500;
		}
		
		private RouteHandler AddRouteHandler (IModule module, string [] patterns, Method [] methods)
		{
			if (module == null)
				throw new ArgumentNullException ("module");
			if (patterns == null)
				throw new ArgumentNullException ("patterns");
			if (methods == null)
				throw new ArgumentNullException ("methods");

			module.StartInternal ();
			module.Routes.MatchOps = SimpleOpsForPatterns (patterns);
			module.Routes.Methods = methods;
			Routes.Children.Add (module.Routes);
			return module.Routes;
		}


		private RouteHandler AddRouteHandler (Routing.Action action, IMatchOperation[] matchOperations, Method [] methods)
		{
			return AddRouteHandler (new ActionTarget (action), matchOperations, methods);
		}

		private RouteHandler AddRouteHandler (Routing.Action action, string [] patterns, Method [] methods)
		{
			return AddRouteHandler (new ActionTarget (action), patterns, methods);
		}
		
		private RouteHandler AddRouteHandler (ITarget target, IMatchOperation[] matchOperations, Method [] methods)
		{
			// TODO: Need to decide if this is a good or bad idea
			// RemoveImplicitHandlers (action);

			if (target == null)
				throw new ArgumentNullException ("action");
			if (matchOperations == null)
				throw new ArgumentNullException ("matchOperations");
			if (methods == null)
				throw new ArgumentNullException ("methods");
			
			RouteHandler res = new RouteHandler (matchOperations, methods, target);
			Routes.Children.Add (res);
			return res;
		}
		
		private RouteHandler AddRouteHandler (ITarget target, string [] patterns, Method [] methods)
		{
			// TODO: Need to decide if this is a good or bad idea
			// RemoveImplicitHandlers (action);

			if (target == null)
				throw new ArgumentNullException ("action");
			if (patterns == null)
				throw new ArgumentNullException ("patterns");
			if (methods == null)
				throw new ArgumentNullException ("methods");
			
			RouteHandler res = new RouteHandler (SimpleOpsForPatterns (patterns), methods, target);
			Routes.Children.Add (res);
			return res;
		}
		
		private RouteHandler AddImplicitRouteHandlerForModule (IModule module, string [] patterns, Method [] methods)
		{
			return AddImplicitRouteHandlerForModule (module, StringOpsForPatterns (patterns), methods);
		}

		private RouteHandler AddImplicitRouteHandlerForModule (IModule module, IMatchOperation [] ops, Method [] methods)
		{
			module.StartInternal ();

			module.Routes.IsImplicit = true;
			module.Routes.MatchOps = ops;
			module.Routes.Methods = methods;
			Routes.Children.Add (module.Routes);
			return module.Routes;
		}

		private RouteHandler AddImplicitRouteHandlerForTarget (ITarget target, string[] patterns, Method[] methods, MatchType matchType)
		{
			return AddImplicitRouteHandlerForTarget (target, OpsForPatterns (patterns, matchType), methods);
			//return AddImplicitRouteHandlerForTarget (target, StringOpsForPatterns (patterns), methods);
		}

		private RouteHandler AddImplicitRouteHandlerForTarget (ITarget target, IMatchOperation [] ops, Method [] methods)
		{
			RouteHandler res = new RouteHandler (ops, methods, target) {
				IsImplicit = true,
			};

			Routes.Children.Add (res);
			return res;
		}

		private IMatchOperation [] SimpleOpsForPatterns (string [] patterns)
		{
			return OpsForPatterns (patterns, MatchType.Simple);
		}

		private IMatchOperation [] StringOpsForPatterns (string [] patterns)
		{
			return OpsForPatterns (patterns, MatchType.String);
		}

		private IMatchOperation [] OpsForPatterns (string [] patterns, MatchType type)
		{
			var ops = new IMatchOperation [patterns.Length];
			for (int i = 0; i < ops.Length; i++) {
				ops [i] = MatchOperationFactory.Create (patterns [i], type);
			}

			return ops;
		}

		public RouteHandler Route (string pattern, IModule module)
		{
			return AddRouteHandler (module, new string [] { pattern }, HttpMethods.RouteMethods);
		}

		public RouteHandler Route (string pattern, Routing.Action action)
		{
			return AddRouteHandler (action, new string [] { pattern }, HttpMethods.RouteMethods);
		}

		public RouteHandler Route (string pattern, MatchType matchType, Routing.Action action)
		{
			IMatchOperation [] ops = OpsForPatterns (new string [] { pattern }, matchType);

			return AddRouteHandler (action, ops, HttpMethods.GetMethods);
		}

		public RouteHandler Route (Routing.Action action, params string [] patterns)
		{
			return AddRouteHandler (action, patterns, HttpMethods.RouteMethods);
		}

		public RouteHandler Route (IModule module, params string [] patterns)
		{
			return AddRouteHandler (module, patterns, HttpMethods.RouteMethods);
		}

		public RouteHandler Get (string pattern, IModule module)
		{
			return AddRouteHandler (module, new string [] { pattern }, HttpMethods.GetMethods);
		}

		public RouteHandler Get (string pattern, Routing.Action action)
		{
			return AddRouteHandler (action, new string [] { pattern }, HttpMethods.GetMethods);
		}

		public RouteHandler Get (string pattern, MatchType matchType, Routing.Action action)
		{
			IMatchOperation [] ops = OpsForPatterns (new string [] { pattern }, matchType);

			return AddRouteHandler (action, ops, HttpMethods.GetMethods);
		}

		public RouteHandler Get (Routing.Action action, params string [] patterns)
		{
			return AddRouteHandler (action, patterns, HttpMethods.GetMethods);
		}

		public RouteHandler Get (IModule module, params string [] patterns)
		{
			return AddRouteHandler (module, patterns, HttpMethods.GetMethods);
		}
		
		//
		
		public RouteHandler Put (string pattern, IModule module)
		{
			return AddRouteHandler (module, new string [] { pattern }, HttpMethods.PutMethods);
		}

		public RouteHandler Put (string pattern, Routing.Action action)
		{
			return AddRouteHandler (action, new string [] { pattern }, HttpMethods.PutMethods);
		}

		public RouteHandler Put (Routing.Action action, params string [] patterns)
		{
			return AddRouteHandler (action, patterns, HttpMethods.PutMethods);
		}

		public RouteHandler Put (string pattern, MatchType matchType, Routing.Action action)
		{
			IMatchOperation [] ops = OpsForPatterns (new string [] { pattern }, matchType);

			return AddRouteHandler (action, ops, HttpMethods.PutMethods);
		}

		public RouteHandler Put (IModule module, params string [] patterns)
		{
			return AddRouteHandler (module, patterns, HttpMethods.PutMethods);
		}
		
		public RouteHandler Post (string pattern, IModule module)
		{
			return AddRouteHandler (module, new string [] { pattern }, HttpMethods.PostMethods);
		}

		public RouteHandler Post (string pattern, Routing.Action action)
		{
			return AddRouteHandler (action, new string [] { pattern }, HttpMethods.PostMethods);
		}

		public RouteHandler Post (string pattern, MatchType matchType, Routing.Action action)
		{
			IMatchOperation [] ops = OpsForPatterns (new string [] { pattern }, matchType);

			return AddRouteHandler (action, ops, HttpMethods.PostMethods);
		}

		public RouteHandler Post (Routing.Action action, params string [] patterns)
		{
			return AddRouteHandler (action, patterns, HttpMethods.PostMethods);
		}

		public RouteHandler Post (IModule module, params string [] patterns)
		{
			return AddRouteHandler (module, patterns, HttpMethods.PostMethods);
		}
		
		//
		
		public RouteHandler Delete (string pattern, IModule module)
		{
			return AddRouteHandler (module, new string [] { pattern }, HttpMethods.DeleteMethods);
		}

		public RouteHandler Delete (string pattern, Routing.Action action)
		{
			return AddRouteHandler (action, new string [] { pattern }, HttpMethods.DeleteMethods);
		}

		public RouteHandler Delete (string pattern, MatchType matchType, Routing.Action action)
		{
			IMatchOperation [] ops = OpsForPatterns (new string [] { pattern }, matchType);

			return AddRouteHandler (action, ops, HttpMethods.DeleteMethods);
		}

		public RouteHandler Delete (Routing.Action action, params string [] patterns)
		{
			return AddRouteHandler (action, patterns, HttpMethods.DeleteMethods);
		}

		public RouteHandler Delete (IModule module, params string [] patterns)
		{
			return AddRouteHandler (module, patterns, HttpMethods.DeleteMethods);
		}
		
		//
		
		public RouteHandler Head (string pattern, IModule module)
		{
			return AddRouteHandler (module, new string [] { pattern }, HttpMethods.HeadMethods);
		}

		public RouteHandler Head (string pattern, Routing.Action action)
		{
			return AddRouteHandler (action, new string [] { pattern }, HttpMethods.HeadMethods);
		}

		public RouteHandler Head (string pattern, MatchType matchType, Routing.Action action)
		{
			IMatchOperation [] ops = OpsForPatterns (new string [] { pattern }, matchType);

			return AddRouteHandler (action, ops, HttpMethods.HeadMethods);
		}

		public RouteHandler Head (Routing.Action action, params string [] patterns)
		{
			return AddRouteHandler (action, patterns, HttpMethods.HeadMethods);
		}

		public RouteHandler Head (IModule module, params string [] patterns)
		{
			return AddRouteHandler (module, patterns, HttpMethods.HeadMethods);
		}
		
		//
		
		public RouteHandler Options (string pattern, IModule module)
		{
			return AddRouteHandler (module, new string [] { pattern }, HttpMethods.OptionsMethods);
		}

		public RouteHandler Options (string pattern, Routing.Action action)
		{
			return AddRouteHandler (action, new string [] { pattern }, HttpMethods.OptionsMethods);
		}

		public RouteHandler Options (string pattern, MatchType matchType, Routing.Action action)
		{
			IMatchOperation [] ops = OpsForPatterns (new string [] { pattern }, matchType);

			return AddRouteHandler (action, ops, HttpMethods.OptionsMethods);
		}

		public RouteHandler Options (Routing.Action action, params string [] patterns)
		{
			return AddRouteHandler (action, patterns, HttpMethods.OptionsMethods);
		}

		public RouteHandler Options (IModule module, params string [] patterns)
		{
			return AddRouteHandler (module, patterns, HttpMethods.OptionsMethods);
		}
		
		//
		
		public RouteHandler Trace (string pattern, IModule module)
		{
			return AddRouteHandler (module, new string [] { pattern }, HttpMethods.TraceMethods);
		}

		public RouteHandler Trace (string pattern, Routing.Action action)
		{
			return AddRouteHandler (action, new string [] { pattern }, HttpMethods.TraceMethods);
		}

		public RouteHandler Trace (string pattern, MatchType matchType, Routing.Action action)
		{
			IMatchOperation [] ops = OpsForPatterns (new string [] { pattern }, matchType);

			return AddRouteHandler (action, ops, HttpMethods.TraceMethods);
		}

		public RouteHandler Trace (Routing.Action action, params string [] patterns)
		{
			return AddRouteHandler (action, patterns, HttpMethods.TraceMethods);
		}

		public RouteHandler Trace (IModule module, params string [] patterns)
		{
			return AddRouteHandler (module, patterns, HttpMethods.TraceMethods);
		}
		
		public static Timeout AddTimeout (TimeSpan timespan, TimeoutCallback callback)
		{
			return AddTimeout (timespan, RepeatBehavior.Single, null, callback);
		}
		
		public static Timeout AddTimeout (TimeSpan timespan, IRepeatBehavior repeat, TimeoutCallback callback)
		{
			return AddTimeout (timespan, repeat, null, callback);
		}
		
		public static Timeout AddTimeout (TimeSpan timespan, object data, TimeoutCallback callback)
		{
			return AddTimeout (timespan, RepeatBehavior.Single, data, callback);
		}
		
		public static Timeout AddTimeout (TimeSpan timespan, IRepeatBehavior repeat, object data, TimeoutCallback callback)
		{
			return ApplicationHost.AddTimeout (timespan, repeat, data, callback);
		}

		public static void AddPipe (Pipe pipe)
		{
			ApplicationHost.AddPipe (pipe);
		}

		public static void RenderTemplate (Context context, string template, object data)
		{
			Engine.RenderToStream (template, context.Response.Writer, data);
		}

		public void StartInternal ()
		{
			AddImplicitRoutes ();
		}

		private void AddImplicitRoutes ()
		{
			MethodInfo[] methods = GetType ().GetMethods ();

			foreach (MethodInfo meth in methods) {
				if (meth.ReturnType != typeof(void))
					continue;

				if (IsIgnored (meth))
					continue;
				
				ParameterInfo [] parameters = meth.GetParameters ();
				if (IsActionSignature (meth, parameters)) {
					AddActionHandler (Routes, meth);
					continue;	
				}
				
				if (IsParameterizedActionSignature (meth, parameters)) {
					AddParameterizedActionHandler (Routes, meth);
					continue;
				}
			}
			
			PropertyInfo [] properties = GetType ().GetProperties ();
			
			foreach (PropertyInfo prop in properties) {
				if (!typeof (Module).IsAssignableFrom (prop.PropertyType))
					continue;
				
				if (IsIgnored (prop))
					continue;
				
				AddImplicitModule (prop);
			}
		}

		private bool IsActionSignature (MethodInfo method, ParameterInfo [] parameters)
		{
			if (parameters.Length != 1)
				return false;

			if (method.DeclaringType.Assembly == typeof (Module).Assembly)
				return false;

			if (parameters [0].ParameterType != typeof (IContext))
				return false;

			return true;
		}

		private bool IsParameterizedActionSignature (MethodInfo method, ParameterInfo [] parameters)
		{
			if (parameters.Length < 1) {
				return false;
			}

			if (method.DeclaringType.Assembly == typeof (Module).Assembly)
				return false;

			if (parameters [0].ParameterType != typeof (IContext))
				return false;
			
			return true;
		}
		
		private bool IsIgnored (MemberInfo info)
		{
			object [] atts = info.GetCustomAttributes (typeof (IgnoreHandlerAttribute), false);

			return atts.Length > 0;
		}
		
		private void AddActionHandler (RouteHandler routes, MethodInfo info)
		{
			HttpMethodAttribute [] atts = (HttpMethodAttribute []) info.GetCustomAttributes (typeof (HttpMethodAttribute), false);

			if (atts.Length == 0) {
				AddDefaultHandlerForAction (routes, info);
				return;
			}

			foreach (HttpMethodAttribute att in atts) {
				AddHandlerForAction (routes, att, info);
			}

		}

		private void AddDefaultHandlerForAction (RouteHandler routes, MethodInfo info)
		{
			Routing.Action action = ActionForMethod (info);

			ActionTarget target = new ActionTarget (action);
			AddImplicitRouteHandlerForTarget (target, new string [] { "/" + info.Name }, HttpMethods.RouteMethods, MatchType.String);
		}

		private void AddHandlerForAction (RouteHandler routes, HttpMethodAttribute att, MethodInfo info)
		{
			Routing.Action action = ActionForMethod (info);
			
			ActionTarget target = new ActionTarget (action);
			
			string[] patterns = null == att.Patterns ? new string [] { "/" + info.Name } : att.Patterns;

			AddImplicitRouteHandlerForTarget (target, OpsForPatterns (patterns, att.MatchType), att.Methods);
		}
		
		private Routing.Action ActionForMethod (MethodInfo info)
		{
			Routing.Action action;
			
			if (info.IsStatic)
				action = (Routing.Action) Delegate.CreateDelegate (typeof (Routing.Action), info);
			else
				action = (Routing.Action) Delegate.CreateDelegate (typeof (Routing.Action), this, info);
			
			return action;
		}
		
		private void AddParameterizedActionHandler (RouteHandler routes, MethodInfo info)
		{
			HttpMethodAttribute [] atts = (HttpMethodAttribute []) info.GetCustomAttributes (typeof (HttpMethodAttribute), false);

			if (atts.Length == 0) {
				AddDefaultHandlerForParameterizedAction (routes, info);
				return;
			}

			foreach (HttpMethodAttribute att in atts) {
				AddHandlerForParameterizedAction (routes, att, info);
			}

		}
		
		private void AddDefaultHandlerForParameterizedAction (RouteHandler routes, MethodInfo info)
		{
			ParameterizedAction action = ParameterizedActionFactory.CreateAction (info);
			ParameterizedActionTarget target = new ParameterizedActionTarget (this, info, action);
			
			AddImplicitRouteHandlerForTarget (target, new string [] { "/" + info.Name }, HttpMethods.RouteMethods, MatchType.String);
		}
		
		private void AddHandlerForParameterizedAction (RouteHandler routes, HttpMethodAttribute att, MethodInfo info)
		{
			ParameterizedAction action = ParameterizedActionFactory.CreateAction (info);
			ParameterizedActionTarget target = new ParameterizedActionTarget (this, info, action);
			
			AddImplicitRouteHandlerForTarget (target, att.Patterns, att.Methods, att.MatchType);
		}
		
		private void AddImplicitModule (PropertyInfo prop)
		{
			var value = (Module) prop.GetValue (this, null);
			
			if (value == null) {
				try {
					value = (Module) Activator.CreateInstance (prop.PropertyType);
				} catch (System.Exception e) {
					throw new Http.Exception (String.Format ("Unable to create default property value for '{0}'", prop), e);
				}
				
				prop.SetValue (this, value, null);
			}
			
			AddImplicitRouteHandlerForModule (value, new string [] { "/" + prop.Name }, HttpMethods.RouteMethods);
		}
		
		private static string default404 = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"" +
		"\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" +
		"<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">" +
		"<head>" +
		"  <meta http-equiv=\"content-type\" content=\"text/html; charset=UTF-8\" />" +
		"  <title>The page you were looking for doesn't exist (404)</title>" +
		"<style type=\"text/css\">" +
		"body { background-color: #fff; color: #666; text-align: center; font-family: arial, sans-serif; }" +
		"div.dialog {" +
		"width: 25em;" +
		"padding: 0 4em;" +
		"margin: 4em auto 0 auto;" +
		"border: 1px solid #ccc;" +
		"border-right-color: #999;" +
		"border-bottom-color: #999;" +
		"}" +
		"h1 { font-size: 100%; color: #f00; line-height: 1.5em; }" +
		"</style>" +
		"</head>" +
		"<body>" +
		"  <div class=\"dialog\">" +
		"    <h1>404 - The page you were looking for doesn't exist.</h1>" +
		"    <p>You may have mistyped the address or the page may have moved.</p>" +
		"    <p><small>Powered by <a href=\"http://manosdemono.org\">Waser</a></small></p>" +
		"  </div>" +
		"</body>" +
		"</html>";
		
		private static string default500 = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"" +
		"\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" +
		"<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">" +
		"<head>" +
		"  <meta http-equiv=\"content-type\" content=\"text/html; charset=UTF-8\" />" +
		"  <title>We're sorry, but something went wrong (500)</title>" +
		"<style type=\"text/css\">" +
		"body { background-color: #fff; color: #666; text-align: center; font-family: arial, sans-serif; }" +
		"div.dialog {" +
		"width: 25em;" +
		"padding: 0 4em;" +
		"margin: 4em auto 0 auto;" +
		"border: 1px solid #ccc;" +
		"border-right-color: #999;" +
		"border-bottom-color: #999;" +
		"}" +
		"h1 { font-size: 100%; color: #f00; line-height: 1.5em; }" +
		"</style>" +
		"</head>" +
		"<body>" +
		"  <div class=\"dialog\">" +
		"    <h1>500 - We're sorry, but something went wrong.</h1>" +
		"    <p><small>Powered by <a href=\"http://manosdemono.org\">Waser</a></small></p>" +
		"  </div>" +
		"</body>" +
		"</html>";
	}
}

