using System;
using System.Net;

namespace Waser.IO
{
	public interface IUdpSocket : IStreamSocket<UdpPacket, IStream<UdpPacket>, IPEndPoint>
	{
	}
}

