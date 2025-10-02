using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class LlamaService
{
    private readonly string _apiKey = "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";

    public async Task<string> EnviarPerguntaAsync(
        string prompt,
        double temperature = 0.0,
        double top_p = 0.0,
        int? seed = null)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Add("HTTP-Referer", "https://openrouter.ai");
            client.DefaultRequestHeaders.Add("X-Title", "lojaCanumaIA");

            var body = new
            {
                model = "meta-llama/llama-3-8b-instruct",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = temperature,
                top_p = top_p,
                seed = seed
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(json);
                return result.choices[0].message.content;
            }
            else
            {
                return $"Erro: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
            }
        }
    }
}
