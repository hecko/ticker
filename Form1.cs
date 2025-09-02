using System;
using System.IO;
using System.Windows.Forms;

namespace ticker
{
    public partial class Form1 : Form
    {
        private string logFile;

        public Form1()
        {
            InitializeComponent();

            // Set up the log file path (same as in TrayApp)
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            logFile = Path.Combine(docs, "TickerLog.csv");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Check if textBox1 has content
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Please enter a task before logging.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
                return;
            }

            // Log the task with current timestamp
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string taskText = textBox1.Text.Trim();
            string logEntry = $"{timestamp},in,{taskText}";

            try
            {
                // Append to CSV file
                File.AppendAllText(logFile, logEntry + Environment.NewLine);

                // Show confirmation and clear the textbox
                // MessageBox.Show($"Task logged successfully!\n\nEntry: {logEntry}", "Task Logged", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBox1.Clear();
                textBox1.Focus();
                // close the form
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing to log file:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            textBox1.Focus();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            // do not allow commas
            if (e.KeyCode == Keys.Oemcomma)
            {
                e.Handled = true;
                e.SuppressKeyPress = true; // Prevent comma input
                return;
            }
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true; // Prevent ding sound
            }
        }
    }
}
