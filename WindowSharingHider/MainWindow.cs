using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WindowSharingHider
{
    public partial class MainWindow : Form
    {
        // 全局热键相关
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 1;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint VK_H = 0x48;

        // 系统托盘
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        // 配置持久化 - 支持多种匹配规则
        private List<HideRule> hideRules = new List<HideRule>();
        private string configPath;

        // 搜索过滤
        private string searchFilter = "";

        // 隐藏规则类
        public class HideRule
        {
            public RuleType Type { get; set; }
            public string Pattern { get; set; }
            public bool IsRegex { get; set; }

            public enum RuleType
            {
                ExactTitle,      // 精确匹配标题
                ContainsTitle,   // 标题包含关键词
                ProcessName,     // 按进程名匹配
                RegexTitle       // 正则匹配标题
            }

            public override string ToString()
            {
                string prefix;
                switch (Type)
                {
                    case RuleType.ExactTitle: prefix = "[标题]"; break;
                    case RuleType.ContainsTitle: prefix = "[包含]"; break;
                    case RuleType.ProcessName: prefix = "[进程]"; break;
                    case RuleType.RegexTitle: prefix = "[正则]"; break;
                    default: prefix = ""; break;
                }
                return $"{prefix} {Pattern}";
            }

            // 检查窗口是否匹配此规则
            public bool Matches(string title, string processName)
            {
                switch (Type)
                {
                    case RuleType.ExactTitle:
                        return title.Equals(Pattern, StringComparison.OrdinalIgnoreCase);
                    case RuleType.ContainsTitle:
                        return title.IndexOf(Pattern, StringComparison.OrdinalIgnoreCase) >= 0;
                    case RuleType.ProcessName:
                        return processName.Equals(Pattern, StringComparison.OrdinalIgnoreCase);
                    case RuleType.RegexTitle:
                        try { return Regex.IsMatch(title, Pattern, RegexOptions.IgnoreCase); }
                        catch { return false; }
                    default:
                        return false;
                }
            }
        }

        public class WindowInfo
        {
            public String Title { get; set; }
            public IntPtr Handle { get; set; }
            public String ProcessName { get; set; }
            public Boolean stillExists = false;
            public Boolean autoHidden = false; // 标记是否被规则自动隐藏
            public override string ToString()
            {
                if (!string.IsNullOrEmpty(ProcessName))
                    return $"{Title} [{ProcessName}]";
                return Title;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            // 初始化配置路径
            configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WindowSharingHider", "rules.txt");

            // 加载配置
            LoadConfig();

            // 初始化系统托盘
            InitializeTrayIcon();

            // 注册全局热键 (Ctrl+Shift+H)
            RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, VK_H);

            // 监听勾选变化事件 - 自动添加/移除规则
            windowListCheckBox.ItemCheck += WindowListCheckBox_ItemCheck;

            // 启动定时器
            var timer = new Timer();
            timer.Interval = 500;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        // 当用户勾选/取消勾选窗口时，自动添加/移除规则
        private bool isUpdatingFromTimer = false;
        private void WindowListCheckBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (isUpdatingFromTimer) return; // 避免定时器更新时触发

            var window = (WindowInfo)windowListCheckBox.Items[e.Index];

            if (e.NewValue == CheckState.Checked)
            {
                // 勾选时自动添加规则（按进程名+标题组合匹配）
                AutoAddRule(window);
            }
            else
            {
                // 取消勾选时移除相关规则
                AutoRemoveRule(window);
            }
        }

        private void AutoAddRule(WindowInfo window)
        {
            // 优先按进程名添加规则，如果进程名为空则按精确标题
            if (!string.IsNullOrEmpty(window.ProcessName))
            {
                // 检查是否已有该进程的规则
                var existingProcessRule = hideRules.FirstOrDefault(r =>
                    r.Type == HideRule.RuleType.ProcessName &&
                    r.Pattern.Equals(window.ProcessName, StringComparison.OrdinalIgnoreCase));

                if (existingProcessRule == null)
                {
                    // 检查该进程是否有多个窗口，如果只有一个则用精确标题
                    var sameProcessWindows = windowListCheckBox.Items.Cast<WindowInfo>()
                        .Where(w => w.ProcessName == window.ProcessName).ToList();

                    if (sameProcessWindows.Count > 1)
                    {
                        // 多个窗口，使用精确标题
                        if (!hideRules.Any(r => r.Type == HideRule.RuleType.ExactTitle && r.Pattern == window.Title))
                        {
                            hideRules.Add(new HideRule { Type = HideRule.RuleType.ExactTitle, Pattern = window.Title });
                        }
                    }
                    else
                    {
                        // 只有一个窗口，使用进程名（隐藏该进程的所有窗口）
                        hideRules.Add(new HideRule { Type = HideRule.RuleType.ProcessName, Pattern = window.ProcessName });
                    }
                }
                else
                {
                    // 已有进程规则，添加精确标题规则
                    if (!hideRules.Any(r => r.Type == HideRule.RuleType.ExactTitle && r.Pattern == window.Title))
                    {
                        hideRules.Add(new HideRule { Type = HideRule.RuleType.ExactTitle, Pattern = window.Title });
                    }
                }
            }
            else
            {
                // 没有进程名，使用精确标题
                if (!hideRules.Any(r => r.Type == HideRule.RuleType.ExactTitle && r.Pattern == window.Title))
                {
                    hideRules.Add(new HideRule { Type = HideRule.RuleType.ExactTitle, Pattern = window.Title });
                }
            }
            SaveConfig();
        }

        private void AutoRemoveRule(WindowInfo window)
        {
            // 移除与此窗口相关的规则
            hideRules.RemoveAll(r =>
                (r.Type == HideRule.RuleType.ExactTitle && r.Pattern == window.Title) ||
                (r.Type == HideRule.RuleType.ProcessName && r.Pattern == window.ProcessName));
            SaveConfig();
        }

        private void InitializeTrayIcon()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("显示主窗口", null, (s, e) => ShowMainWindow());
            trayMenu.Items.Add("隐藏所有选中窗口", null, (s, e) => HideAllSelected());
            trayMenu.Items.Add("显示所有窗口", null, (s, e) => ShowAllWindows());
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("管理规则...", null, (s, e) => ShowRulesDialog());
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("开机自启动", null, ToggleAutoStart);
            ((ToolStripMenuItem)trayMenu.Items[6]).Checked = IsAutoStartEnabled();
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("退出", null, (s, e) => ExitApplication());

            trayIcon = new NotifyIcon();
            trayIcon.Text = "Window Sharing Hider";
            trayIcon.Icon = SystemIcons.Shield;
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += (s, e) => ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void HideAllSelected()
        {
            foreach (var window in windowListCheckBox.Items.Cast<WindowInfo>().ToArray())
            {
                if (windowListCheckBox.GetItemChecked(windowListCheckBox.Items.IndexOf(window)))
                {
                    WindowHandler.SetWindowDisplayAffinity(window.Handle, 0x11);
                }
            }
            UpdateStatusBar();
        }

        private void ShowAllWindows()
        {
            foreach (var window in windowListCheckBox.Items.Cast<WindowInfo>().ToArray())
            {
                WindowHandler.SetWindowDisplayAffinity(window.Handle, 0x0);
                windowListCheckBox.SetItemChecked(windowListCheckBox.Items.IndexOf(window), false);
            }
            UpdateStatusBar();
        }

        private void ShowRulesDialog()
        {
            using (var dialog = new RulesDialog(hideRules))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    hideRules = dialog.Rules;
                    SaveConfig();
                }
            }
        }

        private void ToggleAutoStart(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            if (menuItem.Checked)
            {
                DisableAutoStart();
                menuItem.Checked = false;
            }
            else
            {
                EnableAutoStart();
                menuItem.Checked = true;
            }
        }

        private bool IsAutoStartEnabled()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
            {
                return key?.GetValue("WindowSharingHider") != null;
            }
        }

        private void EnableAutoStart()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                key?.SetValue("WindowSharingHider", Application.ExecutablePath);
            }
        }

        private void DisableAutoStart()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                key?.DeleteValue("WindowSharingHider", false);
            }
        }

        private void ExitApplication()
        {
            SaveConfig();
            trayIcon.Visible = false;
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            Application.Exit();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                trayIcon.ShowBalloonTip(1000, "Window Sharing Hider",
                    "程序已最小化到系统托盘。双击图标可打开主窗口。", ToolTipIcon.Info);
            }
            else
            {
                SaveConfig();
                trayIcon.Visible = false;
                UnregisterHotKey(this.Handle, HOTKEY_ID);
            }
            base.OnFormClosing(e);
        }

        // 处理全局热键
        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                ToggleAllSelectedWindows();
            }
            base.WndProc(ref m);
        }

        private void ToggleAllSelectedWindows()
        {
            bool anyHidden = false;
            foreach (var window in windowListCheckBox.Items.Cast<WindowInfo>().ToArray())
            {
                if (windowListCheckBox.GetItemChecked(windowListCheckBox.Items.IndexOf(window)))
                {
                    if (WindowHandler.GetWindowDisplayAffinity(window.Handle) > 0)
                    {
                        anyHidden = true;
                        break;
                    }
                }
            }

            foreach (var window in windowListCheckBox.Items.Cast<WindowInfo>().ToArray())
            {
                if (windowListCheckBox.GetItemChecked(windowListCheckBox.Items.IndexOf(window)))
                {
                    WindowHandler.SetWindowDisplayAffinity(window.Handle, anyHidden ? 0x0 : 0x11);
                }
            }
            UpdateStatusBar();
        }

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(configPath))
                {
                    hideRules.Clear();
                    foreach (var line in File.ReadAllLines(configPath))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var parts = line.Split(new[] { '|' }, 2);
                        if (parts.Length == 2)
                        {
                            if (Enum.TryParse<HideRule.RuleType>(parts[0], out var type))
                            {
                                hideRules.Add(new HideRule { Type = type, Pattern = parts[1] });
                            }
                        }
                        else
                        {
                            // 兼容旧格式（只有标题）
                            hideRules.Add(new HideRule { Type = HideRule.RuleType.ContainsTitle, Pattern = line });
                        }
                    }
                }
            }
            catch { }
        }

        private void SaveConfig()
        {
            try
            {
                var dir = Path.GetDirectoryName(configPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var lines = hideRules.Select(r => $"{r.Type}|{r.Pattern}").ToList();
                File.WriteAllLines(configPath, lines);
            }
            catch { }
        }

        // 检查窗口是否匹配任何隐藏规则
        private bool MatchesAnyRule(string title, string processName)
        {
            return hideRules.Any(rule => rule.Matches(title, processName));
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            searchFilter = searchBox.Text.ToLower();
        }

        Boolean flagToPreserveSettings = false;
        private void Timer_Tick(object sender, EventArgs e)
        {
            isUpdatingFromTimer = true; // 防止触发 ItemCheck 事件
            try
            {
                foreach (WindowInfo window in windowListCheckBox.Items) window.stillExists = false;

                var currWindows = WindowHandler.GetVisibleWindowsWithDetails();
                foreach (var window in currWindows)
                {
                    // 应用搜索过滤
                    if (!string.IsNullOrEmpty(searchFilter) &&
                        !window.Title.ToLower().Contains(searchFilter) &&
                        !window.ProcessName.ToLower().Contains(searchFilter))
                        continue;

                    var existingWindow = windowListCheckBox.Items.Cast<WindowInfo>().FirstOrDefault(i => i.Handle == window.Handle);
                    if (existingWindow != null)
                    {
                        existingWindow.stillExists = true;
                        existingWindow.Title = window.Title;
                        existingWindow.ProcessName = window.ProcessName;
                    }
                    else
                    {
                        var newWindow = new WindowInfo
                        {
                            Title = window.Title,
                            Handle = window.Handle,
                            ProcessName = window.ProcessName,
                            stillExists = true
                        };
                        windowListCheckBox.Items.Add(newWindow);

                        // 检查是否匹配任何隐藏规则 - 自动选中并隐藏
                        if (MatchesAnyRule(window.Title, window.ProcessName))
                        {
                            newWindow.autoHidden = true;
                            windowListCheckBox.SetItemChecked(windowListCheckBox.Items.IndexOf(newWindow), true);
                            // 立即隐藏新窗口
                            WindowHandler.SetWindowDisplayAffinity(newWindow.Handle, 0x11);
                        }
                    }
                }

                // 移除已关闭的窗口
                foreach (var window in windowListCheckBox.Items.Cast<WindowInfo>().ToArray())
                    if (window.stillExists == false) windowListCheckBox.Items.Remove(window);

                // 同步隐藏状态
                foreach (var window in windowListCheckBox.Items.Cast<WindowInfo>().ToArray())
                {
                    var status = WindowHandler.GetWindowDisplayAffinity(window.Handle) > 0;
                    var target = windowListCheckBox.GetItemChecked(windowListCheckBox.Items.IndexOf(window));

                    if (target != status && flagToPreserveSettings)
                    {
                        WindowHandler.SetWindowDisplayAffinity(window.Handle, target ? 0x11 : 0x0);
                        status = WindowHandler.GetWindowDisplayAffinity(window.Handle) > 0;
                    }
                    windowListCheckBox.SetItemChecked(windowListCheckBox.Items.IndexOf(window), status);
                }
                flagToPreserveSettings = true;
                UpdateStatusBar();
            }
            finally
            {
                isUpdatingFromTimer = false;
            }
        }

        private void UpdateStatusBar()
        {
            int total = windowListCheckBox.Items.Count;
            int hidden = 0;
            foreach (var window in windowListCheckBox.Items.Cast<WindowInfo>())
            {
                if (WindowHandler.GetWindowDisplayAffinity(window.Handle) > 0)
                    hidden++;
            }
            statusLabel.Text = $"窗口: {total} | 已隐藏: {hidden} | 规则: {hideRules.Count} | Ctrl+Shift+H";
        }

        private void BtnHideAll_Click(object sender, EventArgs e)
        {
            HideAllSelected();
        }

        private void BtnShowAll_Click(object sender, EventArgs e)
        {
            ShowAllWindows();
        }

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < windowListCheckBox.Items.Count; i++)
            {
                windowListCheckBox.SetItemChecked(i, true);
            }
        }

        private void BtnDeselectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < windowListCheckBox.Items.Count; i++)
            {
                windowListCheckBox.SetItemChecked(i, false);
            }
        }

        private void BtnAddRule_Click(object sender, EventArgs e)
        {
            // 获取当前选中的窗口，添加为规则
            var selectedWindows = new List<WindowInfo>();
            for (int i = 0; i < windowListCheckBox.Items.Count; i++)
            {
                if (windowListCheckBox.GetItemChecked(i))
                {
                    selectedWindows.Add((WindowInfo)windowListCheckBox.Items[i]);
                }
            }

            if (selectedWindows.Count == 0)
            {
                MessageBox.Show("请先勾选要添加规则的窗口", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var menu = new ContextMenuStrip())
            {
                foreach (var window in selectedWindows)
                {
                    var subMenu = new ToolStripMenuItem($"添加规则: {window.Title.Substring(0, Math.Min(30, window.Title.Length))}...");

                    subMenu.DropDownItems.Add($"精确标题: {window.Title}", null, (s, ev) =>
                    {
                        AddRule(new HideRule { Type = HideRule.RuleType.ExactTitle, Pattern = window.Title });
                    });

                    subMenu.DropDownItems.Add($"进程名: {window.ProcessName}", null, (s, ev) =>
                    {
                        AddRule(new HideRule { Type = HideRule.RuleType.ProcessName, Pattern = window.ProcessName });
                    });

                    // 提取可能的关键词
                    var keywords = window.Title.Split(new[] { ' ', '-', '|', '—' }, StringSplitOptions.RemoveEmptyEntries);
                    if (keywords.Length > 1)
                    {
                        subMenu.DropDownItems.Add("-");
                        foreach (var keyword in keywords.Take(5))
                        {
                            if (keyword.Length > 2)
                            {
                                subMenu.DropDownItems.Add($"包含: {keyword}", null, (s, ev) =>
                                {
                                    AddRule(new HideRule { Type = HideRule.RuleType.ContainsTitle, Pattern = keyword });
                                });
                            }
                        }
                    }

                    menu.Items.Add(subMenu);
                }

                menu.Items.Add("-");
                menu.Items.Add("管理所有规则...", null, (s, ev) => ShowRulesDialog());

                menu.Show(btnAddRule, new Point(0, btnAddRule.Height));
            }
        }

        private void AddRule(HideRule rule)
        {
            // 检查是否已存在相同规则
            if (hideRules.Any(r => r.Type == rule.Type && r.Pattern.Equals(rule.Pattern, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("该规则已存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            hideRules.Add(rule);
            SaveConfig();
            MessageBox.Show($"规则已添加:\n{rule}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    // 规则管理对话框
    public class RulesDialog : Form
    {
        private ListBox listBox;
        private Button btnAdd, btnEdit, btnRemove, btnOK, btnCancel;
        public List<MainWindow.HideRule> Rules { get; private set; }

        public RulesDialog(List<MainWindow.HideRule> existingRules)
        {
            Rules = new List<MainWindow.HideRule>(existingRules);
            InitializeComponents();
            RefreshList();
        }

        private void InitializeComponents()
        {
            this.Text = "管理隐藏规则";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            listBox = new ListBox { Dock = DockStyle.Fill };
            listBox.DoubleClick += (s, e) => EditSelectedRule();

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(5)
            };

            btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel };
            btnOK = new Button { Text = "确定", DialogResult = DialogResult.OK };
            btnRemove = new Button { Text = "删除" };
            btnEdit = new Button { Text = "编辑" };
            btnAdd = new Button { Text = "添加" };

            btnAdd.Click += (s, e) => AddNewRule();
            btnEdit.Click += (s, e) => EditSelectedRule();
            btnRemove.Click += (s, e) => RemoveSelectedRule();

            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Controls.Add(btnOK);
            buttonPanel.Controls.Add(btnRemove);
            buttonPanel.Controls.Add(btnEdit);
            buttonPanel.Controls.Add(btnAdd);

            var label = new Label
            {
                Text = "规则列表 (双击编辑):",
                Dock = DockStyle.Top,
                Height = 25,
                Padding = new Padding(5)
            };

            this.Controls.Add(listBox);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(label);

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void RefreshList()
        {
            listBox.Items.Clear();
            foreach (var rule in Rules)
            {
                listBox.Items.Add(rule);
            }
        }

        private void AddNewRule()
        {
            using (var dialog = new RuleEditDialog(null))
            {
                if (dialog.ShowDialog() == DialogResult.OK && dialog.Rule != null)
                {
                    Rules.Add(dialog.Rule);
                    RefreshList();
                }
            }
        }

        private void EditSelectedRule()
        {
            if (listBox.SelectedIndex < 0) return;
            var rule = Rules[listBox.SelectedIndex];
            using (var dialog = new RuleEditDialog(rule))
            {
                if (dialog.ShowDialog() == DialogResult.OK && dialog.Rule != null)
                {
                    Rules[listBox.SelectedIndex] = dialog.Rule;
                    RefreshList();
                }
            }
        }

        private void RemoveSelectedRule()
        {
            if (listBox.SelectedIndex < 0) return;
            if (MessageBox.Show("确定删除此规则?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Rules.RemoveAt(listBox.SelectedIndex);
                RefreshList();
            }
        }
    }

    // 规则编辑对话框
    public class RuleEditDialog : Form
    {
        private ComboBox cmbType;
        private TextBox txtPattern;
        private Button btnOK, btnCancel;
        public MainWindow.HideRule Rule { get; private set; }

        public RuleEditDialog(MainWindow.HideRule existingRule)
        {
            InitializeComponents();
            if (existingRule != null)
            {
                cmbType.SelectedIndex = (int)existingRule.Type;
                txtPattern.Text = existingRule.Pattern;
            }
        }

        private void InitializeComponents()
        {
            this.Text = "编辑规则";
            this.Size = new Size(400, 180);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            layout.Controls.Add(new Label { Text = "规则类型:", TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            cmbType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.Items.AddRange(new[] { "精确标题", "标题包含", "进程名", "正则表达式" });
            cmbType.SelectedIndex = 1;
            layout.Controls.Add(cmbType, 1, 0);

            layout.Controls.Add(new Label { Text = "匹配内容:", TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            txtPattern = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtPattern, 1, 1);

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft
            };
            btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel };
            btnOK = new Button { Text = "确定" };
            btnOK.Click += BtnOK_Click;
            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Controls.Add(btnOK);
            layout.Controls.Add(buttonPanel, 1, 2);

            this.Controls.Add(layout);
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPattern.Text))
            {
                MessageBox.Show("请输入匹配内容", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Rule = new MainWindow.HideRule
            {
                Type = (MainWindow.HideRule.RuleType)cmbType.SelectedIndex,
                Pattern = txtPattern.Text.Trim()
            };
            this.DialogResult = DialogResult.OK;
        }
    }
}
