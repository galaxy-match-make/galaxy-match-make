using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalaxyMatchGUI.Services
{
    public static class ProfileManager
    {
        private static Profile? _currentProfile;

        public static Profile? CurrentProfile
        {
            get => _currentProfile;
            set => _currentProfile = value;
        }

        public static void Reset()
        {
            _currentProfile = null;
        }
    }
}