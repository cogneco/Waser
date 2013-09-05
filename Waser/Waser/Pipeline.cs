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
using System.Runtime.InteropServices;
using Error = Kean.Error;
using Waser.Http;
using Waser.Routing;
namespace Waser
{
    public enum PipelineStep
    {
        PreProcess,
        Execute,
        WaitingForEnd,
        PostProcess,
        Complete
    }
    /// <summary>
    /// A pipeline coordinates an httprequest/response session.  Making sure all the ManosPipes are invoked
    /// and invoking the actual request exectution.
    /// </summary>
    /// <remarks>
    /// User code should typically not use this type.
    /// </remarks>
    public class Pipeline
    {
        private Application application;
        private Context context;
        private ITransaction transaction;
        private int pending;
        private PipelineStep step;
        private GCHandle handle;
        public Pipeline(Application application, ITransaction transaction)
        {
            this.application = application;
            this.transaction = transaction;

            pending = ApplicationHost.Pipes == null ? 1 : ApplicationHost.Pipes.Count;
            step = PipelineStep.PreProcess;
            handle = GCHandle.Alloc(this);

            transaction.Response.OnEnd += HandleEnd;
        }
        public void Begin()
        {
            if (ApplicationHost.Pipes == null)
            {
                StepCompleted();
                return;
            }

            foreach (IPipe pipe in ApplicationHost.Pipes)
            {
                try
                {
                    pipe.OnPreProcessRequest(application, transaction, StepCompleted);
					
                    if (transaction.Aborted)
                        return;
                }
                catch (System.Exception e)
                {
                    pending--;

                    Console.Error.WriteLine("Exception in {0}::OnPreProcessRequest.", pipe);
                    Console.Error.WriteLine(e);
                }
            }
        }
        private void Execute()
        {
            step = PipelineStep.WaitingForEnd;

            context = new Context(transaction);

            var handler = application.Routes.Find(transaction.Request);

            PipePreProcessTarget((newHandler) =>
            {
                if (newHandler != handler)
                {
                    handler = newHandler;
                }
            });
						
            if (handler == null)
            {
                context.Response.StatusCode = 404;
                context.Response.End(application.Get404Response());
                return;
            }

            context.Response.StatusCode = 200;

            Error.Log.Call<System.Exception>(() => handler.Invoke(application, context), e =>
            {
                Console.Error.WriteLine("Exception in transaction handler:");
                Console.Error.WriteLine(e);
                context.Response.StatusCode = 500;
                context.Response.End(application.Get500Response());
                //
                // TODO: Maybe the cleanest thing to do is
                // have a HandleError, HandleException thing
                // on HttpTransaction, along with an UnhandledException
                // method/event on ManosModule.
                //
                // end = true;
            });

            PipePostProcessTarget(handler);

            if (context.Response.StatusCode == 404)
            {
                step = PipelineStep.WaitingForEnd;
                context.Response.End();
                return;
            }
        }
        private void PipePreProcessTarget(Action<ITarget> callback)
        {
            if (null != ApplicationHost.Pipes)
            {
                for (int i = 0; i < ApplicationHost.Pipes.Count; ++i)
                {
                    IPipe pipe = ApplicationHost.Pipes[i];
                    try
                    {
                        pipe.OnPreProcessTarget(context, callback);
                    }
                    catch (System.Exception e)
                    {
                        Console.Error.WriteLine("Exception in {0}::OnPreProcessTarget.", pipe);
                        Console.Error.WriteLine(e);
                    }
                }
            }
        }
        private void PipePostProcessTarget(ITarget handler)
        {
            if (null != ApplicationHost.Pipes)
            {
                // reset pending pipes
                pending = ApplicationHost.Pipes == null ? 1 : ApplicationHost.Pipes.Count;
		
                for (int i = ApplicationHost.Pipes.Count - 1; i >= 0; --i)
                {
                    IPipe pipe = ApplicationHost.Pipes[i];
                    try
                    {
                        pipe.OnPostProcessTarget(context, handler, StepCompleted);
                    }
                    catch (System.Exception e)
                    {
                        Console.Error.WriteLine("Exception in {0}::OnPostProcessTarget.", pipe);
                        Console.Error.WriteLine(e);
                    }
                }
            }		
        }
        private void PostProcess()
        {
            if (ApplicationHost.Pipes == null)
            {
                StepCompleted();
                return;
            }

            if (null != ApplicationHost.Pipes)
            {
                // reset pending pipes
                pending = ApplicationHost.Pipes == null ? 1 : ApplicationHost.Pipes.Count;
			
                for (int i = ApplicationHost.Pipes.Count - 1; i >= 0; --i)
                {
                    IPipe pipe = ApplicationHost.Pipes[i];
                    try
                    {
                        pipe.OnPostProcessRequest(application, transaction, StepCompleted);

                        if (context.Transaction.Aborted)
                            return;
                    }
                    catch (System.Exception e)
                    {
                        pending--;

                        Console.Error.WriteLine("Exception in {0}::OnPostProcessRequest.", pipe);
                        Console.Error.WriteLine(e);
                    }
                }
            }
        }
        private void Complete()
        {
            transaction.Response.Complete(transaction.OnResponseFinished);

            handle.Free();
        }
        private void StepCompleted()
        {
            if (--pending > 0)
                return;

            pending = ApplicationHost.Pipes == null ? 1 : ApplicationHost.Pipes.Count;
            step++;
			
            switch (step)
            {
                case PipelineStep.Execute:
                    Execute();
                    break;
                case PipelineStep.PostProcess:
                    PostProcess();
                    break;
                case PipelineStep.Complete:
                    Complete();
                    break;
            }
        }
        private void HandleEnd()
        {
            pending = 0;
            StepCompleted();
        }
    }
}


