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
using System.Text.RegularExpressions;

namespace Proiect_Licenta
{
    public partial class Form1 : Form
    {
        public const int rowCount = 13, columnCount = 13;

        public const int featuresPerBox = 5;

        private static readonly (float x, float y)[] boxAnchors = { (0.573f, 0.677f), (1.87f, 2.06f), (3.34f, 5.47f), (7.88f, 3.53f), (9.77f, 9.17f) };

        private PredictionEngine<Face, FacePrediction> PredictionEngine;

        public static List<BoundingBox> ParseOutputs(float[] modelOutput, string[] labels, float probabilityThreshold = .3f)
        {
            var boxes = new List<BoundingBox>();

            for (int row = 0; row < rowCount; row++)
            {
                for (int column = 0; column < columnCount; column++)
                {
                    for (int box = 0; box < boxAnchors.Length; box++)
                    {
                        var channel = box * (labels.Length + featuresPerBox);

                        var boundingBoxPrediction = ExtractBoundingBoxPrediction(modelOutput, row, column, channel);

                        var mappedBoundingBox = MapBoundingBoxToCell(row, column, box, boundingBoxPrediction);

                        if (boundingBoxPrediction.Confidence < probabilityThreshold)
                            continue;

                        float[] classProbabilities = ExtractClassProbabilities(modelOutput, row, column, channel, boundingBoxPrediction.Confidence, labels);

                        var (topProbability, topIndex) = classProbabilities.Select((probability, index) => (Score: probability, Index: index)).Max();

                        if (topProbability < probabilityThreshold)
                            continue;

                        boxes.Add(new BoundingBox
                        {
                            Dimensions = mappedBoundingBox,
                            Confidence = topProbability,
                            Label = labels[topIndex]
                        });
                    }
                }
            }
            return boxes;
        }

        private static BoundingBoxPrediction ExtractBoundingBoxPrediction(float[] modelOutput, int row, int column, int channel)
        {
            return new BoundingBoxPrediction
            {
                X = modelOutput[GetOffset(row, column, channel++)],
                Y = modelOutput[GetOffset(row, column, channel++)],
                Width = modelOutput[GetOffset(row, column, channel++)],
                Height = modelOutput[GetOffset(row, column, channel++)],
                Confidence = Sigmoid(modelOutput[GetOffset(row, column, channel++)])
            };
        }

        private static BoundingBoxDimensions MapBoundingBoxToCell(int row, int column, int box, BoundingBoxPrediction boxDimensions)
        {
            const float cellWidth = ImageSetings.imageWidth / columnCount;
            const float cellHeight = ImageSetings.imageHeight / rowCount;

            var mappedBox = new BoundingBoxDimensions
            {
                X = (row + Sigmoid(boxDimensions.X)) * cellWidth,
                Y = (column + Sigmoid(boxDimensions.Y)) * cellHeight,
                Width = (float)Math.Exp(boxDimensions.Width) * cellWidth * boxAnchors[box].x,
                Height = (float)Math.Exp(boxDimensions.Height) * cellHeight * boxAnchors[box].y,
            };
            mappedBox.X -= mappedBox.Width / 2;
            mappedBox.Y -= mappedBox.Height / 2;

            return mappedBox;
        }

        public static float[] ExtractClassProbabilities(float[] modelOutput, int row, int column, int channel, float confidence, string[] labels)
        {
            var classProbabilitiesOffset = channel + featuresPerBox;
            float[] classProbabilities = new float[labels.Length];
            for (int classProbability = 0; classProbability < labels.Length; classProbability++)
                classProbabilities[classProbability] = modelOutput[GetOffset(row, column, classProbability + classProbabilitiesOffset)];
            return Softmax(classProbabilities).Select(p => p * confidence).ToArray();
        }

        private static float Sigmoid(float value)
        {
            var k = (float)Math.Exp(value);
            return k / (1.0f + k);
        }

        private static float[] Softmax(float[] classProbabilities)
        {
            var max = classProbabilities.Max();
            var exp = classProbabilities.Select(v => Math.Exp(v - max));
            var sum = exp.Sum();
            return exp.Select(v => (float)v / (float)sum).ToArray();
        }

        private static int GetOffset(int row, int column, int channel)
        {
            const int channelStride = rowCount * columnCount;
            return (channel * channelStride) + (column * columnCount) + row;
        }

        public Form1()
        {
            InitializeComponent();
            label5.Visible = false; label6.Visible = false; label7.Visible = false;
            WindowState = FormWindowState.Maximized;
            pictureBox1.Visible = false;
            var context = new MLContext();
            var emptydata = new List<Face>();
            var data = context.Data.LoadFromEnumerable(emptydata);
            var pipeline = context.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: "data",
             imageWidth: ImageSetings.imageWidth,imageHeight:ImageSetings.imageHeight,inputColumnName: nameof(Face.Image))
             .Append(context.Transforms.ExtractPixels(outputColumnName:"data"))
             .Append(context.Transforms.ApplyOnnxModel(modelFile:"./MachineLearningModel/model.onnx",outputColumnName:"model_outputs0",inputColumnName: "data"));

            var model = pipeline.Fit(data);
            PredictionEngine=context.Model.CreatePredictionEngine<Face,FacePrediction>(model);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.informatiiTableAdapter.Fill(this.fețe_licentaDataSet.Informatii);
            Afisare(dataGridView1);
        }

        private bool IsOnlyLetters(string input)
        {
            Regex onlyLetters = new Regex("^[a-zA-Z]+$");
            return onlyLetters.IsMatch(input);
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


        public void Insert(string fileName, byte[] image)
        {

            label5.Visible = false; label6.Visible = false; label7.Visible = false;

            if (!IsOnlyLetters(textBox1.Text))
            {
                label5.Visible = true;
                return;
            }

            if (!IsOnlyLetters(textBox2.Text))
            {
                label6.Visible = true;
                return;
            }

            if (!IsValidCNP(textBox3.Text))
            {
                label7.Visible = true;
                return;
            }

            using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings
                ["Proiect_Licenta.Properties.Settings.Fețe_licentaConnectionString"].ConnectionString))
            {
                if(cn.State == ConnectionState.Closed)
                    cn.Open();
                using(SqlCommand cmd= new SqlCommand("insert into Informatii (Nume, Prenume, CNP, Sex, Imagine) " +
                    "values (@Nume, @Prenume, @CNP, @Sex, @Imagine)", cn))
                {
                    cmd.CommandType=CommandType.Text;
                    cmd.Parameters.AddWithValue("@Nume", textBox1.Text);
                    cmd.Parameters.AddWithValue("@Prenume", textBox2.Text);
                    cmd.Parameters.AddWithValue("@CNP", textBox3.Text);
                    cmd.Parameters.AddWithValue("@Sex", comboBox1.Text);
                    cmd.Parameters.AddWithValue("@Imagine", image);
                    cmd.ExecuteNonQuery();
                }
                textBox1.Clear(); textBox2.Clear(); textBox3.Clear(); comboBox1.Text = "";
            }
        }

        public static void Afisare(DataGridView dataGridView1)
        {
            using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["Proiect_Licenta.Properties.Settings.Fețe_licentaConnectionString"].ConnectionString))
            {
                if (cn.State == ConnectionState.Closed)
                    cn.Open();
                using(DataTable dt = new DataTable("Informatii"))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter("select *from Informatii", cn);
                    adapter.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
        }

        byte[] Convertire_img_byte(Image image)
        {
            using(MemoryStream ms=new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        public Image Convertire_byte_img(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                return Image.FromStream(ms);
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                var image = Image.FromFile(openFileDialog1.FileName);
                Insert(textBox1.Text,Convertire_img_byte(image));
                Afisare(dataGridView1);
            }
            else
            {
                MessageBox.Show("Introduceți o imagine", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataTable dt = dataGridView1.DataSource as DataTable;
            if(dt!=null)
            {
                if (e.RowIndex >= 0 && e.RowIndex < dt.Rows.Count)
                {
                    DataRow row = dt.Rows[e.RowIndex];
                    pictureBox1.Image = Convertire_byte_img((byte[])row["Imagine"]);
                }
                else
                {
                    MessageBox.Show("Nu s-a gasit intrarea în baza de date");
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form2 f = new Form2();
            f.Show();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var image = (Bitmap)Image.FromFile(openFileDialog1.FileName);

                var prediction = PredictionEngine.Predict(new Face { Image = image });

                var labels = File.ReadAllLines("./MachineLearningModel/labels.txt");

                var boundingBoxes = ParseOutputs(prediction.Face, labels);

                var originalWidth = image.Width;
                var originalHeight = image.Height;

                if (boundingBoxes.Count > 1)
                {
                    var maxConfidence = boundingBoxes.Max(b => b.Confidence);
                    var topBoundingBox = boundingBoxes.FirstOrDefault(b => b.Confidence == maxConfidence);

                    float x = Math.Max(topBoundingBox.Dimensions.X, 0);
                    float y = Math.Max(topBoundingBox.Dimensions.Y, 0);
                    float width = Math.Min(originalWidth - x, topBoundingBox.Dimensions.Width);
                    float height = Math.Min(originalHeight - y, topBoundingBox.Dimensions.Height);

                    x = originalWidth * x / ImageSetings.imageWidth;
                    y = originalHeight * y / ImageSetings.imageHeight;
                    width = originalWidth * width / ImageSetings.imageWidth;
                    height = originalHeight * height / ImageSetings.imageHeight;

                    using (var graphics = Graphics.FromImage(image))
                    {
                        graphics.DrawRectangle(new Pen(Color.Red, 3), x, y, width, height);
                        graphics.DrawString(topBoundingBox.Description, new Font(FontFamily.Families[0], 70f), Brushes.Red, x + 5, y + 5);
                    }
                }
                else
                {
                    MessageBox.Show("Nu s-a gasit predicție pentru imagine.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                pictureBox1.Image = image;
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox1.Visible = true;
            }
        }
    }

    class BoundingBoxPrediction : BoundingBoxDimensions
    {
        public float Confidence { get; set; }
    }
}
