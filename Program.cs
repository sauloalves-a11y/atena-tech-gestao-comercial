/*
 * ============================================================================
 *  Projeto De Extensão Atena Tech
 * ============================================================================
 *  Empresa    : Atena Tech
 *  Projeto    : Sistema de Gestão Comercial (simulação em C#)
 *  Disciplina : Lógica e Programação Visual
 *  Integrantes: 1) Pedro Hugo Carvalho - Gerente de Projetos
 *               2) Andrey Rodrigues - Engenharia de Software
 *               3) Andrey Rodrigues - Análise e Modelagem (UML)
 *               4) Saulo Alves - Desenvolvedor Lead C#
 *               5) Vitor Alves - Modelagem de Banco de Dados
 *               6) Elnatan Cainan - Testes, SQL e Documentação
 * ============================================================================
 *  FUNCIONALIDADE: controle simples de clientes, produtos, estoque e vendas
 *  de uma empresa de pequeno porte que está saindo das planilhas manuais.
 * ============================================================================
 */

using System;
using System.Globalization;
using System.Text;

namespace AtenaTech
{
    class Program
    {
        // ====================================================================
        // CONSTANTES: valores fixos usados em várias partes do programa
        // ====================================================================
        const int MAX_CLIENTES = 50;            // capacidade do vetor de clientes
        const int MAX_PRODUTOS = 50;            // capacidade do vetor de produtos
        const int MAX_VENDAS = 200;             // capacidade do vetor de vendas
        const int QTD_PARA_DESCONTO = 10;       // a partir de 10 unidades, ganha desconto
        const decimal PERCENTUAL_DESCONTO = 0.10m; // 10% de desconto
        const int ESTOQUE_MINIMO = 5;           // abaixo disso, o sistema avisa
        const int LARGURA = 60;                 // largura das molduras do layout

        // Formato brasileiro para dinheiro (R$ 1.234,50)
        static readonly CultureInfo ptBR = new CultureInfo("pt-BR");

        // ====================================================================
        // VETORES (arrays): funcionam como "tabelas" na memória.
        // A posição i de cada vetor pertence ao mesmo registro.
        // ====================================================================

        // --- Clientes ---
        static string[] clienteNome = new string[MAX_CLIENTES];
        static string[] clienteTelefone = new string[MAX_CLIENTES];
        static int totalClientes = 0;           // quantos clientes já foram cadastrados

        // --- Produtos ---
        static string[] produtoNome = new string[MAX_PRODUTOS];
        static decimal[] produtoPreco = new decimal[MAX_PRODUTOS];
        static int[] produtoEstoque = new int[MAX_PRODUTOS];
        static int[] produtoVendidos = new int[MAX_PRODUTOS]; // unidades já vendidas
        static int totalProdutos = 0;

        // --- Vendas ---
        static int[] vendaCliente = new int[MAX_VENDAS];   // posição do cliente
        static int[] vendaProduto = new int[MAX_VENDAS];   // posição do produto
        static int[] vendaQuantidade = new int[MAX_VENDAS];
        static decimal[] vendaTotal = new decimal[MAX_VENDAS];
        static int totalVendas = 0;

        // ====================================================================
        // PONTO DE ENTRADA DO PROGRAMA
        // ====================================================================
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8; // para acentos e molduras aparecerem certos

            CarregarDadosDeExemplo(); // facilita os testes e os prints do trabalho

            TelaBoasVindas();

            int opcao;

            // MENU PRINCIPAL: repete até o usuário escolher 0 (Sair)
            do
            {
                MostrarMenu();
                opcao = LerInteiro("  Escolha uma opção: ", 0, 7);

                // Estrutura de decisão: executa a função escolhida
                switch (opcao)
                {
                    case 1: CadastrarCliente(); break;
                    case 2: ListarClientes(); break;
                    case 3: CadastrarProduto(); break;
                    case 4: ConsultarProdutos(); break;
                    case 5: RegistrarVenda(); break;
                    case 6: RelatorioVendas(); break;
                    case 7: ReajustarPrecos(); break;
                    case 0: TelaDespedida(); break;
                }
            } while (opcao != 0);
        }

        // ====================================================================
        // FUNCIONALIDADES DO SISTEMA
        // ====================================================================

        /// Opção 1 - Cadastra um novo cliente
        static void CadastrarCliente()
        {
            Titulo("CADASTRAR CLIENTE");

            // Validação: verifica se ainda há espaço no vetor
            if (totalClientes >= MAX_CLIENTES)
            {
                Aviso("Limite de clientes atingido.");
                Pausa();
                return;
            }

            string nome;
            bool duplicado;

            // Repete até o usuário digitar um nome que ainda não existe
            do
            {
                nome = LerTexto("  Nome do cliente: ");
                duplicado = false;

                for (int i = 0; i < totalClientes; i++)
                {
                    if (clienteNome[i].ToLower() == nome.ToLower())
                    {
                        duplicado = true;
                    }
                }

                if (duplicado)
                {
                    Erro("Esse cliente já está cadastrado. Tente outro nome.");
                }
            } while (duplicado);

            string telefone = LerTexto("  Telefone: ");

            // Guarda os dados na próxima posição livre
            clienteNome[totalClientes] = nome;
            clienteTelefone[totalClientes] = telefone;
            totalClientes++;

            Sucesso($"Cliente \"{nome}\" cadastrado com sucesso!");
            Pausa();
        }

        /// Opção 2 - Lista todos os clientes
        static void ListarClientes()
        {
            Titulo("CLIENTES CADASTRADOS");
            ImprimirClientes();
            Pausa();
        }

        /// Opção 3 - Cadastra um novo produto
        static void CadastrarProduto()
        {
            Titulo("CADASTRAR PRODUTO");

            if (totalProdutos >= MAX_PRODUTOS)
            {
                Aviso("Limite de produtos atingido.");
                Pausa();
                return;
            }

            string nome = LerTexto("  Nome do produto: ");
            decimal preco = LerDecimal("  Preço (ex: 49,90): ");
            int estoque = LerInteiro("  Quantidade em estoque: ", 0, 100000);

            produtoNome[totalProdutos] = nome;
            produtoPreco[totalProdutos] = preco;
            produtoEstoque[totalProdutos] = estoque;
            produtoVendidos[totalProdutos] = 0;
            totalProdutos++;

            Sucesso($"Produto \"{nome}\" cadastrado com sucesso!");
            Pausa();
        }

        /// Opção 4 - Consulta produtos e situação do estoque
        static void ConsultarProdutos()
        {
            Titulo("PRODUTOS E ESTOQUE");
            ImprimirProdutos();
            Pausa();
        }

        /// Opção 5 - Registra uma venda e dá baixa no estoque
        static void RegistrarVenda()
        {
            Titulo("REGISTRAR VENDA");

            // Operador lógico OR: precisa de pelo menos 1 cliente E 1 produto
            if (totalClientes == 0 || totalProdutos == 0)
            {
                Aviso("Cadastre pelo menos 1 cliente e 1 produto antes de vender.");
                Pausa();
                return;
            }

            if (totalVendas >= MAX_VENDAS)
            {
                Aviso("Limite de vendas atingido.");
                Pausa();
                return;
            }

            // 1) Escolha do cliente
            ImprimirClientes();
            int c = LerInteiro("\n  Número do cliente: ", 1, totalClientes) - 1;

            // 2) Escolha do produto
            Console.WriteLine();
            ImprimirProdutos();
            int p = LerInteiro("\n  Número do produto: ", 1, totalProdutos) - 1;

            if (produtoEstoque[p] == 0)
            {
                Aviso("Produto sem estoque no momento.");
                Pausa();
                return;
            }

            // 3) Quantidade: repete enquanto pedir mais do que existe no estoque
            int qtd;
            do
            {
                qtd = LerInteiro("  Quantidade desejada: ", 1, 100000);

                if (qtd > produtoEstoque[p])
                {
                    Erro($"Estoque insuficiente. Disponível: {produtoEstoque[p]} unidade(s).");
                }
            } while (qtd > produtoEstoque[p]);

            // 4) Cálculos com operadores aritméticos
            decimal subtotal = produtoPreco[p] * qtd;
            decimal desconto = 0;

            // Regra de negócio: desconto para compras grandes
            if (qtd >= QTD_PARA_DESCONTO)
            {
                desconto = subtotal * PERCENTUAL_DESCONTO;
            }

            decimal total = subtotal - desconto;

            // 5) Resumo para o cliente conferir antes de confirmar
            Console.WriteLine();
            Linha();
            Console.WriteLine("  RESUMO DA VENDA");
            Linha();
            Console.WriteLine($"  Cliente  : {clienteNome[c]}");
            Console.WriteLine($"  Produto  : {produtoNome[p]}");
            Console.WriteLine($"  Qtd      : {qtd}");
            Console.WriteLine($"  Subtotal : {Moeda(subtotal)}");

            if (desconto > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  Desconto : -{Moeda(desconto)} ({PERCENTUAL_DESCONTO * 100:0}% por volume)");
                Console.ResetColor();
            }

            Console.WriteLine($"  TOTAL    : {Moeda(total)}");
            Linha();

            // 6) Confirmação e gravação
            if (Confirmar("  Confirmar venda? (S/N): "))
            {
                vendaCliente[totalVendas] = c;
                vendaProduto[totalVendas] = p;
                vendaQuantidade[totalVendas] = qtd;
                vendaTotal[totalVendas] = total;
                totalVendas++;

                produtoEstoque[p] -= qtd;     // baixa no estoque
                produtoVendidos[p] += qtd;    // acumula para o relatório

                Sucesso("Venda registrada com sucesso!");

                // Alerta de estoque baixo (operadores relacionais e lógico AND)
                if (produtoEstoque[p] <= ESTOQUE_MINIMO && produtoEstoque[p] > 0)
                {
                    Aviso($"Atenção: restam apenas {produtoEstoque[p]} unidade(s) de \"{produtoNome[p]}\".");
                }
                else if (produtoEstoque[p] == 0)
                {
                    Aviso($"O produto \"{produtoNome[p]}\" acabou. Hora de repor o estoque!");
                }
            }
            else
            {
                Aviso("Venda cancelada. Nada foi alterado.");
            }

            Pausa();
        }

        /// Opção 6 - Mostra o resumo geral das vendas
        static void RelatorioVendas()
        {
            Titulo("RELATÓRIO DE VENDAS");

            if (totalVendas == 0)
            {
                Aviso("Ainda não há vendas registradas.");
                Pausa();
                return;
            }

            decimal faturamento = 0;
            int unidadesTotal = 0;

            // Percorre todas as vendas somando os valores
            for (int i = 0; i < totalVendas; i++)
            {
                faturamento += vendaTotal[i];
                unidadesTotal += vendaQuantidade[i];
            }

            decimal ticketMedio = faturamento / totalVendas;

            // Procura o produto mais vendido
            int maisVendido = 0;
            for (int i = 1; i < totalProdutos; i++)
            {
                if (produtoVendidos[i] > produtoVendidos[maisVendido])
                {
                    maisVendido = i;
                }
            }

            Console.WriteLine($"  Vendas realizadas    : {totalVendas}");
            Console.WriteLine($"  Unidades vendidas    : {unidadesTotal}");
            Console.WriteLine($"  Faturamento total    : {Moeda(faturamento)}");
            Console.WriteLine($"  Ticket médio         : {Moeda(ticketMedio)}");
            Console.WriteLine($"  Produto mais vendido : {produtoNome[maisVendido]} ({produtoVendidos[maisVendido]} un.)");

            Console.WriteLine();
            Console.WriteLine("  HISTÓRICO");
            Linha();
            Console.WriteLine($"  {"Nº",-4}{"Cliente",-18}{"Produto",-18}{"Qtd",-5}{"Total",12}");

            for (int i = 0; i < totalVendas; i++)
            {
                Console.WriteLine($"  {i + 1,-4}{Cortar(clienteNome[vendaCliente[i]], 16),-18}" +
                                  $"{Cortar(produtoNome[vendaProduto[i]], 16),-18}" +
                                  $"{vendaQuantidade[i],-5}{Moeda(vendaTotal[i]),12}");
            }

            Pausa();
        }

        /// Opção 7 - Reajusta o preço de todos os produtos por um percentual
        static void ReajustarPrecos()
        {
            Titulo("REAJUSTE DE PREÇOS");

            if (totalProdutos == 0)
            {
                Aviso("Nenhum produto cadastrado.");
                Pausa();
                return;
            }

            Console.WriteLine("  Use valor positivo para aumentar e negativo para reduzir.");
            int percentual = LerInteiro("  Percentual de reajuste (-50 a 100): ", -50, 100);

            if (percentual == 0)
            {
                Aviso("Percentual zero: nenhum preço foi alterado.");
                Pausa();
                return;
            }

            // Aplica o reajuste a cada produto usando um laço for
            for (int i = 0; i < totalProdutos; i++)
            {
                decimal precoAntigo = produtoPreco[i];
                produtoPreco[i] = Math.Round(precoAntigo * (1 + percentual / 100m), 2);

                Console.WriteLine($"  {Cortar(produtoNome[i], 20),-22}{Moeda(precoAntigo),12}  →  {Moeda(produtoPreco[i]),12}");
            }

            Sucesso($"Reajuste de {percentual}% aplicado a {totalProdutos} produto(s).");
            Pausa();
        }

        // ====================================================================
        // TABELAS (usadas por mais de uma funcionalidade)
        // ====================================================================

        static void ImprimirClientes()
        {
            if (totalClientes == 0)
            {
                Aviso("Nenhum cliente cadastrado ainda.");
                return;
            }

            Console.WriteLine($"  {"Nº",-4}{"Nome",-28}{"Telefone",-16}");
            Linha();

            for (int i = 0; i < totalClientes; i++)
            {
                Console.WriteLine($"  {i + 1,-4}{Cortar(clienteNome[i], 26),-28}{clienteTelefone[i],-16}");
            }
        }

        static void ImprimirProdutos()
        {
            if (totalProdutos == 0)
            {
                Aviso("Nenhum produto cadastrado ainda.");
                return;
            }

            Console.WriteLine($"  {"Nº",-4}{"Produto",-24}{"Preço",12}{"Estoque",9}  Situação");
            Linha();

            for (int i = 0; i < totalProdutos; i++)
            {
                Console.Write($"  {i + 1,-4}{Cortar(produtoNome[i], 22),-24}{Moeda(produtoPreco[i]),12}{produtoEstoque[i],9}  ");

                // Situação do estoque com cores
                if (produtoEstoque[i] == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Esgotado");
                }
                else if (produtoEstoque[i] <= ESTOQUE_MINIMO)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Estoque baixo");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("OK");
                }
                Console.ResetColor();
            }
        }

        // ====================================================================
        // LAYOUT: telas e mensagens amigáveis
        // ====================================================================

        static void TelaBoasVindas()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔" + new string('═', LARGURA) + "╗");
            Centralizar("");
            Centralizar("ATENA TECH");
            Centralizar("Soluções digitais simples, seguras e inteligentes");
            Centralizar("");
            Centralizar("Sistema de Gestão Comercial");
            Centralizar("");
            Console.WriteLine("╚" + new string('═', LARGURA) + "╝");
            Console.ResetColor();

            Console.WriteLine();
            Console.WriteLine("  Olá! Seja bem-vindo(a). Vamos organizar o seu negócio?");
            Pausa();
        }

        static void TelaDespedida()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔" + new string('═', LARGURA) + "╗");
            Centralizar("");
            Centralizar("Obrigado por usar o sistema da Atena Tech!");
            Centralizar("Até a próxima e bons negócios!");
            Centralizar("");
            Console.WriteLine("╚" + new string('═', LARGURA) + "╝");
            Console.ResetColor();
        }

        static void MostrarMenu()
        {
            Titulo("MENU PRINCIPAL");
            Console.WriteLine("  O que você gostaria de fazer hoje?\n");
            Console.WriteLine("   1  -  Cadastrar cliente");
            Console.WriteLine("   2  -  Listar clientes");
            Console.WriteLine("   3  -  Cadastrar produto");
            Console.WriteLine("   4  -  Consultar produtos e estoque");
            Console.WriteLine("   5  -  Registrar venda");
            Console.WriteLine("   6  -  Relatório de vendas");
            Console.WriteLine("   7  -  Reajustar preços");
            Console.WriteLine("   0  -  Sair\n");
            Linha();
        }

        static void Titulo(string texto)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔" + new string('═', LARGURA) + "╗");
            Centralizar(texto);
            Console.WriteLine("╚" + new string('═', LARGURA) + "╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void Centralizar(string texto)
        {
            int espacoEsq = (LARGURA - texto.Length) / 2;
            int espacoDir = LARGURA - texto.Length - espacoEsq;
            Console.WriteLine("║" + new string(' ', espacoEsq) + texto + new string(' ', espacoDir) + "║");
        }

        static void Linha()
        {
            Console.WriteLine("  " + new string('─', LARGURA - 4));
        }

        static void Sucesso(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n  ✔ {msg}");
            Console.ResetColor();
        }

        static void Aviso(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n  ⚠ {msg}");
            Console.ResetColor();
        }

        static void Erro(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ✖ {msg}");
            Console.ResetColor();
        }

        static void Pausa()
        {
            Console.WriteLine("\n  Pressione ENTER para continuar...");
            Console.ReadLine();
        }

        // ====================================================================
        // FUNÇÕES AUXILIARES: leitura segura de dados (evitam que o programa quebre)
        // ====================================================================

        /// Lê um texto e só aceita se não estiver vazio
        static string LerTexto(string mensagem)
        {
            string texto;
            do
            {
                Console.Write(mensagem);
                texto = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(texto))
                {
                    Erro("Este campo não pode ficar vazio.");
                }
            } while (string.IsNullOrWhiteSpace(texto));

            return texto.Trim();
        }

        /// Lê um número inteiro dentro de um intervalo (min a max)
        static int LerInteiro(string mensagem, int min, int max)
        {
            int valor;
            while (true)
            {
                Console.Write(mensagem);

                // TryParse tenta converter sem causar erro se o usuário digitar letras
                if (int.TryParse(Console.ReadLine(), out valor) && valor >= min && valor <= max)
                {
                    return valor;
                }

                Erro($"Valor inválido. Digite um número entre {min} e {max}.");
            }
        }

        /// Lê um valor decimal positivo (aceita vírgula ou ponto)
        static decimal LerDecimal(string mensagem)
        {
            decimal valor;
            while (true)
            {
                Console.Write(mensagem);
                string entrada = (Console.ReadLine() ?? "").Replace(',', '.');

                if (decimal.TryParse(entrada, NumberStyles.Number, CultureInfo.InvariantCulture, out valor) && valor > 0)
                {
                    return valor;
                }

                Erro("Valor inválido. Digite um número maior que zero (ex: 25,50).");
            }
        }

        /// Pergunta S/N e devolve true para "S"
        static bool Confirmar(string mensagem)
        {
            while (true)
            {
                Console.Write(mensagem);
                string resposta = (Console.ReadLine() ?? "").Trim().ToUpper();

                if (resposta == "S") return true;
                if (resposta == "N") return false;

                Erro("Digite apenas S (sim) ou N (não).");
            }
        }

        static string Moeda(decimal valor)
        {
            return valor.ToString("C2", ptBR);
        }

        /// Corta textos longos para não desalinhar as tabelas
        static string Cortar(string texto, int tamanho)
        {
            if (texto.Length <= tamanho) return texto;
            return texto.Substring(0, tamanho - 1) + "…";
        }

        // ====================================================================
        // DADOS DE EXEMPLO (remova a chamada no Main se quiser começar vazio)
        // ====================================================================
        static void CarregarDadosDeExemplo()
        {
            string[] nomesC = { "Maria Souza", "João Pereira", "Padaria Bom Pão" };
            string[] fonesC = { "(11) 91111-2222", "(11) 93333-4444", "(11) 3555-6666" };

            for (int i = 0; i < nomesC.Length; i++)
            {
                clienteNome[i] = nomesC[i];
                clienteTelefone[i] = fonesC[i];
                totalClientes++;
            }

            string[] nomesP = { "Teclado sem fio", "Mouse óptico", "Monitor 24\"", "Cabo HDMI" };
            decimal[] precosP = { 89.90m, 39.90m, 749.00m, 24.50m };
            int[] estoquesP = { 30, 4, 12, 50 };

            for (int i = 0; i < nomesP.Length; i++)
            {
                produtoNome[i] = nomesP[i];
                produtoPreco[i] = precosP[i];
                produtoEstoque[i] = estoquesP[i];
                totalProdutos++;
            }
        }
    }
}
