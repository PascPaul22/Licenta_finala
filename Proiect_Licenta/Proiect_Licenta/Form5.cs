using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proiect_Licenta
{
    public partial class Form5 : Form
    {
        public Form5()
        {
            InitializeComponent();
            this.informatiiTableAdapter.Fill(this.fețe_licentaDataSet.Informatii);
            this.StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            button1.Visible= false;
            textBox2.Visible = false; textBox3.Visible = false; textBox4.Visible = false; textBox5.Visible = false;
            label2.Visible = false; label3.Visible = false; label4.Visible = false; label5.Visible = false; label6.Visible = false;
            label10.Visible = false; label9.Visible = false; label8.Visible = false; label7.Visible = false;
        }

        private bool IsValidCNP(string cnp)
        {
            if (cnp.Length != 13 || !cnp.All(char.IsDigit))
                return false;

            int[] weights = { 2, 7, 9, 1, 4, 6, 3, 5, 8, 2, 7, 9 };
            int sum = 0;

            for (int i = 0; i < 12; i++)
            {
                sum += (cnp[i] - '0') * weights[i];
            }

            int controlDigit = sum % 11;
            if (controlDigit == 10)
                controlDigit = 1;

            return controlDigit == (cnp[12] - '0');
        }
        private bool ValidareSex(string sex)
        {
            return sex == "M" || sex == "F";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form2 f = new Form2();
            f.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["Proiect_Licenta.Properties.Settings.Fețe_licentaConnectionString"].ConnectionString))
            {
                if (cn.State == ConnectionState.Closed)
                    cn.Open();
                using (DataTable dt = new DataTable("Informatii"))
                {    
                    int id;
                    if (int.TryParse(textBox1.Text, out id))
                    {
                        string query = "SELECT * FROM Informatii WHERE id = @id";
                        using (SqlCommand command = new SqlCommand(query, cn))
                        {
                            command.Parameters.AddWithValue("@ID", id);
                            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                            {
                                adapter.Fill(dt);
                            }
                        }
                        if (dt.Rows.Count > 0)
                        {
                            MessageBox.Show("ID-ul se găsește în baza de date", "Continuă", MessageBoxButtons.OK);
                            button1.Visible = true;
                            textBox2.Visible = true; textBox3.Visible = true; textBox4.Visible = true; textBox5.Visible = true;
                            label2.Visible = true; label3.Visible = true; label4.Visible = true; label5.Visible = true; label6.Visible = true;
                        }
                        else
                        {
                            MessageBox.Show("ID-ul nu se găsește în baza de date", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            button1.Visible = false;
                            textBox2.Visible = false; textBox3.Visible = false; textBox4.Visible = false; textBox5.Visible = false;
                            label2.Visible = false; label3.Visible = false; label4.Visible = false; label5.Visible = false; label6.Visible = false;
                            label7.Visible = false; label8.Visible = false; label9.Visible = false; label10.Visible = false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Introduceți un ID valid.");
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label10.Visible = false; label9.Visible = false; label8.Visible = false; label7.Visible = false;

            Regex onlyLetters = new Regex("^[a-zA-Z]+$");

            if (!onlyLetters.IsMatch(textBox2.Text))
            {
                label10.Visible = true;
                return;
            }
            if (!onlyLetters.IsMatch(textBox3.Text))
            {
                label9.Visible = true;
                return;
            }
            if (!IsValidCNP(textBox4.Text))
            {
                label7.Visible = true;
                return;
            }

            if (!ValidareSex(textBox5.Text))
            {
                label8.Visible = true;
                return;
            }

            using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["Proiect_Licenta.Properties.Settings.Fețe_licentaConnectionString"].ConnectionString))
            {
                if (cn.State == ConnectionState.Closed)
                    cn.Open();

                string newValue1 = textBox2.Text;
                string newValue2 = textBox3.Text;
                string newValue3 = textBox4.Text;
                string newValue4 = textBox5.Text;

                string query = "UPDATE Informatii SET Nume = @Nume, Prenume = @Prenume, CNP = @CNP, Sex = @Sex WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, cn))
                {
                    command.Parameters.AddWithValue("@id", textBox1.Text);
                    command.Parameters.AddWithValue("@Nume", newValue1);
                    command.Parameters.AddWithValue("@Prenume", newValue2);
                    command.Parameters.AddWithValue("@CNP", newValue3);
                    command.Parameters.AddWithValue("@Sex", newValue4);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Modificarea s-a realizat cu succes");
                    }
                    else
                    {
                        MessageBox.Show("Nu s-a putut modifica");
                    }
                    using (DataTable dt = new DataTable("Informatii"))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter("select *from Informatii", cn);
                        adapter.Fill(dt);
                        dataGridView1.DataSource = dt;
                    }
                }
            }

        }

        private void Form5_Load(object sender, EventArgs e)
        {
            //this.informatiiTableAdapter.Fill(this.fețe_licentaDataSet.Informatii);
        }
    }
}
