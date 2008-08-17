﻿using System.Drawing;
using System.Net;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;

namespace Lextm.SharpSnmpLib.Browser
{
    public partial class AgentProfilePanel : DockContent
    {
        public AgentProfilePanel()
        {
            InitializeComponent();
            ProfileRegistry.Instance.OnChanged += UpdateView;

            //
            // Actions in the context menu
            //
            contextAgentMenu.Items.Add("Set Default", null, actDefault_Execute);
            contextAgentMenu.Items.Add("Delete", null, actDelete_Execute);
            contextAgentMenu.Items.Add("Edit", null, actEdit_Execute);
        }

        private void AgentProfilePanel_Load(object sender, System.EventArgs e)
        {
            UpdateView(this, e);
        }

        private void UpdateView(object sender, System.EventArgs e)
        {
            string display = "";

            listView1.Items.Clear();
            foreach (AgentProfile profile in ProfileRegistry.Instance.Profiles)
            {
                if (profile.Name != "")
                {
                    display = profile.Name;
                }
                else
                {
                    display = profile.Agent.ToString();
                }
                ListViewItem item = listView1.Items.Add(display);
                item.Tag = profile;

                switch (profile.VersionCode)
                {
                    case VersionCode.V1:
                        {
                            item.Group = listView1.Groups["lvgV1"];
                            break;
                        }
                    case VersionCode.V2:
                        {
                            item.Group = listView1.Groups["lvgV2"];
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

                //
                // Lets make the default Agent bold
                //
                if (profile.Agent.Equals(ProfileRegistry.Instance.Default))
                {
                    item.Font = new Font(listView1.Font, FontStyle.Bold);
                }

                item.ToolTipText = profile.Agent.ToString();
            }
        }

        private void actDelete_Update(object sender, System.EventArgs e)
        {
            actDelete.Enabled = listView1.SelectedItems.Count == 1;
        }

        private void actEdit_Update(object sender, System.EventArgs e)
        {
            actEdit.Enabled = listView1.SelectedItems.Count == 1;
        }

        private void actDefault_Update(object sender, System.EventArgs e)
        {
            actDefault.Enabled = listView1.SelectedItems.Count == 1;
        }

        private void actDefault_Execute(object sender, System.EventArgs e)
        {
            ProfileRegistry.Instance.Default = (listView1.SelectedItems[0].Tag as AgentProfile).Agent;

            //
            // Update view for new default agent
            //
            UpdateView(null, null);
        }

        private void actionList1_Update(object sender, System.EventArgs e)
        {
            tslblDefault.Text = "Default agent is " + ProfileRegistry.Instance.DefaultString;
        }

        private void actDelete_Execute(object sender, System.EventArgs e)
        {
            try
            {
                ProfileRegistry.Instance.DeleteProfile((listView1.SelectedItems[0].Tag as AgentProfile).Agent);
            }
            catch (MibBrowserException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void actAdd_Execute(object sender, System.EventArgs e)
        {
            using (FormProfile editor = new FormProfile(null))
            {
                if (editor.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ProfileRegistry.Instance.AddProfile(new AgentProfile(editor.VersionCode, new IPEndPoint(editor.IP, editor.Port), editor.GetCommunity, editor.SetCommunity, editor.AgentName));
                    }
                    catch (MibBrowserException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void actEdit_Execute(object sender, System.EventArgs e)
        {
            AgentProfile profile = ProfileRegistry.Instance.GetProfile((listView1.SelectedItems[0].Tag as AgentProfile).Agent);
            using (FormProfile editor = new FormProfile(profile))
            {
                if (editor.ShowDialog() == DialogResult.OK)
                {
                    ProfileRegistry.Instance.ReplaceProfile(new AgentProfile(editor.VersionCode, new IPEndPoint(editor.IP, editor.Port), editor.GetCommunity, editor.SetCommunity, editor.AgentName));
                }
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextAgentMenu.Show(listView1, e.Location);
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && listView1.SelectedItems.Count == 1)
            {
                actEdit.DoExecute();
            }
        }
    }
}
