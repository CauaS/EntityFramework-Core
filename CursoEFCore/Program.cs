using CursoEFCore.DomainEF;
using CursoEFCore.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CursoEFCore
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            using var db = new Data.ApplicationContext();
            var existeMigrationPending = db.Database.GetAppliedMigrations().Any();

            if (existeMigrationPending)
            {
                // Existe....
            }
            */

            InserirDados();
            InserirDadosEmMassa();
            ConsultaDados();
            AtualizaDados(); 
            AtualizaCamposDesconectado();
            ConsultaCarregamentoAdiantado();
        }
        private static void InserirDados()
        {
            var produto = new Produto
            {
                Descricao = "Produto Teste",
                CodigoBarras = "1234567891234",
                Valor = "10",
                TipoProduto = TipoProduto.MercadoriaParaRevenda,
                Ativo = true
            };

            using var db = new Data.ApplicationContext();
            //db.Produtos.Add(produto);
            //db.Set<Produto>().Add(produto);
            //db.Entry(produto).State = EntityState.Added;
            db.Add(produto);

            var registros = db.SaveChanges(); // persiste as informaçoes no BD

            Console.WriteLine($"Total Registros: {registros}.");
        }

        private static void InserirDadosEmMassa()
        {
            using var db = new Data.ApplicationContext();

            var produto = new Produto
            {
                Descricao = "Produto Teste",
                CodigoBarras = "1234567891234",
                Valor = "10",
                TipoProduto = TipoProduto.MercadoriaParaRevenda,
                Ativo = true
            };

            var cliente = new Cliente
            {
               Nome = "Cali",
               CEP = "93950000",
               Cidade = "Dois Irmãos",
               Estado = "RS",
               Telefone = "5197979797"
            };

            db.AddRange(produto, cliente);

            var listaClientes = new[]
            {
                new Cliente
                {
                   Nome = "Cali",
                   CEP = "93950000",
                   Cidade = "Dois Irmãos",
                   Estado = "RS",
                   Telefone = "5197979797"
                },
                new Cliente
                {
                   Nome = "Cali 2",
                   CEP = "93950000",
                   Cidade = "Dois Irmãos",
                   Estado = "RS",
                   Telefone = "5197979797"
                }
            };
            
            db.Clientes.AddRange(listaClientes);

            var registros = db.SaveChanges();
        }

        public static void ConsultaDados()
        {
            using var db = new Data.ApplicationContext();

            var consultaPorSintaxe = (from cliente in db.Clientes where cliente.Id > 0 select cliente).ToList();
            var consultaPorMetodo = db.Clientes
                //.AsNoTracking()
                .Where(cliente => cliente.Id > 0)
                .OrderBy(p=> p.Id)
                .ToList();

            //percorrendo o resultado da consulta:
            foreach(var cliente in consultaPorMetodo)
            {
                //db.Clientes.Find(cliente.Id ); // busca primeiro em memória, senão vai no banco.
                db.Clientes.FirstOrDefault(p => p.Id == cliente.Id); // busca direto do banco de dados, sem olhar a memória.
            }
        }

        public static void CadastraPedido()
        {
            using var db = new Data.ApplicationContext(); //busca instancia do ApplicationContext

            var cliente = db.Clientes.FirstOrDefault(); // vai banco busca o 1º
            var produto = db.Produtos.FirstOrDefault(); // vai banco busca o 1º

            var pedido = new Pedido
            {
                ClienteId = cliente.Id, 
                IniciadoEm = DateTime.Now,
                FinalizadoEm = DateTime.Now,
                StatusPedido = StatusPedido.Analise,
                TipoFrete = TipoFrete.SemFrete,
                Items = new List<PedidoItem> // Cria nova lista de Pedido Item
                {
                    new PedidoItem
                    {
                        ProdutoId = produto.Id,
                        Desconto = 0,
                        Quantidade = 10,
                        Valor = 10
                    }
                }
            };

            db.Pedido.Add(pedido);
            db.SaveChanges();
        }

        public static void ConsultaCarregamentoAdiantado()
        {
            using var db = new Data.ApplicationContext(); // pega instancia do DataContext

            //var pedidos = db.Pedido.Include(p => p.Items).ToList(); // busca todos os pedidos e joga para uma lista, sem os Itens
            // var pedidos = db.Pedido.Include("Items").ToList();
            var pedidos = db
                .Pedido
                .Include(p => p.Items) // busca Pedido item
                .ThenInclude(p => p.Produto) // busca o produto do pedido item
                .ToList(); // com o include, ele busca os itens

            Console.WriteLine(pedidos.Count());
        }
       
        private static void AtualizaDados()
        {
            using var db = new Data.ApplicationContext();

            var UpdateCliente = db.Clientes.FirstOrDefault(p => p.Id == 1);

            UpdateCliente.Nome = "Altera o nome";
            /*
                O metodo db.Clientes.Update() Atualiza todas as propriedades na tabela
                Sem esse invocar esse método, somente invocar o db.SaveChanges() ja altera somente as colunas alteradas
                neste caso "Nome" da tabela de "Cliente"
            */
            db.Clientes.Update(UpdateCliente);
            db.SaveChanges();
        }
        
        private static void AtualizaCamposDesconectado()
        {
            /*
                Quando os dados ainda não estão estanciados.
                -> Temos um frontend e o usuário está preenchendo os dados em formulário.
                -> Envimos os dados para uma API, e essa API em acesso aos BD.
                -> Assim, a api trata essa inserção
                -> Recebemos o id do cliente, no caso 1
                -> E os dados estão em outro objeto NovoCliente
                -> somente alguns campos sofrem alteração
            */
            using var db = new Data.ApplicationContext();

            var cliente = new Cliente
            {
                Id = 1
            };

            var novoCliente = new
            {
                Nome = "José",
                Telefone = "5187542154"
            };

            db.Attach(cliente); //anexa o cliente
            db.Entry(cliente).CurrentValues.SetValues(novoCliente); //rastreia 
        }
        
        private static void DeletarDados()
        {
            using var db = new Data.ApplicationContext();

            //var cliente = db.Clientes.Find(1); // método Find busca direto pelo ID;
            
            var clienteId = new Cliente { Id = 1 };

            var clienteToRemove = db.Clientes.Where(p => p.Id == clienteId.Id);
            db.Remove(clienteToRemove);
            
            //ou da forma desconectada

            db.Entry(clienteToRemove).State = EntityState.Deleted;
            db.SaveChanges();
        }
    }
}
