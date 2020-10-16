using System;
using System.Collections.Generic;
using System.Text;

namespace NETCore.LittleSpider.Infrastructure
{
	public interface IHashAlgorithmService
	{
		byte[] ComputeHash(byte[] bytes);
	}
}
