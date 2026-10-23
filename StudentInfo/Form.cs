using StudentInfo.Model;
using StudentInfo.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StudentInfo
{
    public enum State
    {
        View,
        Add,
        Update
    }
    public partial class Form : System.Windows.Forms.Form
    {
        private State _currentState = State.View;
        private readonly IStudentDataServices _studentDataServices;
        private List<StudentModel> _students = new List<StudentModel>();

        private StudentModel _selectedStudent;
        public Form(IStudentDataServices studentDataServices)
        {
            _studentDataServices = studentDataServices;
            InitializeComponent();
        }

        public async void Form_Load(object sender, EventArgs e)
        {
            var acadLevels = await _studentDataServices.GetAcadLevelsAsync();
            var sections = await _studentDataServices.GetSectionsAsync();
            var students = await _studentDataServices.GetAllStudentsAsync();

            PopulateAcadLevelDropDown(acadLevels);
            PopulateSectionDropDown(sections);
            DisplayStudents(students);

            ResetButtons();
            EnableFields(false);
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            var search = txtSearch.Text;
            var result = await searchStudentsAsync(search);
            DisplayStudents(result ?? new List<StudentModel>());
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < _students.Count)
            {
                _selectedStudent = _students[e.RowIndex];
                DisplayStudent(_selectedStudent);

                ResetButtons();
                btnUpdate.Visible = true;
                btnDelete.Visible = true;
                EnableFields(false);
                _currentState = State.View;
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            ResetButtons();
            EnableFields(true);
            btnSave.Visible = true;
            btnCancel.Visible = true;
            _currentState = State.Update;
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            ResetButtons();
            if (_selectedStudent == null)
            {
                MessageBox.Show("No student selected to delete.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show("Are you sure to delete this student?", "Confirm Delete", MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    await _studentDataServices.DeleteStudentAsync(_selectedStudent.Id);
                    MessageBox.Show($"{_selectedStudent.Firstname} {_selectedStudent.Lastname} deleted.");
                    ClearFields();
                    var students = await _studentDataServices.GetAllStudentsAsync();
                    DisplayStudents(students);
                }
                catch (Exception)
                {
                    MessageBox.Show("An error occurred while deleting the student.");
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ClearFields();
            ResetButtons();
            EnableFields(true);
            btnSave.Visible = true;
            btnCancel.Visible = true;
            _currentState = State.Add;
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (!validateFields()) return;
            
            if (_currentState == State.Add) await AddNewStudent();
            
                
            
            else if (_currentState == State.Update) await EditStudent();
            
            
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ResetButtons();
            ClearFields();
        }

        private void PopulateAcadLevelDropDown(List<AcadLevelModel> acadLevels)
        {
            cmbAcadLevel.DataSource = acadLevels;
            cmbAcadLevel.DisplayMember = "Name";
            cmbAcadLevel.ValueMember = "Id";
        }

        private void PopulateSectionDropDown(List<SectionModel> sections)
        {
            cmbSection.DataSource = sections;
            cmbSection.DisplayMember = "Name";
            cmbSection.ValueMember = "Id";
        }

        private void DisplayStudents(List<StudentModel> students)
        {
            _students = students;
            dataGridView1.Rows.Clear();

            foreach (var student in _students)
            {
                dataGridView1.Rows.Add(
                    student.Id,
                    student.Lastname,
                    student.Firstname,
                    student.Middlename,
                    NormalizeGender(student.Gender),
                    student.AcadLevelId,
                    student.SectionId,
                    student.Email
                );
            }
        }
        private async Task<List<StudentModel>> searchStudentsAsync(string search)
        {
            return await _studentDataServices.SearchStudentsAsync(search);
        }
        private async Task AddNewStudent()
        {
            try
            {
                var student = new StudentModel
                {
                    Lastname = txtLastname.Text,
                    Firstname = txtFirstname.Text,
                    Middlename = txtMiddlename.Text,
                    Gender = !string.IsNullOrEmpty(cmbGender.Text)
                        ? cmbGender.Text.Substring(0, 1).ToUpper()
                        : string.Empty,
                    AcadLevelId = (int)cmbAcadLevel.SelectedValue,
                    SectionId = (int)cmbSection.SelectedValue,
                    Email = txtEmail.Text
                };
                NormalizeStudentModel(student);
                await _studentDataServices.AddStudentAsync(student);
                MessageBox.Show($"{student.Firstname} {student.Lastname} added successfully.");
                var students = await _studentDataServices.GetAllStudentsAsync();
                DisplayStudents(students);
            }
            catch (Exception)
            {
                MessageBox.Show("An error occurred while adding the student.");
            }
        }
        private async Task EditStudent()
        {
            try
            {
                if (_selectedStudent == null)
                {
                    MessageBox.Show("No student selected for update.");
                    return;
                }

                var genderCode = !string.IsNullOrEmpty(cmbGender.Text)
                        ? cmbGender.Text.Substring(0, 1).ToUpper()
                        : "";

                _selectedStudent.Lastname = txtLastname.Text;
                _selectedStudent.Firstname = txtFirstname.Text;
                _selectedStudent.Middlename = txtMiddlename.Text;
                _selectedStudent.Gender = genderCode;
                _selectedStudent.AcadLevelId = (int)cmbAcadLevel.SelectedValue;
                _selectedStudent.SectionId = (int)cmbSection.SelectedValue;
                _selectedStudent.Email = txtEmail.Text;
                NormalizeStudentModel(_selectedStudent);
                await _studentDataServices.UpdateStudentAsync(_selectedStudent);
                MessageBox.Show($"{_selectedStudent.Firstname} {_selectedStudent.Lastname} updated successfully.");
                var students = await _studentDataServices.GetAllStudentsAsync();
                DisplayStudents(students);
            }
            catch (Exception)
            {
                MessageBox.Show("An error occurred while updating the student.");
            }
        }
        private void DisplayStudent(StudentModel student)
        {
            txtLastname.Text = student.Lastname;
            txtFirstname.Text = student.Firstname;
            txtMiddlename.Text = student.Middlename;
            cmbGender.Text = NormalizeGender(student.Gender) ?? student.Gender;
            cmbAcadLevel.SelectedValue = student.AcadLevelId;
            cmbSection.SelectedValue = student.SectionId;
            txtEmail.Text = student.Email;
        }
        private void ResetButtons()
        {
            btnAdd.Visible = true;
            btnUpdate.Visible = false;
            btnDelete.Visible = false;
            btnSave.Visible = false;
            btnCancel.Visible = false;
        }
        private void EnableFields(bool enable)
        {
            txtLastname.Enabled = enable;
            txtFirstname.Enabled = enable;
            txtMiddlename.Enabled = enable;
            cmbGender.Enabled = enable;
            cmbAcadLevel.Enabled = enable;
            cmbSection.Enabled = enable;
            txtEmail.Enabled = enable;
        }
        private void ClearFields()
        {
            txtLastname.Text = string.Empty;
            txtFirstname.Text = string.Empty;
            txtMiddlename.Text = string.Empty;
            cmbGender.SelectedIndex = -1;
            cmbAcadLevel.SelectedIndex = -1;
            cmbSection.SelectedIndex = -1;
            txtEmail.Text = string.Empty;
        }
        private void NormalizeStudentModel(StudentModel student)
        {
            student.Lastname = student.Lastname?.Trim();
            student.Firstname = student.Firstname?.Trim();
            student.Middlename = student.Middlename?.Trim();
            student.Gender = student.Gender?.Trim();
            student.Email = student.Email?.Trim();
        }
        private string NormalizeGender(string gender)
        {
            if (string.IsNullOrWhiteSpace(gender)) return null;
            var g = gender.Trim().ToUpper();
            if (g == "M" || g == "MALE") return "Male";
            if (g == "F" || g == "FEMALE") return "Female";
            if (g == "T" || g == "TRANSFORMER" || g == "OTHER") return "Other";
            // If already a display word, capitalize first letter
            return char.ToUpper(g[0]) + g.Substring(1).ToLower();
        }
        private bool validateFields()
        {
            if (string.IsNullOrWhiteSpace(txtLastname.Text) ||
            string.IsNullOrWhiteSpace(txtFirstname.Text) ||
            string.IsNullOrWhiteSpace(txtMiddlename.Text) ||
            string.IsNullOrWhiteSpace(txtEmail.Text) ||
            cmbGender.SelectedIndex < 0 ||
            cmbAcadLevel.SelectedIndex < 0 ||
            cmbSection.SelectedIndex < 0)
            {
                MessageBox.Show("Please fill in all required fields.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }
}
