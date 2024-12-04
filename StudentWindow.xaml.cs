using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;

namespace Dziennik
{
    public partial class StudentWindow : Window
    {
        private string connectionString;
        private int studentPesel;

        public StudentWindow(string connectionString, int studentPesel)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            this.studentPesel = studentPesel;
            LoadStudentData();
            LoadGrades();  // Ładowanie ocen
        }

        // Metoda do załadowania punktów ucznia
        private void LoadStudentData()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Punkty FROM uczen WHERE PESEL = @StudentPESEL";
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@StudentPESEL", studentPesel);
                    var points = cmd.ExecuteScalar();

                    // Wyświetlenie punktów ucznia
                    if (points != null)
                    {
                        PointsLabel.Text = $"Punkty: {points}";
                    }
                    else
                    {
                        PointsLabel.Text = "Nie znaleziono punktów.";
                    }
                }
            }
        }

        private void LoadGrades()
        {
            List<StudentGrade> grades = new List<StudentGrade>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Zapytanie, które łączy tabele ocena i przedmiot, aby pobrać oceny ucznia oraz nazwy przedmiotów
                string query = @"
            SELECT p.nazwa, o.ocena 
            FROM ocena o
            INNER JOIN przedmiot p ON o.id_przedmiotu = p.id
            WHERE o.id_ucznia = @StudentPESEL";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@StudentPESEL", studentPesel);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Pobieranie nazwy przedmiotu
                            string subjectName = reader.GetString(0); // Nazwa przedmiotu (zakładamy, że to zawsze jest string)

                            // Pobieranie oceny, zakładając, że zawsze jest typu int
                            // Jeśli wartości w tabeli mogą być NULL, dodamy odpowiednią obsługę
                            int grade = 0;

                            if (!reader.IsDBNull(1))
                            {
                                grade = reader.GetInt32(1); // Ocena, tylko jeśli nie jest NULL
                            }

                            grades.Add(new StudentGrade
                            {
                                Przedmiot = subjectName,
                                Ocena = grade // Przechowujemy ocenę
                            });
                        }
                    }
                }
            }

            // Ustawiamy dane w DataGrid
            GradesDataGrid.ItemsSource = grades;
        }



    }

    // Klasa pomocnicza do przechowywania przedmiotów i ocen ucznia
    public class StudentGrade
    {
        public string Przedmiot { get; set; }
        public int Ocena { get; set; }
    }
}
