using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lojaCanuma
{
    public class ItemVenda
    {
        public int Id { get; set; }
        public double PrecoOriginal { get; set; }
        private double _descontoPercentual;

        public double DescontoPercentual
        {
            get => _descontoPercentual;
            set
            {
                _descontoPercentual = value;
                ValorDoDesconto = PrecoOriginal * (value / 100);
            }
        }

        public double ValorDoDesconto { get; private set; }
    }

}
