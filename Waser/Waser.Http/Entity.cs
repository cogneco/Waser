//
// Copyright (C) 2013 Simon Mika (smika@hx.se)
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
using System.Text;
using System.Net;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Error = Kean.Error;
using Libev;
using Waser.IO;
using Waser.Collections;
using Kean;
using Kean.Extension;
namespace Waser.Http
{
	/// <summary>
	///  A base class for HttpRequest and HttpResponse.  Generally user code should not care at all about
	///  this class, it just exists to eliminate some code duplication between the two derived types.
	/// </summary>
	public abstract class Entity : IDisposable
	{
		static readonly long MAX_BUFFERED_CONTENT_LENGTH = 2621440;
		// 2.5MB (Eventually this will be an environment var)
		Headers headers;
		Parser parser;
		public ParserSettings ParserSettings { get; private set; }
		StringBuilder currentHeaderField = new StringBuilder();
		StringBuilder currentHeaderValue = new StringBuilder();
		DataDictionary data;
		DataDictionary postData;
		Dictionary<string,object> properties;
		Dictionary<string,UploadedFile> uploadedFiles;
		IBodyHandler bodyHandler;
		bool finishedReading;
		IAsyncWatcher endWatcher;
		public IO.Context Context { get; private set; }
		public ITcpSocket Socket { get; protected set; }
		public Stream Stream { get; protected set; }
		public Headers Headers
		{
			get
			{
				if (this.headers.IsNull())
					this.headers = new Headers();
				return this.headers;
			}
			set { this.headers = value; }
		}
		public Method Method { get; set; }
		public int MajorVersion { get; set; }
		public int MinorVersion { get; set; }
		public string RemoteAddress { get; set; }
		public int RemotePort { get; set; }
		public string Path { get; set; }
		public Encoding ContentEncoding
		{
			get { return Headers.ContentEncoding; }
			set { Headers.ContentEncoding = value; }
		}
		public DataDictionary Data
		{
			get
			{
				if (this.data.IsNull())
					this.data = new DataDictionary();
				return this.data;
			}
		}
		public DataDictionary PostData
		{
			get
			{
				if (this.postData.IsNull())
				{
					this.postData = new DataDictionary();
					this.Data.Children.Add(this.postData);
				}
				return this.postData;
			}
			set
			{
				this.SetDataDictionary(this.postData, value);
				this.postData = value;
			}
		}
		public string PostBody { get; set; }
		public Dictionary<string, UploadedFile> Files
		{
			get
			{
				if (this.uploadedFiles.IsNull())
					this.uploadedFiles = new Dictionary<string, UploadedFile>();
				return this.uploadedFiles;
			}
		}
		public Dictionary<string, object> Properties
		{
			get
			{
				if (this.properties.IsNull())
					this.properties = new Dictionary<string, object>();
				return this.properties;
			}
		}
		public Entity(IO.Context context)
		{
			this.Context = context;
			this.endWatcher = context.CreateAsyncWatcher(this.HandleEnd);
			this.endWatcher.Start();
		}
		~Entity ()
		{
			this.Dispose();
		}
		public void Dispose()
		{
			this.Socket = null;
			if (this.Stream.NotNull())
			{
				this.Stream.Dispose();
				this.Stream = null;
			}

			if (this.endWatcher.NotNull())
			{
				this.endWatcher.Dispose();
				this.endWatcher = null;
			}
		}
		public void SetProperty(string name, object o)
		{
			if (name.NotNull())
				throw new ArgumentNullException("name");
			if (o.NotNull() || this.properties.NotNull())
			{
				if (this.properties.IsNull())
					this.properties = new Dictionary<string,object>();
				if (o.IsNull())
				{
					this.properties.Remove(name);
					if (this.properties.Count == 0)
						this.properties = null;
				}
				else
					this.properties[name] = o;
			}
		}
		public object GetProperty(string name)
		{
			if (name.IsNull())
				throw new ArgumentNullException("name");
			object result = null;
			return this.properties.NotNull() && this.properties.TryGetValue(name, out result) ? result : null;
		}
		public T GetProperty<T>(string name)
		{
			object result = GetProperty(name);
			return result.IsNull() ? default(T) : (T)result;
		}
		protected void SetDataDictionary(DataDictionary old, DataDictionary @new)
		{
			if (this.data.NotNull() && old.NotNull())
				this.data.Children.Remove(old);
			if (@new.NotNull())
				this.Data.Children.Add(@new);
		}
		protected void CreateParserSettingsInternal()
		{
			this.ParserSettings = CreateParserSettings();
			this.ParserSettings.OnError = OnParserError;
			this.ParserSettings.OnBody = OnBody;
			this.ParserSettings.OnMessageBegin = OnMessageBegin;
			this.ParserSettings.OnMessageComplete = OnMessageComplete;
			this.ParserSettings.OnHeaderField = OnHeaderField;
			this.ParserSettings.OnHeaderValue = OnHeaderValue;
			this.ParserSettings.OnHeadersComplete = OnHeadersComplete;
		}
		private int OnMessageBegin(Parser parser)
		{
			return 0;
		}
		private int OnMessageComplete(Parser parser)
		{
			// Upgrade connections will raise this event at the end of OnBytesRead
			if (!parser.Upgrade)
				this.OnFinishedReading(parser);
			this.finishedReading = true;
			return 0;
		}
		public int OnHeaderField(Parser parser, ByteBuffer data, int pos, int len)
		{
			string str = Encoding.ASCII.GetString(data.Bytes, pos, len);

			if (currentHeaderValue.Length != 0)
				FinishCurrentHeader();

			currentHeaderField.Append(str);
			return 0;
		}
		public int OnHeaderValue(Parser parser, ByteBuffer data, int pos, int len)
		{
			string str = Encoding.ASCII.GetString(data.Bytes, pos, len);

			if (currentHeaderField.Length == 0)
				throw new System.Exception("Header Value raised with no header field set.");

			currentHeaderValue.Append(str);
			return 0;
		}
		private void FinishCurrentHeader()
		{
			try
			{
				if (headers == null)
					headers = new Headers();
				headers.SetHeader(currentHeaderField.ToString(), currentHeaderValue.ToString());
				currentHeaderField.Length = 0;
				currentHeaderValue.Length = 0;
			}
			catch (System.Exception e)
			{
				Console.WriteLine(e);
			}
		}
		protected virtual int OnHeadersComplete(Parser parser)
		{
			if (currentHeaderField.Length != 0)
				FinishCurrentHeader();

			MajorVersion = parser.Major;
			MinorVersion = parser.Minor;
			Method = parser.HttpMethod;

			return 0;
		}
		public int OnBody(Parser parser, ByteBuffer data, int pos, int len)
		{
			if (bodyHandler == null)
				CreateBodyHandler();

			if (bodyHandler != null)
				bodyHandler.HandleData(this, data, pos, len);

			return 0;
		}
		private void CreateBodyHandler()
		{
			string ct;

			if (!Headers.TryGetValue("Content-Type", out ct))
			{
				bodyHandler = new BufferedBodyHandler();
				return;
			}

			if (ct.StartsWith("application/x-www-form-urlencoded", StringComparison.InvariantCultureIgnoreCase))
			{
				bodyHandler = new FormDataHandler();
				return;
			}

			if (ct.StartsWith("multipart/form-data", StringComparison.InvariantCultureIgnoreCase))
			{
				string boundary = ParseBoundary(ct);
				IUploadedFileCreator file_creator = GetFileCreator();

				bodyHandler = new MultiPartFormDataHandler(boundary, ContentEncoding, file_creator);
				return;
			}

			bodyHandler = new BufferedBodyHandler();
		}
		private IUploadedFileCreator GetFileCreator()
		{
			if (Headers.ContentLength == null || Headers.ContentLength >= MAX_BUFFERED_CONTENT_LENGTH)
				return new TempFileUploadedFileCreator();
			return new InMemoryUploadedFileCreator();
		}
		private void OnParserError(Parser parser, string message, ByteBuffer buffer, int initial_position)
		{
			// Transaction.Abort (-1, "HttpParser error: {0}", message);
			Socket.Close();
		}
		public virtual void Reset()
		{
			this.Path = null;
			this.ContentEncoding = null;
			this.headers = null;
			this.data = null;
			this.postData = null;
			this.uploadedFiles = null;
			this.finishedReading = false;
			if (this.ParserSettings.IsNull())
				this.CreateParserSettingsInternal();
			this.parser = new Parser();
		}
		public void Read()
		{
			this.Read(() =>
			{
			});
		}
		public void Read(Action onClose)
		{
			this.Reset();
			this.Socket.GetSocketStream().Read(this.OnBytesRead, (@object) =>
			{
			}, onClose);
		}
		void OnBytesRead(ByteBuffer bytes)
		{
			Error.Log.Call<System.Exception>(() => this.parser.Execute(this.ParserSettings, bytes), e =>
			{
				Console.WriteLine("Exception while parsing");
				Console.WriteLine(e);
			});

			if (finishedReading && parser.Upgrade)
			{

				//
				// Well, this is a bit of a hack.  Ideally, maybe there should be a putback list
				// on the socket so we can put these bytes back into the stream and the upgrade
				// protocol handler can read them out as if they were just reading normally.
				//

				if (bytes.Position < bytes.Length)
				{
					byte[] upgrade_head = new byte [bytes.Length - bytes.Position];
					Array.Copy(bytes.Bytes, bytes.Position, upgrade_head, 0, upgrade_head.Length);

					SetProperty("UPGRADE_HEAD", upgrade_head);
				}

				// This is delayed until here with upgrade connnections.
				OnFinishedReading(parser);
			}
		}
		protected virtual void OnFinishedReading(Parser parser)
		{
			if (bodyHandler != null)
			{
				bodyHandler.Finish(this);
				bodyHandler = null;
			}

			if (OnCompleted != null)
				OnCompleted();
		}
		public static string ParseBoundary(string ct)
		{
			if (ct == null)
				return null;

			int start = ct.IndexOf("boundary=");
			if (start < 1)
				return null;
			
			return ct.Substring(start + "boundary=".Length);
		}
		public void Write(string str)
		{
			byte[] data = ContentEncoding.GetBytes(str);

			WriteToBody(data, 0, data.Length);
		}
		public void Write(byte[] data)
		{
			WriteToBody(data, 0, data.Length);
		}
		public void Write(byte[] data, int offset, int length)
		{
			WriteToBody(data, offset, length);
		}
		public void Write(string str, params object[] prms)
		{
			Write(String.Format(str, prms));	
		}
		public void End(string str)
		{
			Write(str);
			End();
		}
		public void End(byte[] data)
		{
			Write(data);
			End();
		}
		public void End(byte[] data, int offset, int length)
		{
			Write(data, offset, length);
			End();
		}
		public void End(string str, params object[] prms)
		{
			Write(str, prms);
			End();
		}
		public void End()
		{
			endWatcher.Send();
		}
		internal virtual void HandleEnd()
		{
			if (OnEnd != null)
				OnEnd();
		}
		public void Complete(Action callback)
		{
			IAsyncWatcher completeWatcher = null;
			completeWatcher = Context.CreateAsyncWatcher(delegate
			{
				completeWatcher.Dispose();
				callback();
			});
			completeWatcher.Start();
			Stream.End(completeWatcher.Send);
		}
		public void WriteLine(string str)
		{
			Write(str + System.Environment.NewLine);	
		}
		public void WriteLine(string str, params object[] prms)
		{
			WriteLine(String.Format(str, prms));	
		}
		public void SendFile(string file)
		{
			Stream.SendFile(file);
		}
		private void WriteToBody(byte[] data, int offset, int length)
		{
			Stream.Write(data, offset, length);
		}
		public byte [] GetBody()
		{
			StringBuilder data = null;

			if (PostBody != null)
			{
				data = new StringBuilder();
				data.Append(PostBody);
			}

			if (postData != null)
			{
				data = new StringBuilder();
				bool first = true;
				foreach (string key in postData.Keys)
				{
					if (!first)
						data.Append('&');
					first = false;

					UnsafeString s = postData.Get(key);
					if (s != null)
					{
						data.AppendFormat("{0}={1}", key, s.UnsafeValue);
						continue;
					}
				}
			}

			if (data == null)
				return null;

			return ContentEncoding.GetBytes(data.ToString());
			
		}
		public abstract void WriteMetadata(StringBuilder builder);
		public abstract ParserSettings CreateParserSettings();
		public event Action OnEnd;
		public event Action OnCompleted;
	}
}


