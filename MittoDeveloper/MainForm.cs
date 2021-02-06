using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace MittoDeveloper
{

    public partial class MainForm : Form
    {
        int oldRuntimeSize;
        bool wasRuntime;
        bool defChanged;
        bool alreadyWarned = false;
        Color clrWarning = Color.FromArgb(102, 204, 0);
        Color clrBuffer = Color.FromArgb(204, 102, 0);
        Color clrQueue = Color.FromArgb(204, 102, 204);
        Color clrOutput = Color.FromArgb(0, 102, 204);
        //string whiteSpace = " \n\t";
        string documentName = "Untitled";
        char lastKey;
        int currentState;

        ArrayList inputHistory = new ArrayList();

        clsAssemblyInfo assInfo = new clsAssemblyInfo();

        MittoInterpreter mittoInterpreter = new MittoInterpreter();

        private void MittoOutput(string p, Color clrColor, RichTextBox output)
        {
          output.Select(output.Text.Length, 0);
          output.SelectionColor = clrColor;
          output.SelectedText = p;
          output.ScrollToCaret();
          Application.DoEvents();
        }

        public void MittoOutput(string p, int intType)
        {
          Color clrT;
          switch (intType)
          {
            case 1:
              if (rtbRuntime.Text.Length > 0 && rtbRuntime.Text.Substring(rtbRuntime.Text.Length - 1, 1) != "\n")
                rtbRuntime.AppendText("\n");
              clrT = clrWarning;
              break;
            case 2:
              if (rtbRuntime.Text.Length > 0 && rtbRuntime.Text.Substring(rtbRuntime.Text.Length - 1, 1) != "\n")
                rtbRuntime.AppendText("\n");
              p = "# " + p;
              clrT = clrBuffer;
              break;
            case 3:
              if (rtbRuntime.Text.Length > 0 && rtbRuntime.Text.Substring(rtbRuntime.Text.Length - 1, 1) != "\n")
                rtbRuntime.AppendText("\n");
              p = "@ " + p;
              clrT = clrQueue;
              break;
            case 4:
              clrT = clrOutput;
              break;
            default:
              clrT = Color.Black;
              break;
          }
          MittoOutput(p, clrT, rtbRuntime);
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void defintionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //defintionsToolStripMenuItem.Checked = !defintionsToolStripMenuItem.Checked;
            if (!runtimeToolStripMenuItem.Checked)
            {
                wasRuntime = runtimeToolStripMenuItem.Checked;
                runtimeToolStripMenuItem_Click(null, null);
            }
            else if (runtimeToolStripMenuItem.Checked && wasRuntime)
            {
                runtimeToolStripMenuItem_Click(null, null);
            }

            splitContainer1.Panel1Collapsed = !defintionsToolStripMenuItem.Checked;
            runtimeToolStripMenuItem.Enabled = defintionsToolStripMenuItem.Checked;
        }

        // Go button
        private void btnGo_Click(object sender, EventArgs e)
        {
            if (runtimeToolStripMenuItem.Checked == false)
                runtimeToolStripMenuItem_Click(null, null);

            if (defChanged == true && alreadyWarned == false)
            {
                MittoOutput("(Warning: Definitions have changed)\n", 1);
                alreadyWarned = true;
            }

            if (rtbRuntime.Text.Length > 0 && rtbRuntime.Text.Substring(rtbRuntime.Text.Length - 1, 1) != "\n")
              rtbRuntime.AppendText("\n");

            MittoOutput("> " + rtbEntry.Text + "\n", 0);
            //MittoParse(rtbEntry.Text.Trim(), rtbRuntime, 0, null, MittoBuffer, null, MittoQueue);
            mittoInterpreter.Eval(rtbEntry.Text);

            if (inputHistory.Count >= 20)
                inputHistory.RemoveAt(0);
            inputHistory.Add(rtbEntry.Text);
            currentState = -1;
            rtbEntry.Text = "";
        }

        private void runtimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runtimeToolStripMenuItem.Checked = !runtimeToolStripMenuItem.Checked;
            if (runtimeToolStripMenuItem.Checked)
            {
                splitContainer1.SplitterDistance = splitContainer1.Height - oldRuntimeSize;
            }
            else
            {
                oldRuntimeSize = splitContainer1.Panel2.Height;
                splitContainer1.SplitterDistance = splitContainer1.Height - splitContainer2.Panel2.Height;
            }

            splitContainer2.Panel1Collapsed = !runtimeToolStripMenuItem.Checked;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void rtbDefinitions_TextChanged(object sender, EventArgs e)
        {
          if (defChanged == false)
          {
            defChanged = true;
            if (this.Text.Substring(this.Text.Length - 1, 1) != "*")
              this.Text += "*";
          }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //rtbDefinitions.Undo();
          rtbDefinitions.UndoRedo.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //rtbDefinitions.Redo();
          rtbDefinitions.UndoRedo.Redo();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            //rtbDefinitions.Cut();
          rtbDefinitions.Clipboard.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //rtbDefinitions.Copy();
          rtbDefinitions.Clipboard.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //rtbDefinitions.Paste();
          rtbDefinitions.Clipboard.Paste();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //rtbDefinitions.SelectedText = "";
          rtbDefinitions.Selection.Text = "";
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //rtbDefinitions.SelectAll();
          rtbDefinitions.Selection.SelectAll();
        }

        private void aboutMittoDeveloperToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(ProductName + " v" + ProductVersion + "\n\n© 2008 Zdeněk Gromnica", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            frmAboutBox frmAbout = new frmAboutBox();
            frmAbout.ShowDialog(this);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.Text.Substring(this.Text.Length - 1, 1) == "*") {
                switch (MessageBox.Show("File " + documentName + " not saved.\n\nDo you wish to save?", this.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        saveToolStripMenuItem_Click(null, null);
                        break;
                    case DialogResult.Cancel:
                        return;
                }
            }

            documentName = "Untitled";
            rtbDefinitions.Text = "";
            rtbRuntime.Text = "";
            defChanged = false;
            alreadyWarned = false;

            this.Text = "Mitto Developer - " + documentName;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ToolStripManager.SaveSettings(this);

            //this.Visible = false;
            MittoDeveloper.Properties.Settings.Default.MainForm_WindowState = this.WindowState;
            if (this.WindowState != FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Normal;
            }
            MittoDeveloper.Properties.Settings.Default.MainForm_Location = this.Location;
            MittoDeveloper.Properties.Settings.Default.MainForm_Size = this.Size;
            MittoDeveloper.Properties.Settings.Default.MainForm_Splitter1 = splitContainer1.SplitterDistance;
            MittoDeveloper.Properties.Settings.Default.MainForm_Splitter2 = splitContainer2.SplitterDistance;

            MittoDeveloper.Properties.Settings.Default.Save();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (MittoDeveloper.Properties.Settings.Default.MainForm_Location.X != -1 && MittoDeveloper.Properties.Settings.Default.MainForm_Location.Y != -1)
            {
                this.Location = MittoDeveloper.Properties.Settings.Default.MainForm_Location;
                this.Size = MittoDeveloper.Properties.Settings.Default.MainForm_Size;
            }
            this.WindowState = MittoDeveloper.Properties.Settings.Default.MainForm_WindowState;
            splitContainer1.SplitterDistance = MittoDeveloper.Properties.Settings.Default.MainForm_Splitter1;
            splitContainer2.SplitterDistance = MittoDeveloper.Properties.Settings.Default.MainForm_Splitter2;

            //this.Text = "Mitto Developer - " + documentName;

            ToolStripManager.LoadSettings(this);

            mittoInterpreter.OutputF = MittoOutput;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO: Open
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO: Save
        }

        private void tlbRun_Click(object sender, EventArgs e)
        {
            if (runtimeToolStripMenuItem.Checked == false)
                runtimeToolStripMenuItem_Click(null, null);
            defChanged = false;
            alreadyWarned = false;

            if (rtbRuntime.Text.Length > 0 && rtbRuntime.Text.Substring(rtbRuntime.Text.Length - 1, 1) != "\n")
              rtbRuntime.AppendText("\n");

            //MittoOutput("\n", 0);
            //rtbRuntime.Text = "";
            //MittoParse(rtbDefinitions.Text.Trim(), rtbRuntime, 0, null, MittoBuffer, null, MittoQueue);
            mittoInterpreter.Eval(rtbDefinitions.Text);
        }

        private void tblClearRuntime_Click(object sender, EventArgs e)
        {
            rtbRuntime.Text = "";
        }

        private void tlbPause_Click(object sender, EventArgs e)
        {
            //TODO: Pause
        }

        private void tlbStop_Click(object sender, EventArgs e)
        {
            //TODO: Stop
        }

        private void tblRestart_Click(object sender, EventArgs e)
        {
            //TODO: Restart
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO: Help
            //MessageBox.Show("Sorry, not implemented yet!", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void rtbEntry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && e.Shift == false)
            {
                btnGo_Click(null, null);
                e.SuppressKeyPress = true;
            }

            if (lastKey == '')
            {
                switch (e.KeyCode)
                {
                    case Keys.P:
                        previousInputToolStripMenuItem_Click(null, null);
                        e.SuppressKeyPress = true;
                        lastKey = ' ';
                        break;
                    case Keys.N:
                        nextInputToolStripMenuItem_Click(null, null);
                        e.SuppressKeyPress = true;
                        lastKey = ' ';
                        break;
                }
            }
        }

        private void rtbEntry_KeyPress(object sender, KeyPressEventArgs e)
        {
            lastKey = e.KeyChar;
        }

        private void previousInputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (inputHistory.Count > 0)
            {
                if (currentState <= 0)
                    currentState = inputHistory.Count;
                currentState--;

                rtbEntry.Text = inputHistory[currentState].ToString();
                rtbEntry.Select(rtbEntry.Text.Length, 0);
            }
        }

        private void nextInputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (inputHistory.Count > 0)
            {
                if (currentState >= inputHistory.Count - 1)
                    currentState = -1;
                currentState++;

                rtbEntry.Text = inputHistory[currentState].ToString();
                rtbEntry.Select(rtbEntry.Text.Length, 0);
            }
        }

        private void rtbDefinitions_MouseMove(object sender, MouseEventArgs e)
        {
          /*int tPos = rtbDefinitions.GetCharIndexFromPosition(new Point(e.X, e.Y));
          int wordStart, wordEnd;
          string strWord;

          wordStart = MittoInterpreter.FindPrevWhite(rtbDefinitions.Text, tPos);
          wordEnd = MittoInterpreter.FindNextWhite(rtbDefinitions.Text, tPos);
          if (wordStart == -1)
            wordStart = 0;
          else
            wordStart++;

          if (wordEnd == -1)
            wordEnd = rtbDefinitions.Text.Length;

          if (wordEnd > wordStart)
          {
            strWord = rtbDefinitions.Text.Substring(wordStart, wordEnd - wordStart);
          }*/
        }




    }
}
