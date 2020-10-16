using NETCore.LittleSpider.Http;
using NETCore.LittleSpider.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NETCore.LittleSpider.Scheduler.Impl
{
    public class Scheduler : IScheduler
    {
        private readonly List<Request> _requests =
            new List<Request>();

        private readonly IHashAlgorithmService HashAlgorithm;

        private SpinLock _spinLock;

        public Scheduler(IHashAlgorithmService hashAlgorithmService)
        {
            HashAlgorithm = hashAlgorithmService;
        }

        public long Total => _requests.Count;

        public Task<IEnumerable<Request>> DequeueAsync(int count = 1)
        {
            var locker = false;

            try
            {
                //申请获取锁
                _spinLock.Enter(ref locker);

                if (count > _requests.Count)
                    count = _requests.Count;

                var requests = _requests.Take(count).ToArray();
                if (requests.Length > 0)
                {
                    _requests.RemoveRange(0, count);
                }

                return Task.FromResult(requests.Select(x => x.Clone()));
            }
            finally
            {
                //工作完毕，或者发生异常时，检测一下当前线程是否占有锁，如果咱有了锁释放它
                //以避免出现死锁的情况
                if (locker)
                {
                    _spinLock.Exit();
                }
            }
        }

        public void Dispose()
        {
            _requests?.Clear();
        }

        public Task EnqueueAsync(IEnumerable<Request> requests)
        {
            foreach (var request in requests)
            {
                request.Hash = request.ComputeHash(HashAlgorithm);
                _requests.Add(request);
            }
            return Task.CompletedTask;
        }
    }
}
