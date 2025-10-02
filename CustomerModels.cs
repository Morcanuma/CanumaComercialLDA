using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lojaCanuma
{
    public class CustomerModels
    {
        public int CustomerId { get; set; }   // 1. ID do cliente (PK)
        public string Name { get; set; }   // 2. Nome do cliente
        public string Phone { get; set; }   // 3. Telefone
        public string Email { get; set; }   // 4. Email
        public int Points { get; set; }   // 5. Pontos de fidelidade
    }
}
