using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KeyEventLogger
{
    /// <summary>
    /// Tuş kullanım istatistiklerini gösteren ikincil form.
    /// </summary>
    public class StatsForm : Form
    {
        private readonly KeyLogger _logger;

        public StatsForm(KeyLogger logger)
        {
            _logger = logger;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "Tuş İstatistikleri";
            Size = new Size(480, 420);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.WhiteSmoke;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // Özet etiketleri
            var lblTitle = new Label
            {
                Text = "📊  Tuş Kullanım İstatistikleri",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.SkyBlue,
                Location = new Point(12, 12),
                AutoSize = true
            };

            var lblTotal = new Label
            {
                Text = $"Toplam tuş basımı: {_logger.TotalKeyPresses}",
                Location = new Point(12, 42),
                AutoSize = true,
                ForeColor = Color.Silver
            };

            var lblUnique = new Label
            {
                Text = $"Benzersiz tuş sayısı: {_logger.Entries.Where(e => e.EventType == "KeyDown").Select(e => e.KeyName).Distinct().Count()}",
                Location = new Point(12, 62),
                AutoSize = true,
                ForeColor = Color.Silver
            };

            // En sık kullanılan tuşlar listesi
            var lblTopTitle = new Label
            {
                Text = "En Sık Kullanılan Tuşlar (İlk 10):",
                Location = new Point(12, 92),
                AutoSize = true,
                ForeColor = Color.Gold
            };

            var lstTop = new ListBox
            {
                Location = new Point(12, 114),
                Size = new Size(440, 220),
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 9.5f),
                BorderStyle = BorderStyle.FixedSingle
            };

            var topKeys = _logger.GetTopKeys(10).ToList();
            int rank = 1;
            foreach (var (key, count) in topKeys)
            {
                int pct = _logger.TotalKeyPresses > 0
                    ? (int)((double)count / _logger.TotalKeyPresses * 100)
                    : 0;
                lstTop.Items.Add($"#{rank++,2}  {key,-15}  {count,5} kez  ({pct}%)");
            }

            if (!topKeys.Any())
                lstTop.Items.Add("Henüz kayıt yok.");

            var btnClose = new Button
            {
                Text = "Kapat",
                Location = new Point(370, 345),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };

            Controls.AddRange(new Control[]
                { lblTitle, lblTotal, lblUnique, lblTopTitle, lstTop, btnClose });
        }
    }
}
