using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NETCore.LittleSpider.DataFlow;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NETCore.LittleSpider.Tests
{
    public class HostService : BackgroundService
    {
        private readonly SpiderFactory _spiderFactory;

        public HostService(SpiderFactory spiderFactory)
        {
            _spiderFactory = spiderFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _spiderFactory.AddRequestsAsync("https://www1.hkex.com.hk/hkexwidget/data/getequityquote?sym=700&token=evLtsLsBNAUVTPxtGqVeG2kWK2tkRx82BArPtGQBbW8FuTNNILpbHnxN9ScxYlfV&lang=chi&qid=1602811160864&callback=jQuery35106208578400623672_1602811160864&_=1602811160864");
            await _spiderFactory.AddRequestsAsync("https://www1.hkex.com.hk/hkexwidget/data/getequityquote?sym=701&token=evLtsLsBNAUVTPxtGqVeG2kWK2tkRx82BArPtGQBbW8FuTNNILpbHnxN9ScxYlfV&lang=chi&qid=1602811160864&callback=jQuery35106208578400623672_1602811160864&_=1602811160864");
            _spiderFactory.AddDataFlow(new MyDataFlow());
            _spiderFactory.SetName("Test_Spider");
            await _spiderFactory.ExecuteAsync(stoppingToken);
        }
    }

    public class MyDataFlow : DataFlowBase
    {
        public override async Task HandleAsync(DataFlowContext context)
        {
            Console.WriteLine("MyDataFlow");
            await Task.CompletedTask;
        }
    }
}
