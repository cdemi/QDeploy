using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Client.Server;
using Logic;

namespace Client
{
    public partial class Form1 : Form
    {
        private void log(string text, params string[] args)
        {
            txtConsole.Text += String.Format(text, args) + Environment.NewLine;
            txtConsole.SelectionStart = txtConsole.TextLength;
            txtConsole.ScrollToCaret();
        }
        private Config config = new Config();
        public Form1()
        {
            InitializeComponent();
        }

        public Form1(string filePath)
        {
            InitializeComponent();
            openConfigurationFile(filePath);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(saveFileDialog1.FileName))
                saveFileDialog1.ShowDialog();
            else
                save();
        }

        private void save()
        {
            cleanDataGrid();

            File.WriteAllText(saveFileDialog1.FileName, JsonConvert.SerializeObject(config));
        }

        private void cleanDataGrid()
        {
            var toDelete = config.RemoteDeployments.Where(rd => rd.FriendlyName == null)
                            .Select(rd => config.RemoteDeployments.IndexOf(rd)).ToList();

            if (toDelete != null)
            {
                foreach (var index in toDelete)
                {
                    config.RemoteDeployments.RemoveAt(index);
                }
            }
        }

        private void openConfigurationFile(string path)
        {
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
            txtLocalDeployment.Text = config.LocalDeployment;
            saveFileDialog1.FileName = openFileDialog1.FileName;
            dataGridView1.DataSource = config.RemoteDeployments;
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            openConfigurationFile(openFileDialog1.FileName);
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            save();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            txtLocalDeployment.Text = folderBrowserDialog1.SelectedPath;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = config.RemoteDeployments;
        }

        private async void deploy()
        {
            txtConsole.Text = String.Empty;
            progressBar1.Value = 0;
            btnDeploy.Enabled = false;
            cleanDataGrid();
            IEnumerable<FileDetail> localFileDetails =
                    Directory.GetFiles(txtLocalDeployment.Text, "*", SearchOption.AllDirectories).Select(f => new FileDetail
                    {
                        Path = f,
                        Hash = f.MD5Hash(),
                    });
            progressBar1.Maximum = localFileDetails.Count() * config.RemoteDeployments.Count;

            foreach (var deployment in config.RemoteDeployments)
            {
                log("Starting Deployment: {0}", deployment.FriendlyName);

                var dc = new DeployerClient("NetTcpBinding_IDeployer",
                    "net.tcp://" + deployment.Host + ":6969/Deployer");
                log("Connected to server: {0}", deployment.Host);

                //IEnumerable<string> exclusionList;
                //if (deployment.Exclusions != null)
                //    exclusionList = Config.Exclusions.Concat(deployment.Exclusions);
                //else
                //    exclusionList = Config.Exclusions;

                FileDetail[] remoteFileDetails = await dc.GetAllFilesAsync(deployment.Path);
                foreach (FileDetail localFileDetail in localFileDetails)
                {
                    progressBar1.Value++;

                    /*var toExclude = exclusionList.Any(
                        el => localFileDetail.Path.Contains(el, StringComparison.InvariantCultureIgnoreCase));*/

                    var toExclude = false;

                    string relativeLocalPath =
                        localFileDetail.Path.Remove(localFileDetail.Path.IndexOf(txtLocalDeployment.Text),
                            txtLocalDeployment.Text.Length).TrimStart('\\');
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

                log("Finished Deployment on: {0}{1}", deployment.FriendlyName, Environment.NewLine);
            }

            btnDeploy.Enabled = true;
        }

        private void txtLocalDeployment_TextChanged(object sender, EventArgs e)
        {
            config.LocalDeployment = txtLocalDeployment.Text;
            try
            {
                folderBrowserDialog1.SelectedPath = config.LocalDeployment;
            }
            catch
            {
                
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void btnDeploy_Click(object sender, EventArgs e)
        {
            deploy();
        }

    }
}
