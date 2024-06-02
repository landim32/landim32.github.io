using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PessoaDAO
{
    public class PessoaConsole
    {
        private PessoaDAO _pessoaDAO = new PessoaDAO();

        public void executar()
        {
            exibirTelaInicial();
        }

        private void exibirPessoa(Pessoa p)
        {
            Console.WriteLine("        ID: " + p.id.ToString());
            Console.WriteLine("      Nome: " + p.nome);
            Console.WriteLine("       CPF: " + p.cpf);
            if (p.endereco != null)
            {
                Console.WriteLine("------------------------------------------------");
                Console.WriteLine("Endereço Atual");
                Console.WriteLine("Logradouro: " + p.endereco.logradouro);
                Console.WriteLine("    Número: " + p.endereco.numero);
                Console.WriteLine("       CEP: " + p.endereco.cep);
                Console.WriteLine("    Bairro: " + p.endereco.bairro);
                Console.WriteLine("    Cidade: " + p.endereco.cidade);
                Console.WriteLine("    Estado: " + p.endereco.estado);
            }
            if (p.telefones != null && p.telefones.Count > 0)
            {
                Console.WriteLine("------------------------------------------------");
                Console.WriteLine("Telefones");
                foreach (var telefone in p.telefones)
                {
                    Console.WriteLine(String.Format("{0}: ({1}){2}", telefone.tipo.tipo, telefone.ddd, telefone.numero));
                }
            }
        }

        private void consultarPorCpf()
        {
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("Consultar Pessoa Por CPF:");
            long cpf = preencherCPF("Digite o Cpf: ");
            var p = _pessoaDAO.consulte(cpf);
            if (p != null)
            {
                exibirPessoa(p);
            }
            else
            {
                Console.WriteLine("Erro: Pessoa não encontrada.");
            }
            exibirTelaInicial();
        }

        private long preencherCPF(string titulo)
        {
            Console.Write(titulo);
            var cpfStr = Console.ReadLine();
            //Console.WriteLine();
            long cpf = 0;
            if (long.TryParse(cpfStr, out cpf))
            {
                return cpf;
            }
            else
            {
                Console.WriteLine(String.Format("Erro: '{0}' não é um CPF válido.", cpfStr));
                return preencherCPF(titulo);
            }
        }

        private string preencherTexto(string titulo)
        {
            Console.Write(titulo);
            return Console.ReadLine();
        }

        private int preencherNumero(string titulo)
        {
            Console.Write(titulo);
            var str = Console.ReadLine();
            int numero = 0;
            if (int.TryParse(str, out numero))
            {
                return numero;
            }
            else
            {
                Console.WriteLine(String.Format("Erro: '{0}' não é um número válido.", str));
                return preencherNumero(titulo);
            }
        }

        private bool perguntar(string titulo)
        {
            Console.Write(titulo);
            ConsoleKeyInfo key;
            bool vResposta = false, vContinue = true;
            do
            {
                key = Console.ReadKey();
                Console.WriteLine();
                switch (key.KeyChar)
                {
                    case 's':
                        vResposta = true;
                        vContinue = false;
                        break;
                    case 'n':
                        vContinue = false;
                        break;
                    default:
                        Console.WriteLine(String.Format("Erro: '{0}' não é um comando válido.", key.KeyChar));
                        vContinue = true;
                        break;
                }
            }
            while (vContinue);
            return vResposta;
        }

        private Endereco inserirEndereco()
        {
            var e = new Endereco();
            e.logradouro = preencherTexto("Logradouro: ");
            e.numero = preencherNumero("Número: ");
            e.cep = preencherNumero("CEP: ");
            e.bairro = preencherTexto("Bairro: ");
            e.cidade = preencherTexto("Cidade: ");
            e.estado = preencherTexto("Estado: ");
            return e;
        }

        private void alterarEndereco(Endereco e)
        {
            e.logradouro = preencherTexto(String.Format("Logradouro[{0}]: ", e.logradouro));
            e.numero = preencherNumero(String.Format("Número[{0}]: ", e.numero));
            e.cep = preencherNumero(String.Format("CEP[{0}]: ", e.cep));
            e.bairro = preencherTexto(String.Format("Bairro[{0}]: ", e.bairro));
            e.cidade = preencherTexto(String.Format("Cidade[{0}]: ", e.cidade));
            e.estado = preencherTexto(String.Format("Estado[{0}]: ", e.estado));
        }

        private Telefone inserirTelefone()
        {
            var t = new Telefone();
            t.ddd = preencherNumero("DDD: ");
            t.numero = preencherNumero("Número: ");
            var tipo = preencherTexto("Tipo: ");
            if (!string.IsNullOrEmpty(tipo))
            {
                t.tipo = new TipoTelefone
                {
                    tipo = tipo
                };
            }
            return t;
        }

        private void inserirPessoa()
        {
            var p = new Pessoa();

            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("Inserir Pessoa - Preencha dos dados");
            p.nome = preencherTexto("Nome: ");
            p.cpf = preencherCPF("CPF: ");
            if (perguntar("Deseja cadastrar o endereço?[s,n]: "))
            {
                p.endereco = inserirEndereco();
            }
            p.telefones = new List<Telefone>();
            while (perguntar("Deseja cadastrar um novo telefone?[s,n]: "))
            {
                p.telefones.Add(inserirTelefone());
            }

            if (_pessoaDAO.insira(p))
            {
                Console.WriteLine("Pessoa incluída com sucesso!");
            }
            else
            {
                Console.WriteLine(String.Format("Erro: Não foi possível incluir a pessoa '{0}' .", p.nome));
            }
            exibirTelaInicial();
        }

        private void alterarPessoa()
        {

            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("Alterar Pessoa - Preencha dos dados");
            long cpf = preencherCPF("Digite o Cpf: ");
            var p = _pessoaDAO.consulte(cpf);
            if (p == null)
            {
                Console.WriteLine("Erro: Pessoa não encontrada.");
                exibirTelaInicial();
                return;
            }
            p.nome = preencherTexto(String.Format("Nome[{0}]: ", p.nome));
            p.cpf = preencherCPF(String.Format("CPF[{0}]: ", p.cpf));
            if (p.endereco != null)
            {
                if (perguntar("Deseja alterar o endereço?[s,n]: "))
                {
                    alterarEndereco(p.endereco);
                }
            }
            else
            {
                if (perguntar("Não tem endereço cadastrado.Deseja cadastrar?[s,n]: "))
                {
                    p.endereco = inserirEndereco();
                }
            }
            if (p.telefones == null) {
                p.telefones = new List<Telefone>();
            }
            if (p.telefones.Count > 0)
            {
                if (perguntar(String.Format("Tem {0} telefone(s) cadastrado(s). Deseja limpar?[s,n]: ", p.telefones.Count)))
                {
                    p.telefones = new List<Telefone>();
                }
            }
            while (perguntar("Deseja cadastrar um novo telefone?[s,n]: "))
            {
                p.telefones.Add(inserirTelefone());
            }

            if (_pessoaDAO.altere(p))
            {
                Console.WriteLine("Pessoa alterada com sucesso!");
            }
            else
            {
                Console.WriteLine(String.Format("Erro: Não foi possível alterar a pessoa '{0}' .", p.nome));
            }
            exibirTelaInicial();
        }

        private void excluirPessoa()
        {
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("Excluir Pessoa - Preencha dos dados");
            long cpf = preencherCPF("Digite o Cpf: ");

            var p = _pessoaDAO.consulte(cpf);
            if (p == null)
            {
                Console.WriteLine("Erro: Pessoa não encontrada.");
                exibirTelaInicial();
                return;
            }
            if (_pessoaDAO.exclua(p))
            {
                Console.WriteLine("Pessoa excluída com sucesso!");
            }
            else
            {
                Console.WriteLine(String.Format("Erro: Não foi possível alterar a pessoa '{0}' .", p.nome));
            }
            exibirTelaInicial();
        }
        private void sair()
        {
            Console.WriteLine("Obrigado por usar o PessoaDAO. :)");
            Console.ReadLine();
        }

        private void exibirTelaInicial()
        {
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("Selecione a opção:");
            Console.WriteLine("[1] - Consultar por CPF");
            Console.WriteLine("[2] - Inserir Pessoa");
            Console.WriteLine("[3] - Alterar Pessoa");
            Console.WriteLine("[4] - Excluir Pessoa");
            Console.WriteLine("[x] - Sair");
            Console.WriteLine("------------------------------------------------");
            Console.Write("Digite o comando: ");
            var key = Console.ReadKey();
            Console.WriteLine();
            switch (key.KeyChar)
            {
                case '1':
                    consultarPorCpf();
                    break;
                case '2':
                    inserirPessoa();
                    break;
                case '3':
                    alterarPessoa();
                    break;
                case '4':
                    excluirPessoa();
                    break;
                case 'x':
                    sair();
                    break;
                default:
                    Console.WriteLine(String.Format("Erro: '{0}' não é um comando válido.", key.KeyChar));
                    exibirTelaInicial();
                    break;
            }
        }
    }
}
