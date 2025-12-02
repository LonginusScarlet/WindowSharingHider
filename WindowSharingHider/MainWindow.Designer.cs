namespace WindowSharingHider
{
    partial class MainWindow
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
            this.windowListCheckBox = new System.Windows.Forms.CheckedListBox();
            this.topPanel = new System.Windows.Forms.Panel();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.searchLabel = new System.Windows.Forms.Label();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.btnAddRule = new System.Windows.Forms.Button();
            this.btnDeselectAll = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnShowAll = new System.Windows.Forms.Button();
            this.btnHideAll = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.topPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // windowListCheckBox
            //
            this.windowListCheckBox.CheckOnClick = true;
            this.windowListCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.windowListCheckBox.FormattingEnabled = true;
            this.windowListCheckBox.Location = new System.Drawing.Point(0, 70);
            this.windowListCheckBox.Name = "windowListCheckBox";
            this.windowListCheckBox.Size = new System.Drawing.Size(550, 338);
            this.windowListCheckBox.TabIndex = 0;
            //
            // topPanel
            //
            this.topPanel.Controls.Add(this.searchBox);
            this.topPanel.Controls.Add(this.searchLabel);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Padding = new System.Windows.Forms.Padding(10);
            this.topPanel.Size = new System.Drawing.Size(550, 35);
            this.topPanel.TabIndex = 1;
            //
            // searchBox
            //
            this.searchBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchBox.Location = new System.Drawing.Point(60, 10);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(480, 20);
            this.searchBox.TabIndex = 1;
            this.searchBox.TextChanged += new System.EventHandler(this.SearchBox_TextChanged);
            //
            // searchLabel
            //
            this.searchLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.searchLabel.Location = new System.Drawing.Point(10, 10);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Size = new System.Drawing.Size(50, 15);
            this.searchLabel.TabIndex = 0;
            this.searchLabel.Text = "搜索:";
            this.searchLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // buttonPanel
            //
            this.buttonPanel.Controls.Add(this.btnAddRule);
            this.buttonPanel.Controls.Add(this.btnDeselectAll);
            this.buttonPanel.Controls.Add(this.btnSelectAll);
            this.buttonPanel.Controls.Add(this.btnShowAll);
            this.buttonPanel.Controls.Add(this.btnHideAll);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonPanel.Location = new System.Drawing.Point(0, 35);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Padding = new System.Windows.Forms.Padding(5);
            this.buttonPanel.Size = new System.Drawing.Size(550, 35);
            this.buttonPanel.TabIndex = 2;
            //
            // btnAddRule
            //
            this.btnAddRule.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnAddRule.Location = new System.Drawing.Point(365, 5);
            this.btnAddRule.Name = "btnAddRule";
            this.btnAddRule.Size = new System.Drawing.Size(90, 25);
            this.btnAddRule.TabIndex = 4;
            this.btnAddRule.Text = "添加规则";
            this.btnAddRule.UseVisualStyleBackColor = true;
            this.btnAddRule.Click += new System.EventHandler(this.BtnAddRule_Click);
            //
            // btnDeselectAll
            //
            this.btnDeselectAll.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnDeselectAll.Location = new System.Drawing.Point(275, 5);
            this.btnDeselectAll.Name = "btnDeselectAll";
            this.btnDeselectAll.Size = new System.Drawing.Size(90, 25);
            this.btnDeselectAll.TabIndex = 3;
            this.btnDeselectAll.Text = "取消全选";
            this.btnDeselectAll.UseVisualStyleBackColor = true;
            this.btnDeselectAll.Click += new System.EventHandler(this.BtnDeselectAll_Click);
            //
            // btnSelectAll
            //
            this.btnSelectAll.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnSelectAll.Location = new System.Drawing.Point(185, 5);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(90, 25);
            this.btnSelectAll.TabIndex = 2;
            this.btnSelectAll.Text = "全选";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.BtnSelectAll_Click);
            //
            // btnShowAll
            //
            this.btnShowAll.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnShowAll.Location = new System.Drawing.Point(95, 5);
            this.btnShowAll.Name = "btnShowAll";
            this.btnShowAll.Size = new System.Drawing.Size(90, 25);
            this.btnShowAll.TabIndex = 1;
            this.btnShowAll.Text = "显示全部";
            this.btnShowAll.UseVisualStyleBackColor = true;
            this.btnShowAll.Click += new System.EventHandler(this.BtnShowAll_Click);
            //
            // btnHideAll
            //
            this.btnHideAll.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnHideAll.Location = new System.Drawing.Point(5, 5);
            this.btnHideAll.Name = "btnHideAll";
            this.btnHideAll.Size = new System.Drawing.Size(90, 25);
            this.btnHideAll.TabIndex = 0;
            this.btnHideAll.Text = "隐藏选中";
            this.btnHideAll.UseVisualStyleBackColor = true;
            this.btnHideAll.Click += new System.EventHandler(this.BtnHideAll_Click);
            //
            // statusStrip
            //
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 408);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(550, 22);
            this.statusStrip.TabIndex = 3;
            this.statusStrip.Text = "statusStrip1";
            //
            // statusLabel
            //
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(280, 17);
            this.statusLabel.Text = "窗口: 0 | 已隐藏: 0 | 规则: 0 | Ctrl+Shift+H";
            //
            // MainWindow
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 430);
            this.Controls.Add(this.windowListCheckBox);
            this.Controls.Add(this.buttonPanel);
            this.Controls.Add(this.topPanel);
            this.Controls.Add(this.statusStrip);
            this.MinimumSize = new System.Drawing.Size(500, 350);
            this.Name = "MainWindow";
            this.Text = "Window Sharing Hider";
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.buttonPanel.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.CheckedListBox windowListCheckBox;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.Label searchLabel;
        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.Button btnHideAll;
        private System.Windows.Forms.Button btnShowAll;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnDeselectAll;
        private System.Windows.Forms.Button btnAddRule;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
    }
}
