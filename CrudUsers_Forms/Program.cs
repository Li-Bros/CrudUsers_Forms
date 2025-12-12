using CrudUsers_Forms.Data;
using CrudUsers_Forms.Views;

namespace CrudUsers_Forms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            DatabaseInitializer.EnsureStoredProcedures();
            Application.Run(new LoginForm());
        }
    }
}
