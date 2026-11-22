using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeyEventLogger
{
    /// <summary>
    /// Ana form — tüm UI olayları ve loglama iş akışı burada yönetilir.
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly KeyLogger _logger = new();

        // ---- UI Kontrolleri ----
        private RichTextBox rtbLog = null!;
        private Button btnStart = null!;
        private Button btnStop = null!;
        private Button btnClear = null!;
        private Button btnSave = null!;
        private Button btnStats = null!;
        private Label lblStatus = null!;
        private Label lblCount = null!;
        private Panel pnlIndicator = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel tsslInfo = null!;

        // Filtre seçenekleri
        private CheckBox chkKeyDown = null!;
        private CheckBox chkKeyUp = null!;
        private CheckBox chkKeyPress = null!;

        public MainForm()
        {
            InitializeComponent();
            SetupLogger();
        }

        // ========================================================
        //  InitializeComponent  (Designer yerine kod ile)
        // ========================================================
        private void InitializeComponent()
        {
            Text = "YMH219 — Key Event Logger";
            Size = new Size(820, 600);
            MinimumSize = new Size(640, 480);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.WhiteSmoke;
            Font = new Font("Segoe UI", 9f);
            KeyPreview = true; // Form, tuş olaylarını alabilsin

            // ---- Üst Panel ----
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(20, 20, 20),
                Padding = new Padding(8, 8, 8, 8)
            };

            // Durum göstergesi (yeşil/kırmızı daire)
            pnlIndicator = new Panel
            {
                Size = new Size(20, 20),
                Location = new Point(16, 18),
                BackColor = Color.Gray
            };
            MakeCircular(pnlIndicator);

            lblStatus = new Label
            {
                Text = "Durduruldu",
                Location = new Point(44, 18),
                AutoSize = true,
                ForeColor = Color.Silver,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };

            lblCount = new Label
            {
                Text = "Kayıt: 0",
                Location = new Point(170, 18),
                AutoSize = true,
                ForeColor = Color.Silver,
                Font = new Font("Segoe UI", 9f)
            };

            pnlTop.Controls.AddRange(new Control[] { pnlIndicator, lblStatus, lblCount });

            // ---- Butonlar ----
            var pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 140,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(6),
                BackColor = Color.FromArgb(25, 25, 25),
                WrapContents = false,
                AutoScroll = false
            };

            btnStart = CreateButton("▶  Başlat", Color.FromArgb(0, 153, 76));
            btnStop  = CreateButton("■  Durdur", Color.FromArgb(180, 50, 50));
            btnClear = CreateButton("🗑  Temizle", Color.FromArgb(80, 80, 80));
            btnSave  = CreateButton("💾  Kaydet", Color.FromArgb(50, 100, 160));
            btnStats = CreateButton("📊  İstatistik", Color.FromArgb(100, 60, 140));

            btnStop.Enabled = false;

            pnlButtons.Controls.AddRange(
                new Control[] { btnStart, btnStop, btnClear, btnSave, btnStats });

            // ---- Filtre CheckBox'ları ----
            chkKeyDown = new CheckBox { Text = "KeyDown", Checked = true,
                ForeColor = Color.LightGreen, BackColor = Color.Transparent,
                Width = 120, Margin = new Padding(0, 8, 0, 0) };
            chkKeyUp = new CheckBox { Text = "KeyUp", Checked = true,
                ForeColor = Color.Orange, BackColor = Color.Transparent,
                Width = 120, Margin = new Padding(0, 2, 0, 0) };
            chkKeyPress = new CheckBox { Text = "KeyPress", Checked = true,
                ForeColor = Color.SkyBlue, BackColor = Color.Transparent,
                Width = 120, Margin = new Padding(0, 2, 0, 0) };

            pnlButtons.Controls.AddRange(
                new Control[] { chkKeyDown, chkKeyUp, chkKeyPress });

            // ---- Log Alanı ----
            rtbLog = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 9.5f),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                WordWrap = false
            };

            // ---- Status Strip ----
            statusStrip = new StatusStrip
            {
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.Silver
            };
            tsslInfo = new ToolStripStatusLabel("Hazır — Başlatmak için Başlat butonuna basın.")
            {
                ForeColor = Color.Silver
            };
            statusStrip.Items.Add(tsslInfo);

            // ---- Layout ----
            var pnlMain = new Panel { Dock = DockStyle.Fill };
            pnlMain.Controls.Add(rtbLog);
            pnlMain.Controls.Add(pnlButtons);

            Controls.Add(pnlMain);
            Controls.Add(pnlTop);
            Controls.Add(statusStrip);

            // ---- Olay Bağlama ----
            btnStart.Click += BtnStart_Click;
            btnStop.Click  += BtnStop_Click;
            btnClear.Click += BtnClear_Click;
            btnSave.Click  += BtnSave_Click;
            btnStats.Click += BtnStats_Click;

            KeyDown  += MainForm_KeyDown;
            KeyUp    += MainForm_KeyUp;
            KeyPress += MainForm_KeyPress;
        }

        // ========================================================
        //  Yardımcı: buton fabrika metodu
        // ========================================================
        private static Button CreateButton(string text, Color back)
        {
            return new Button
            {
                Text = text,
                Width = 120,
                Height = 36,
                Margin = new Padding(0, 4, 0, 0),
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
        }

        /// <summary>Panel'i görsel olarak dairesel yapar (Region).</summary>
        private static void MakeCircular(Panel p)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(0, 0, p.Width, p.Height);
            p.Region = new Region(path);
        }

        // ========================================================
        //  Logger kurulumu
        // ========================================================
        private void SetupLogger()
        {
            _logger.EntryAdded += (_, entry) =>
            {
                // Cross-thread güvenli UI güncellemesi
                if (InvokeRequired)
                    Invoke(() => AppendEntry(entry));
                else
                    AppendEntry(entry);
            };
        }

        // ========================================================
        //  UI güncelleme
        // ========================================================
        private void AppendEntry(KeyLogEntry entry)
        {
            // Renk seçimi: KeyDown=yeşil, KeyUp=turuncu, KeyPress=mavi
            Color color = entry.EventType switch
            {
                "KeyDown"  => Color.LightGreen,
                "KeyUp"    => Color.Orange,
                "KeyPress" => Color.SkyBlue,
                _          => Color.White
            };

            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = color;
            rtbLog.AppendText(entry.ToString() + Environment.NewLine);
            rtbLog.SelectionColor = rtbLog.ForeColor;
            rtbLog.ScrollToCaret();

            lblCount.Text = $"Kayıt: {_logger.Entries.Count}";
            tsslInfo.Text = $"Son: {entry.EventType}  →  {entry.KeyName}  |  {entry.Timestamp:HH:mm:ss.fff}";
        }

        private void UpdateIndicator(bool running)
        {
            pnlIndicator.BackColor = running ? Color.LimeGreen : Color.Gray;
            lblStatus.Text = running ? "Kayıt Aktif" : "Durduruldu";
            lblStatus.ForeColor = running ? Color.LimeGreen : Color.Silver;
        }

        // ========================================================
        //  Buton Olayları
        // ========================================================
        private void BtnStart_Click(object? sender, EventArgs e)
        {
            _logger.StartLogging();
            btnStart.Enabled = false;
            btnStop.Enabled  = true;
            chkKeyDown.Enabled = false;
            chkKeyUp.Enabled   = false;
            chkKeyPress.Enabled = false;
            UpdateIndicator(true);
            tsslInfo.Text = "Kayıt başlatıldı. Tuşlara basın...";
            this.Focus(); // Klavye odağının formda olduğundan emin ol
        }

        private void BtnStop_Click(object? sender, EventArgs e)
        {
            _logger.StopLogging();
            btnStart.Enabled = true;
            btnStop.Enabled  = false;
            chkKeyDown.Enabled = true;
            chkKeyUp.Enabled   = true;
            chkKeyPress.Enabled = true;
            UpdateIndicator(false);
            tsslInfo.Text = $"Kayıt durduruldu. Toplam tuş basımı: {_logger.TotalKeyPresses}";
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            if (_logger.Entries.Count == 0) return;

            var result = MessageBox.Show(
                "Tüm kayıtlar silinecek. Emin misiniz?",
                "Temizle", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _logger.ClearAll();
                rtbLog.Clear();
                lblCount.Text = "Kayıt: 0";
                tsslInfo.Text = "Kayıtlar temizlendi.";
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (_logger.Entries.Count == 0)
            {
                MessageBox.Show("Kaydedilecek kayıt yok.", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Metin Dosyası (*.txt)|*.txt|Tüm Dosyalar (*.*)|*.*",
                FileName = $"keylog_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                Title = "Kayıtları Kaydet"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                bool ok = _logger.SaveToFile(sfd.FileName, out string err);
                if (ok)
                {
                    MessageBox.Show($"Kayıtlar başarıyla kaydedildi:\n{sfd.FileName}",
                        "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    tsslInfo.Text = $"Dosyaya kaydedildi: {sfd.FileName}";
                }
                else
                {
                    MessageBox.Show($"Dosya kaydedilirken hata:\n{err}",
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnStats_Click(object? sender, EventArgs e)
        {
            using var statsForm = new StatsForm(_logger);
            statsForm.ShowDialog(this);
        }

        // ========================================================
        //  Klavye Olayları (Yalnızca form odaktayken)
        // ========================================================
        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!chkKeyDown.Checked) return;
            var entry = new KeyLogEntry(
                e.KeyCode.ToString(), "KeyDown", e.KeyCode,
                e.Control, e.Alt, e.Shift);
            _logger.RecordEntry(entry);
        }

        private void MainForm_KeyUp(object? sender, KeyEventArgs e)
        {
            if (!chkKeyUp.Checked) return;
            var entry = new KeyLogEntry(
                e.KeyCode.ToString(), "KeyUp", e.KeyCode,
                e.Control, e.Alt, e.Shift);
            _logger.RecordEntry(entry);
        }

        private void MainForm_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!chkKeyPress.Checked) return;
            if (e.KeyChar == '\0') return; // Özel karakterleri atla
            string keyName = char.IsControl(e.KeyChar)
                ? $"[Char:{(int)e.KeyChar}]"
                : e.KeyChar.ToString();

            var entry = new KeyLogEntry(keyName, "KeyPress", Keys.None, false, false, false);
            _logger.RecordEntry(entry);
        }
    }
}
