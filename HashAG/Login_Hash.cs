﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;

namespace HashAG
{
    public partial class Login_Hash : Form
    {
        public Login_Hash()
        {
            InitializeComponent();
        }


        public static class Crypto
        {
           
            public static string sha256encrypt(string frase)
            {
                UTF8Encoding encoder = new UTF8Encoding();
                SHA256Managed sha256hasher = new SHA256Managed();
                byte[] hashedDataBytes = sha256hasher.ComputeHash(encoder.GetBytes(frase));
                return byteArrayToString(hashedDataBytes);
            }
            public static string byteArrayToString(byte[] inputArray)
            {
                StringBuilder output = new StringBuilder("");
                for (int i = 0; i < inputArray.Length; i++)
                {
                    output.Append(inputArray[i].ToString("X2"));
                }
                return output.ToString();
            }
        }


        private void informacoesSMTP()
        {
            txtEnderecoSMTP.Text = Properties.Settings.Default.enderecoSMTP;
            nupPortaSMTP.Value = Convert.ToInt32(Properties.Settings.Default.portaSMTP);
            txtEmailUsuarioSMTP.Text = Properties.Settings.Default.emailSMTP;
            txtSenhaEmailSMTP.Text = Properties.Settings.Default.senhaSMTP;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: esta linha de código carrega dados na tabela 'hashcj3022501DataSet.acessos'. Você pode movê-la ou removê-la conforme necessário.
            this.acessosTableAdapter.Fill(this.hashcj3022501DataSet.acessos);

            // TODO: This line of code loads data into the 'loginsDataSet.Acessos' table. You can move, or remove it, as needed.
            this.acessosTableAdapter.Fill(this.hashcj3022501DataSet.acessos);
            informacoesSMTP();
        }

        private void bindingNavigator1_RefreshItems(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            //variaveis locais para tratar o usuario e a senha
            string usuario = txtUsuarioLogin.Text;
            string senha = Crypto.sha256encrypt(txtSenhaLogin.Text);

            //percorre cada tabela do banco de dados
            foreach (DataRow row in hashcj3022501DataSet.acessos)
            {
                //e verifica pelo usuário e senha que coincidem
                if (row.ItemArray[1].Equals(usuario) && row.ItemArray[2].Equals(senha))
                {
                    txtUsuarioLogin.Text = String.Empty;
                    txtSenhaLogin.Text = String.Empty;
                    MessageBox.Show("Login realizado com sucesso !");
                    break;
                }
                //Se não achar então
                else
                {
                    MessageBox.Show("Usuário/Senha incorretos");
                    break;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AdicionarUsuario(txtUsuario.Text, txtSenha.Text, txtConfrimarSenha.Text, txtEmail.Text);
        }

        

        private void AdicionarUsuario(string _nomeUsuario, string _senha, string _confirmaSenha, string _email)
        {
            //variaveis locais para tratar os valores
            string smtpEmail = txtEmailUsuarioSMTP.Text;
            string smtpPassword = txtSenhaEmailSMTP.Text;
            int smtpPorta = (int)nupPortaSMTP.Value;
            string smtpAddress = txtEnderecoSMTP.Text;

            //Regex para validar o email
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(_email);

            //Percorre as tabelas do banco de dados
            foreach (DataRow row in hashcj3022501DataSet.acessos)
            {
                //E procura por nomes de usuários existentes
                if (row.ItemArray[1].Equals(_nomeUsuario))
                {
                    //Se achar um então avisa
                    MessageBox.Show("O nome do usuário já existe, tente informar outro nome.");
                    return;
                }
            }
            //Confirma a senha
            if (_senha != _confirmaSenha)
            {
                MessageBox.Show("A senha não confere.");
            }
            // A senha tem que ter no minimo 8 caracteres
            else if (_senha.Length < 8)
            {
                MessageBox.Show("A senha deve conter no mínimo 8 caracteres");
            }
            //Se o email não for válido
            else if (!match.Success)
            {
                MessageBox.Show("Email inválido");
            }
            //Se não informou o usuário
            else if (_nomeUsuario == null)
            {
                MessageBox.Show("VOcê deve informar um usuário");
            }
            //Se estiver tudo certo então cria o usuário
            else
            {
                string _hashSenha = Crypto.sha256encrypt(_senha);
                AdicionaUsuarioNoBD(_nomeUsuario, _hashSenha, _email);

                txtUsuario.Text = String.Empty;
                txtSenha.Text = String.Empty;
                txtConfrimarSenha.Text = String.Empty;
                txtEmail.Text = String.Empty;

                MessageBox.Show("Obrigado por seu registro !");

                if (String.IsNullOrWhiteSpace(smtpEmail) || String.IsNullOrWhiteSpace(smtpPassword) ||
                    String.IsNullOrWhiteSpace(smtpAddress) || smtpPorta <= 0)
                {
                    MessageBox.Show("A configuração do Email não foi definida corretamente! \nNão é possível enviar emails!");
                }
                else
                {
                    EnviaMensagem(_email.ToString(), _nomeUsuario.ToString(), _senha.ToString());
                }
            }
        }

        public void EnviaMensagem(string ParaEndereco, string ParaNome, string _senha)
        {
            var cliente = new SmtpClient(txtEnderecoSMTP.Text, (int)nupPortaSMTP.Value)
            {
                Credentials = new NetworkCredential(txtEmailUsuarioSMTP.Text, txtSenhaEmailSMTP.Text),
                EnableSsl = true
            };
            cliente.Send(txtEmailUsuarioSMTP.Text, ParaEndereco, "Obrigado !", "Obrigado por seu registro ! \n Seu usuário/senha são: \n \nUsuário: "
                     + ParaNome.ToString() + "\nSenha: " + _senha.ToString());
        }

        private void AdicionaUsuarioNoBD(string _nomeUsuario, string _senha, string _email)
        {
            string ConnectString = Properties.Settings.Default.LoginsConnectionString;
            SqlCeConnection cn = new SqlCeConnection(ConnectString);
            
            if (cn.State == ConnectionState.Closed)
            {
                cn.Open();
            }
            SqlCeCommand cmd;
            string sql = "insert into Acessos "
                           + "(usuario, senha, email) "
                           + "values (@usuario, @senha, @email)";
            cmd = new SqlCeCommand(sql, cn);
            try
            {
                
                cmd.Parameters.Add("@usuario", SqlDbType.NVarChar, 50).Value = _nomeUsuario;
                cmd.Parameters.Add("@senha", SqlDbType.NVarChar, 50).Value = _senha;
                cmd.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = _email;

                cmd.ExecuteNonQuery();
                MessageBox.Show("Usuario incluído.");
            }
            catch (SqlCeException sqlexception)
            {
                MessageBox.Show(sqlexception.Message, "Arre Égua.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Arre Égua, a coisa falhou...", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cn.Close();
            }
        }

        private void txtSenhaEmailSMTP_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
