# Window Sharing Hider

在屏幕共享时隐藏指定窗口。支持 Teams、Zoom、Discord 等各种屏幕共享应用。

Hides Windows during screen sharing. Works with Teams, Zoom, Discord, etc.

## 功能特性

- **智能规则系统** - 勾选窗口自动保存规则，窗口消失再出现会自动隐藏
- **多种匹配方式** - 支持精确标题、标题包含、进程名、正则表达式匹配
- **系统托盘** - 最小化到托盘运行，不占用任务栏
- **全局热键** - `Ctrl+Shift+H` 快速切换隐藏/显示
- **开机自启动** - 可选开机自动运行
- **窗口搜索** - 快速过滤查找窗口
- **配置持久化** - 规则自动保存，重启后生效

## 使用方法

1. 运行程序
2. 勾选要隐藏的窗口
3. 窗口会自动被隐藏，即使关闭后重新打开也会自动隐藏
4. 取消勾选可移除规则

## 快捷键

| 快捷键 | 功能 |
|--------|------|
| `Ctrl+Shift+H` | 切换所有选中窗口的隐藏/显示状态 |

## 托盘菜单

- 显示主窗口
- 隐藏所有选中窗口
- 显示所有窗口
- 管理规则
- 开机自启动
- 退出

## 技术说明

Single app, no dll's, works on both x86/x64

Relies on [SetWindowDisplayAffinity](https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowdisplayaffinity). Microsoft specifically restricted it to only work on windows where the current process is the owner of the window. This works by creating a thread in the target process to bypass that restriction.

## 配置文件

规则保存在: `%AppData%\WindowSharingHider\rules.txt`

## 编译

```bash
dotnet build -c Release
```

输出文件: `bin\Release\WindowSharingHider.exe`

123

## License

MIT License (Copyright 2021 shalzuth)
