using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace NETCore.LittleSpider.DataFlow
{
    public abstract class DataFlowBase : IDataFlow
	{
		protected ILogger Logger { get; private set; }

		public virtual string Name => GetType().Name;

		/// <summary>
		/// 初始化
		/// </summary>
		/// <returns></returns>
		public virtual Task InitAsync()
		{
			return Task.CompletedTask;
		}

		public void SetLogger(ILogger logger)
		{
			if (logger == null)
				throw new ArgumentNullException(nameof(logger));
			Logger = logger;
		}

		/// <summary>
		/// 流处理
		/// </summary>
		/// <param name="context">处理上下文</param>
		/// <returns></returns>
		public abstract Task HandleAsync(DataFlowContext context);

		/// <summary>
		/// 释放
		/// </summary>
		public virtual void Dispose()
		{
		}
	}
}
