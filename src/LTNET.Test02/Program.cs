namespace LTNET.Test02;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Control.CheckForIllegalCrossThreadCalls = true;
        Application.Run(new frmExerciseLauncher());
    }
}
