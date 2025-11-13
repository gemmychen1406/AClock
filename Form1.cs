using System;
using System.Drawing; 
using System.Media; 
using System.Windows.Forms; 

namespace AClock
{
    // Rất quan trọng: phải là partial class
    public partial class Form1 : Form
    
    {
        // Biến lưu trữ thời gian báo thức đã đặt
        private DateTime alarmTime;
        // Biến kiểm tra báo thức đã được đặt hay chưa (Tên đúng là isAlarmSet)
        private bool isAlarmSet = false;
        
        // BỔ SUNG: Khai báo SoundPlayer ở cấp độ Class để kiểm soát nó
        private System.Media.SoundPlayer alarmPlayer = null; 

        public Form1()
        {
            // InitializeComponent được định nghĩa trong Form1.Designer.cs
            InitializeComponent();
            
            // Khởi tạo trạng thái ban đầu của NotifyIcon
            notifyIcon1.Visible = true;
            
            // Bắt đầu Timer để cập nhật thời gian
            timer1.Start();
        }
        
        // --- 1. Xử lý Timer (Cập nhật thời gian & Kiểm tra báo thức) ---
        private void timer1_Tick(object sender, EventArgs e)
        {
            lblCurrentTime.Text = DateTime.Now.ToLongTimeString();

            if (isAlarmSet)
            {
                if (DateTime.Now.Hour == alarmTime.Hour && DateTime.Now.Minute == alarmTime.Minute)
                {
                    TriggerAlarm();
                }
            }
        }

        // --- 2. Xử lý Đặt Báo thức ---
        private void btnSetAlarm_Click(object sender, EventArgs e)
        {
            alarmTime = dtpAlarmTime.Value;
            isAlarmSet = true;
            
            btnSetAlarm.Text = "Đã Đặt: " + alarmTime.ToShortTimeString();
            btnSetAlarm.Enabled = false; 
            
            // Thông báo ngắn gọn qua System Tray
            notifyIcon1.ShowBalloonTip(5000, "Báo thức Đã Đặt", 
                "Báo thức sẽ kêu lúc: " + alarmTime.ToShortTimeString(), 
                ToolTipIcon.Info);
        }

        // --- 3. Kích hoạt Báo thức ---
        private void TriggerAlarm()
        {
            // Sửa lỗi đánh máy: dùng isAlarmSet
            isAlarmSet = false;
            btnSetAlarm.Enabled = true;
            btnSetAlarm.Text = "Đặt Báo thức";

            // Phát âm thanh
            try
            {
                string alarmSoundPath = "alarm.wav";

                // Sử dụng biến cấp Class
                alarmPlayer = new SoundPlayer(alarmSoundPath);

                // THAY ĐỔI: Sử dụng PlayLooping() để âm thanh lặp lại
                alarmPlayer.PlayLooping();
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Lỗi phát file âm thanh: " + ex.Message, "Lỗi Âm Thanh", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Hiển thị thông báo Pop-up
            // Ứng dụng sẽ TẠM DỪNG ở đây cho đến khi người dùng nhấn OK
            MessageBox.Show("Đã đến giờ báo thức!", "BÁO THỨC!", 
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            
            // DỪNG âm thanh sau khi người dùng bấm OK
            if (alarmPlayer != null)
            {
                alarmPlayer.Stop();
                alarmPlayer.Dispose(); // Giải phóng tài nguyên
                alarmPlayer = null;
            }
        }

        // --- 4. Quản lý Chạy Ngầm (System Tray) ---
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Nếu người dùng đóng cửa sổ (nhấn X)
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Ngăn không cho Form thoát
                this.Hide();     // Ẩn Form đi
                notifyIcon1.ShowBalloonTip(2000, "Chạy Ngầm", "Ứng dụng đang chạy ẩn.", ToolTipIcon.Info);
            }
        }

        // Double click vào icon sẽ mở cửa sổ
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        // Sự kiện click mục "Mở" từ Context Menu
        private void Open_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        // Sự kiện click mục "Thoát" từ Context Menu
        private void Exit_Click(object sender, EventArgs e)
        {
            // Tắt Timer và ẩn NotifyIcon trước khi thoát hẳn
            timer1.Stop();
            notifyIcon1.Visible = false;
            Application.Exit();
        }
    }
}