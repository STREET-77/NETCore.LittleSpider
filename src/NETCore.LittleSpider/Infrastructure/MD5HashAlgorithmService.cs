﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace NETCore.LittleSpider.Infrastructure
{
	public class MD5HashAlgorithmService : HashAlgorithmService
	{
		private readonly HashAlgorithm _hashAlgorithm;

		public MD5HashAlgorithmService()
		{
			_hashAlgorithm = MD5.Create();
		}

		protected override HashAlgorithm GetHashAlgorithm()
		{
			return _hashAlgorithm;
		}
	}
}
