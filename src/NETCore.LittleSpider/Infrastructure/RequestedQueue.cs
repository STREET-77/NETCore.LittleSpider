using NETCore.LittleSpider.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace NETCore.LittleSpider.Infrastructure
{
	public class RequestedQueue : IDisposable
	{
		private readonly Dictionary<string, Request> _dict;
		private readonly List<Request> _queue;

		public RequestedQueue()
		{
			_dict = new Dictionary<string, Request>();
			_queue = new List<Request>();
		}

		public int Count => _dict.Count;

		public bool Enqueue(Request request)
		{
			if (!_dict.ContainsKey(request.Hash))
			{
				_dict.Add(request.Hash, request);
				return true;
			}

			return false;
		}

		public Request Dequeue(string hash)
		{
			var request = _dict[hash];
			_dict.Remove(hash);
			return request;
		}

		public Request[] GetAllTimeoutList()
		{
			var data = _queue.ToArray();
			_queue.Clear();
			return data;
		}

		private void Timeout(string hash)
		{
			if (_dict.ContainsKey(hash))
			{
				var request = _dict[hash];
				_queue.Add(request);
				_dict.Remove(hash);
			}
		}

		public void Dispose()
		{
			_dict.Clear();
			_queue.Clear();
		}
	}
}
