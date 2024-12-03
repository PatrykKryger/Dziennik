using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Dziennik
{
    /// <summary>
    /// Logika interakcji dla klasy mainPanel.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string connection = "Data Source=10.100.100.146;Initial Catalog=szkola;User ID=Admin2; Password=zaq1@WSX";
        private readonly bool isTeacher;
        private readonly string username;
        private string selectedStudent;

        public MainWindow(bool isTeacher, string pesel)
        {
            InitializeComponent();
            this.isTeacher = isTeacher;
            this.username = pesel;

            if (this.isTeacher)
            {
                LoadTeacherData();
            }
            else
            {
                LoadStudentData();
            }
        }

        private void LoadTeacherData()
        {
            setHeader(username, "nauczyciel");
            string classId = GetTeacherClass(username);
            if (!string.IsNullOrEmpty(classId))
            {
                _classId.Content = classId;
                LoadStudents(classId);
                LoadGrades(username, "2");
            }
        }

        private void LoadStudentData()
        {
            setHeader(username, "uczen");
            LoadStudentDetails(username);
        }

        private void setHeader(string pesel, string tableName)
        {
            string query = $"SELECT imie, nazwisko FROM dbo.{tableName} WHERE PESEL=@pesel";
            using (SqlConnection connection = new SqlConnection(this.connection))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@pesel", pesel);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        _userHeader.Content = $"Witaj {reader["imie"]} {reader["nazwisko"]}";
                    }
                }
            }
        }

        private void LoadStudentDetails(string pesel)
        {
            string query = "SELECT imie, nazwisko, klasa_id, punkty FROM dbo.uczen WHERE PESEL=@pesel";
            using (SqlConnection connection = new SqlConnection(this.connection))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@pesel", pesel);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        _studentData.Content = $"Pesel: {pesel}\n" +
                                               $"Imię: {reader["imie"]}\n" +
                                               $"Nazwisko: {reader["nazwisko"]}\n" +
                                               $"Klasa: {reader["klasa_id"]}\n" +
                                               $"Punkty: {reader["punkty"]}";
                    }
                }
            }
        }

        private string GetTeacherClass(string pesel)
        {
            string query = "SELECT Id FROM dbo.klasa WHERE wychowawca_id=@pesel";
            using (SqlConnection connection = new SqlConnection(this.connection))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@pesel", pesel);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader["Id"].ToString();
                    }
                }
            }
            return string.Empty;
        }

        private void LoadStudents(string classId)
        {
            string query = "SELECT imie, nazwisko, PESEL FROM dbo.uczen WHERE klasa_id=@classId";
            using (SqlConnection connection = new SqlConnection(this.connection))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@classId", classId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    _studentList.Items.Clear();
                    while (reader.Read())
                    {
                        string pesel = reader["PESEL"].ToString();
                        TreeViewItem treeItem = new TreeViewItem
                        {
                            Header = $"{reader["imie"]} {reader["nazwisko"]}"
                        };
                        treeItem.Selected += (s, e) =>
                        {
                            LoadStudentDetails(pesel);
                            selectedStudent = pesel;
                        };
                        _studentList.Items.Add(treeItem);
                    }
                }
            }
        }

        private string GetSubjectName(string subjectId)
        {
            string query = "SELECT nazwa FROM dbo.przedmiot WHERE Id=@subjectId";
            using (SqlConnection connection = new SqlConnection(this.connection))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@subjectId", subjectId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader["nazwa"].ToString();
                    }
                }
            }
            return string.Empty;
        }

        private void LoadGrades(string pesel, string subjectId)
        {
            string query = "SELECT ocena FROM dbo.ocena WHERE id_ucznia=@pesel AND id_przedmiotu=@subjectId";
            using (SqlConnection connection = new SqlConnection(this.connection))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@pesel", pesel);
                command.Parameters.AddWithValue("@subjectId", subjectId);

                List<string> grades = new List<string>();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        grades.Add(reader["ocena"].ToString());
                    }
                }

                _gradesDataGrid.ItemsSource = grades.Select(g => new { Grade = g }).ToList();
                _gradesDataGrid.Columns.Clear();
                _gradesDataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = GetSubjectName(subjectId),
                    Binding = new Binding("Grade")
                });
            }
        }
    }
}
