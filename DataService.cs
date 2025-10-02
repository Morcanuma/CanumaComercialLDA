using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lojaCanuma
{
    public class DataService
    {
        private readonly string _cnString;
        private readonly LlamaService _llama;

        public DataService(string connectionString, LlamaService llama)
        {
            _cnString = connectionString;
            _llama = llama;
        }

        // 2.1) Busca histórico de vendas por dia
        public async Task<List<(DateTime dia, double total)>> GetHistoricalSalesAsync(
            DateTime de, DateTime ate)
        {
            var lista = new List<(DateTime, double)>();

            using (var cn = new SqlConnection(_cnString))
            {
                using (var cmd = new SqlCommand(@"
                    SELECT CAST(sdate AS date), SUM(total)
                      FROM tblCar
                     WHERE status='Sold'
                       AND sdate BETWEEN @de AND @ate
                     GROUP BY CAST(sdate AS date)
                     ORDER BY CAST(sdate AS date)", cn))
                {
                    cmd.Parameters.AddWithValue("@de", de);
                    cmd.Parameters.AddWithValue("@ate", ate.AddDays(1).AddTicks(-1));

                    await cn.OpenAsync().ConfigureAwait(false);

                    using (var rd = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await rd.ReadAsync().ConfigureAwait(false))
                        {
                            var dia = rd.GetDateTime(0);
                            var total = Convert.ToDouble(rd.GetDecimal(1));
                            lista.Add((dia, total));
                        }
                    }
                }
            }

            return lista;
        }

        public async Task<List<(DateTime dia, double previsao)>> ForecastSalesAsync(
     List<(DateTime dia, double total)> historico,
     int diasFuturos)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Histórico de vendas diárias:");
            foreach (var (d, t) in historico)
                sb.AppendLine($"{d:dd/MM}: {t}");

            DateTime inicioPrevisao = historico.Max(h => h.dia).AddDays(1);

            sb.AppendLine();
            sb.AppendLine("Responda apenas com um JSON puro, sem explicações.");
            sb.AppendLine(@"Exemplo: [ { ""date"": ""28/07"", ""value"": 15430.0 }, ... ]");
            sb.AppendLine($"Preveja as vendas para os próximos {diasFuturos} dias, iniciando em {inicioPrevisao:dd/MM}.");
            sb.AppendLine("A resposta deve conter apenas dias consecutivos a partir da data informada.");

            string prompt = sb.ToString();

            string resposta = await _llama.EnviarPerguntaAsync(prompt, temperature: 0.0, top_p: 0.0, seed: 42);
            Console.WriteLine("RESPOSTA DA IA:");
            Console.WriteLine(resposta);

            JArray arr;
            try
            {
                arr = JArray.Parse(resposta);
            }
            catch (Exception ex)
            {
                MessageBox.Show("A IA respondeu algo que não conseguimos entender.");
                Console.Error.WriteLine($"Erro ao interpretar JSON:\n{resposta}\n\n{ex.Message}");
                return new List<(DateTime, double)>();
            }

            var lista = new List<(DateTime, double)>();
            var ultimaData = historico.Max(h => h.dia); // garante que é o dia mais recente

          


            foreach (var tok in arr)
            {
                string ds = (string)tok["date"];
                double val = (double)tok["value"];

                if (DateTime.TryParseExact(ds, "dd/MM",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var d))
                {
                    var dataCorrigida = new DateTime(ultimaData.Year, d.Month, d.Day);
                    if (dataCorrigida > ultimaData)
                        lista.Add((dataCorrigida, val));
                }
            }

            return lista;
        }



    }
}
