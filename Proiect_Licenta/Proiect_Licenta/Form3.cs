using Microsoft.ML;
using Microsoft.ML.Transforms.Image;
using MLNET_ObjectDetection_WinForms.Models;
using Proiect_Licenta.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Face;
using System.Drawing.Imaging;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Configuration;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Proiect_Licenta
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
        }

        public void Afisare_Nume(DataGridView dataGridView1)
        {
            using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings
                ["Proiect_Licenta.Properties.Settings.Fețe_licentaConnectionString"].ConnectionString))
            {
                if (cn.State == ConnectionState.Closed)
                    cn.Open();
                using (DataTable dt = new DataTable("Informatii"))
                {
                    string nume = textBox1.Text;
                    string query = "SELECT * FROM Informatii WHERE Nume = @Nume";
                    using (SqlCommand command = new SqlCommand(query, cn))
                    {
                        command.Parameters.AddWithValue("@Nume", nume);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dt);
                        }
                    }
                    dataGridView1.DataSource = dt;
                }
            }
        }

        public void Afisare_Prenume(DataGridView dataGridView1)
        {
            using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["Proiect_Licenta.Properties.Settings.Fețe_licentaConnectionString"].ConnectionString))
            {
                if (cn.State == ConnectionState.Closed)
                    cn.Open();
                using (DataTable dt = new DataTable("Informatii"))
                {
                    string prenume = textBox2.Text;
                    string query = "SELECT * FROM Informatii WHERE Prenume = @Prenume";
                    using (SqlCommand command = new SqlCommand(query, cn))
                    {
                        command.Parameters.AddWithValue("@Prenume", prenume);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dt);
                        }
                    }
                    dataGridView1.DataSource = dt;
                }
            }
        }

        public void Afisare_CNP(DataGridView dataGridView1)
        {
            using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["Proiect_Licenta.Properties.Settings.Fețe_licentaConnectionString"].ConnectionString))
            {
                if (cn.State == ConnectionState.Closed)
                    cn.Open();
                using (DataTable dt = new DataTable("Informatii"))
                {
                    string CNP = textBox3.Text;
                    string query = "SELECT * FROM Informatii WHERE CNP = @CNP";
                    using (SqlCommand command = new SqlCommand(query, cn))
                    {
                        command.Parameters.AddWithValue("@CNP", CNP);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dt);
                        }
                    }
                    dataGridView1.DataSource = dt;
                }
            }
        }

        public void Afisare_Sex(DataGridView dataGridView1)
        {
            using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["Proiect_Licenta.Properties.Settings.Fețe_licentaConnectionString"].ConnectionString))
            {
                if (cn.State == ConnectionState.Closed)
                    cn.Open();
                using (DataTable dt = new DataTable("Informatii"))
                {
                    string sex = textBox4.Text;
                    string query = "SELECT * FROM Informatii WHERE Sex = @Sex";
                    using (SqlCommand command = new SqlCommand(query, cn))
                    {
                        command.Parameters.AddWithValue("@Sex", sex);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dt);
                        }
                    }
                    dataGridView1.DataSource = dt;
                }
            }
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            this.informatiiTableAdapter.Fill(this.fețe_licentaDataSet.Informatii);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Afisare_Nume(dataGridView1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form2 f = new Form2();
            f.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Afisare_Prenume(dataGridView1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Afisare_CNP(dataGridView1);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Afisare_Sex(dataGridView1);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex==6)
            {
                if (MessageBox.Show("Vreți să ștergeți această înregistare?", "Messaj", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var rowIndex = e.RowIndex;
                    if (rowIndex >= 0)
                    {
                        int id = Convert.ToInt32(dataGridView1.Rows[rowIndex].Cells["idDataGridViewTextBoxColumn"].Value);
                        using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["Proiect_Licenta.Properties.Settings.Fețe_licentaConnectionString"].ConnectionString))
                        {
                            if (cn.State == ConnectionState.Closed)
                                cn.Open();

                            string deleteQuery = "DELETE FROM Informatii WHERE Id = @Id";
                            using (SqlCommand command = new SqlCommand(deleteQuery, cn))
                            {
                                command.Parameters.AddWithValue("@Id", id);
                                try
                                {
                                    int rowsAffected = command.ExecuteNonQuery();
                                    if (rowsAffected > 0)
                                    {
                                        MessageBox.Show("Ștergerea s-a realizat cu succes.");
                                        dataGridView1.Rows.RemoveAt(rowIndex);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Nu s-a găsit nicio înregistrare cu ID-ul specificat.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"A apărut o eroare: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
