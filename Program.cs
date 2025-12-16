namespace SkillManagementSystem
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]

        // In Program.cs, before Application.Run():


        static void Main()
        {
          
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}