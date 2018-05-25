﻿using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Node.Feed
{
    public class FeedWatcherScheduler
    {
        private static IScheduler scheduler;
        private static StdSchedulerFactory factory;

        static FeedWatcherScheduler()
        {
            factory = new StdSchedulerFactory();
        }

        public static async void Start(string proxyUrl)
        {
            FeedJob.ProxyUrl = proxyUrl;

            scheduler = await factory.GetScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<FeedJob>().Build();

            ITrigger trigger = TriggerBuilder.Create().WithCronSchedule("0 0/1 * * * ?").Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        public static async void Stop()
        {
            await scheduler.Shutdown(false);
        }
    }
}
