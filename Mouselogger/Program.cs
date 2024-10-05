using System;
using System.Windows.Forms;

namespace Mouselogger
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            var mh = new MouseHook();
            mh.OnMouseActivity += Mh_OnMouseActivity;
            Application.Run();
        }

        private static void Mh_OnMouseActivity(object sender, MouseEventArgs e)
        {
            Console.WriteLine($"{e.X}, {e.Y}, {e.Button}");
        }
    }
}
