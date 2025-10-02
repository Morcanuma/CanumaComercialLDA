using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lojaCanuma
{
    public static class MessageProvider
    {
        private static readonly string[] SpecialMessages = new string[]
        {
        "01-01: Feliz Ano Novo! Que 2026 traga prosperidade e saúde. – Canuma Comercial, LDA",
        "02-04: Dia do Início da Luta Armada em Angola. Honramos nossa história. – Canuma Comercial, LDA",
        "02-14: Dia dos Namorados! Espalhe amor e carinho. – Canuma Comercial, LDA",
        "03-08: Dia Internacional da Mulher. Toda honra às mulheres angolanas e do mundo! – Canuma Comercial, LDA",
        "03-22: Dia Mundial da Água. Vamos valorizar cada gota! – Canuma Comercial, LDA",
        "04-04: Dia da Paz em Angola. Celebramos a harmonia e a união. – Canuma Comercial, LDA",
        "04-07: Dia da Mulher Moçambicana. Nossos respeitos e homenagens! – Canuma Comercial, LDA",
        "04-22: Dia da Terra. Protegê-la é dever de todos nós. – Canuma Comercial, LDA",
        "05-01: Dia do Trabalhador. Nosso reconhecimento a todos que constroem este país! – Canuma Comercial, LDA",
        "05-25: Dia de África. Celebremos nossa identidade e cultura! – Canuma Comercial, LDA",
        "06-01: Dia da Criança. Que o sorriso das crianças ilumine nosso futuro. – Canuma Comercial, LDA",
        "06-05: Dia Mundial do Meio Ambiente. Por um planeta mais verde. – Canuma Comercial, LDA",
        "06-17: Dia Mundial de Combate à Desertificação. Cuidemos da nossa terra. – Canuma Comercial, LDA",
        "07-11: Dia Mundial da População. Um olhar para as gerações futuras. – Canuma Comercial, LDA",
        "08-19: Dia Mundial Humanitário. Gratidão aos que ajudam o próximo. – Canuma Comercial, LDA",
        "09-17: Dia do Herói Nacional em Angola. Honramos Agostinho Neto e todos os heróis! – Canuma Comercial, LDA",
        "10-01: Dia Internacional do Idoso. Todo nosso carinho e respeito. – Canuma Comercial, LDA",
        "10-05: Dia Mundial do Professor. Reconhecemos os que ensinam com paixão. – Canuma Comercial, LDA",
        "10-16: Dia Mundial da Alimentação. Que nunca falte pão em nossa mesa. – Canuma Comercial, LDA",
        "11-02: Dia de Finados. Recordamos com carinho os que partiram. – Canuma Comercial, LDA",
        "11-11: Dia da Independência de Angola. Viva Angola! – Canuma Comercial, LDA",
        "12-01: Dia Mundial de Luta contra a SIDA. Informação é proteção. – Canuma Comercial, LDA",
        "12-10: Dia Internacional dos Direitos Humanos. Que haja justiça e dignidade. – Canuma Comercial, LDA",
        "12-25: Feliz Natal! Que a paz reine em sua casa. – Canuma Comercial, LDA",
        // +26 mensagens adicionais semelhantes com outras datas relevantes ou citações de valor
        "03-20: Dia Internacional da Felicidade. Sorrir é um ato revolucionário! – Canuma Comercial, LDA",
        "05-15: Dia Internacional da Família. Família é base de tudo. – Canuma Comercial, LDA",
        "10-15: Dia Mundial da Lavagem das Mãos. Saúde começa com higiene. – Canuma Comercial, LDA",
        "11-20: Dia Universal da Criança. Protejamos o futuro. – Canuma Comercial, LDA",
        "12-03: Dia Internacional da Pessoa com Deficiência. Inclusão é respeito. – Canuma Comercial, LDA",
        "09-21: Dia Internacional da Paz. Que a harmonia guie nossos passos. – Canuma Comercial, LDA",
        "08-12: Dia Internacional da Juventude. A força de Angola está nos jovens! – Canuma Comercial, LDA",
        "04-02: Dia Mundial da Conscientização sobre o Autismo. Respeito e empatia. – Canuma Comercial, LDA",
        "06-21: Dia Internacional da Música. Que a melodia embale nosso progresso. – Canuma Comercial, LDA",
        "07-20: Dia Internacional da Amizade. Que nunca falte afeto ao nosso redor. – Canuma Comercial, LDA",
        "10-10: Dia Mundial da Saúde Mental. Cuidar da mente é essencial. – Canuma Comercial, LDA",
        "09-15: Dia Internacional da Democracia. A liberdade é sagrada. – Canuma Comercial, LDA",
        "12-05: Dia Internacional do Voluntariado. Ajudar é um ato de grandeza. – Canuma Comercial, LDA",
        "06-12: Dia Mundial Contra o Trabalho Infantil. Por uma infância livre. – Canuma Comercial, LDA",
        "07-18: Dia de Nelson Mandela. Um símbolo eterno de resistência e amor. – Canuma Comercial, LDA",
        "01-24: Dia Mundial da Educação. Educar é libertar. – Canuma Comercial, LDA",
        "02-13: Dia Mundial do Rádio. Comunicação é poder. – Canuma Comercial, LDA",
        "03-21: Dia Internacional contra a Discriminação Racial. Somos todos iguais! – Canuma Comercial, LDA",
        "04-28: Dia Mundial da Segurança e Saúde no Trabalho. Cuidar de vidas é essencial. – Canuma Comercial, LDA",
        "05-12: Dia Mundial da Enfermagem. Gratidão a quem cuida com amor. – Canuma Comercial, LDA",
        "09-29: Dia Mundial do Coração. Cuide de você. – Canuma Comercial, LDA",
        "10-24: Dia das Nações Unidas. Pela paz entre os povos. – Canuma Comercial, LDA",
        "11-14: Dia Mundial da Diabetes. Informação e prevenção salvam vidas. – Canuma Comercial, LDA",
        "03-24: Dia Mundial da Tuberculose. Juntos contra essa luta. – Canuma Comercial, LDA",
        "07-28: Dia Mundial das Hepatites. Prevenir é viver. – Canuma Comercial, LDA",
        "12-20: Dia Internacional da Solidariedade Humana. A união faz a força. – Canuma Comercial, LDA"
        };

        private static readonly string[] DefaultMessages = new string[]
        {
        "Agradecemos de coração pela sua visita. Volte sempre!",
        "Foi um privilégio atendê-lo. Canuma Comercial, sempre ao seu dispor.",
        "A sua presença torna o nosso trabalho ainda mais gratificante.",
        "Esperamos vê-lo em breve. Sucesso e prosperidade no seu dia!",
        "Obrigado por confiar na Canuma Comercial, LDA. Até logo!",
        "É sempre um prazer servir pessoas especiais como você.",
        "A sua preferência nos inspira a sermos melhores a cada dia.",
        "A Canuma Comercial agradece a honra da sua escolha.",
        "Com você, nossa história se torna mais significativa.",
        "Volte sempre que quiser. As portas estão sempre abertas!",
        "Sua satisfação é o nosso maior compromisso.",
        "Nosso sucesso é construído por clientes como você.",
        "Conte conosco sempre que precisar. Estamos aqui por você.",
        "Obrigado por fazer parte da nossa jornada.",
        "Cliente feliz, empresa realizada. A Canuma agradece!",
        "Gratidão define nossa relação com você!",
        "O seu apoio é combustível para o nosso crescimento.",
        "Servir bem é nossa missão. Agradecemos sua visita.",
        "Esperamos que sua experiência tenha sido incrível!",
        "Nossa maior propaganda é a sua satisfação. Obrigado!",
        "Confiança se constrói com respeito. Obrigado pela sua!",
        "De cliente para amigo. Conte com a Canuma sempre!",
        "Cada visita sua é um privilégio para nós.",
        "Obrigado por valorizar o comércio local. Juntos crescemos!",
        "A Canuma Comercial cresce com você!",
        "Volte sempre, com alegria e boas energias!",
        "Você é parte importante da nossa história!",
        "Nossa gratidão se transforma em dedicação.",
        "É por clientes assim que existimos. Obrigado!",
        "Cliente feliz é missão cumprida!",
        "Agradecemos pela companhia e confiança!",
        "Sua escolha faz toda a diferença. Obrigado!",
        "Foi um prazer receber você hoje.",
        "Sua preferência é o nosso maior prêmio.",
        "Mais que clientes, temos amigos.",
        "Obrigado por construir conosco uma Angola mais próspera.",
        "Cada venda é uma oportunidade de servir com excelência.",
        "A sua confiança move nossa vontade de evoluir.",
        "A Canuma está sempre à sua disposição!",
        "Gratidão pela visita. Que seu dia seja especial!",
        "Aqui, você é sempre bem-vindo!",
        "Seu apoio fortalece o nosso propósito!",
        "Atendê-lo é motivo de orgulho para nós!",
        "Agradecemos por caminhar ao nosso lado.",
        "Nosso compromisso é com você e seu bem-estar.",
        "Nada nos alegra mais do que ver um cliente satisfeito.",
        "Nossa equipe valoriza imensamente sua escolha!",
        "Obrigado por ser parte da família Canuma.",
        "Sua presença ilumina o nosso negócio.",
        "Volte logo. Estamos com saudades desde já!",
        "Mais que uma compra, uma conexão!"
        };

        private static readonly Random rnd = new Random();

        public static string GetMessageForToday()
        {
            string key = DateTime.Now.ToString("MM-dd");
            string special = SpecialMessages.FirstOrDefault(m => m.StartsWith(key));
            if (special != null)
            {
                return special.Substring(6); // Remove a chave da data ("MM-dd: ")
            }

            int idx = rnd.Next(DefaultMessages.Length);
            return DefaultMessages[idx];
        }
    }

}
