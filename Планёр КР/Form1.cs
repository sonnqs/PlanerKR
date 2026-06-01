using System;
using System.Windows.Forms;

namespace Планёр_КР
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            // Проверяем, введено ли количество факторов
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Пожалуйста, введите количество факторов.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(textBox1.Text, out int factorCount) || factorCount < 1 || factorCount > 6)
            {
                MessageBox.Show("Введите корректное количество факторов (целое число от 1 до 6).", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Определяем, какой вид эксперимента выбран
            if (radioButton1.Checked) // Полный факторный
            {
                Form2 form2 = new Form2(factorCount, "FullFactorial");
                this.Hide();
                form2.ShowDialog();
                this.Show();
            }
            else if (radioButton2.Checked) // Дробный факторный
            {
                Form3 form3 = new Form3(factorCount, "FractionalFactorial");
                this.Hide();
                form3.ShowDialog();
                this.Show();
            }
            else if (radioButton3.Checked) // Рандомизированный
            {
                Form4 form4 = new Form4(factorCount, "Randomized");
                this.Hide();
                form4.ShowDialog();
                this.Show();
            }
            else if (radioButton4.Checked) // Латинский квадрат
            {
                if (factorCount != 2)
                {
                    MessageBox.Show("Латинский квадрат работает только для двух факторов.\n" +
                        "Вы выбрали " + factorCount + " факторов.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                Form5 form5 = new Form5(factorCount, "LatinSquare");
                this.Hide();
                form5.ShowDialog();
                this.Show();
            }
            else if (radioButton5.Checked) // Классический
            {
                Form6 form6 = new Form6(factorCount, "Classical");
                this.Hide();
                form6.ShowDialog();
                this.Show();
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(e.KeyChar >= '1' && e.KeyChar <= '6' || (int)e.KeyChar == 8)) e.KeyChar = (char)0;
                else if ((int)e.KeyChar != 8 && textBox1.Text.Length >= 1) e.KeyChar = (char)0;
        }
    }
}