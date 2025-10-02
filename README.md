# 🏪 CanumaComercialLDA

Sistema de gestão e ponto de venda (**PDV**) desenvolvido em **C# WinForms**, pensado para lojas, restaurantes e negócios locais.  
O projeto tem como foco **controle financeiro, estoque, fidelização de clientes e relatórios detalhados**.

---

## 📂 Estrutura do Projeto

- **Forms (WinForms)**
  - `frmPOS` → Ponto de venda principal
  - `frmProduct` / `frmProductListcs` → Cadastro e listagem de produtos
  - `frmCustomer` / `FrmDadosCliente` → Gestão de clientes
  - `frmUserAccount` → Contas de usuários
  - `frmStore` → Dados da loja
  - `frmDashboard` → Painel geral
  - `frmFluxoCaixa` → Fluxo de caixa
  - `frmRelatorioLucros`, `frmRelatorioPrevisao`, `frmRelatorioInventario` → Relatórios financeiros
  - `frmLucroEOutros`, `frmDespesaGrafico`, `frmPagarFunc` → Gestão de lucro, despesas e salários
  - Outros formulários: estoque (`frmStockIn`, `frmAjustement`), promoções (`frmLoyaltyAndPromos`), cancelamentos (`frmCancelDetais`), etc.

- **Relatórios (RDLC)**
  - `ReportReceipt.rdlc` → Recibos de vendas
  - `ReportSoldItem.rdlc`, `ReportSoldItems2.rdlc` → Vendas realizadas
  - `ReportInventory.rdlc`, `rptInventarioMensal.rdlc` → Inventário
  - `rptLucroVendas.rdlc`, `rptFluxoCaixaNovo.rdlc` → Lucros e fluxo de caixa
  - `rptPrevisaoCompleta.rdlc` → Previsão de vendas (IA integrada)

- **Serviços**
  - `DataService.cs` → Camada de dados
  - `CustomerRepository.cs` / `CustomerModels.cs` → Repositório de clientes
  - `LoyaltyService.cs` → Sistema de fidelidade
  - `PromotionGuardService.cs` → Gestão de promoções
  - `RiscoFinanceiroService.cs` → Análise de risco financeiro
  - `LlamaService.cs` → Integração com IA (sugestões inteligentes e relatórios)

- **Modelos e Recursos**
  - `ItemVenda.cs`, `LoyaltyModels.cs`, `SessaoUsuario.cs`
  - Pasta `Resources/` → imagens, ícones e recursos visuais
  - `MessageProvider.cs` → Mensagens especiais e notificações

---

## ⚙️ Funcionalidades

- ✅ Registro e autenticação de usuários com permissões
- ✅ PDV completo com recibos automáticos
- ✅ Gestão de estoque (entradas, saídas, inventário mensal)
- ✅ Relatórios detalhados (vendas, lucros, cancelamentos, despesas)
- ✅ Sistema de fidelidade com pontos e descontos
- ✅ Integração com **IA** (previsão de vendas, análise de risco, sugestões de estoque)
- ✅ Controle financeiro: fluxo de caixa, pagamento de funcionários, análise de lucro líquido
- ✅ Suporte a múltiplas filiais (modelo escalável)

---

## 🚀 Como Executar

1. Clone o repositório:
   ```bash
   git clone https://github.com/Morcanuma/CanumaComercialLDA.git
