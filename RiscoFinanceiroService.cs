using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace lojaCanuma.Services
{
    public class RiscoFinanceiroService
    {
        private readonly string _connString;

        public RiscoFinanceiroService(string connString)
        {
            _connString = connString;
        }

        public async Task<RiscoFinanceiroDados> AnalisarRiscoAsync(DateTime inicio, DateTime fim)
        {
            decimal lucroMensal = 0, lucroTotal = 0;
            decimal despesasMensais = 0, despesasTotais = 0;
            decimal salariosMensais = 0, salariosTotais = 0;
            decimal cancelamentosMensais = 0, cancelamentosTotais = 0;

            using (var cn = new SqlConnection(_connString))
            {
                await cn.OpenAsync();

                // LUCRO MENSAL
                lucroMensal = await ExecutarDecimalAsync(cn, @"
                    SELECT SUM((p.price - p.cost_price) * v.qty)
                      FROM vwSoldItems v
                      JOIN tblProduct p ON v.pcode = p.pcode
                     WHERE v.sdate BETWEEN @inicio AND @fim",
                     inicio, fim);

                // LUCRO TOTAL
                lucroTotal = await ExecutarDecimalAsync(cn, @"
                    SELECT SUM((p.price - p.cost_price) * v.qty)
                      FROM vwSoldItems v
                      JOIN tblProduct p ON v.pcode = p.pcode");

                // DESPESAS MENSAL
                despesasMensais = await ExecutarDecimalAsync(cn, @"
                    SELECT SUM(valor)
                      FROM tblDespesasFixas
                     WHERE data_despesa BETWEEN @inicio AND @fim",
                     inicio, fim);

                despesasTotais = await ExecutarDecimalAsync(cn,
                    "SELECT SUM(valor) FROM tblDespesasFixas");

                // SALÁRIOS
                salariosMensais = await ExecutarDecimalAsync(cn, @"
                    SELECT SUM(valor)
                      FROM tblPagamentosFuncionarios
                     WHERE data_pagamento BETWEEN @inicio AND @fim",
                     inicio, fim);

                salariosTotais = await ExecutarDecimalAsync(cn,
                    "SELECT SUM(valor) FROM tblPagamentosFuncionarios");

                // CANCELAMENTOS
                cancelamentosMensais = await ExecutarDecimalAsync(cn, @"
                    SELECT SUM(total)
                      FROM tblCancel
                     WHERE sdate BETWEEN @inicio AND @fim",
                     inicio, fim);

                cancelamentosTotais = await ExecutarDecimalAsync(cn,
                    "SELECT SUM(total) FROM tblCancel");
            }

            // Cálculos finais
            decimal gastoMensal = despesasMensais + salariosMensais + cancelamentosMensais;
            decimal gastoTotal = despesasTotais + salariosTotais + cancelamentosTotais;

            string alertaMensal = GerarAlerta(lucroMensal, gastoMensal, "MENSAL");
            string alertaTotal = GerarAlerta(lucroTotal, gastoTotal, "TOTAL");

            return new RiscoFinanceiroDados
            {
                AlertaMensal = alertaMensal,
                AlertaTotal = alertaTotal,
                DespesasMensais = despesasMensais,
                SalariosMensais = salariosMensais,
                CancelamentosMensais = cancelamentosMensais,
                LucroMensal = lucroMensal
            };
        }

        private async Task<decimal> ExecutarDecimalAsync(SqlConnection cn, string sql,
            DateTime? inicio = null, DateTime? fim = null)
        {
            using (var cmd = new SqlCommand(sql, cn))
            {
                if (inicio.HasValue)
                    cmd.Parameters.AddWithValue("@inicio", inicio.Value);
                if (fim.HasValue)
                    cmd.Parameters.AddWithValue("@fim", fim.Value);

                object result = await cmd.ExecuteScalarAsync();
                return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
            }
        }

        private string GerarAlerta(decimal lucro, decimal gasto, string tipo)
        {
            if (lucro <= 0)
                return $"🔴 Nenhum lucro {tipo.ToLower()} registrado. Risco total.";

            decimal percentual = (gasto / lucro) * 100;

            if (percentual < 50)
                return $"🟢 Risco {tipo.ToLower()}: saudável ({percentual:N1}%).";
            else if (percentual < 80)
                return $"🟡 Risco {tipo.ToLower()}: alerta moderado ({percentual:N1}%).";
            else
                return $"🔴 Risco {tipo.ToLower()}: alto! ({percentual:N1}%). Reveja os custos.";
        }
    }

    public struct RiscoFinanceiroDados
    {
        public string AlertaMensal;
        public string AlertaTotal;
        public decimal DespesasMensais;
        public decimal SalariosMensais;
        public decimal CancelamentosMensais;
        public decimal LucroMensal;
    }
}
