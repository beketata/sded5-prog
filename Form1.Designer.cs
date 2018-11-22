namespace Programmer
{
	partial class FrmMain
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.toolStrip = new System.Windows.Forms.ToolStrip();
			this.progBarAll = new System.Windows.Forms.ToolStripProgressBar();
			this.progBarOne = new System.Windows.Forms.ToolStripProgressBar();
			this.toolStripLbl = new System.Windows.Forms.ToolStripLabel();
			this.txtInfo = new System.Windows.Forms.ToolStripTextBox();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.grpBox2 = new System.Windows.Forms.GroupBox();
			this.txtBox = new System.Windows.Forms.RichTextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.txtBox2 = new System.Windows.Forms.RichTextBox();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.deviceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.dINFOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip2 = new System.Windows.Forms.MenuStrip();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip.SuspendLayout();
			this.grpBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.menuStrip2.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip
			// 
			this.toolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.toolStrip.Font = new System.Drawing.Font("Tahoma", 10F);
			this.toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progBarAll,
            this.progBarOne,
            this.toolStripLbl,
            this.txtInfo});
			this.toolStrip.Location = new System.Drawing.Point(0, 735);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = new System.Drawing.Size(1190, 31);
			this.toolStrip.TabIndex = 0;
			this.toolStrip.Text = "toolStrip1";
			// 
			// progBarAll
			// 
			this.progBarAll.Name = "progBarAll";
			this.progBarAll.Size = new System.Drawing.Size(350, 28);
			// 
			// progBarOne
			// 
			this.progBarOne.Name = "progBarOne";
			this.progBarOne.Size = new System.Drawing.Size(350, 28);
			// 
			// toolStripLbl
			// 
			this.toolStripLbl.AutoSize = false;
			this.toolStripLbl.Name = "toolStripLbl";
			this.toolStripLbl.Size = new System.Drawing.Size(70, 22);
			this.toolStripLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtInfo
			// 
			this.txtInfo.BackColor = System.Drawing.Color.Pink;
			this.txtInfo.Font = new System.Drawing.Font("Tahoma", 10F);
			this.txtInfo.Name = "txtInfo";
			this.txtInfo.ReadOnly = true;
			this.txtInfo.Size = new System.Drawing.Size(249, 31);
			this.txtInfo.Text = "USB is Not Connected";
			// 
			// openFileDialog
			// 
			this.openFileDialog.FileName = "openFileDialog1";
			this.openFileDialog.ReadOnlyChecked = true;
			// 
			// grpBox2
			// 
			this.grpBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.grpBox2.Controls.Add(this.txtBox);
			this.grpBox2.Location = new System.Drawing.Point(10, 57);
			this.grpBox2.Margin = new System.Windows.Forms.Padding(4);
			this.grpBox2.Name = "grpBox2";
			this.grpBox2.Padding = new System.Windows.Forms.Padding(4);
			this.grpBox2.Size = new System.Drawing.Size(1174, 505);
			this.grpBox2.TabIndex = 2;
			this.grpBox2.TabStop = false;
			// 
			// txtBox
			// 
			this.txtBox.BackColor = System.Drawing.Color.Black;
			this.txtBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtBox.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.txtBox.ForeColor = System.Drawing.Color.White;
			this.txtBox.Location = new System.Drawing.Point(4, 23);
			this.txtBox.Margin = new System.Windows.Forms.Padding(4);
			this.txtBox.Name = "txtBox";
			this.txtBox.Size = new System.Drawing.Size(1166, 478);
			this.txtBox.TabIndex = 0;
			this.txtBox.Text = "";
			this.txtBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtBox_KeyPress);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.txtBox2);
			this.groupBox1.Location = new System.Drawing.Point(10, 558);
			this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
			this.groupBox1.Size = new System.Drawing.Size(1174, 163);
			this.groupBox1.TabIndex = 12;
			this.groupBox1.TabStop = false;
			// 
			// txtBox2
			// 
			this.txtBox2.BackColor = System.Drawing.Color.Black;
			this.txtBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtBox2.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.txtBox2.ForeColor = System.Drawing.Color.White;
			this.txtBox2.Location = new System.Drawing.Point(4, 23);
			this.txtBox2.Margin = new System.Windows.Forms.Padding(4);
			this.txtBox2.Name = "txtBox2";
			this.txtBox2.Size = new System.Drawing.Size(1166, 136);
			this.txtBox2.TabIndex = 0;
			this.txtBox2.Text = "DIMAGE /WINSRC:0 /FILETRG:sded5.img\nDFORMAT /WIN:0 /BDTLL0:10M /FAT0:fs4:lab0 /FA" +
    "T1:fs4:lab1 /EP0:0 /EP1:0\nDFORMAT /WIN:0 /UNFORMAT\nDINFO /WIN:0 /IPL\nDINFO /WIN:" +
    "0 /BDTL\n";
			this.txtBox2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtBox2_KeyPress);
			this.txtBox2.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.txtBox2_MouseDoubleClick);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deviceToolStripMenuItem,
            this.testToolStripMenuItem,
            this.dINFOToolStripMenuItem,
            this.stopToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
			this.menuStrip1.Size = new System.Drawing.Size(1190, 29);
			this.menuStrip1.TabIndex = 13;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// deviceToolStripMenuItem
			// 
			this.deviceToolStripMenuItem.Name = "deviceToolStripMenuItem";
			this.deviceToolStripMenuItem.Size = new System.Drawing.Size(72, 25);
			this.deviceToolStripMenuItem.Text = "Device";
			// 
			// testToolStripMenuItem
			// 
			this.testToolStripMenuItem.Name = "testToolStripMenuItem";
			this.testToolStripMenuItem.Size = new System.Drawing.Size(55, 25);
			this.testToolStripMenuItem.Text = "Test";
			this.testToolStripMenuItem.Click += new System.EventHandler(this.testToolStripMenuItem_Click);
			// 
			// dINFOToolStripMenuItem
			// 
			this.dINFOToolStripMenuItem.Name = "dINFOToolStripMenuItem";
			this.dINFOToolStripMenuItem.Size = new System.Drawing.Size(72, 25);
			this.dINFOToolStripMenuItem.Text = "DINFO";
			this.dINFOToolStripMenuItem.Click += new System.EventHandler(this.dINFOToolStripMenuItem_Click);
			// 
			// stopToolStripMenuItem
			// 
			this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
			this.stopToolStripMenuItem.Size = new System.Drawing.Size(55, 25);
			this.stopToolStripMenuItem.Text = "Stop";
			// 
			// menuStrip2
			// 
			this.menuStrip2.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.menuStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem5});
			this.menuStrip2.Location = new System.Drawing.Point(0, 29);
			this.menuStrip2.Name = "menuStrip2";
			this.menuStrip2.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
			this.menuStrip2.Size = new System.Drawing.Size(1190, 29);
			this.menuStrip2.TabIndex = 14;
			this.menuStrip2.Text = "menuStrip2";
			// 
			// toolStripMenuItem5
			// 
			this.toolStripMenuItem5.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem6});
			this.toolStripMenuItem5.Name = "toolStripMenuItem5";
			this.toolStripMenuItem5.Size = new System.Drawing.Size(73, 25);
			this.toolStripMenuItem5.Text = "XILINX";
			// 
			// toolStripMenuItem6
			// 
			this.toolStripMenuItem6.Name = "toolStripMenuItem6";
			this.toolStripMenuItem6.Size = new System.Drawing.Size(235, 26);
			this.toolStripMenuItem6.Text = "Bitstream Converter";
			this.toolStripMenuItem6.Click += new System.EventHandler(this.toolStripMenuItem6_Click);
			// 
			// FrmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(1190, 766);
			this.Controls.Add(this.menuStrip2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.grpBox2);
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.menuStrip1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MainMenuStrip = this.menuStrip1;
			this.Margin = new System.Windows.Forms.Padding(5);
			this.Name = "FrmMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "6100 Programmer";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.Shown += new System.EventHandler(this.Form1_Shown);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.grpBox2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.menuStrip2.ResumeLayout(false);
			this.menuStrip2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.ToolStripTextBox txtInfo;
		private System.Windows.Forms.ToolStripProgressBar progBarOne;
		private System.Windows.Forms.GroupBox grpBox2;
		internal System.Windows.Forms.RichTextBox txtBox;
		private System.Windows.Forms.ToolStripLabel toolStripLbl;
		internal System.Windows.Forms.RichTextBox txtBox2;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem deviceToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
		private System.Windows.Forms.ToolStripProgressBar progBarAll;
		private System.Windows.Forms.ToolStripMenuItem dINFOToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
		private System.Windows.Forms.MenuStrip menuStrip2;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
	}
}

