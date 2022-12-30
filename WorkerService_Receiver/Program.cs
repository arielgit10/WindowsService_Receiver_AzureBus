using Microsoft.EntityFrameworkCore;
using WorkerService_Receiver;
using WorkerService_Receiver.Models;
using WorkerService_Receiver.Repository;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "Receiver Service";
    })
    .ConfigureServices((hostContext, services) =>
       {
           IConfiguration configuration = hostContext.Configuration;
           AppSettings.ConnectionString = configuration.GetConnectionString("DefaultConnection");
           AppSettings.QueueConnection = configuration.GetConnectionString("QueueConnection");
           AppConfiguration.IntervalMinutes = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["IntervalMinutes"]);

           var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
           optionsBuilder.UseSqlServer(AppSettings.ConnectionString);
           services.AddScoped<AppDbContext>(db => new AppDbContext(optionsBuilder.Options));

           services.AddScoped<SampleService>();
           services.AddSingleton<Worker>();
           services.AddSingleton<IServerRepository, ServerRepository>();
           services.AddHostedService(
                 provider => provider.GetRequiredService<Worker>());
       })
    .Build();

await host.RunAsync();
