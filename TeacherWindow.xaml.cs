using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;

namespace Dziennik
{
    public partial class TeacherWindow : Window
    {
        private string connectionString;
        private int teacherId;

        public TeacherWindow(string connectionString, int teacherId)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            this.teacherId = teacherId;
            LoadStudents();
        }

        private void LoadStudents()
        {
            List<Student> students = new List<Student>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Name FROM Students WHERE ClassId = (SELECT ClassId FROM Teachers WHERE Id = @TeacherId)";
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@TeacherId", teacherId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(new Student
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            StudentsList.ItemsSource = students;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsList.SelectedItem is Student selectedStudent &&
                int.TryParse(GradeTextBox.Text, out int grade) &&
                int.TryParse(BehaviorPointsTextBox.Text, out int points))
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Students SET Grade = @Grade, BehaviorPoints = @Points WHERE Id = @StudentId";
                    using (var cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Grade", grade);
                        cmd.Parameters.AddWithValue("@Points", points);
                        cmd.Parameters.AddWithValue("@StudentId", selectedStudent.Id);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Zapisano!");
            }
            else
            {
                MessageBox.Show("Wybierz ucznia i podaj poprawne wartości.");
            }
        }
    }

    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
