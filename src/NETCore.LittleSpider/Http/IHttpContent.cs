using System;

namespace NETCore.LittleSpider.Http
{
    public interface IHttpContent : IDisposable, ICloneable
	{
		ContentHeaders Headers { get; }
	}
}
