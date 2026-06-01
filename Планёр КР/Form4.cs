using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace Планёр_КР
{
    public partial class Form4 : Form
    {
        private int factorCount;
        private List<FactorData> factors;
        private Random random = new Random();

        public Form4()
        {
            InitializeComponent();
            this.buttonBack.Click += buttonBack_Click;
            this.buttonConduct.Click += buttonConduct_Click;
        }

        public Form4(int factors, string type)
        {
            InitializeComponent();
            factorCount = factors;

            this.buttonBack.Click += buttonBack_Click;
            this.buttonConduct.Click += buttonConduct_Click;

            this.Text = "Рандомизированный эксперимент";
            ShowOnlyNeededFactors();
        }

        private void ShowOnlyNeededFactors()
        {
            if (panel1 == null) return;

            for (int i = 1; i <= 6; i++)
            {
                GroupBox group = panel1.Controls.OfType<GroupBox>().FirstOrDefault(g => g.Name == $"factor{i}");
                if (group != null)
                {
                    group.Visible = (i <= factorCount);
                }
            }
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            Form f1 = Application.OpenForms[0];
            f1.Show();
            this.Hide();
        }

        private void buttonConduct_Click(object sender, EventArgs e)
        {
            try
            {
                factors = new List<FactorData>();

                for (int i = 1; i <= factorCount; i++)
                {
                    TextBox txtLevels = this.Controls.Find($"textBoxLevel{i}", true).FirstOrDefault() as TextBox;
                    TextBox txtFrom = this.Controls.Find($"textBoxFrom{i}", true).FirstOrDefault() as TextBox;
                    TextBox txtBefore = this.Controls.Find($"textBoxBefore{i}", true).FirstOrDefault() as TextBox;

                    if (txtLevels == null || txtFrom == null || txtBefore == null)
                    {
                        MessageBox.Show($"Не найдены поля ввода для фактора {i}.\n\nОжидаемые имена:\n- textBoxLevel{i}\n- textBoxFrom{i}\n- textBoxBefore{i}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (!int.TryParse(txtLevels.Text, out int levels) || levels < 2)
                    {
                        MessageBox.Show($"Для фактора {i} введите количество уровней (целое число ≥ 2).\nВы ввели: '{txtLevels.Text}'",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string fromText = txtFrom.Text.Trim().Replace('.', ',');
                    if (!double.TryParse(fromText, out double from))
                    {
                        MessageBox.Show($"Для фактора {i} введите числовое значение 'от'.\nВы ввели: '{txtFrom.Text}'",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string beforeText = txtBefore.Text.Trim().Replace('.', ',');
                    if (!double.TryParse(beforeText, out double before))
                    {
                        MessageBox.Show($"Для фактора {i} введите числовое значение 'до'.\nВы ввели: '{txtBefore.Text}'",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (from >= before)
                    {
                        MessageBox.Show($"Для фактора {i} значение 'от' ({from}) должно быть меньше значения 'до' ({before}).",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    double center = (from + before) / 2;
                    double halfRange = (before - from) / 2;

                    factors.Add(new FactorData
                    {
                        Levels = levels,
                        MinValue = from,
                        MaxValue = before,
                        Center = center,
                        HalfRange = halfRange
                    });
                }

                if (textBoxCountExperiment == null)
                {
                    MessageBox.Show("Не найдено поле для ввода количества экспериментов (textBoxCountExperiment).",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!int.TryParse(textBoxCountExperiment.Text, out int experimentCount) || experimentCount <= 0)
                {
                    MessageBox.Show("Введите корректное количество экспериментов (целое число > 0).",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                List<ExperimentResult> results = GenerateRandomizedExperiment(experimentCount);

                if (results.Count > 0)
                {
                    DisplayResults(results);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<ExperimentResult> GenerateRandomizedExperiment(int experimentCount)
        {
            List<ExperimentResult> results = new List<ExperimentResult>();

            for (int exp = 0; exp < experimentCount; exp++)
            {
                ExperimentResult result = new ExperimentResult();
                result.CodedLevels = new List<int>();
                result.NaturalValues = new List<double>();

                for (int i = 0; i < factorCount; i++)
                {
                    FactorData factor = factors[i];
                    double randomCode = -1 + (2.0 * random.NextDouble());
                    int codedValue = (int)Math.Round(randomCode);
                    if (codedValue < -1) codedValue = -1;
                    if (codedValue > 1) codedValue = 1;

                    result.CodedLevels.Add(codedValue);
                    double naturalValue = ConvertCodeToNatural(codedValue, i);
                    result.NaturalValues.Add(Math.Round(naturalValue, 4));
                }

                result.ResultY = CalculateY(result.NaturalValues);
                results.Add(result);
            }

            return results;
        }

        private double ConvertCodeToNatural(int codedValue, int factorIndex)
        {
            FactorData factor = factors[factorIndex];
            return factor.Center + codedValue * factor.HalfRange;
        }

        // Расчет Y по формуле взвешенной суммы для 6 факторов
        // Y = w1*X1 + w2*X2 + w3*X3 + w4*X4 + w5*X5 + w6*X6
        private double CalculateY(List<double> naturalValues)
        {
            // Весовые коэффициенты для каждого фактора
            // Чем больше вес, тем сильнее фактор влияет на результат
            double[] weights = { 2.0, 1.5, 1.0, 0.8, 0.5, 0.3 };

            double sum = 0;

            for (int i = 0; i < naturalValues.Count && i < weights.Length; i++)
            {
                sum += weights[i] * naturalValues[i];
            }

            return Math.Round(sum, 4);
        }

        private void DisplayResults(List<ExperimentResult> results)
        {
            if (results.Count == 0) return;

            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.ColumnStyles.Clear();

            int columnCount = 1 + factorCount + factorCount + 1;
            tableLayoutPanel1.ColumnCount = columnCount;
            tableLayoutPanel1.RowCount = results.Count + 1;

            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            for (int i = 0; i < factorCount; i++)
            {
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85));
            }
            for (int i = 0; i < factorCount; i++)
            {
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            }
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));

            for (int i = 0; i <= results.Count; i++)
            {
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            }

            int tableWidth = 70 + factorCount * 85 + factorCount * 80 + 80;
            int tableHeight = (results.Count + 1) * 30;
            tableLayoutPanel1.Size = new Size(tableWidth, tableHeight);

            int col = 0;
            AddCell(0, col++, "№ эксп.", true);

            for (int i = 0; i < factorCount; i++)
            {
                AddCell(0, col++, $"Фактор {i + 1} (нат.)", true);
            }

            for (int i = 0; i < factorCount; i++)
            {
                AddCell(0, col++, $"Фактор {i + 1} (код)", true);
            }

            AddCell(0, col, "Y (результат)", true);

            for (int row = 0; row < results.Count; row++)
            {
                col = 0;
                AddCell(row + 1, col++, (row + 1).ToString(), false);

                for (int i = 0; i < factorCount; i++)
                {
                    AddCell(row + 1, col++, results[row].NaturalValues[i].ToString("F3"), false);
                }

                for (int i = 0; i < factorCount; i++)
                {
                    AddCell(row + 1, col++, results[row].CodedLevels[i].ToString(), false);
                }

                AddCell(row + 1, col, results[row].ResultY.ToString("F3"), false);
            }

            if (panelScroll != null)
            {
                panelScroll.AutoScrollMinSize = new Size(tableWidth, tableHeight);
            }
        }

        private void AddCell(int row, int col, string text, bool isHeader)
        {
            Label label = new Label();
            label.Text = text;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Dock = DockStyle.Fill;
            label.Font = new Font("Microsoft Sans Serif", 9F);

            if (isHeader)
            {
                label.BackColor = Color.LightGray;
                label.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            }

            tableLayoutPanel1.Controls.Add(label, col, row);
        }
    }
}