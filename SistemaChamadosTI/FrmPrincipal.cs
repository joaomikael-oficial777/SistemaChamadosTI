
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaChamadosTI
{
    public partial class FrmPrincipal : Form
    {
        private readonly ChamadoDAO dao = new ChamadoDAO();

        private TextBox txtSolicitante = new TextBox();
        private TextBox txtSetor = new TextBox();
        private TextBox txtEquipamento = new TextBox();
        private TextBox txtCategoria = new TextBox();
        private TextBox txtResponsavel = new TextBox();
        private TextBox txtDescricao = new TextBox();

        private ComboBox cmbPrioridade = new ComboBox();
        private ComboBox cmbStatusCadastro = new ComboBox();
        private ComboBox cmbNovoStatus = new ComboBox();

        private DataGridView dgvChamados = new DataGridView();

        public FrmPrincipal()
        {
            InitializeComponent();
            MontarTela();
        }

        private void MontarTela()
        {
            Text = "Sistema de Chamados de TI";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1200, 800);
            MinimumSize = new Size(1000, 700);

            Controls.Clear();

            Label titulo = new Label
            {
                Text = "SISTEMA DE CHAMADOS DE TI",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 10),
                Size = new Size(600, 35)
            };
            Controls.Add(titulo);

            AdicionarCampo("Solicitante", txtSolicitante, 20, 65, 180);
            AdicionarCampo("Setor", txtSetor, 220, 65, 150);
            AdicionarCampo("Equipamento", txtEquipamento, 390, 65, 180);
            AdicionarCampo("Categoria", txtCategoria, 590, 65, 160);
            AdicionarCampo("Responsável", txtResponsavel, 770, 65, 180);
            AdicionarCampo("Descrição do problema", txtDescricao, 20, 130, 900);

            txtDescricao.Multiline = true;
            txtDescricao.ScrollBars = ScrollBars.Vertical;
            txtDescricao.AcceptsReturn = true;

            AdicionarCombo("Prioridade", cmbPrioridade, 970, 65,
                new string[] { "Baixa", "Média", "Alta", "Urgente" });

            AdicionarCombo("Status inicial", cmbStatusCadastro, 970, 130,
                new string[] { "Aberto", "Em andamento", "Aguardando",
                    "Resolvido", "Cancelado" });

            cmbPrioridade.SelectedItem = "Média";
            cmbStatusCadastro.SelectedItem = "Aberto";

            Button btnCadastrar = CriarBotao("Cadastrar chamado", 20, 220, 170);
            btnCadastrar.Click += BtnCadastrar_Click;
            Controls.Add(btnCadastrar);

            Button btnLimpar = CriarBotao("Limpar campos", 205, 220, 140);
            btnLimpar.Click += (s, e) => LimparCampos();
            Controls.Add(btnLimpar);

            Button btnAtualizar = CriarBotao("Atualizar lista", 360, 220, 150);
            btnAtualizar.Click += (s, e) => CarregarChamados();
            Controls.Add(btnAtualizar);

            AdicionarCombo("Novo status do selecionado", cmbNovoStatus, 530, 220,
                new string[] { "Aberto", "Em andamento", "Aguardando",
                    "Resolvido", "Cancelado" });

            Button btnStatus = CriarBotao("Alterar status", 770, 220, 140);
            btnStatus.Click += BtnStatus_Click;
            Controls.Add(btnStatus);

            Button btnExcluir = CriarBotao("Excluir selecionado", 930, 220, 180);
            btnExcluir.Click += BtnExcluir_Click;
            Controls.Add(btnExcluir);

            dgvChamados.Location = new Point(20, 310);
            dgvChamados.Size = new Size(1140, 420);
            dgvChamados.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                | AnchorStyles.Left | AnchorStyles.Right;

            dgvChamados.ReadOnly = true;
            dgvChamados.AllowUserToAddRows = false;
            dgvChamados.AllowUserToDeleteRows = false;
            dgvChamados.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dgvChamados.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dgvChamados.MultiSelect = false;
            dgvChamados.AutoGenerateColumns = true;

            // Exibe a numeração sequencial na margem esquerda.
            dgvChamados.RowHeadersVisible = true;
            dgvChamados.RowHeadersWidth = 55;
            dgvChamados.RowPostPaint += dgvChamados_RowPostPaint;

            Controls.Add(dgvChamados);

            Load += (s, e) => CarregarChamados();
        }

        // Numeração visual: 1, 2, 3, 4...
        // Não altera os IDs verdadeiros do banco de dados.
        private void dgvChamados_RowPostPaint(
            object? sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvChamados.Rows.Count)
            {
                dgvChamados.Rows[e.RowIndex].HeaderCell.Value =
                    (e.RowIndex + 1).ToString();
            }
        }

        private void AdicionarCampo(
            string titulo, TextBox campo, int x, int y, int largura)
        {
            Label label = new Label
            {
                Text = titulo,
                Location = new Point(x, y),
                Size = new Size(largura, 20)
            };

            campo.Location = new Point(x, y + 22);
            campo.Size = new Size(largura, 25);

            if (campo == txtDescricao)
                campo.Size = new Size(largura, 65);

            Controls.Add(label);
            Controls.Add(campo);
        }

        private void AdicionarCombo(
            string titulo, ComboBox combo, int x, int y, string[] opcoes)
        {
            Label label = new Label
            {
                Text = titulo,
                Location = new Point(x, y),
                Size = new Size(200, 20)
            };

            combo.Location = new Point(x, y + 22);
            combo.Size = new Size(190, 25);
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.Items.AddRange(opcoes);

            Controls.Add(label);
            Controls.Add(combo);
        }

        private Button CriarBotao(string texto, int x, int y, int largura)
        {
            return new Button
            {
                Text = texto,
                Location = new Point(x, y),
                Size = new Size(largura, 35)
            };
        }

        private void BtnCadastrar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSolicitante.Text) ||
                string.IsNullOrWhiteSpace(txtSetor.Text) ||
                string.IsNullOrWhiteSpace(txtEquipamento.Text) ||
                string.IsNullOrWhiteSpace(txtCategoria.Text) ||
                string.IsNullOrWhiteSpace(txtDescricao.Text) ||
                string.IsNullOrWhiteSpace(txtResponsavel.Text))
            {
                MessageBox.Show("Preencha todos os campos antes de cadastrar.");
                return;
            }

            try
            {
                Chamado chamado = new Chamado
                {
                    Solicitante = txtSolicitante.Text.Trim(),
                    Setor = txtSetor.Text.Trim(),
                    Equipamento = txtEquipamento.Text.Trim(),
                    Categoria = txtCategoria.Text.Trim(),
                    Descricao = txtDescricao.Text.Trim(),
                    Prioridade = cmbPrioridade.Text,
                    Responsavel = txtResponsavel.Text.Trim(),
                    Status = cmbStatusCadastro.Text,
                    DataAbertura = DateTime.Now
                };

                dao.Cadastrar(chamado);

                MessageBox.Show("Chamado cadastrado com sucesso!");

                LimparCampos();
                CarregarChamados();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao cadastrar:\n" + ex.Message);
            }
        }

        private void CarregarChamados()
        {
            try
            {
                dgvChamados.DataSource = null;
                dgvChamados.DataSource = dao.Listar();

                string[,] nomes =
                {
                    { "Id", "ID" },
                    { "Solicitante", "Solicitante" },
                    { "Setor", "Setor" },
                    { "Equipamento", "Equipamento" },
                    { "Categoria", "Categoria" },
                    { "Descricao", "Descrição" },
                    { "Prioridade", "Prioridade" },
                    { "Responsavel", "Responsável" },
                    { "DataAbertura", "Data de abertura" },
                    { "Status", "Status" }
                };

                for (int i = 0; i < nomes.GetLength(0); i++)
                {
                    if (dgvChamados.Columns[nomes[i, 0]] != null)
                    {
                        dgvChamados.Columns[nomes[i, 0]]!.HeaderText =
                            nomes[i, 1];
                    }
                }

                // Mantém a numeração visual ativada após atualizar a lista.
                dgvChamados.RowHeadersVisible = true;
                dgvChamados.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar chamados:\n" + ex.Message);
            }
        }

        private Chamado? ChamadoSelecionado()
        {
            if (dgvChamados.CurrentRow == null)
                return null;

            return dgvChamados.CurrentRow.DataBoundItem as Chamado;
        }

        private void BtnStatus_Click(object? sender, EventArgs e)
        {
            Chamado? chamado = ChamadoSelecionado();

            if (chamado == null)
            {
                MessageBox.Show("Selecione um chamado na tabela.");
                return;
            }

            try
            {
                dao.AlterarStatus(chamado.Id, cmbNovoStatus.Text);

                MessageBox.Show("Status alterado com sucesso!");

                CarregarChamados();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao alterar status:\n" + ex.Message);
            }
        }

        private void BtnExcluir_Click(object? sender, EventArgs e)
        {
            Chamado? chamado = ChamadoSelecionado();

            if (chamado == null)
            {
                MessageBox.Show("Selecione um chamado na tabela.");
                return;
            }

            DialogResult resposta = MessageBox.Show(
                "Deseja realmente excluir o chamado " + chamado.Id + "?",
                "Confirmar exclusão",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (resposta != DialogResult.Yes)
                return;

            try
            {
                dao.Excluir(chamado.Id);

                MessageBox.Show("Chamado excluído com sucesso!");

                CarregarChamados();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao excluir chamado:\n" + ex.Message);
            }
        }

        private void InitializeComponent()
        {
        }

        private void LimparCampos()
        {
            txtSolicitante.Clear();
            txtSetor.Clear();
            txtEquipamento.Clear();
            txtCategoria.Clear();
            txtResponsavel.Clear();
            txtDescricao.Clear();

            cmbPrioridade.SelectedItem = "Média";
            cmbStatusCadastro.SelectedItem = "Aberto";
        }
    }
}
