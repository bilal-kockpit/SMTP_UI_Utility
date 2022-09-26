using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;

namespace SMTPUtilitySetup.Custom
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult CustomAction1(Session session)
        {
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!hasAdministrativeRight)
            {
                if (MessageBox.Show("This installer requires administrator privileges.\r\n\r\nDo you want to attempt to restart it with administrator privileges?", "Administrator Privileges Required", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    ProcessStartInfo processInfo = new ProcessStartInfo();
                    processInfo.Verb = "runas";
                    processInfo.FileName = "msiexec";
                    processInfo.Arguments = "/i \"" + session["OriginalDatabase"] + "\"";
                    try
                    {
                        //Process.Start(processInfo);
                        using (Process wrapperProcess = Process.Start(processInfo))
                        {
                            wrapperProcess.WaitForExit();
                        }
                        session["ElevatedFromCA"] = "1";
                        return ActionResult.SkipRemainingActions;
                    }
                    catch (Win32Exception)
                    {
                        //Do nothing. Probably the user canceled the UAC window
                        return ActionResult.UserExit;
                    }
                }
                else
                {
                    return ActionResult.UserExit;
                }
            }
            else
            {
                return ActionResult.Success;
            }
        }
    }
}
