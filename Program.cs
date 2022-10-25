using Quartz;
using Quartz.Impl;
using System;
using System. Threading. Tasks;
using Microsoft.Extensions.Configuration;

namespace FuelPriceReadAPI
{ 
    class Program
    {
        //Parameter to determine number of days between the runs read from appSettings.json
        static async Task Main(string[] args)
            {
            IConfiguration Config = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json")
                .Build();
        int daysTaskDelayed = Convert.ToInt32(Config.GetSection("daysTaskDelayed").Value);
        ISchedulerFactory schedulerFactory = new StdSchedulerFactory();

        IScheduler scheduler = await schedulerFactory.GetScheduler();
        await scheduler.Start();

        IJobDetail job = JobBuilder.Create<SCheduleAPIReadJob>()
            .WithIdentity("SChedule Read Data Job", "SChedule Read Data Job")
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("Read Fuel Price Trigger", "Read Fuel Price group")
            .WithCalendarIntervalSchedule(x => x.WithIntervalInDays(daysTaskDelayed))
            .Build();
        
            await scheduler.ScheduleJob(job, trigger);
            Console.ReadLine();
    }
}
}