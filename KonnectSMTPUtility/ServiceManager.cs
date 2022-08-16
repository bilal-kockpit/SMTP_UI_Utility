using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Configuration.Install;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.ServiceProcess;
using System.Text;

namespace KonnectSMTPUtility
{
    class ServiceManager
    {
        public static string RunScript(string script) 
        {

            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            Pipeline pipeline=runspace.CreatePipeline();
            pipeline.Commands.AddScript(script);
            pipeline.Commands.Add("Out-String");
            Collection<PSObject> results =pipeline.Invoke();
            runspace.Close(); 
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PSObject psObject in results)
                stringBuilder.AppendLine(psObject.ToString());
            return stringBuilder.ToString();
        }

    }


}

