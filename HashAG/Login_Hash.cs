using System;
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

        string conexaoString;
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
        private void Form1_Load(object sender, EventArgs e)
        {
            conexaoString = "Data Source=10.125.50.191;Initial Catalog=hashcj3022501;User ID=aluno;Password=aluno";

            using (SqlConnection conexao = new SqlConnection(conexaoString))
            {
                try
                {
                    conexao.Open();
                    MessageBox.Show("Conexão bem-sucedida!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao conectar: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                finally
                {

                }
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuarioLogin.Text.Trim();
            string senha = Crypto.sha256encrypt(txtSenhaLogin.Text.Trim());

            string connectionString = "Data Source=10.125.50.191;Initial Catalog=hashcj3022501;User ID=aluno;Password=aluno";

            string query = "SELECT COUNT(*) FROM acessos WHERE usuario = @usuario AND senha = @senha";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@usuario", usuario);
                cmd.Parameters.AddWithValue("@senha", senha);

                conn.Open();

                int resultado = (int)cmd.ExecuteScalar();

                if (resultado > 0)
                {
                    txtUsuarioLogin.Text = String.Empty;
                    txtSenhaLogin.Text = String.Empty;
                    MessageBox.Show("Login realizado com sucesso!");
                }
                else
                {
                    MessageBox.Show("Usuário/Senha incorretos");
                }
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            AdicionarUsuario(txtUsuario.Text, txtSenha.Text, txtConfrimarSenha.Text, txtEmail.Text);
        }



        private void AdicionarUsuario(string _nomeUsuario, string _senha, string _confirmaSenha, string _email)
        {
            // String de conexão com SQL Server
            string conexaoString = "Data Source=10.125.50.191;Initial Catalog=hashcj3022501;User ID=aluno;Password=aluno";

            // Variáveis para configuração do SMTP
            string smtpEmail = txtEmailUsuarioSMTP.Text;
            string smtpPassword = txtSenhaEmailSMTP.Text;
            int smtpPorta = (int)nupPortaSMTP.Value;
            string smtpAddress = txtEnderecoSMTP.Text;

            // Regex para validar o email
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(_email);

            using (SqlConnection conexao = new SqlConnection(conexaoString))
            {
                try
                {
                    conexao.Open();

                    // Verifica se o usuário já existe no banco de dados
                    string queryVerifica = "SELECT COUNT(*) FROM acessos WHERE Usuario = @Usuario";
                    using (SqlCommand cmdVerifica = new SqlCommand(queryVerifica, conexao))
                    {
                        cmdVerifica.Parameters.AddWithValue("Usuario", _nomeUsuario);
                        int usuarioExiste = (int)cmdVerifica.ExecuteScalar();

                        if (usuarioExiste > 0)
                        {
                            MessageBox.Show("O nome do usuário já existe, tente informar outro nome.");
                            return;
                        }
                    }

                    // Confirmação da senha
                    if (_senha != _confirmaSenha)
                    {
                        MessageBox.Show("A senha não confere.");
                        return;
                    }
                    else if (_senha.Length < 8)
                    {
                        MessageBox.Show("A senha deve conter no mínimo 8 caracteres");
                        return;
                    }
                    else if (!match.Success)
                    {
                        MessageBox.Show("Email inválido");
                        return;
                    }
                    else if (string.IsNullOrWhiteSpace(_nomeUsuario))
                    {
                        MessageBox.Show("Você deve informar um usuário.");
                        return;
                    }

                    // Criptografa a senha
                    string _hashSenha = Crypto.sha256encrypt(_senha);

                    // Insere o usuário no banco de dados
                    string queryInsercao = "INSERT INTO acessos (Usuario, senha, email) VALUES (@Usuario, @senha, @email)";
                    using (SqlCommand cmdInserir = new SqlCommand(queryInsercao, conexao))
                    {
                        cmdInserir.Parameters.AddWithValue("@Usuario", _nomeUsuario);
                        cmdInserir.Parameters.AddWithValue("@senha", _hashSenha);
                        cmdInserir.Parameters.AddWithValue("@email", _email);
                        cmdInserir.ExecuteNonQuery();
                    }

                    // Limpa os campos após a inserção
                    txtUsuario.Text = string.Empty;
                    txtSenha.Text = string.Empty;
                    txtConfrimarSenha.Text = string.Empty;
                    txtEmail.Text = string.Empty;

                    MessageBox.Show("Obrigado por seu registro!");



                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao conectar ao banco: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }






        private void txtSenhaEmailSMTP_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
