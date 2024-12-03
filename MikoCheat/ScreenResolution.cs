using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MikoCheat
{
    public class ScreenResolution
    {
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        public static (int width, int height) GetResolution()
        {
            int screenWidth = GetSystemMetrics(0);  // SM_CXSCREEN
            int screenHeight = GetSystemMetrics(1); // SM_CYSCREEN
            return (screenWidth, screenHeight);
        }
    }
}
