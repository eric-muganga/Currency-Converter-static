using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CurrencyConverter_static
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection sqlConnection = new SqlConnection(); //creating object for Sqlconnection
        SqlCommand sqlCommand = new SqlCommand(); //creating object for Sqlcommand
        SqlDataAdapter dataAdapter = new SqlDataAdapter(); //creating object for SqlAdapter

        private int currencyId = 0;
        private double fromAmount = 0;
        private double toAmount = 0;
        public MainWindow()
        {
            InitializeComponent();
            BindCurrency();
            GetData();
        }

        private void MyConnection()
        {
            string connection = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            sqlConnection = new SqlConnection(connection);
            sqlConnection.Open();
        }

        private void BindCurrency()
        {
            MyConnection();
            //creating an object for dataTable
            DataTable dtCurrency = new DataTable();
            //writing the querry to get data from the Currency_Master Table
            sqlCommand = new SqlCommand("select Id, CurrencyName from Currency_Master",sqlConnection);
            // setting sqlCommand CommandType to text
            sqlCommand.CommandType = CommandType.Text;

            dataAdapter = new SqlDataAdapter(sqlCommand);
            dataAdapter.Fill(dtCurrency);
           
            //creating an object for DataRow
            DataRow row = dtCurrency.NewRow();
            //assigning a value to Id Column
            row["Id"] = 0;
            //assigning a value to CurrencyName Column
            row["CurrencyName"] = "--SELECT--";

            //Inserting a row in dtCurrency at position 0
            dtCurrency.Rows.InsertAt(row, 0);

            if(dtCurrency != null && dtCurrency.Rows.Count > 0)
            {
                //assigning dtCurrency data to cmbFromCurrency using itemsSource
                cmbFromCurrency.ItemsSource = dtCurrency.DefaultView;

                //assigning dtCurrency data to cmbToCurrency using itemsSource
                cmbToCurrency.ItemsSource = dtCurrency.DefaultView;
            }
            sqlConnection.Close();

            //for the from comb box
            cmbFromCurrency.DisplayMemberPath = "CurrencyName";
            cmbFromCurrency.SelectedValuePath = "Id";
            cmbFromCurrency.SelectedIndex = 0;

            //for the to comb box
            cmbToCurrency.DisplayMemberPath= "CurrencyName";
            cmbToCurrency.SelectedValuePath= "Id";
            cmbToCurrency.SelectedIndex = 0;

        }
        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            //convertedValue variable to store currency converted value
            double convertedValue;

            if(txtCurrency.Text == null || txtCurrency.Text.Trim() == string.Empty)
            {
                //if textBox is null or blank it will show this messageBox
                MessageBox.Show("Please enter the currency", "Information", MessageBoxButton.OK, 
                    MessageBoxImage.Information);
                //after clicking ok on the message box the focus is set on amount textBox
                txtCurrency.Focus();
                return;
            }
            // else if the form currency is not selected
            else if(cmbFromCurrency.SelectedValue == null || cmbFromCurrency.SelectedIndex == 0)
            {
                //show the  message
                MessageBox.Show("Please select the from currency", "Information", MessageBoxButton.OK,
                    MessageBoxImage.Information);

                //set focus to the from ComboBox
                cmbFromCurrency.Focus();
                return;
            }
            // else if the to currency is not selected
            else if(cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0)
            {
                //show the  message
                MessageBox.Show("Please select the to currency", "Information", MessageBoxButton.OK, 
                    MessageBoxImage.Information);

                //set focus to the from ComboBox
                cmbToCurrency.Focus();
                return;
            }

            // checking if the from and to comboBox selected values are the same
            if(cmbFromCurrency.Text == cmbToCurrency.Text)
            {
                //amount textbox value is set to be the ConvertedValue
                //double.parse is converting txtCurrency string into a double
                convertedValue = double.Parse(txtCurrency.Text);
                //showing the convertedValue at the lblCurrency label and ToString("N3") is used to add 3 zeros after a period
                IbICurrency.Content = cmbToCurrency.Text + " " + convertedValue.ToString("N3");
            }
            else
            {
                convertedValue = (double.Parse(cmbFromCurrency.SelectedValue.ToString())) * double.Parse(txtCurrency.Text)
                    /double.Parse(cmbToCurrency.SelectedValue.ToString());
                // showing the convertedValue and converted currency name at the lblCurrency label
                IbICurrency.Content = cmbToCurrency.Text + " " + convertedValue.ToString("N3"); 
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearControls();
        }

        /// <summary>
        /// NumberValidationTextBox method is used to restrict user input in a text box
        /// to numeric characters only by preventing the entry of non-numeric characters and ensuring 
        /// that the input adheres to the specified pattern.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]+"); //this regex only accepts numbers and dot
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ClearControls()
        {
            txtCurrency.Text = string.Empty;
            if (cmbFromCurrency.Items.Count > 0)
            {
                cmbFromCurrency.SelectedIndex = 0;
            }

            if (cmbToCurrency.Items.Count > 0)
            {
                cmbToCurrency.SelectedIndex = 0;
            }

            IbICurrency.Content = string.Empty;
            txtCurrency.Focus();

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtAmount.Text == null || txtAmount.Text.Trim() == string.Empty)
                {
                    MessageBox.Show("Please enter amount", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtAmount.Focus();
                    return;
                }else if(txtCurrency.Text == null || txtCurrencyName.Text.Trim() == string.Empty)
                {
                    MessageBox.Show("Please enter currency name", "Information", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    txtCurrency.Focus();
                    return;
                }
                else
                {
                    if(currencyId > 0) // code for the Update button 
                    {

                        if(MessageBox.Show("Are you sure you want to Update?", "Information", 
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            MyConnection();
                            DataTable dataTable = new DataTable();
                            //updating Querry record Update using Id
                            sqlCommand = new SqlCommand("UPDATE Currency_Master SET Amount = @Amount, CurrencyName = @CurrencyName WHERE Id = @Id", sqlConnection);
                            sqlCommand.CommandType = CommandType.Text;
                            sqlCommand.Parameters.AddWithValue("@Id", currencyId);
                            sqlCommand.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            sqlCommand.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            sqlCommand.ExecuteNonQuery();
                            sqlConnection.Close();

                            MessageBox.Show("Data updated successfully", "Information", MessageBoxButton.OK,
                                MessageBoxImage.Information);

                        }
                    }
                    else //for the save button
                    {
                        if (MessageBox.Show("Are you sure you want to save ?", "Information",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            MyConnection();

                            sqlCommand = new SqlCommand("INSERT INTO Currency_Master(Amount, CurrencyName) VALUES(@Amount, @CurrencyName)", sqlConnection);
                            sqlCommand.CommandType = CommandType.Text;
                            sqlCommand.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            sqlCommand.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            sqlCommand.ExecuteNonQuery();
                            sqlConnection.Close();

                            MessageBox.Show("Data saved successfully", "Information", MessageBoxButton.OK,
                                MessageBoxImage.Information);

                        }
                    }
                    ClearMaster();
                }

            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ClearMaster()
        {
            try
            {
                txtAmount.Text = string.Empty;
                txtCurrencyName.Text = string.Empty;
                btnSave.Content = "Save";
                GetData();
                currencyId = 0;
                BindCurrency();
                txtAmount.Focus();
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GetData()   // for binding data in DataGrid View
        {
            MyConnection();
            DataTable dt = new DataTable();
            sqlCommand = new SqlCommand("SELECT * FROM Currency_Master", sqlConnection);
            sqlCommand.CommandType = CommandType.Text; 
            dataAdapter = new SqlDataAdapter(sqlCommand);
            dataAdapter.Fill(dt);
            if(dt != null && dt.Rows.Count > 0)
            {
                dgvCurrency.ItemsSource = dt.DefaultView;

            }else { dgvCurrency.ItemsSource = null;}
            sqlConnection.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMaster();
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgvCurrency_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                DataGrid dg = (DataGrid)sender;
                DataRowView row_selected = dg.CurrentItem as DataRowView;

                if(row_selected != null)
                {
                    if(dgvCurrency.Items.Count > 0)
                    {
                        if(dg.SelectedCells.Count > 0)
                        {
                            // getting selected row Id column value and assigning it to the currentId
                            currencyId = Int32.Parse(row_selected["Id"].ToString());

                            if (dg.SelectedCells[0].Column.DisplayIndex == 0) // if the displayIndex is equal to zero then it is edit cell
                            {
                                txtAmount.Text = row_selected["Amount"].ToString();// gets the Amount column value and sets it to txtAmount.text content
                                txtCurrencyName.Text = row_selected["CurrencyName"].ToString();// gets the CurrencyName column value and sets it to txtCurrency.text content
                                btnSave.Content = "Update"; // changing save button text to update
                            }

                            if (dg.SelectedCells[0].Column.DisplayIndex == 1) // if the displayIndex is equal to 1 then it is delete cell
                            {
                                if (MessageBox.Show("Are you sure you want to delete ?", "Information",
                                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    MyConnection();
                                    DataTable table = new DataTable();
                                    sqlCommand = new SqlCommand("DELETE FROM Currency_Master WHERE Id =@Id", sqlConnection);
                                    sqlCommand.CommandType = CommandType.Text;
                                    sqlCommand.Parameters.AddWithValue("@Id", currencyId);
                                    sqlCommand.ExecuteNonQuery();
                                    sqlConnection.Close();

                                    MessageBox.Show("Data delete successfully", "Information", MessageBoxButton.OK,
                                        MessageBoxImage.Information);
                                    ClearMaster();
                                }

                            }
                        }
                    }
                }

                }catch (Exception ex)
            {

            }

        }

    }
}
