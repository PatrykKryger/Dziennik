using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;

namespace Dziennik
{
    public partial class TeacherWindow : Window
    {
        private string connectionString;
        private int teacherPesel;

        public TeacherWindow(string connectionString, int pesel)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            this.teacherPesel = pesel;

            LoadStudents();
            LoadSubjects();
        }

        private void LoadStudents()
        {
            // Załaduj listę uczniów przypisanych do wychowawcy
            var students = new List<Uczen>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT u.PESEL, u.imie, u.nazwisko, u.punkty
                    FROM uczen u
                    INNER JOIN klasa k ON u.klasa_id = k.Id
                    WHERE k.wychowawca_id = @PeselWychowawcy";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PeselWychowawcy", teacherPesel);

                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        students.Add(new Uczen
                        {
                            Pesel = reader["PESEL"].ToString(),
                            Imie = reader["Imie"].ToString(),
                            Nazwisko = reader["Nazwisko"].ToString(),
                            Punkty = Convert.ToInt32(reader["Punkty"])
                        });
                    }
                }
            }

            StudentsListBox.ItemsSource = students;
        }

        private void LoadSubjects()
        {
            // Załaduj listę przedmiotów
            var subjects = new List<Przedmiot>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Nazwa FROM przedmiot";

                using (var command = new SqlCommand(query, connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        subjects.Add(new Przedmiot
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nazwa = reader["Nazwa"].ToString()
                        });
                    }
                }
            }

            SubjectsComboBox.ItemsSource = subjects;
        }

        private void StudentsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (StudentsListBox.SelectedItem is Uczen selectedStudent)
            {
                // Wyświetl dane ucznia w polach, jeśli zostanie wybrany
                SelectedStudentName.Text = $"{selectedStudent.Imie} {selectedStudent.Nazwisko}";
                SelectedStudentPoints.Text = selectedStudent.Punkty.ToString();
            }
        }

        private void UpdatePoints_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsListBox.SelectedItem is Uczen selectedStudent &&
                int.TryParse(NewPointsTextBox.Text, out int newPoints))
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("UPDATE uczen SET Punkty = @Punkty WHERE PESEL = @Pesel", connection);
                    command.Parameters.AddWithValue("@Punkty", newPoints);
                    command.Parameters.AddWithValue("@Pesel", selectedStudent.Pesel);
                    command.ExecuteNonQuery();
                }

                MessageBox.Show($"Zaktualizowano punkty dla ucznia: {selectedStudent.FullName}");
                LoadStudents(); // Odśwież listę uczniów
            }
            else
            {
                MessageBox.Show("Wybierz ucznia i podaj poprawną liczbę punktów.");
            }
        }

        private void AddGrade_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsListBox.SelectedItem is Uczen selectedStudent &&
                SubjectsComboBox.SelectedValue is int subjectId &&
                int.TryParse(NewGradeTextBox.Text, out int gradeValue))
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("INSERT INTO ocena (ocena, id_ucznia, id_przedmiotu) VALUES (@Wartosc, @Pesel, @PrzedmiotId)", connection);
                    command.Parameters.AddWithValue("@Wartosc", gradeValue);
                    command.Parameters.AddWithValue("@Pesel", selectedStudent.Pesel);
                    command.Parameters.AddWithValue("@PrzedmiotId", subjectId);
                    command.ExecuteNonQuery();
                }

                MessageBox.Show($"Dodano ocenę dla ucznia: {selectedStudent.FullName}");
            }
            else
            {
                MessageBox.Show("Wybierz ucznia, przedmiot i podaj poprawną ocenę.");
            }
        }
    }

    public class Uczen
    {
        public string Pesel { get; set; }
        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        public int Punkty { get; set; }
        public string FullName => $"{Imie} {Nazwisko} ({Punkty} pkt)";
    }

    public class Przedmiot
    {
        public int Id { get; set; }
        public string Nazwa { get; set; }
    }
}
