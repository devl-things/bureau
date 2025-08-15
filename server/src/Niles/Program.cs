using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Niles.Data;
using Niles.Data.Contexts;
using Niles.Data.Repositories;
using Niles.Etl.Abstractions.Configurations;
using Niles.Etl.Abstractions.Extract;
using Niles.Etl.Extract;
using Niles.Etl.Jobs;
using Niles.Etl.Lidl.Services;
using Niles.Etl.Transform;
using Niles.Workers;

namespace Niles
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddOptions<DownloaderOptions>().Bind(builder.Configuration.GetSection("Lidl:Downloader"))
                .Validate(options => !string.IsNullOrWhiteSpace(options.MainUrl))
                .ValidateOnStart();

            //TODO: change db context this as in Sven
            builder.Services.AddDbContext<NilesContext>(options =>
            {
                options.UseInMemoryDatabase("etl-inmemory");
            });

            builder.Services.AddSingleton<IProcessedStore, FileProcessedStore>();

            builder.Services.AddSingleton<IJobManager, JobManagerInMemory>();
            builder.Services.AddSingleton<IJobDispatcher, JobDispatcher>();
            builder.Services.AddHostedService<JobsBackgroundWorker>();

            builder.Services.AddHttpClient<DownloaderService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(100);
            });

            // Add services to the container.
            builder.Services.AddKeyedTransient<IJobHandler, DownloaderJobHandler>(JobType.Downloader);
            builder.Services.AddKeyedTransient<IJobHandler, PricesEtlJobHandler>(JobType.Importer);
            builder.Services.AddTransient<Niles.Etl.Lidl.Services.DownloaderService>();

            builder.Services.AddSingleton<IFileEnumerator, FileEnumerator>();
            builder.Services.AddSingleton<IFileNameParser, LidlFileNameParser>();
            builder.Services.AddSingleton<CsvExtractorService>();
            builder.Services.AddSingleton<RowTransformerService>();
            builder.Services.AddSingleton<IChecksumService, Sha256ChecksumService>();
            builder.Services.AddSingleton<IFileArchiver>(sp => new FileArchiver("archive", "error"));

            builder.Services.AddScoped<IImportFileRepository, EfImportFileRepository>();
            builder.Services.AddScoped<IPriceRepository, EfPriceRepository>();

            builder.Services.AddScoped<Niles.Etl.Lidl.Services.LoaderService>();

            // job manager + background worker
            builder.Services.AddSingleton<Niles.Etl.Jobs.IJobManager, Niles.Etl.Jobs.JobManagerInMemory>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.MapType<JobType>(() => new OpenApiSchema
                {
                    Type = "string",
                    Enum = Enum.GetNames(typeof(JobType))
                               .Select(n => (IOpenApiAny)new OpenApiString(n))
                               .ToList()
                });
            });

            WebApplication app = builder.Build();

            // Optional: warm up the in-memory DB
            using (IServiceScope scope = app.Services.CreateScope())
            {
                NilesContext db = scope.ServiceProvider.GetRequiredService<NilesContext>();
                db.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
