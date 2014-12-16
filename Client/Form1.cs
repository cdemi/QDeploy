using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Client.DeployerService;
using Logic;
using Newtonsoft.Json;

namespace Client
{
    public partial class Form1 : Form
    {
        private Config Config;

        public Form1()
        {
            InitializeComponent();
        }

        private void log(string text, params string[] args)
        {
            txtConsole.Text += String.Format(text, args) + Environment.NewLine;
            txtConsole.SelectionStart = txtConsole.TextLength;
            txtConsole.ScrollToCaret();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists("config.json"))
            {
                var sampleConfig = new Config
                {
                    Exclusions = new List<string> { "web.config" },
                    Deployments = new List<Deployment>
                    {
                        new Deployment
                        {
                            Name = "Sample App 1",
                            Path = @"C:\Deployments\SampleApp1\",
                            Server = "127.0.0.1"
                        }
                    }
                };

                File.WriteAllText("config.json", JsonConvert.SerializeObject(sampleConfig));
                MessageBox.Show("You must define your configuration in config.json", "No configuration found",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            else
            {
                Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

                chkDeployTo.DataSource = Config.Deployments.OrderBy(d => d.Name).ToList();
                chkDeployTo.DisplayMember = "Name";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            fbLocalDeploy.ShowDialog();
            txtLocalDeploy.Text = fbLocalDeploy.SelectedPath;
        }

        private void btnDeploy_Click(object sender, EventArgs e)
        {
            DialogResult result =
                MessageBox.Show("Are you really sure that you want to deploy your application to the selected servers?",
                    "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                txtConsole.Text = String.Empty;
                progressBar1.Value = 0;
                deploy();
            }
        }

        private async void deploy()
        {
            btnDeploy.Enabled = false;
            try
            {
                IEnumerable<FileDetail> localFileDetails =
                    Directory.GetFiles(txtLocalDeploy.Text, "*", SearchOption.AllDirectories).Select(f => new FileDetail
                    {
                        Path = f,
                        Hash = f.MD5Hash(),
                    });
                progressBar1.Maximum = localFileDetails.Count() * chkDeployTo.CheckedItems.Count;
                foreach (object selectedItem in chkDeployTo.CheckedItems)
                {
                    var deployment = (Deployment)selectedItem;
                    log("Starting Deployment: {0}", deployment.Name);

                    var dc = new DeployerClient("NetTcpBinding_IDeployer",
                        "net.tcp://" + deployment.Server + ":6969/Deployer");
                    log("Connected to server: {0}", deployment.Server);

                    IEnumerable<string> exclusionList;
                    if (deployment.Exclusions != null)
                        exclusionList = Config.Exclusions.Concat(deployment.Exclusions);
                    else
                        exclusionList = Config.Exclusions;

                    FileDetail[] remoteFileDetails = await dc.GetAllFilesAsync(deployment.Path);
                    foreach (FileDetail localFileDetail in localFileDetails)
                    {
                        progressBar1.Value++;

                        var toExclude = exclusionList.Any(
                            el => localFileDetail.Path.Contains(el, StringComparison.InvariantCultureIgnoreCase));


                        string relativeLocalPath =
                            localFileDetail.Path.Remove(localFileDetail.Path.IndexOf(txtLocalDeploy.Text),
                                txtLocalDeploy.Text.Length).TrimStart('\\');
                        if (!toExclude)
                        {
                            FileDetail remoteFile =
                                remoteFileDetails.SingleOrDefault(
                                    rf =>
                                        rf.Path.Remove(rf.Path.IndexOf(deployment.Path), deployment.Path.Length)
                                            .TrimStart('\\')
                                            .Equals(relativeLocalPath, StringComparison.InvariantCultureIgnoreCase));

                            bool upload = false;

                            if (remoteFile != null)
                            {
                                bool isDifferent = !localFileDetail.Hash.IsEqualTo(remoteFile.Hash);

                                if (isDifferent)
                                {
                                    log("{0} is different. Uploading...", relativeLocalPath);
                                    upload = true;
                                }
                            }
                            else
                            {
                                log("{0} does not exist at the destination server. Uploading...", relativeLocalPath);
                                upload = true;
                            }

                            if (upload)
                            {
                                using (FileStream stream = File.OpenRead(localFileDetail.Path))
                                {
                                    await
                                        dc.SendFileAsync(deployment.Path + "\\" + relativeLocalPath, stream);
                                }
                            }
                        }
                        else
                        {
                            log("{0} excluded", relativeLocalPath);
                        }
                    }

                    log("Finished Deployment on: {0}{1}", deployment.Name, Environment.NewLine);
                }
            }
            catch (Exception e)
            {
                throw;
            }
            btnDeploy.Enabled = true;
        }
    }
}