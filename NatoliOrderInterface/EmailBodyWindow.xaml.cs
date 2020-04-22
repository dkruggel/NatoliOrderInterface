using System;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Windows;
using System.Windows.Documents;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for EmailBodyWindow.xaml
    /// </summary>
    public partial class EmailBodyWindow : Window
    {
        private string CSR;
        private int orderNumber;
        private readonly MainWindow grandParent;

        public EmailBodyWindow(WorkOrder workOrder, MainWindow grandParent)
        {
            try
            {
                InitializeComponent();
                CSR = workOrder.Csr;
                this.orderNumber = workOrder.OrderNumber;
                this.grandParent = grandParent;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SendEmailToCSR()
        {
            try
            {
                // Send email
                SmtpClient smtpServer = new SmtpClient();
                MailMessage mail = new MailMessage();
                smtpServer.Port = 25;
                smtpServer.Host = "192.168.1.186";
                mail.IsBodyHtml = true;
                mail.From = new MailAddress("AutomatedEmail@natoli.com");
                mail.To.Add(GetEmailAddress(CSR));
                // mail.Bcc.Add("eng6@natoli.com");
                mail.Bcc.Add("intlcs6@natoli.com");
                mail.Bcc.Add("customerservice5@natoli.com");
                mail.CC.Add(GetEmailAddress(Environment.UserName.ToLower().Substring(0, 1) + "%" + Environment.UserName.ToLower().Substring(1, 1) + Environment.UserName.ToLower().Substring(2)));
                mail.Subject = "REQUEST FOR CHANGES WO# " + orderNumber.ToString();
                mail.Body = new TextRange(EmailBodyTextBlock.Document.ContentStart, EmailBodyTextBlock.Document.ContentEnd).Text;
                smtpServer.Send(mail);
                mail.Dispose();
                smtpServer.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                // WriteToErrorLog("SendEmailToCSR", ex.Message);
            }
            Close();
        }

        private string GetEmailAddress(string userName)
        {
            string connectionString = @"Data Source=" + App.Server + ";Initial Catalog=Driveworks;Persist Security Info=True; User ID=" + App.UserID+";Password="+App.Password+"";
            string email = string.Empty;
            if (userName == "Gregory") { userName = "Greg"; }
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "SELECT EmailAddress FROM SecurityUsers WHERE DisplayName LIKE '" + userName + "'";
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    email = reader.GetString(0);
                                }
                            }
                        }
                    }
                }
                return email;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error resolving email address\n" + ex.Message);
                Close();
            }
            return null;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendEmailToCSR();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            grandParent.BoolValue = true;
        }

        //private void WriteToErrorLog(string errorLoc, string errorMessage)
        //{
        //    string path = @"\\nshare\vb_apps\NatoliOrderInterface\Logs\Error_Log.txt";
        //    System.IO.StreamReader sr = new System.IO.StreamReader(path);
        //    string existing = sr.ReadToEnd();
        //    existing = existing.TrimEnd();
        //    sr.Close();
        //    System.IO.StreamWriter sw = new System.IO.StreamWriter(path, false);
        //    sw.Write(DateTime.Now + "  " + grandParent.User.GetUserName() + "  " + errorLoc + "\r\n" + errorMessage.PadLeft(20) + "\r\n" + existing);
        //    sw.Flush();
        //    sw.Close();
        //}
    }
}
