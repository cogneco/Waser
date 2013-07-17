using System;
using System.Net;

namespace Waser.IO
{
	public interface ITcpSocket : IStreamSocket<ByteBuffer, IByteStream, IPEndPoint>
	{
	}
}

