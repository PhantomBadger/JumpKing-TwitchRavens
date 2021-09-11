
namespace JumpKingMod.Install.UI
{
    partial class InstallerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblInstallDir = new System.Windows.Forms.Label();
            this.grpJumpKing = new System.Windows.Forms.GroupBox();
            this.btnModDir = new System.Windows.Forms.Button();
            this.txtModDir = new System.Windows.Forms.TextBox();
            this.lblModDir = new System.Windows.Forms.Label();
            this.btnInstallDir = new System.Windows.Forms.Button();
            this.txtInstallDir = new System.Windows.Forms.TextBox();
            this.btnInstall = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tlstrpFile = new System.Windows.Forms.ToolStripMenuItem();
            this.tlstrpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.tlstrpExit = new System.Windows.Forms.ToolStripMenuItem();
            this.grpJumpKing.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblInstallDir
            // 
            this.lblInstallDir.AutoSize = true;
            this.lblInstallDir.Location = new System.Drawing.Point(6, 26);
            this.lblInstallDir.Name = "lblInstallDir";
            this.lblInstallDir.Size = new System.Drawing.Size(82, 13);
            this.lblInstallDir.TabIndex = 0;
            this.lblInstallDir.Text = "Install Directory:";
            // 
            // grpJumpKing
            // 
            this.grpJumpKing.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpJumpKing.Controls.Add(this.btnModDir);
            this.grpJumpKing.Controls.Add(this.txtModDir);
            this.grpJumpKing.Controls.Add(this.lblModDir);
            this.grpJumpKing.Controls.Add(this.btnInstallDir);
            this.grpJumpKing.Controls.Add(this.txtInstallDir);
            this.grpJumpKing.Controls.Add(this.lblInstallDir);
            this.grpJumpKing.Location = new System.Drawing.Point(12, 27);
            this.grpJumpKing.Name = "grpJumpKing";
            this.grpJumpKing.Size = new System.Drawing.Size(520, 88);
            this.grpJumpKing.TabIndex = 1;
            this.grpJumpKing.TabStop = false;
            this.grpJumpKing.Text = "Jump King";
            // 
            // btnModDir
            // 
            this.btnModDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnModDir.Location = new System.Drawing.Point(485, 49);
            this.btnModDir.Name = "btnModDir";
            this.btnModDir.Size = new System.Drawing.Size(26, 20);
            this.btnModDir.TabIndex = 5;
            this.btnModDir.Text = "...";
            this.btnModDir.UseVisualStyleBackColor = true;
            this.btnModDir.Click += new System.EventHandler(this.btnModDir_Click);
            // 
            // txtModDir
            // 
            this.txtModDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtModDir.Location = new System.Drawing.Point(94, 49);
            this.txtModDir.Name = "txtModDir";
            this.txtModDir.Size = new System.Drawing.Size(385, 20);
            this.txtModDir.TabIndex = 4;
            // 
            // lblModDir
            // 
            this.lblModDir.AutoSize = true;
            this.lblModDir.Location = new System.Drawing.Point(6, 52);
            this.lblModDir.Name = "lblModDir";
            this.lblModDir.Size = new System.Drawing.Size(76, 13);
            this.lblModDir.TabIndex = 3;
            this.lblModDir.Text = "Mod Directory:";
            // 
            // btnInstallDir
            // 
            this.btnInstallDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInstallDir.Location = new System.Drawing.Point(485, 23);
            this.btnInstallDir.Name = "btnInstallDir";
            this.btnInstallDir.Size = new System.Drawing.Size(26, 20);
            this.btnInstallDir.TabIndex = 2;
            this.btnInstallDir.Text = "...";
            this.btnInstallDir.UseVisualStyleBackColor = true;
            this.btnInstallDir.Click += new System.EventHandler(this.btnInstallDir_Click);
            // 
            // txtInstallDir
            // 
            this.txtInstallDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInstallDir.Location = new System.Drawing.Point(94, 23);
            this.txtInstallDir.Name = "txtInstallDir";
            this.txtInstallDir.Size = new System.Drawing.Size(385, 20);
            this.txtInstallDir.TabIndex = 1;
            // 
            // btnInstall
            // 
            this.btnInstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInstall.Enabled = false;
            this.btnInstall.Location = new System.Drawing.Point(457, 121);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(75, 23);
            this.btnInstall.TabIndex = 2;
            this.btnInstall.Text = "Install";
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tlstrpFile});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(544, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tlstrpFile
            // 
            this.tlstrpFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tlstrpAbout,
            this.tlstrpExit});
            this.tlstrpFile.Name = "tlstrpFile";
            this.tlstrpFile.Size = new System.Drawing.Size(37, 20);
            this.tlstrpFile.Text = "File";
            // 
            // tlstrpAbout
            // 
            this.tlstrpAbout.Name = "tlstrpAbout";
            this.tlstrpAbout.Size = new System.Drawing.Size(107, 22);
            this.tlstrpAbout.Text = "About";
            this.tlstrpAbout.Click += new System.EventHandler(this.aboutToolStripMenuItem1_Click);
            // 
            // tlstrpExit
            // 
            this.tlstrpExit.Name = "tlstrpExit";
            this.tlstrpExit.Size = new System.Drawing.Size(107, 22);
            this.tlstrpExit.Text = "Exit";
            this.tlstrpExit.Click += new System.EventHandler(this.tlstrpExit_Click);
            // 
            // InstallerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 161);
            this.Controls.Add(this.btnInstall);
            this.Controls.Add(this.grpJumpKing);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(560, 200);
            this.Name = "InstallerForm";
            this.Text = "JumpKingMod Installer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.InstallerForm_FormClosed);
            this.Load += new System.EventHandler(this.InstallerForm_Load);
            this.grpJumpKing.ResumeLayout(false);
            this.grpJumpKing.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblInstallDir;
        private System.Windows.Forms.GroupBox grpJumpKing;
        private System.Windows.Forms.Button btnInstallDir;
        private System.Windows.Forms.TextBox txtInstallDir;
        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tlstrpFile;
        private System.Windows.Forms.ToolStripMenuItem tlstrpAbout;
        private System.Windows.Forms.ToolStripMenuItem tlstrpExit;
        private System.Windows.Forms.Button btnModDir;
        private System.Windows.Forms.TextBox txtModDir;
        private System.Windows.Forms.Label lblModDir;
    }
}

