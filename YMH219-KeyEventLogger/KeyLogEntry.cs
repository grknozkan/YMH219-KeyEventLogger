using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace KeyEventLogger
{
    /// <summary>
    /// Klavye olaylarını temsil eden kayıt sınıfı.
    /// </summary>
    public class KeyLogEntry
    {
        public string KeyName { get; set; }
        public string EventType { get; set; }  // KeyDown, KeyUp, KeyPress
        public DateTime Timestamp { get; set; }
        public Keys KeyCode { get; set; }
        public bool IsCtrl { get; set; }
        public bool IsAlt { get; set; }
        public bool IsShift { get; set; }

        public KeyLogEntry(string keyName, string eventType, Keys keyCode,
                           bool ctrl, bool alt, bool shift)
        {
            KeyName = keyName;
            EventType = eventType;
            Timestamp = DateTime.Now;
            KeyCode = keyCode;
            IsCtrl = ctrl;
            IsAlt = alt;
            IsShift = shift;
        }

        /// <summary>
        /// Modifier tuş kombinasyonunu metin olarak döndürür.
        /// </summary>
        public string ModifierText
        {
            get
            {
                var parts = new List<string>();
                if (IsCtrl) parts.Add("Ctrl");
                if (IsAlt) parts.Add("Alt");
                if (IsShift) parts.Add("Shift");
                return parts.Count > 0 ? string.Join("+", parts) + "+" : "";
            }
        }

        /// <summary>
        /// ListBox'ta gösterilecek biçimlendirilmiş metin.
        /// </summary>
        public override string ToString()
        {
            string modText = string.IsNullOrEmpty(ModifierText) ? "" : $"[{ModifierText}]";
            return $"[{Timestamp:HH:mm:ss.fff}]  {EventType,-9}  {modText}{KeyName}";
        }

        /// <summary>
        /// Dosyaya yazılacak biçimlendirilmiş metin.
        /// </summary>
        public string ToFileString()
        {
            return $"{Timestamp:yyyy-MM-dd HH:mm:ss.fff} | {EventType,-9} | {ModifierText}{KeyName}";
        }
    }
}
