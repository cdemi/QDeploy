using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;
using Client.Server;
using Logic;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Client
{
    public partial class Form1 : Form
    {
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

        private void log(string text, params object[] args)
        {
            txtConsole.Text += String.Format("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text, args) +
                               Environment.NewLine;
            txtConsole.SelectionStart = txtConsole.TextLength;
            txtConsole.ScrollToCaret();
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
            List<int> toDelete = config.RemoteDeployments.Where(rd => rd.FriendlyName == null)
                .Select(rd => config.RemoteDeployments.IndexOf(rd)).ToList();

            if (toDelete != null)
            {
                foreach (int index in toDelete)
                {
                    config.RemoteDeployments.RemoveAt(index);
                }
            }
        }

        private void openConfigurationFile(string path)
        {
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
            txtLocalDeployment.Text = config.LocalDeployment;
            saveFileDialog1.FileName = path;
            dataGridView1.DataSource = config.RemoteDeployments;
            loadExclusionList();
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
            progressBar1.Maximum = localFileDetails.Count()*config.RemoteDeployments.Count;

            foreach (RemoteDeployment deployment in config.RemoteDeployments)
            {
                log("Deploying to {0}", deployment.FriendlyName);

                var dc = new DeployerClient("NetTcpBinding_IDeployer",
                    "net.tcp://" + deployment.Host + ":6969/Deployer");
                log("Connecting to server: {0}", deployment.Host);

                int uploaded = 0;
                int same = 0;
                int excluded = 0;

                try
                {
                    FileDetail[] remoteFileDetails = await dc.GetAllFilesAsync(deployment.Path);
                    foreach (FileDetail localFileDetail in localFileDetails)
                    {
                        progressBar1.Value++;

                        bool toExclude = config.ExclusionList.Any(
                            el => localFileDetail.Path.Contains(el, StringComparison.InvariantCultureIgnoreCase));


                        string relativeLocalPath =
                            localFileDetail.Path.Remove(txtLocalDeployment.Text).TrimStart('\\');
                        if (!toExclude)
                        {
                            FileDetail remoteFile =
                                remoteFileDetails.SingleOrDefault(
                                    rf =>
                                        rf.Path.Remove(deployment.Path)
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
                                uploaded++;
                                using (FileStream stream = File.OpenRead(localFileDetail.Path))
                                {
                                    await
                                        dc.SendFileAsync(deployment.Path + "\\" + relativeLocalPath, stream);
                                }
                            }
                            else
                            {
                                same++;
                            }
                        }
                        else
                        {
                            excluded++;
                        }
                    }
                    log("Finished Deployment: Uploaded: {0} - Unchanged: {1} - Excluded: {2}{3}", uploaded, same, excluded, Environment.NewLine);
                }
                catch (EndpointNotFoundException enfe)
                {
                    log("Couldn't connect to: {0}... Skipping Deployment{1}", deployment.Host, Environment.NewLine);
                }
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
            refreshTreeView();
        }

        private void refreshTreeView()
        {
            tvExclusions.Nodes.Clear();
            try
            {
                ListDirectory(tvExclusions, config.LocalDeployment);
            }
            catch (Exception)
            {
            }
        }

        private void ListDirectory(TreeView treeView, string path)
        {
            treeView.Nodes.Clear();
            var rootDirectoryInfo = new DirectoryInfo(path);
            treeView.Nodes.AddRange(CreateDirectoryNode(rootDirectoryInfo).Nodes.Cast<TreeNode>().ToArray());
        }

        private TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeNode(directoryInfo.Name)
            {
                Tag = directoryInfo.FullName.Remove(config.LocalDeployment).TrimStart('\\')
            };
            foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
                directoryNode.Nodes.Add(CreateDirectoryNode(directory));
            foreach (FileInfo file in directoryInfo.GetFiles())
                directoryNode.Nodes.Add(new TreeNode(file.Name)
                {
                    Tag = file.FullName.Remove(config.LocalDeployment).TrimStart('\\')
                });
            return directoryNode;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void btnDeploy_Click(object sender, EventArgs e)
        {
            deploy();
        }

        private void tvExclusions_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Nodes.Count > 0)
            {
                foreach (TreeNode node in e.Node.Nodes)
                {
                    node.Checked = e.Node.Checked;
                }
            }

            updateExclusionList();
        }

        private void updateExclusionList()
        {
            config.ExclusionList = new List<string>();
            foreach (TreeNode node in tvExclusions.Nodes)
            {
                updateSubExclusionList(node);
            }
        }

        private void updateSubExclusionList(TreeNode node)
        {
            if (node.Checked)
                config.ExclusionList.Add(node.Tag.ToString());

            if (node.Nodes.Count > 0)
            {
                foreach (TreeNode innerTreeNode in node.Nodes)
                {
                    updateSubExclusionList(innerTreeNode);
                }
            }
        }

        private void loadExclusionList()
        {
            tvExclusions.AfterCheck -= tvExclusions_AfterCheck;
            foreach (TreeNode treeNode in tvExclusions.Nodes)
            {
                loadSubExclusions(treeNode);
            }
            tvExclusions.AfterCheck += tvExclusions_AfterCheck;
        }

        private void loadSubExclusions(TreeNode treeNode)
        {
            if (config.ExclusionList.Contains(treeNode.Tag.ToString(), StringComparer.InvariantCultureIgnoreCase))
                treeNode.Checked = true;

            if (treeNode.Nodes.Count > 0)
            {
                foreach (TreeNode innerTreeNode in treeNode.Nodes)
                {
                    loadSubExclusions(innerTreeNode);
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/cdemi/QDeploy");
        }
    }
}