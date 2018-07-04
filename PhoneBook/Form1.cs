using System;
using System.Data;
using System.Windows.Forms;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace PhoneBook
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
		
        IPAddress ip = null;
        bool connected = true;
		const int createCode=1;
		const int deleteCode=2;
		const int searchCode=3;
		const int showAllCode=4;
        String firstName = "";
        String lastName = "";
        String phoneNumber = "";
		
        public void showRecords()
        {
            try
            {
                string response = Communicator.sendRequest(showAllCode.ToString()+"^");
                string[] stringParts = response.Split('^');
                connected = true;
                dataGridView1.DataSource = createTableFromData(stringParts);
            }
            catch
            {
                MessageBox.Show("Database connection hasn't been established.");
                connected = false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.ActiveControl = txtIP;
        }

        private DataTable createTableFromData(string[] stringParts)
        {
            DataTable table = new DataTable("Users");
            table.Columns.Add("LastName");
            table.Columns.Add("FirstName");
            table.Columns.Add("PhoneNumber");

            for (int i = 0; i < stringParts.Length - 1; i += 3)
            {
                DataRow row = table.NewRow();
                row["LastName"] = stringParts[i + 0];
                row["FirstName"] = stringParts[i + 1];
                row["PhoneNumber"] = stringParts[i + 2];
                table.Rows.Add(row);
            }
            return table;
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
             
                if ((txtFirstName.Text.Length == 0) && (txtFirstName.Text.Length == 0))
                {
                    MessageBox.Show("Both name fields are empty.");
                    txtFirstName.Focus();
                    return;
                }
                if (txtPhoneNumber.Text.Length < 9)
                {
                    MessageBox.Show("Phone number doesn't have enough digits.");
                    txtPhoneNumber.Focus();
                    return;
                }
				
                string insert = String.Format(createCode.ToString()+"^{0}^{1}^{2}", txtFirstName.Text, txtLastName.Text, txtPhoneNumber.Text);
				string response = Communicator.sendRequest(insert);
                   
                MessageBox.Show("The record has been created.");
                showRecords();
                
            }
            catch { MessageBox.Show("Unsuccesfully."); }
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtPhoneNumber.Text = "";
            txtFirstName.Visible = false;
            txtLastName.Visible = false;
            txtPhoneNumber.Visible = false;
            btnInsert.Visible = false;
            btnCreate.Visible = true;
            label1.Visible = false;
            label2.Visible = false;
            label3.Visible = false;
            label5.Visible = false;
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                {
                    if (txtSearch.Text == "")
                    {
                        showRecords();
                    }
                    else
                    {

						string newText = Regex.Replace(txtSearch.Text, "[^a-zA-Z]", "");
                        bool shouldGoToEnd = newText.Length < txtSearch.Text.Length;
                        txtSearch.Text = newText;
                        if (shouldGoToEnd) txtSearch.SelectionStart = txtSearch.Text.Length;

                        string select =String.Format( searchCode.ToString()+"^{0}", txtSearch.Text);
					    string response = Communicator.sendRequest(select);
                        string[] stringParts = response.Split('^');

                        dataGridView1.DataSource = createTableFromData(stringParts);
                    }      
                }
            }
            catch
            {
                MessageBox.Show("Database connection hasn't been established.");
            }
        }
		
        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        { 
            firstName = dataGridView1.SelectedCells[1].Value.ToString();
            lastName = dataGridView1.SelectedCells[0].Value.ToString();
            phoneNumber = dataGridView1.SelectedCells[2].Value.ToString();
            btnDelete.Visible = true;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
				 string delete =String.Format( deleteCode.ToString()+"^{0}^{1}^{2}", firstName, lastName, phoneNumber);
                 string response = Communicator.sendRequest(delete);
                 MessageBox.Show("Record has been successfully deleted.");
                 showRecords();
            }
            catch { MessageBox.Show("Deleting unsuccessful."); }
			
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtPhoneNumber.Text = "";
        }

        private string removeNonLetters(string s)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            s = textInfo.ToTitleCase(s);
            s = Regex.Replace(s, "[^a-zA-Z]", "");
            return s;
        }

        private void txtFirstName_Leave(object sender, EventArgs e)
        {
            txtFirstName.Text = removeNonLetters(txtFirstName.Text);
        }

        private void txtLastName_Leave(object sender, EventArgs e)
        {
            txtLastName.Text = removeNonLetters(txtLastName.Text);
        }

        private void txtPhoneNumber_Leave(object sender, EventArgs e)
        {
            txtPhoneNumber.Text = Regex.Replace(txtPhoneNumber.Text, "[^0-9+]", "");
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            txtFirstName.Visible = true;
            txtLastName.Visible = true;
            txtPhoneNumber.Visible = true;
            btnInsert.Visible = true;
            btnDelete.Visible = false;
            btnCreate.Visible = false;
            label1.Visible = true;
            label2.Visible = true;
            label3.Visible = true;
            label5.Visible = true;
            txtFirstName.Focus();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                ip = IPAddress.Parse(txtIP.Text);
                Communicator.connecToServer(ip.ToString());
                showRecords();
                if (connected)
                {
                    dataGridView1.Visible = true;
                    txtSearch.Visible = true;
                    btnCreate.Visible = true;
                    label4.Visible = true;
                    label6.Visible = false;
                    btnConnect.Visible = false;
                    txtIP.Visible = false;
                    txtSearch.Focus();
                }
                else
                {
                    txtIP.Text = "";
                    this.ActiveControl = txtIP;
                }
            }
            catch
            {
                MessageBox.Show("Wrong IP address format. Try again.");
                this.ActiveControl = txtIP;
                txtIP.Text = "";
            }
        }

        private void form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Communicator.close();
        }
    }
}
