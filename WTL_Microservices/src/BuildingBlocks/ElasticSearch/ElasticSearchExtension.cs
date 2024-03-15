using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace ElasticSearch
{
    public static class ElasticSearchExtension
    {
        public static void AddElasticSearch<T>(IServiceCollection services, IConfiguration configuration,
            Action<ConnectionSettings>? addDefaultMappings) where T : class
        {
            var baseUrl = configuration["ElasticConfiguration:Uri"];
            var index = configuration["ElasticConfiguration:DefaultIndex"];
            var settings = new ConnectionSettings(new Uri(baseUrl ?? "")).PrettyJson()
                .CertificateFingerprint("6b6a8c2ad2bc7b291a7363f7bb96a120b8de326914980c868c1c0bc6b3dc41fd")
                .BasicAuthentication("elastic", "admin").DefaultIndex(index);
            settings.EnableApiVersioningHeader();

            addDefaultMappings?.Invoke(settings);
            //AddDefaultMappings<T>(settings);

            var client = new ElasticClient(settings);
            services.AddSingleton<IElasticClient>(client);
            CreateIndex<T>(client, index);
        }

        private static void CreateIndex<T>(IElasticClient client, string indexName) where T : class
        {
            var createIndexResponse = client.Indices.Create(indexName, index => index.Map<T>(x => x.AutoMap()));
        }
    }
}
