using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalaxyMatchGUI.Models
{
    public class ChatMessage
    {
        public string Text { get; set; } = string.Empty;
        public bool IsIncoming { get; set; } // true = received, false = sent
    }
}
