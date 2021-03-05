using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MachineryProcessingDemo;
using QualityCheckDemo.Forms;

namespace QualityCheckDemo
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UserLoginForm());
            // Application.Run(new MainPanel(1, " ",""));

        }
    }
}
