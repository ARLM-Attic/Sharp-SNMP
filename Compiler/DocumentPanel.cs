﻿using System;
using System.Windows.Forms;
using Lextm.SharpSnmpLib.Messaging;
using WeifenLuo.WinFormsUI.Docking;

namespace Lextm.SharpSnmpLib.Compiler
{
    internal partial class DocumentPanel : DockContent
    {
        private readonly string _fileName;

        public DocumentPanel(string fileName)
        {
            InitializeComponent();
            if (!Helper.IsRunningOnMono())
            {
                Icon = Properties.Resources.document_properties;
            }

            _fileName = fileName;
            TabText = _fileName;
            txtDocument.LoadFile(_fileName, RichTextBoxStreamType.PlainText);
        }

        public string FileName
        {
            get { return _fileName; }
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseAllDocuments(DockPanel);
        }
        
        private static void CloseAllDocuments(DockPanel panel)
        {
            if (panel.DocumentStyle == DocumentStyle.SystemMdi)
            {
                throw new InvalidOperationException("cannot work in System MDI mode");
            }
            
            IDockContent[] documents = panel.DocumentsToArray();
            foreach (IDockContent content in documents)
            {
                content.DockHandler.Close();
            }
        }
    }
}
