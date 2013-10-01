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
using System.IO;
using Kean;
using Kean.Extension;
using Error = Kean.Error;
using Waser.IO;
namespace Waser.Http
{
    public class ParserSettings
    {
        public Callback OnMessageBegin;
        public DataCallback OnPath;
        public DataCallback OnQueryString;
        public DataCallback OnUrl;
        public DataCallback OnFragment;
        public DataCallback OnHeaderField;
        public DataCallback OnHeaderValue;
        public Callback OnHeadersComplete;
        public DataCallback OnBody;
        public Callback OnMessageComplete;
        public ErrorCallback OnError;
        public void RaiseOnMessageBegin(Parser p)
        {
            Raise(OnMessageBegin, p);
        }
        public void RaiseOnMessageComplete(Parser p)
        {
            Raise(OnMessageComplete, p);
        }
        // this one is a little bit different:
        // the current `position` of the buffer is the location of the
        // error, `ini_pos` indicates where the position of
        // the buffer when it was passed to the `execute` method of the parser, i.e.
        // using this information and `limit` we'll know all the valid data
        // in the buffer around the error we can use to print pretty error
        // messages.
        public void RaiseOnError(Parser p, string message, ByteBuffer buf, int ini_pos)
        {
            if (this.OnError.NotNull())
                this.OnError(p, message, buf, ini_pos);

			
            // if on_error gets called it MUST throw an exception, else the parser 
            // will attempt to continue parsing, which it can't because it's
            // in an invalid state.
            Console.WriteLine("ERROR: '{0}'", message);
            throw new System.Exception(message);
        }
        public void RaiseOnHeaderField(Parser p, ByteBuffer buf, int pos, int len)
        {
            Raise(OnHeaderField, p, buf, pos, len);
        }
        public void RaiseOnQueryString(Parser p, ByteBuffer buf, int pos, int len)
        {
            Raise(OnQueryString, p, buf, pos, len);
        }
        public void RaiseOnFragment(Parser p, ByteBuffer buf, int pos, int len)
        {
            Raise(OnFragment, p, buf, pos, len);
        }
        public void RaiseOnPath(Parser p, ByteBuffer buf, int pos, int len)
        {
            Raise(OnPath, p, buf, pos, len);
        }
        public void RaiseOnHeaderValue(Parser p, ByteBuffer buf, int pos, int len)
        {
            Raise(OnHeaderValue, p, buf, pos, len);
        }
        public void RaiseOnUrl(Parser p, ByteBuffer buf, int pos, int len)
        {
            Raise(OnUrl, p, buf, pos, len);
        }
        public void RaiseOnBody(Parser p, ByteBuffer buf, int pos, int len)
        {
            Raise(OnBody, p, buf, pos, len);
        }
        public int RaiseOnHeadersComplete(Parser p)
        {
            return Raise(OnHeadersComplete, p);
        }
        int Raise(Callback callback, Parser parser)
        {
            int result = 0;
            if (callback.NotNull())
                Error.Log.Call<System.Exception>(() => result = callback(parser), e =>
                {
                    Console.WriteLine(e);
                    this.RaiseOnError(parser, e.Message, null, -1);
                    result = -1;
                });
            return result;
        }
        int Raise(DataCallback callback, Parser parser, ByteBuffer buffer, int position, int length)
        {
            int result = 0;
            if (callback.NotNull() && position >= 0)
                Error.Log.Call<System.Exception>(() => result = callback(parser, buffer, position, length), e =>
                { 
                    Console.WriteLine(e);

                    RaiseOnError(parser, e.Message, buffer, position);
                    result = -1;
                });
            return result;
        }
    }
}
