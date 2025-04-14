using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalaxyMatchGUI
{
    public static class SignalRIncomingMessageHandler
    {
        public static ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

        public static void MessageReader(string message)
        {
            Messages.Add(message);
        }
    }
}
