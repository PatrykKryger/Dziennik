using Dziennik;
using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Dziennik
{
    public partial class MainWindow : Window
    {
        private string connectionString = "Data Source=10.100.100.146;Initial Catalog=szkola;User ID=Admin2;Password=zaq1@WSX";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(LoginTextBox.Text, out int pesel))
            {
                ErrorMessage.Text = "Nieprawidłowy format PESEL.";
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            string password = PasswordBox.Password;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Sprawdź, czy PESEL i hasło pasują do nauczyciela
                string teacherQuery = "SELECT PESEL FROM nauczyciel WHERE PESEL = @PESEL AND Haslo = @Haslo";
                using (var cmd = new SqlCommand(teacherQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@PESEL", pesel);
                    cmd.Parameters.AddWithValue("@Haslo", password);

                    var teacherPesel = cmd.ExecuteScalar();
                    if (teacherPesel != null)
                    {
                        var teacherWindow = new TeacherWindow(connectionString, pesel);
                        teacherWindow.Show();
                        this.Close();
                        return;
                    }
                }

                // Sprawdź, czy PESEL i hasło pasują do ucznia
                string studentQuery = "SELECT PESEL FROM uczen WHERE PESEL = @PESEL AND Haslo = @Haslo";
                using (var cmd = new SqlCommand(studentQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@PESEL", pesel);
                    cmd.Parameters.AddWithValue("@Haslo", password);

                    var studentPesel = cmd.ExecuteScalar();
                    if (studentPesel != null)
                    {
                        var studentWindow = new StudentWindow(connectionString, pesel);
                        studentWindow.Show();
                        this.Close();
                        return;
                    }
                }
            }

            ErrorMessage.Text = "Nieprawidłowy PESEL lub hasło.";
            ErrorMessage.Visibility = Visibility.Visible;
        }
    }
}
