using Logistics.IdentityServer;
using Logistics.HostDefaults;

LogisticsHost.Run(args, builder => builder
    .ConfigureServices()
    .ConfigurePipeline());
