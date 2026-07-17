using Logistics.API;
using Logistics.HostDefaults;

LogisticsHost.Run(args, builder => builder
    .ConfigureServices()
    .ConfigurePipeline()
    .ScheduleJobs());
