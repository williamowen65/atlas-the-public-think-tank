using System.Diagnostics;

namespace atlas_the_public_think_tank.Email.Infrastructure
{
    public static class MailHogHelper
    {

        /// <summary>
        /// This starts up the local email server for testing
        /// </summary>
        /// <param name="app"></param>
        public static void StartMailHogIfDevelopment(WebApplication app)
        {
            if (!app.Environment.IsDevelopment()) 
                return;

            Console.WriteLine("Turning on development email server: http://localhost:8025/. When done developing, stop app with ctrl+c to also stop this service, or stop it manually in task manager.");

            Process? mailHogProcess = null;
            var exePath = Path.Combine(app.Environment.ContentRootPath, "..", "development-email-setup", "executables", "MailHog_windows_amd64.exe");
            if (File.Exists(exePath))
            {
                mailHogProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            else
            {
                app.Logger.LogWarning("Could not find exe at {ExePath}", exePath);
            }

            app.Lifetime.ApplicationStopped.Register(() =>
            {
                app.Logger.LogInformation("ApplicationStopped event triggered.");
                if (mailHogProcess == null)
                {
                    app.Logger.LogWarning("MailHog process reference is null.");
                }
                else if (mailHogProcess.HasExited)
                {
                    app.Logger.LogInformation("MailHog process has already exited.");
                }
                else
                {
                    try
                    {
                        app.Logger.LogInformation("Killing MailHog process...");
                        mailHogProcess.Kill(true);
                        mailHogProcess.Dispose();
                        app.Logger.LogInformation("MailHog process killed.");
                    }
                    catch (Exception ex)
                    {
                        app.Logger.LogWarning(ex, "Failed to kill MailHog process.");
                    }
                }
            });
        }
    }
}