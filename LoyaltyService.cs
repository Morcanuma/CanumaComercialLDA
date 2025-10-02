using lojaCanuma.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace lojaCanuma.Services
{
    public class LoyaltyService
    {
        private readonly string _cn;

        public LoyaltyService(string cn) { _cn = cn; }

        // ====== SETTINGS ======
        public LoyaltySettings GetSettings()
        {
            using (var cn = new SqlConnection(_cn))
            using (var cmd = new SqlCommand("dbo.sp_Loyalty_GetSettings", cn) { CommandType = CommandType.StoredProcedure })
            {
                cn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                    {
                        // Defaults seguros
                        return new LoyaltySettings
                        {
                            IsEnabled = false,
                            IsPromoEnabled = false,
                            PointsExpireDays = 60,
                            InactivityMaxDays = 7,
                            KzPerPointEarn = 1000m,
                            MinPointsToRedeem = 10,
                            RedeemUnitPoints = 10,
                            RedeemDiscountKz = 1000m,
                            MaxDiscountPct = 10m,
                            MinGrossMarginPct = 15m,
                            AutoDisableIfBelowDays = 3
                        };
                    }

                    return new LoyaltySettings
                    {
                        IsEnabled = Convert.ToBoolean(r["IsEnabled"]),
                        IsPromoEnabled = Convert.ToBoolean(r["IsPromoEnabled"]),
                        PointsExpireDays = Convert.ToInt32(r["PointsExpireDays"]),
                        InactivityMaxDays = Convert.ToInt32(r["InactivityMaxDays"]),
                        KzPerPointEarn = Convert.ToDecimal(r["KzPerPointEarn"]),
                        MinPointsToRedeem = Convert.ToInt32(r["MinPointsToRedeem"]),
                        RedeemUnitPoints = Convert.ToInt32(r["RedeemUnitPoints"]),
                        RedeemDiscountKz = Convert.ToDecimal(r["RedeemDiscountKz"]),
                        MaxDiscountPct = Convert.ToDecimal(r["MaxDiscountPct"]),
                        MinGrossMarginPct = Convert.ToDecimal(r["MinGrossMarginPct"]),
                        AutoDisableIfBelowDays = Convert.ToInt32(r["AutoDisableIfBelowDays"])
                    };
                }
            }
        }

        public void SaveSettings(LoyaltySettings s)
        {
            using (var cn = new SqlConnection(_cn))
            using (var cmd = new SqlCommand("dbo.sp_Loyalty_SaveSettings", cn) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@IsEnabled", s.IsEnabled);
                cmd.Parameters.AddWithValue("@IsPromoEnabled", s.IsPromoEnabled);
                cmd.Parameters.AddWithValue("@PointsExpireDays", s.PointsExpireDays);
                cmd.Parameters.AddWithValue("@InactivityMaxDays", s.InactivityMaxDays);
                cmd.Parameters.AddWithValue("@KzPerPointEarn", s.KzPerPointEarn);
                cmd.Parameters.AddWithValue("@MinPointsToRedeem", s.MinPointsToRedeem);
                cmd.Parameters.AddWithValue("@RedeemUnitPoints", s.RedeemUnitPoints);
                cmd.Parameters.AddWithValue("@RedeemDiscountKz", s.RedeemDiscountKz);
                cmd.Parameters.AddWithValue("@MaxDiscountPct", s.MaxDiscountPct);
                cmd.Parameters.AddWithValue("@MinGrossMarginPct", s.MinGrossMarginPct);
                cmd.Parameters.AddWithValue("@AutoDisableIfBelowDays", s.AutoDisableIfBelowDays);

                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void RunExpireJob()
        {
            using (var cn = new SqlConnection(_cn))
            using (var cmd = new SqlCommand("dbo.sp_Loyalty_ExpirePoints", cn) { CommandType = CommandType.StoredProcedure })
            {
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ====== ELEGIBILIDADE/REGRAS ======
        public (bool ok, string motivo) CheckEligibilityToRedeem(int customerId, LoyaltySettings set)
        {
            if (!set.IsEnabled || !set.IsPromoEnabled)
                return (false, "Programa/promoção desativados.");

            using (var cn = new SqlConnection(_cn))
            using (var cmd = new SqlCommand(@"SELECT Points, LastPurchaseAt FROM tblCustomer WHERE CustomerId=@id", cn))
            {
                cmd.Parameters.AddWithValue("@id", customerId);
                cn.Open();

                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return (false, "Cliente não encontrado.");

                    int pts = r["Points"] == DBNull.Value ? 0 : Convert.ToInt32(r["Points"]);
                    DateTime? last = r["LastPurchaseAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["LastPurchaseAt"]);

                    if (pts < set.MinPointsToRedeem)
                        return (false, $"Mínimo de {set.MinPointsToRedeem} pontos para resgatar.");

                    if (!last.HasValue || (DateTime.Now - last.Value).TotalDays > set.InactivityMaxDays)
                        return (false, $"Para usar pontos, o cliente precisa ter comprado nos últimos {set.InactivityMaxDays} dias.");

                    return (true, "");
                }
            }
        }

        // Calcula desconto seguro por pontos para um carrinho
        public (bool aplicar, decimal descontoKz, string motivoBloqueio) ComputeSafeRedeem(LoyaltySettings set, List<CartLine> cart)
        {
            decimal subtotal = cart.Sum(l => l.LineTotal);
            decimal costTotal = cart.Sum(l => l.LineCost);
            if (subtotal <= 0) return (false, 0, "Carrinho vazio.");

            decimal descontoKz = set.RedeemDiscountKz; // por recibo (uma vez)

            // Limite % do subtotal
            decimal maxByPct = Math.Round(subtotal * (set.MaxDiscountPct / 100m), 2);
            if (descontoKz > maxByPct) descontoKz = maxByPct;

            // Margem bruta após desconto
            decimal margemBruta = (subtotal - descontoKz) - costTotal;
            decimal margemPct = subtotal > 0 ? (margemBruta / subtotal) * 100m : 0m;

            if (margemPct < set.MinGrossMarginPct)
                return (false, 0, $"Margem ficaria {margemPct:N1}% (< {set.MinGrossMarginPct:N1}%).");

            if (descontoKz <= 0) return (false, 0, "Desconto calculado igual a zero.");
            return (true, descontoKz, "");
        }

        public void RedeemPoints(int customerId, int points, string transno)
        {
            using (var cn = new SqlConnection(_cn))
            using (var cmd = new SqlCommand("dbo.sp_Loyalty_RedeemPoints", cn) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                cmd.Parameters.AddWithValue("@PointsToUse", points);
                cmd.Parameters.AddWithValue("@Transno", transno);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void AwardPoints(int customerId, decimal amountKz, string transno)
        {
            using (var cn = new SqlConnection(_cn))
            using (var cmd = new SqlCommand("dbo.sp_Loyalty_AwardPoints", cn) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                cmd.Parameters.AddWithValue("@AmountKz", amountKz);
                cmd.Parameters.AddWithValue("@Transno", transno);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
