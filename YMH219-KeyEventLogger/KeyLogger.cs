using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace KeyEventLogger
{
    /// <summary>
    /// Klavye olay kayıtlarını yöneten sınıf.
    /// Kayıt başlatma/durdurma, istatistik ve dosyaya yazma işlemlerini üstlenir.
    /// </summary>
    public class KeyLogger
    {
        private readonly List<KeyLogEntry> _entries = new();
        private bool _isLogging = false;

        // ---- Ayarlar ----
        /// <summary>Listedeki maksimum kayıt sayısı (0 = sınırsız).</summary>
        public int MaxEntries { get; set; } = 5000;

        // ---- Özellikler ----
        public bool IsLogging => _isLogging;
        public int TotalKeyPresses => _entries.Count(e => e.EventType == "KeyDown");
        public IReadOnlyList<KeyLogEntry> Entries => _entries.AsReadOnly();

        // ---- Olaylar ----
        public event EventHandler<KeyLogEntry>? EntryAdded;

        // ---- Kayıt Yönetimi ----
        public void StartLogging()
        {
            _isLogging = true;
        }

        public void StopLogging()
        {
            _isLogging = false;
        }

        public void ClearAll()
        {
            _entries.Clear();
        }

        /// <summary>
        /// Yeni bir tuş olayını listeye ekler; loglama aktifse kaydeder.
        /// </summary>
        public void RecordEntry(KeyLogEntry entry)
        {
            if (!_isLogging) return;
            // Maksimum kayıt sınırı aşıldıysa en eskiyi sil
            if (MaxEntries > 0 && _entries.Count >= MaxEntries)
                _entries.RemoveAt(0);
            _entries.Add(entry);
            EntryAdded?.Invoke(this, entry);
        }

        // ---- İstatistik ----
        /// <summary>
        /// En sık kullanılan N tuşu döndürür.
        /// </summary>
        public IEnumerable<(string Key, int Count)> GetTopKeys(int topN = 10)
        {
            return _entries
                .Where(e => e.EventType == "KeyDown")
                .GroupBy(e => e.KeyName)
                .Select(g => (Key: g.Key, Count: g.Count()))
                .OrderByDescending(x => x.Count)
                .Take(topN);
        }

        // ---- Dosya İşlemleri ----
        /// <summary>
        /// Kayıtları belirtilen dosya yoluna yazar; başarısız olursa false döner.
        /// </summary>
        public bool SaveToFile(string filePath, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using var writer = new StreamWriter(filePath, append: false,
                    System.Text.Encoding.UTF8);
                writer.WriteLine("===== YMH219 Key Event Logger — Kayıt Dosyası =====");
                writer.WriteLine($"Oluşturulma: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"Toplam Kayıt: {_entries.Count}");
                writer.WriteLine(new string('-', 60));
                foreach (var entry in _entries)
                    writer.WriteLine(entry.ToFileString());
                writer.WriteLine(new string('=', 60));
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
