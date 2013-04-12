using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace FileWatcher
{
    public partial class SystemTray : Form
    {
        private WatchFiles filewatcher = new WatchFiles();
        private NotifyIcon trayicon = new NotifyIcon();
        private ProcessFiles processFiles = new ProcessFiles();

        public static int numOfGroupBoxes = 0;
        public static int groupboxYlocation = 25;
        public static List<int> groupboxindexes = new List<int>();

        private TabPage xmlPage = new TabPage("XML");
        private TabPage maintPage = new TabPage("Maintenance");
        
        [STAThread]
        public static void Main()
        {
            Application.Run(new SystemTray());
            Logger.Info("Application started...", "Main");
        }

        public SystemTray()
        {
            Logger.Info("Initializing...", "Constructor");
            InitializeComponent();
            trayicon.Icon = new Icon(SystemIcons.Application, 16, 16);
            trayicon.ContextMenu = new ContextMenu();
            trayicon.ContextMenu.MenuItems.Add(new MenuItem("Exit", new EventHandler(OnExit)));
            trayicon.DoubleClick += trayicon_DoubleClick;
            trayicon.Visible = true;
            Logger.Info("Initialization complete", "Constructor");
        }

        void trayicon_DoubleClick(object sender, EventArgs e)
        {
            Logger.Info("Showing form...", "trayicon_DoubleClick");
            this.Show();
            Logger.Info("form shown", "trayicon_DoubleClick");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Logger.Info("Form closing...", "OnFormClosing");
            this.Visible = false;
            e.Cancel = true;
            Logger.Info("Form closed", "OnFormClosing");
        }

        private void OnExit(object sender, EventArgs e)
        {
            Logger.ApplicationExiting();
            trayicon.Visible = false;
            Environment.Exit(0);
        }

        void LoadXMLTab()
        {
            Logger.Info("Loading XML tab...", "LoadXMLTab");
            foreach (var folder in WatchFiles.folders)
            {
                GroupBox grpbox = new GroupBox()
                {
                    Width = 375,
                    Height = 90,
                    Name = string.Format("grpbox{0}", numOfGroupBoxes),
                    Location = new Point(0, groupboxYlocation)
                };

                CheckBox chkRemove = new CheckBox()
                {
                    Text = "Remove from XML file",
                    Name = string.Format("chkRemove{0}", numOfGroupBoxes),
                    Width = 155,
                    Location = new Point(5, 10)
                };
                chkRemove.Click += chkRemove_Click;

                Label lblFrom = new Label()
                {
                    Text = "From",
                    Name = string.Format("lblFrom{0}", numOfGroupBoxes),
                    Width = 35,
                    Location = new Point(chkRemove.Location.X, chkRemove.Location.Y + 25)
                };

                TextBox txtFrom = new TextBox()
                {
                    Width = 250,
                    Name = string.Format("txtFrom{0}", numOfGroupBoxes),
                    Location = new Point(lblFrom.Location.X + lblFrom.Width + 3, lblFrom.Location.Y),
                    Text = folder.Key.ToString()
                };

                Button btnFrom = new Button()
                {
                    Text = "Browse",
                    Name = string.Format("btnFrom{0}", numOfGroupBoxes),
                    Location = new Point(txtFrom.Location.X + txtFrom.Width + 3, txtFrom.Location.Y - 1)
                };
                btnFrom.Click += btnFrom_Click;

                Label lblTo = new Label()
                {
                    Text = "To",
                    Name = string.Format("lblTo{0}", numOfGroupBoxes),
                    Width = 35,
                    Location = new Point(lblFrom.Location.X, lblFrom.Location.Y + 25)
                };

                TextBox txtTo = new TextBox()
                {
                    Width = 250,
                    Name = string.Format("txtTo{0}", numOfGroupBoxes),
                    Location = new Point(lblTo.Location.X + lblTo.Width + 3, lblTo.Location.Y),
                    Text = folder.Value.ToString()
                };

                Button btnTo = new Button()
                {
                    Text = "Browse",
                    Name = string.Format("btnTo{0}", numOfGroupBoxes),
                    Location = new Point(txtTo.Location.X + txtTo.Width + 3, txtTo.Location.Y - 1)
                };
                btnTo.Click += btnTo_Click;

                grpbox.Controls.Add(chkRemove);
                grpbox.Controls.Add(lblFrom);
                grpbox.Controls.Add(txtFrom);
                grpbox.Controls.Add(btnFrom);
                grpbox.Controls.Add(lblTo);
                grpbox.Controls.Add(txtTo);
                grpbox.Controls.Add(btnTo);

                xmlPage.Controls.Add(grpbox);

                groupboxindexes.Add(numOfGroupBoxes);
                numOfGroupBoxes++;
                groupboxYlocation += grpbox.Height;
            }
            Logger.Info("Loaded XML tab", "LoadXMLTab");
        }

        void LoadMaintenanceTab()
        {
            Logger.Info("Loading Maintenance tab...", "LoadMaintenanceTab");
            int i = 0;
            int Height = 0;
            foreach (var folder in WatchFiles.folders)
            {
                GroupBox grpboxMaint = new GroupBox()
                {
                    //AutoSize = true,
                    Width = 375,
                    Height = 80,
                    Name = string.Format("grpboxMaint{0}", i),
                    Location = new Point(0, Height)
                };

                RadioButton rdoEnabled = new RadioButton()
                {
                    Text = "Enabled",
                    Name = string.Format("rdoEnabled{0}", i),
                    Height = 13,
                    Width = 64,
                    Checked = true, //might want to add enabled as an attribute in xml for each folder
                    Location = new Point(5, 10)
                };
                rdoEnabled.Click += rdoEnabled_Click;

                RadioButton rdoDisabled = new RadioButton()
                {
                    Text = "Disabled",
                    Name = string.Format("rdoDisabled{0}", i),
                    Height = 13,
                    Width = 70,
                    Location = new Point(rdoEnabled.Width + 5, 10)
                };
                rdoDisabled.Click += rdoDisabled_Click;

                Label lblFromMaint = new Label()
                {
                    Text = string.Format("From: {0}", folder.Key),
                    AutoSize = true,
                    Name = string.Format("lblFromMaint{0}", i),
                    Location = new Point(rdoEnabled.Location.X, rdoEnabled.Location.Y + 20)
                };

                Label lblToMaint = new Label()
                {
                    Text = string.Format("To:    {0}", folder.Value),
                    AutoSize = true,
                    Name = string.Format("lblToMaint{0}", i),
                    Location = new Point(lblFromMaint.Location.X, lblFromMaint.Location.Y + 25)
                };

                grpboxMaint.Controls.Add(rdoEnabled);
                grpboxMaint.Controls.Add(rdoDisabled);
                grpboxMaint.Controls.Add(lblFromMaint);
                grpboxMaint.Controls.Add(lblToMaint);

                maintPage.Controls.Add(grpboxMaint);

                Height += grpboxMaint.Height;

                i++;
            }
            Logger.Info("Loaded Maintenance tab", "LoadMaintenanceTab");
        }

        protected override void OnLoad(EventArgs e)
        {
            Logger.Info("Loading form...", "OnLoad");
            this.Visible = false;
            this.ShowInTaskbar = false;
            base.OnLoad(e);

            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.AutoSize = true;
            this.AutoScroll = true;
            this.MinimumSize = new System.Drawing.Size(430, 340);

            TabControl tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            xmlPage.AutoScroll = true;
            xmlPage.Name = "xmlPage";

            maintPage.AutoScroll = true;
            maintPage.Name = "maintPage";

            Button btnAdd = new Button()
            {
                Text = "Add",
                Location = new Point(0, 0)
            };
            btnAdd.Click += btnAdd_Click;

            Button btnDone = new Button()
            {
                Text = "Save",
                Name = "btnDone",
                Location = new Point(btnAdd.Width, 0),
                Enabled = false
            };
            btnDone.Click += btnDone_Click;

            xmlPage.Controls.Add(btnAdd);
            xmlPage.Controls.Add(btnDone);

            LoadXMLTab();
            LoadMaintenanceTab();

            tabControl.Controls.Add(maintPage);
            tabControl.Controls.Add(xmlPage);
            this.Controls.Add(tabControl);

            StartPosition = FormStartPosition.Manual;
            var screen = Screen.FromPoint(this.Location);
            this.Location = new Point(screen.WorkingArea.Right - this.Width, screen.WorkingArea.Bottom - this.Height);
            this.TopMost = true;
            Logger.Info("Loaded form", "OnLoad");
        }

        void rdoDisabled_Click(object sender, EventArgs e)
        {
            try
            {
                RadioButton rdo = (RadioButton)sender;
                int index = Convert.ToInt32(rdo.Name.Replace("rdoDisabled", string.Empty));
                Label label = (Label)this.Controls.Find(string.Format("lblFromMaint{0}", index), true)[0];
                string directory = processFiles.FormatDirectory(label.Text.Replace("From: ", string.Empty));

                if (WatchFiles.fileWatchers.ContainsKey(directory))
                {
                    Logger.Info(string.Format("Disabling filewatcher: {0}", directory), "rdoDisabled_Click");
                    WatchFiles.fileWatchers[directory].EnableRaisingEvents = false;
                    Logger.Info("Filewatcher disabled", "rdoDisabled_Click");
                }
                else
                {
                    Logger.Warning(string.Format("{0} was not found in filewatchers dictionary.", directory), "rdoDisabled_Click");
                    MessageBox.Show(string.Format("{0}\nwas not found in filewatchers dictionary.", directory));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "rdoDisabled_Click");
                MessageBox.Show(ex.Message);
            }
        }

        void rdoEnabled_Click(object sender, EventArgs e)
        {
            try
            {
                RadioButton rdo = (RadioButton)sender;
                int index = Convert.ToInt32(rdo.Name.Replace("rdoEnabled", string.Empty));
                Label label = (Label)this.Controls.Find(string.Format("lblFromMaint{0}", index), true)[0];
                string directory = processFiles.FormatDirectory(label.Text.Replace("From: ", string.Empty));

                if (WatchFiles.fileWatchers.ContainsKey(directory))
                {
                    Logger.Info(string.Format("Enabling filewatcher: {0}", directory), "rdoEnabled_Click");
                    WatchFiles.fileWatchers[directory].EnableRaisingEvents = true;
                    Logger.Info("Filewatcher enabled", "rdoEnabled_Click");
                }
                else
                {
                    Logger.Warning(string.Format("{0} was not found in filewatchers dictionary.", directory), "rdoEnabled_Click");
                    MessageBox.Show(string.Format("{0}\nwas not found in filewatchers dictionary.", directory));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "rdoEnabled_Click");
                MessageBox.Show(ex.Message);
            }
        }

        //void btnRefresh_Click(object sender, EventArgs e)
        //{
        //    OnLoad(new EventArgs());
        //}

        string GetFolderFromDialog()
        {
            Logger.Info("Selecting file from dialog...", "GetFolderFromDialog");
            string folder = string.Empty;
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select folder";
            dialog.ShowNewFolderButton = true;

            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                folder = processFiles.FormatDirectory(dialog.SelectedPath);
            }
            Logger.Info("Selected file from dialog", "GetFolderFromDialog");
            ((Button)this.Controls.Find("btnDone", true)[0]).Enabled = true;

            return folder;
        }

        void btnTo_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                int index = Convert.ToInt32(btn.Name.Replace("btnTo", string.Empty));
                TextBox txtFrom = (TextBox)this.Controls.Find(string.Format("txtFrom{0}", index), true)[0];
                TextBox txtTo = (TextBox)this.Controls.Find(string.Format("txtTo{0}", index), true)[0];
                Label label = (Label)this.Controls.Find(string.Format("lblToMaint{0}", index), true)[0];
                string to = txtTo.Text;
                txtTo.Text = GetFolderFromDialog();
                Logger.Info(string.Format("Checking dictionary for {0}", to), "btnTo_Click");
                if (WatchFiles.folders.ContainsValue(to) && !WatchFiles.folders.ContainsValue(txtTo.Text))
                {
                    Logger.Info(string.Format("found {0} at key {1}", to, txtFrom.Text), "btnTo_Click");
                    WatchFiles.folders.Remove(txtFrom.Text);
                    Logger.Info(string.Format("removed key {0}", txtFrom.Text), "btnTo_Click");
                    WatchFiles.fileWatchers[txtFrom.Text].Dispose();
                    WatchFiles.fileWatchers.Remove(txtFrom.Text);
                    Logger.Info(string.Format("removed filewatcher for {0}", txtFrom.Text), "btnTo_Click");
                    WatchFiles.folders.Add(txtFrom.Text, txtTo.Text);
                    Logger.Info(string.Format("added dictionary for {0} & {1}", txtFrom.Text, txtTo.Text), "btnTo_Click");
                    label.Text = string.Format("From: {0}", txtTo.Text);
                    Logger.Info("setting up filewatcher...", "btnTo_Click");
                    WatchFiles.SetupFileWatcher();
                    Logger.Info("set up filewatcher", "btnTo_Click");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "btnTo_Click");
                MessageBox.Show(ex.Message);
            }
        }

        void btnFrom_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                int index = Convert.ToInt32(btn.Name.Replace("btnFrom", string.Empty));
                TextBox txtFrom = (TextBox)this.Controls.Find(string.Format("txtFrom{0}", index), true)[0];
                TextBox txtTo = (TextBox)this.Controls.Find(string.Format("txtTo{0}", index), true)[0];
                Label label = (Label)this.Controls.Find(string.Format("lblFromMaint{0}", index), true)[0];
                string from = txtFrom.Text;
                txtFrom.Text = GetFolderFromDialog();
                Logger.Info(string.Format("Checking dictionary for {0}", from), "btnFrom_Click");
                if (WatchFiles.folders.ContainsKey(from) && !WatchFiles.folders.ContainsKey(txtFrom.Text))
                {
                    Logger.Info(string.Format("found {0}", from), "btnFrom_Click");
                    WatchFiles.folders.Remove(from);
                    Logger.Info(string.Format("removed key {0}", from), "btnFrom_Click");
                    WatchFiles.fileWatchers[from].Dispose();
                    WatchFiles.fileWatchers.Remove(from);
                    Logger.Info(string.Format("removed filewatcher for {0}", from), "btnFrom_Click");
                    WatchFiles.folders.Add(txtFrom.Text, txtTo.Text);
                    Logger.Info(string.Format("added dictionary for {0} & {1}", txtFrom.Text, txtTo.Text), "btnFrom_Click");
                    label.Text = string.Format("From: {0}", txtFrom.Text);
                    Logger.Info("setting up filewatcher...", "btnFrom_Click");
                    WatchFiles.SetupFileWatcher();
                    Logger.Info("set up filewatcher", "btnFrom_Click");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "btnFrom_Click");
                MessageBox.Show(ex.Message);
            }
        }

        void chkRemove_Click(object sender, EventArgs e)
        {
            try
            {
                CheckBox chk = (CheckBox)sender;
                TabPage xmlPage = (TabPage)this.Controls.Find("xmlPage", true)[0];
                DialogResult result = MessageBox.Show("Are you sure you want to delete this from the xml file?",
                    "Confirmation", MessageBoxButtons.YesNoCancel);

                switch (result)
                {
                    case DialogResult.Yes:
                        Logger.Info("removing key...", "chkRemove_Click");
                        int index = Convert.ToInt32(chk.Name.Replace("chkRemove", string.Empty));
                        TextBox txtFrom = (TextBox)xmlPage.Controls.Find(string.Format("txtFrom{0}", index), true)[0];
                        WatchFiles.folders.Remove(txtFrom.Text);
                        Logger.Info(string.Format("removed key {0}", txtFrom.Text), "chkRemove_Click");

                        GroupBox groupbox = (GroupBox)xmlPage.Controls.Find(string.Format("grpbox{0}", index), true)[0];
                        Point point = groupbox.Location;
                        xmlPage.Controls.Remove(groupbox);
                        groupboxindexes.Remove(index);
                        groupboxYlocation -= groupbox.Height;

                        foreach (int i in groupboxindexes.Where(x => x > index))
                        {
                            GroupBox grp = (GroupBox)xmlPage.Controls.Find(string.Format("grpbox{0}", i), true)[0];
                            grp.Location = new Point(point.X, point.Y);
                            point = new Point(point.X, point.Y + groupbox.Height);
                        }

                        btnDone_Click(null, null);
                        break;
                    case DialogResult.No:
                    case DialogResult.Cancel:
                        chk.Checked = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "chkRemove_Click");
                MessageBox.Show(ex.Message);
            }
        }

        void btnDone_Click(object sender, EventArgs e)
        {
            try
            {
                TabPage xmlPage = (TabPage)this.Controls.Find("xmlPage", true)[0];
                int numOfGroupBoxes = 0;
                bool isOkToSave = true;

                foreach (GroupBox control in xmlPage.Controls.OfType<GroupBox>())
                {
                    numOfGroupBoxes++;
                    int index = Convert.ToInt32(control.Name.Replace("grpbox", string.Empty));
                    CheckBox chkRemove = (CheckBox)control.Controls.Find(string.Format("chkRemove{0}", index), true)[0];
                    TextBox txtFrom = (TextBox)control.Controls.Find(string.Format("txtFrom{0}", index), true)[0];
                    TextBox txtTo = (TextBox)control.Controls.Find(string.Format("txtTo{0}", index), true)[0];

                    if (txtFrom.Text != string.Empty && txtTo.Text != string.Empty && !WatchFiles.folders.ContainsKey(txtFrom.Text))
                    {
                        Logger.Info("adding key...", "btnDone_Click");
                        WatchFiles.folders.Add(txtFrom.Text, txtTo.Text);
                        Logger.Info(string.Format("added {0} & {1}", txtFrom.Text, txtTo.Text), "btnDone_Click");
                    }
                    else
                    {
                        isOkToSave = false;
                        if (txtFrom.Text == string.Empty)
                        {
                            Logger.Warning("from text is empty", "btnDone_Click");
                        }
                        else if (txtTo.Text == string.Empty)
                        {
                            Logger.Warning("to text is empty", "btnDone_Click");
                        }
                        else if (!WatchFiles.folders.ContainsKey(txtFrom.Text))
                        {
                            Logger.Warning(string.Format("could not find key in dictionary: {0}", txtFrom.Text), "btnDone_Click");
                        }
                        MessageBox.Show("Please fill all textboxes with values or remove the unused textboxes");
                        ((Button)this.Controls.Find("btnDone", true)[0]).Enabled = true;
                    }
                }

                //Logger.Info("checking if dictionary count and groupbox count match...", "btnDone_Click");
                //if (WatchFiles.folders.Count == numOfGroupBoxes)
                //{
                    //Logger.Info("counts match", "btnDone_Click");
                if(isOkToSave)
                {
                    Logger.Info("saving XML file...", "btnDone_Click");
                    XDocument xdoc = new XDocument(
                        new XDeclaration("1.0", null, null),
                        new XElement("root",
                        WatchFiles.folders.Select(pair =>
                            new XElement("folder",
                                new XElement("from", pair.Key),
                                new XElement("to", pair.Value)))));
                    xdoc.Save(WatchFiles.configFile);
                    Logger.Info("saved XML file", "btnDone_Click");

                    //LoadMaintenanceTab();
                    //WatchFiles.SetupFileWatcher();
                    //((Button)this.Controls.Find("btnDone", true)[0]).Enabled = false;
                    this.Close();
                }
                else
                {
                    Logger.Warning("XML file was not saved", "btnDone_Click");
                    MessageBox.Show("XML file was not saved. Please fill all textboxes with values or remove the unused textboxes");
                    ((Button)this.Controls.Find("btnDone", true)[0]).Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "chkRemove_Click");
                MessageBox.Show(ex.Message);
            }
        }

        void btnAdd_Click(object sender, EventArgs e)
        {
            ((Button)this.Controls.Find("btnDone", true)[0]).Enabled = true;
            TabPage xmlPage = (TabPage)this.Controls.Find("xmlPage", true)[0];

            GroupBox grpbox = new GroupBox()
            {
                Width = 375,
                Height = 90,
                Name = string.Format("grpbox{0}", numOfGroupBoxes),
                Location = new Point(0, groupboxYlocation)
            };

            CheckBox chkRemove = new CheckBox()
            {
                Text = "Remove from XML file",
                Name = string.Format("chkRemove{0}", numOfGroupBoxes),
                Width = 155,
                Location = new Point(5, 10)
            };
            chkRemove.Click += chkRemove_Click;

            Label lblFrom = new Label()
            {
                Text = "From",
                Name = string.Format("lblFrom{0}", numOfGroupBoxes),
                Width = 35,
                Location = new Point(chkRemove.Location.X, chkRemove.Location.Y + 25)
            };

            TextBox txtFrom = new TextBox()
            {
                Width = 250,
                Name = string.Format("txtFrom{0}", numOfGroupBoxes),
                Location = new Point(lblFrom.Location.X + lblFrom.Width + 3, lblFrom.Location.Y)
            };

            Button btnFrom = new Button()
            {
                Text = "Browse",
                Name = string.Format("btnFrom{0}", numOfGroupBoxes),
                Location = new Point(txtFrom.Location.X + txtFrom.Width + 3, txtFrom.Location.Y - 1)
            };
            btnFrom.Click += btnFrom_Click;

            Label lblTo = new Label()
            {
                Text = "To",
                Name = string.Format("lblTo{0}", numOfGroupBoxes),
                Width = 35,
                Location = new Point(lblFrom.Location.X, lblFrom.Location.Y + 25)
            };

            TextBox txtTo = new TextBox()
            {
                Width = 250,
                Name = string.Format("txtTo{0}", numOfGroupBoxes),
                Location = new Point(lblTo.Location.X + lblTo.Width + 3, lblTo.Location.Y)
            };

            Button btnTo = new Button()
            {
                Text = "Browse",
                Name = string.Format("btnTo{0}", numOfGroupBoxes),
                Location = new Point(txtTo.Location.X + txtTo.Width + 3, txtTo.Location.Y - 1)
            };
            btnTo.Click += btnTo_Click;

            grpbox.Controls.Add(chkRemove);
            grpbox.Controls.Add(lblFrom);
            grpbox.Controls.Add(txtFrom);
            grpbox.Controls.Add(btnFrom);
            grpbox.Controls.Add(lblTo);
            grpbox.Controls.Add(txtTo);
            grpbox.Controls.Add(btnTo);
            xmlPage.Controls.Add(grpbox);

            groupboxindexes.Add(numOfGroupBoxes);
            numOfGroupBoxes++;
            groupboxYlocation += grpbox.Height;
        }
    }
}
