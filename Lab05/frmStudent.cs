    using Lab05.BUS;
    using Lab05.DAL.Entities;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    namespace Lab05
{
    public partial class frmStudent : Form
    {
        private readonly StudentService studentService = new StudentService();
        private readonly FacultyService facultyService = new FacultyService();
        public frmStudent()
        {
            InitializeComponent();
        }

        private void dgvStudents_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Kiểm tra nếu dòng được chọn hợp lệ
            if (e.RowIndex >= 0)
            {
                // Lấy dòng được chọn
                DataGridViewRow selectedRow = dgvStudents.Rows[e.RowIndex];

                // Hiển thị thông tin từ dòng vào các TextBox và ComboBox
                txtStudentID.Text = selectedRow.Cells[0].Value.ToString();  // Mã sinh viên
                txtFullName.Text = selectedRow.Cells[1].Value.ToString();   // Tên sinh viên
                txtAverageScore.Text = selectedRow.Cells[3].Value.ToString(); // Điểm trung bình

                // Chọn giá trị khoa (Faculty) trong ComboBox
                if (selectedRow.Cells[2].Value != null)
                {
                    cmbFaculty.SelectedIndex = cmbFaculty.FindStringExact(selectedRow.Cells[2].Value.ToString());
                }

                string studentID = txtStudentID.Text;
                var student = studentService.FindById(studentID);

                if (student != null)
                {
                    // Hiển thị avatar (nếu có)
                    ShowAvatar(student.Avatar);  // Lấy avatar từ đối tượng sinh viên trong database
                }
            }
        }

        private void frmStudent_Load(object sender, EventArgs e)
        {
            try
            {
                setGridViewStyle(dgvStudents);
                var listFacultys = facultyService.GetAll();
                var listStudents = studentService.GetAll();
                FillFalcultyCombobox(listFacultys);
                BindGrid(listStudents);

                // Đăng ký sự kiện CellClick cho DataGridView
                dgvStudents.CellClick += dgvStudents_CellContentClick;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Hàm binding list dữ liệu khoa vào combobox có tên hiện thị là tên khoa,giá trị là Mã khoa

        private void FillFalcultyCombobox(List<Faculty> listFacultys)
        {
            listFacultys.Insert(0, new Faculty());
            this.cmbFaculty.DataSource = listFacultys;
            this.cmbFaculty.DisplayMember = "FacultyName";
            this.cmbFaculty.ValueMember = "FacultyID";
        }
        //Hàm binding gridView từ list sinh viên
        private void BindGrid(List<Student> listStudent)
        {
            dgvStudents.Rows.Clear();
            foreach (var item in listStudent)
            {
                int index = dgvStudents.Rows.Add();
                dgvStudents.Rows[index].Cells[0].Value = item.StudentID;
                dgvStudents.Rows[index].Cells[1].Value = item.FullName;
                if (item.Faculty != null)
                    dgvStudents.Rows[index].Cells[2].Value =
                    item.Faculty.FacultyName;
                dgvStudents.Rows[index].Cells[3].Value = item.AverageScore + "";
                if (item.MajorID != null)
                    dgvStudents.Rows[index].Cells[4].Value = item.Major.Name + "";
                
            }
        }
        private void ShowAvatar(string ImageName)
        {
            if (string.IsNullOrEmpty(ImageName))
            {
                picAvatar.Image = null;
            }
            else
            {
                string parentDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                string imagePath = Path.Combine(parentDirectory, "Images", ImageName);

                if (File.Exists(imagePath))
                {
                    picAvatar.Image = Image.FromFile(imagePath);
                    picAvatar.Refresh();
                }
                else
                {
                    picAvatar.Image = null;  // Nếu không tìm thấy file ảnh, để trống PictureBox
                }
            }
        }

        public void setGridViewStyle(DataGridView dgview)
        {
            dgview.BorderStyle = BorderStyle.None;
            dgview.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            dgview.CellBorderStyle =
            DataGridViewCellBorderStyle.SingleHorizontal;
            dgview.BackgroundColor = Color.White;
            dgview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void chkUnregisterMajor_CheckedChanged(object sender, EventArgs e)
        {
            var listStudents = new List<Student>();
            if (this.chkUnregisterMajor.Checked)
                listStudents = studentService.GetAllHasNoMajor();
            else
                listStudents = studentService.GetAll();
            BindGrid(listStudents);
        }

        private void btnAddOrUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra nếu thông tin nhập vào là hợp lệ
                if (string.IsNullOrEmpty(txtStudentID.Text) || string.IsNullOrEmpty(txtFullName.Text))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin sinh viên!");
                    return;
                }

                // Tạo mới hoặc cập nhật thông tin sinh viên
                Student student = new Student
                {
                    StudentID = txtStudentID.Text,
                    FullName = txtFullName.Text,
                    FacultyID = (cmbFaculty.SelectedItem as Faculty)?.FacultyID,
                    AverageScore = float.Parse(txtAverageScore.Text),  // Đảm bảo kiểm tra hợp lệ của dữ liệu
                    Avatar = GetAvatarName()   // Lấy tên ảnh từ chức năng ShowAvatar
                };

                // Kiểm tra sinh viên đã tồn tại chưa
                var existingStudent = studentService.FindById(student.StudentID);
                if (existingStudent != null)
                {
                    // Cập nhật sinh viên (Sử dụng InsertUpdate)
                    studentService.InsertUpdate(student);
                    MessageBox.Show("Cập nhật thông tin sinh viên thành công!");
                }
                else
                {
                    // Thêm mới sinh viên (Cũng sử dụng InsertUpdate)
                    studentService.InsertUpdate(student);
                    MessageBox.Show("Thêm sinh viên mới thành công!");
                }

                // Cập nhật lại danh sách sinh viên hiển thị
                BindGrid(studentService.GetAll());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message);
            }
        }


        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtStudentID.Text))
                {
                    MessageBox.Show("Vui lòng nhập hoặc chọn sinh viên cần xóa!");
                    return;
                }

                // Xác nhận trước khi xóa
                var confirmResult = MessageBox.Show("Bạn có chắc muốn xóa sinh viên này?",
                                             "Xác nhận xóa",
                                             MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    // Xóa sinh viên
                    studentService.Delete(txtStudentID.Text);
                    MessageBox.Show("Xóa sinh viên thành công!");

                    // Cập nhật lại danh sách sinh viên
                    BindGrid(studentService.GetAll());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra khi xóa sinh viên: " + ex.Message);
            }
        }






        private string GetAvatarName()
        {
            // Lấy tên hoặc đường dẫn ảnh từ PictureBox
            return picAvatar.ImageLocation != null ? Path.GetFileName(picAvatar.ImageLocation) : null;
        }

        private void đăngKýChuyênNgànhToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmRegister register = new frmRegister();
            register.ShowDialog();
        }

        private void btnSetAvartar_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"; // Lọc tệp hình ảnh
            openFileDialog.Title = "Chọn hình ảnh làm avatar";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedImagePath = openFileDialog.FileName;
                string fileExtension = Path.GetExtension(selectedImagePath);  // Lấy đuôi mở rộng của file (.jpg, .png, ...)
                string studentID = txtStudentID.Text;  // Mã sinh viên lấy từ TextBox

                if (string.IsNullOrEmpty(studentID))
                {
                    MessageBox.Show("Vui lòng nhập mã sinh viên trước khi chọn ảnh!");
                    return;
                }

                // Tên file ảnh được đặt theo dạng {studentID}.{typeFile}
                string imageName = $"{studentID}{fileExtension}";

                // Copy ảnh vào thư mục "Images" của dự án
                string targetDirectory = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "Images");
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory); // Tạo thư mục nếu chưa có
                }

                string targetImagePath = Path.Combine(targetDirectory, imageName);
                File.Copy(selectedImagePath, targetImagePath, true);  // Copy tệp vào thư mục Images

                // Cập nhật PictureBox để hiển thị ảnh đã chọn
                picAvatar.Image = Image.FromFile(targetImagePath);
                picAvatar.Refresh();

                // Lưu tên ảnh vào trường Avatar của sinh viên hiện tại (chỉ lưu tên ảnh, không lưu đường dẫn đầy đủ)
                //txtAvatarPath.Text = imageName;  // txtAvatarPath là một TextBox lưu tên ảnh (có thể không cần hiển thị)
                MessageBox.Show("Đã chọn ảnh làm avatar thành công!");
            }
        }



       

    }
}
