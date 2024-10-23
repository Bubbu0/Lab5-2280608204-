using Lab05.BUS;
using Lab05.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab05
{
    public partial class frmRegister : Form
    {
        private readonly StudentService studentService = new StudentService();
        private readonly FacultyService facultyService = new FacultyService();
        private readonly MajorService majorService = new MajorService();
        public frmRegister()
        {
            InitializeComponent();
        }

        private void frmRegister_Load(object sender, EventArgs e)
        {
            try
            {
                var listFacultys = facultyService.GetAll();
                FillFalcultyCombobox(listFacultys);
                cmbFaculty.SelectedIndexChanged += cmbFaculty_SelectedIndexChanged;
                dgvStudent.DataError += dgvStudent_DataError;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FillFalcultyCombobox(List<Faculty> listFacultys)
        {
            this.cmbFaculty.DataSource = listFacultys;
            this.cmbFaculty.DisplayMember = "FacultyName";
            this.cmbFaculty.ValueMember = "FacultyID";
        }
        private void FillMajorCombobox(List<Major> listMajors)
        {
            this.cmbMajor.DataSource = listMajors;
            this.cmbMajor.DisplayMember = "MajorName";  // Hiển thị tên chuyên ngành
            this.cmbMajor.ValueMember = "MajorID";  // Giá trị là ID của chuyên ngành
        }


        private void cmbFaculty_SelectedIndexChanged(object sender, EventArgs e)
        {
            Faculty selectedFaculty = cmbFaculty.SelectedItem as Faculty;
            if (selectedFaculty != null)
            {
                var listMajor = majorService.GetAllByFaculty(selectedFaculty.FacultyID);
                FillMajorCombobox(listMajor);
                var listStudents = studentService.GetAllHasNoMajor(selectedFaculty.FacultyID);
                BindGrid(listStudents);
            }
        }

        private void BindGrid(List<Student> listStudents)
        {
            dgvStudent.Rows.Clear();


            foreach (var item in listStudents)
            {
                int index = dgvStudent.Rows.Add();
                dgvStudent.Rows[index].Cells[1].Value = item.StudentID;
                dgvStudent.Rows[index].Cells[2].Value = item.FullName;
                if (item.Faculty != null)
                {
                    dgvStudent.Rows[index].Cells[3].Value = item.Faculty.FacultyName;
                    dgvStudent.Rows[index].Cells[4].Value = item.AverageScore + "";
                }
                if (item.MajorID != null)
                {
                    dgvStudent.Rows[index].Cells[5].Value = item.Major.Name + "";
                }
            }
        }

        private void dgvStudent_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Ngăn lỗi FormatException khi có vấn đề về chuyển đổi dữ liệu trong DataGridView
            e.ThrowException = false;
        }



        private void btnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                // Lấy chuyên ngành được chọn
                Major selectedMajor = cmbMajor.SelectedItem as Major;
                if (selectedMajor == null)
                {
                    MessageBox.Show("Vui lòng chọn chuyên ngành để đăng ký!");
                    return;
                }

                // Duyệt qua các sinh viên đã chọn trong DataGridView
                foreach (DataGridViewRow row in dgvStudent.Rows)
                {
                    if (Convert.ToBoolean(row.Cells["Select"].Value))  // Cột checkbox "Select" để chọn sinh viên
                    {
                        // Lấy mã sinh viên từ DataGridView
                        string studentID = row.Cells[1].Value.ToString();  // Cột StudentID là cột thứ 1

                        // Tìm sinh viên theo StudentID
                        var student = studentService.FindById(studentID);

                        if (student != null)
                        {
                            // Cập nhật MajorID cho sinh viên
                            student.MajorID = selectedMajor.MajorID;
                            studentService.InsertUpdate(student);
                        }
                    }
                }

                MessageBox.Show("Đăng ký chuyên ngành thành công!");

                // Sau khi đăng ký, tải lại danh sách sinh viên chưa có chuyên ngành
                Faculty selectedFaculty = cmbFaculty.SelectedItem as Faculty;
                if (selectedFaculty != null)
                {
                    var listStudents = studentService.GetAllHasNoMajor(selectedFaculty.FacultyID);
                    BindGrid(listStudents);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra khi đăng ký chuyên ngành: " + ex.Message);
            }
        }

        private void LoadStudentsWithoutMajor()
        {
            Faculty selectedFaculty = cmbFaculty.SelectedItem as Faculty;
            if (selectedFaculty != null)
            {
                var listStudents = studentService.GetAllHasNoMajor(selectedFaculty.FacultyID);
                BindGrid(listStudents);
            }
        }

    }
}
