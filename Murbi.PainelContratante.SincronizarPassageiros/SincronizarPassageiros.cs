using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Murbi.PainelContratante.SincronizarPassageiros;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Teste
{
    public static class SincronizarPassageiros
    {
        [FunctionName("SincronizarPassageiros")]
        public static async Task Run([TimerTrigger("0 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            //log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");


            var apiRandon = new APIRandon();
            await apiRandon.AutenticarApi();

            if (apiRandon.Response.Success)
            {
                var token = apiRandon.Response.Data;

                var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", true)
                .Build();
                var conexao = configuration.GetConnectionString("DefaultConnection");

                using SqlConnection conn = new SqlConnection(conexao);

                conn.Open();

                var sql = $@"insert into LogsSeguranca values ('{Guid.NewGuid() }', '{Guid.Empty }', '{Guid.Empty }', '', '', '', '', 'Token [{token}] obtido em {DateTime.Now}', '','', '{DateTime.Now}')";

                using SqlCommand cmd = new SqlCommand(sql, conn);
                var rows = await cmd.ExecuteNonQueryAsync();
                log.LogInformation($"{rows} rows were updated");
            }

            

        }
    }
}
