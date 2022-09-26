using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.ServiceProcess;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KonnectSMTPUtility
{
    public partial class frmBase : Form
    {
        public frmBase()
        {
            InitializeComponent();
        }
        
        private void frmBase_Load(object sender, EventArgs e)
        { 
            try
            {
                //Method 1. center at initilization
                //Method 2. The manual way                
                ///btnUpdate.Visible = false;
              //  lblCheckVersion.Text = "";
                //lblVersion.Text = "";

                //code to check service is running
                //code to get access token 
                EnableControls();
            }
            catch (Exception)
            {
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {

            //UpdateAppConfig();

            if (!Check_validation())
            {
                return;
            }
            else
            {
                if (IsServiceInstalled(Constants.ServiceName) && IsServiceRunning(Constants.ServiceName).Item1)
                {
                    DialogResult dialogResult = MessageBox.Show("This will override Configurator setting, it may resume the service." + Environment.NewLine + " Are you sure want to restart the service ?",
                        "Restart service", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        ServiceManager.RunScript("stope-service -Name " + Constants.ServiceName);
                        UpdateAppConfig();
                    }
                }
                else
                {
                    UpdateAppConfig();
                }
            }
        }
        private bool UpdateAppConfig()
        {
            bool lretval = false;
      
            var Directorypath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Service");
            try
            {
                var config = new ConfigurationBuilder()
                            .SetBasePath(Directorypath)
                            .AddJsonFile("appsettings.json")
                            .Build()
                            .Get<Config>();
                config.Logging.LogLevel.Default = "Information";
                config.Logging.LogLevel.Microsoft = "Warning";               
                config.Logging.LogLevel.Microsoft_Hosting_Lifetime = "Information";
                config.Socket.Host = "wss://localhost:44301/ws?session=" + txtAccessToken.Text.Trim();
                config.Smtp.Host = txtHost.Text.Trim();
                config.Smtp.Port = txtPort.Text.Trim();
                config.Smtp.Email = txtEmail.Text.Trim();
                config.Smtp.Password = txtPasssword.Text.Trim();
                
                var jsonWriteOptions = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };
                jsonWriteOptions.Converters.Add(new JsonStringEnumConverter());

                var newJson = JsonSerializer.Serialize(config, jsonWriteOptions);
                newJson = newJson.Replace("_", ".");
                var appSettingsPath = Path.Combine(Directorypath, "appsettings.json");
                File.WriteAllText(appSettingsPath, newJson);
                MessageBox.Show("Your details saved successfully..");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return lretval;
        }
     
        public bool Check_validation()
        {       
            

            if (string.IsNullOrEmpty(txtAccessToken.Text.Trim()))
            {
                MessageBox.Show("Please enter access token", "Invalid Access Token", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtAccessToken.Focus();
                return false;
            }
            else if (string.IsNullOrEmpty(txtHost.Text.Trim()) || !Regex.IsMatch(txtHost.Text.Trim(), @"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$"))
            {

                MessageBox.Show("Either Host has left a blank or your host address is wrong", "Invalid Host Address", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtHost.Focus();
                return false;
            }
            else if (string.IsNullOrEmpty(txtPort.Text.Trim()) || !Regex.IsMatch(txtPort.Text.Trim(), @"^[1-9]{1}$|^[0-9]{2,4}$|^[0-9]{3,4}$|^[1-5]{1}[0-9]{1}[0-9]{1}[0-9]{1}[0-9]{1}$|^[1-6]{1}[0-4]{1}[0-9]{1}[0-9]{1}[0-9]{1}$|^[1-6]{1}[0-5]{1}[0-4]{1}[0-9]{1}[0-9]{1}$|^[1-6]{1}[0-5]{1}[0-5]{1}[0-3]{1}[0-5]{1}$"))
            {
                MessageBox.Show("Either Port has left a blank or your Port address is wrong", "Invalid Port Number", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPort.Focus();
                return false;
            }
            else if (string.IsNullOrEmpty(txtEmail.Text.Trim()) || !Regex.IsMatch(txtEmail.Text.Trim(), @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$"))
            {
                MessageBox.Show("Either Email has left a blank or your Email address is wrong", "Invalid Email Address", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtEmail.Focus();
                return false;
            }
            else if (string.IsNullOrEmpty(txtPasssword.Text.Trim()))
            {
                MessageBox.Show("Please enter Password", "Invalid Password", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPasssword.Focus();
                return false;
            }
            
            else 
            {
                return true;
            }              

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.Default;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                string result = ServiceManager.RunScript("start-service -Name "+Constants.ServiceName);
                if (!string.IsNullOrEmpty(result))
                {
                    if (IsServiceRunning(Constants.ServiceName).Item2 == "Running")
                    {
                        MessageBox.Show("Service Running Succesfully", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        EnableControls();
                    }
                    else if (IsServiceRunning(Constants.ServiceName).Item2=="Stopped" || IsServiceRunning(Constants.ServiceName).Item2 == "Paused" || IsServiceRunning(Constants.ServiceName).Item2== "Stopping")
                    {
                        result = ServiceManager.RunScript("restart-service -Name " + Constants.ServiceName);
                        MessageBox.Show(result, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        EnableControls();
                    }
                    else
                    {
                        MessageBox.Show("Cannot find service installed ,Please install first", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }                    
                }
                else
                {
                    MessageBox.Show("Service is not Starting", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    

                }               
            
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
               
            }            

        }    
     
        private void EnableControls()
        {
            bool lInstalled=false;
            if (IsServiceRunning(Constants.ServiceName).Item2 == "Running" || IsServiceRunning(Constants.ServiceName).Item2 == "Starting")
            {
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                lInstalled = true;
            }
            else 
            {
                btnStop.Enabled = false;
                btnStart.Enabled = true;
            }
            if (IsServiceInstalled(Constants.ServiceName))
            {
                btnUninstall.Enabled = true;
                btnInstall.Enabled = false;
                lInstalled = true;

            }
            else
            {
                btnUninstall.Enabled = false;
                btnInstall.Enabled = true;
            }            
            Tuple<bool, string> tplRunningStatus = IsServiceRunning(Constants.ServiceName);
              
            lblServiceStatus.Text = !lInstalled
                ? "Service not installed"
                : (!tplRunningStatus.Item1)
                ? "Service not started, to start click on the start service button."
                : "Service status : " + tplRunningStatus.Item2;
        }
        private bool IsServiceInstalled(string strServiceName)
        {
            try
            {
                var serviceExists = ServiceController.GetServices().Any(s => s.ServiceName == strServiceName);
                return Convert.ToBoolean(serviceExists);
            }
            catch (Exception)
            {
                return false;
            }
        }
        private Tuple<bool, string> IsServiceRunning(string strServiceName)
        {
            try
            {
                ServiceController sc = new ServiceController(strServiceName);
                string ServiceStatus = string.Empty;
                var temp = sc.Status.ToString();
                switch (sc.Status)
                {
                    case ServiceControllerStatus.Running:
                        ServiceStatus = "Running";
                        break;
                    case ServiceControllerStatus.Stopped:
                        ServiceStatus = "Stopped";
                        break;
                    case ServiceControllerStatus.Paused:
                        ServiceStatus = "Paused";
                        break;
                    case ServiceControllerStatus.StopPending:
                        ServiceStatus = "Stopping";
                        break;
                    case ServiceControllerStatus.StartPending:
                        ServiceStatus = "Starting";
                        break;
                    default:
                        ServiceStatus = "Changing";
                        break;
                }

                if (!string.IsNullOrEmpty(ServiceStatus) && (ServiceStatus == "Running" || ServiceStatus == "Starting"))
                {
                    return new Tuple<bool, string>(true, ServiceStatus);
                }
                else
                    return new Tuple<bool, string>(false, ServiceStatus);
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(false, ex.Message);
            }
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {            
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                string result = ServiceManager.RunScript("SC.exe create " + Constants.ServiceName + " binpath='" + Constants.asmPath + "' start=auto");
                if (result.Contains("Access is denied"))
                {
                    MessageBox.Show("Please run the application as an administrator", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                      
                }
                else if(result.Contains("The specified service already exists"))
                {
                    MessageBox.Show("Please Start the service since it is already installed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(result, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    EnableControls();
                }
                Cursor.Current = Cursors.Default;           
            }           
         
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
            }
            Cursor.Current = Cursors.Default;            
        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                string result = string.Empty;               

                    if (IsServiceRunning(Constants.ServiceName).Item2 == "Stopped")
                    {
                        result = ServiceManager.RunScript("SC.exe delete " + Constants.ServiceName);
                        MessageBox.Show(result, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        EnableControls();
                    }
                    else if (IsServiceRunning(Constants.ServiceName).Item2.Contains("Service WorkerService1 was not found on computer"))
                    {
                    result = IsServiceRunning(Constants.ServiceName).Item2;
                    MessageBox.Show("Service was not found on Computer,please install it first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (IsServiceRunning(Constants.ServiceName).Item2 == "Running" || IsServiceRunning(Constants.ServiceName).Item2 == "Starting")
                    {
                        result = ServiceManager.RunScript("stop-service -Name " + Constants.ServiceName);
                        if (IsServiceRunning(Constants.ServiceName).Item2 == "Stopped")
                        {
                            result = ServiceManager.RunScript("SC.exe delete " + Constants.ServiceName);
                            MessageBox.Show(result, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            EnableControls();
                        }
                        else
                        {
                            MessageBox.Show("Cannot find service installed ,Please install first", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show(result, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);


                    }
       
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;            
        
        }        
        
        private void btnStop_Click(object sender, EventArgs e)
        {

            Cursor.Current = Cursors.Default;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                string result = ServiceManager.RunScript("stop-service -Name "+Constants.ServiceName);

                if (!string.IsNullOrEmpty(result))
                {
                    if (IsServiceRunning(Constants.ServiceName).Item2 == "Stopped")
                    {
                        MessageBox.Show("Service Stopped Succesfully", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        EnableControls();
                    }
                    else if (IsServiceRunning(Constants.ServiceName).Item2 == "Running" || IsServiceRunning(Constants.ServiceName).Item2 == "Starting")
                    {
                        result = ServiceManager.RunScript("stop-service -Name " + Constants.ServiceName);
                    }
                    else
                    {
                        MessageBox.Show("Cannot find service installed ,Please install first", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show(result, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);


                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Please run the application as an administrator", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
              
            }   
        }

        private void frmBase_Load_1(object sender, EventArgs e)
        {

        }
    }
    class Constants
    {
        public static string DirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Resources", "Service");
        public static string asmPath = Path.Combine(DirectoryPath, "WorkerService1.exe");
        public static string ServiceName = "WorkerService1";

    }


}
