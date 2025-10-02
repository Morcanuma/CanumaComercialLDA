using lojaCanuma.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace lojaCanuma.Services
{
    public class PromotionGuardService
    {
        private readonly string _cn;
        public PromotionGuardService(string cn) { _cn = cn; }

        /// <summary>
        /// Verifica margem média dos últimos "dias" e quantos dias ficaram abaixo do limite.
        /// Se a contagem de dias abaixo for >= AutoDisableIfBelowDays, recomenda pausar.
        /// </summary>
        public (bool pausar, string motivo, decimal margemMedia) EvaluateAutoPause(LoyaltySettings set, int dias = 7)
        {
            // 1) Lê vendas e custos diários
            var diarias = ReadDailyMarginRows(dias);

            decimal vendaTotal = diarias.Sum(d => d.Venda);
            decimal custoTotal = diarias.Sum(d => d.Custo);
            decimal margemMedia = vendaTotal > 0 ? ((vendaTotal - custoTotal) / vendaTotal) * 100m : 0m;
            margemMedia = Math.Round(margemMedia, 1);

            // 2) Conta dias abaixo da meta
            int diasAbaixo = diarias.Count(d => d.Venda > 0 && (((d.Venda - d.Custo) / d.Venda) * 100m) < set.MinGrossMarginPct);

            bool pausar = diasAbaixo >= set.AutoDisableIfBelowDays;
            string motivo = pausar
                ? $"Margem média {margemMedia:N1}% e {diasAbaixo} dia(s) abaixo de {set.MinGrossMarginPct:N1}% (limite {set.AutoDisableIfBelowDays})."
                : string.Empty;

            return (pausar, motivo, margemMedia);
        }

        private List<(DateTime Dia, decimal Venda, decimal Custo)> ReadDailyMarginRows(int dias)
        {
            var lista = new List<(DateTime, decimal, decimal)>();

            using (var cn = new SqlConnection(_cn))
            using (var cmd = new SqlCommand(@"
                SELECT
            CAST(c.sdate AS date)                AS Dia,
            SUM(c.total)                          AS Venda,
            SUM(c.qty * ISNULL(p.cost_price, 0))  AS Custo
        FROM tblCar c
        INNER JOIN tblProduct p ON p.pcode = c.pcode
        WHERE c.status = 'Sold'
          AND CAST(c.sdate AS date) >= DATEADD(day, -@dias, CAST(GETDATE() AS date))
        GROUP BY CAST(c.sdate AS date)
        ORDER BY Dia;
            ", cn))
            {
                cmd.Parameters.AddWithValue("@dias", dias);
                cn.Open();

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        DateTime dia = Convert.ToDateTime(r["Dia"]);
                        decimal venda = r["Venda"] == DBNull.Value ? 0m : Convert.ToDecimal(r["Venda"]);
                        decimal custo = r["Custo"] == DBNull.Value ? 0m : Convert.ToDecimal(r["Custo"]);
                        lista.Add((dia, venda, custo));
                    }
                }
            }

            return lista;
        }
    }
}
