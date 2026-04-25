# SukiUI 控件用法总结

## 环境配置

```xml
<!-- App.axaml -->
<Application.Styles>
    <FluentTheme />
    <StyleInclude Source="avares://SukiUI.Dock/Index.axaml" />
    <suki:SukiTheme Locale="en-US" ThemeColor="Blue" BackgroundStyle="Gradient" />
</Application.Styles>
```

命名空间：
```xml
xmlns:sukiUi="clr-namespace:SukiUI.Controls;assembly=SukiUI"
xmlns:theme="clr-namespace:SukiUI.Theme;assembly=SukiUI"
```

---

## Button

| Class | 样式 |
|-------|------|
| 无 | Standard（默认主色） |
| `Flat` | 扁平风格 |
| `Flat Rounded` | 扁平圆角 |
| `Outlined` | 描边样式 |
| `Basic` | 基础简洁 |
| `Flat Accent` | 扁平强调色 |
| `Flat Large` | 扁平大尺寸 |

```xml
<Button Content="Standard" />
<Button Content="Flat" Classes="Flat" />
<Button Content="Rounded" Classes="Flat Rounded" />
<Button Content="Outlined" Classes="Outlined" />
<Button Content="Basic" Classes="Basic" />
<Button Content="Flat Accent" Classes="Flat Accent" />
<Button Content="Large" Classes="Flat Large" />
```

Busy/Loading 按钮：
```xml
<Button theme:ButtonExtensions.ShowProgress="true" Content="Busy" />
```

```csharp
MyButton.ShowProgress();
MyButton.HideProgress();
```

---

## DropDownButton

点击弹出自定义内容：
```xml
<DropDownButton Content="Click To Open">
    <DropDownButton.Flyout>
        <Flyout>
            <StackPanel Spacing="8" Margin="8">
                <TextBlock Text="Menu Item 1" />
                <TextBlock Text="Menu Item 2" />
            </StackPanel>
        </Flyout>
    </DropDownButton.Flyout>
</DropDownButton>
```

---

## ToggleSwitch

两种状态切换：
```xml
<ToggleSwitch IsChecked="True" />
```

---

## ToggleButton

两种状态切换（按钮样式）：
```xml
<ToggleButton Content="Toggle Me" />
```

---

## Slider

带刻度滑块：
```xml
<Slider IsSnapToTickEnabled="True"
        Maximum="100"
        Minimum="0"
        TickFrequency="1"
        Value="50" />
```

---

## ComboBox

从数据集中选择：
```xml
<ComboBox ItemsSource="{Binding}" SelectedItem="{Binding}" />
```

---

## NumericUpDown

数字输入：
```xml
<NumericUpDown Value="10" />
<NumericUpDown theme:NumericUpDownExtensions.Unit="inch" Value="10" />
<NumericUpDown theme:NumericUpDownExtensions.Unit="inch" ShowButtonSpinner="False" Value="10" />
```

---

## TextBox

文本输入：
```xml
<TextBox Text="Hello" Watermark="Watermark" />
<TextBox theme:TextBoxExtensions.AddDeleteButton="True" Text="Hello" />
<TextBox theme:TextBoxExtensions.Prefix="https://" Watermark="www.google.com" />
```

---

## CheckBox

多选：
```xml
<CheckBox Content="Option One" IsChecked="True" />
<CheckBox IsThreeState="True" Content="Option Three" />
```

---

## RadioButton

单选：
```xml
<RadioButton Content="Option One" GroupName="A" IsChecked="True" />
<RadioButton Classes="Chips" Content="Chips Option" GroupName="B" />
<RadioButton Classes="GigaChips" Content="GigaChips Option" GroupName="C" />
```

---

## ContextMenu

右键菜单：
```xml
<Border.ContextMenu>
    <ContextMenu>
        <MenuItem Header="Option 1" />
        <MenuItem Header="Disabled" IsEnabled="False" />
        <MenuItem Header="-" />
        <MenuItem Header="Submenu">
            <MenuItem Header="Sub-Option 1" />
        </MenuItem>
    </ContextMenu>
</Border.ContextMenu>
```

---

## TabControl

标签页：
```xml
<TabControl>
    <TabItem Header="Tab 1">
        <ScrollViewer>
            <StackPanel Spacing="16" Margin="24">
                <!-- Content -->
            </StackPanel>
        </ScrollViewer>
    </TabItem>
    <TabItem Header="Tab 2">
        <!-- Content -->
    </TabItem>
</TabControl>
```

---

## SukiSideMenu

侧边导航菜单：
```xml
<sukiUi:SukiSideMenu IsSearchEnabled="True">
    <sukiUi:SukiSideMenu.HeaderContent>
        <TextBlock Text="Header" FontSize="20" FontWeight="Bold" Margin="16"/>
    </sukiUi:SukiSideMenu.HeaderContent>

    <sukiUi:SukiSideMenu.Items>
        <sukiUi:SukiSideMenuItem Header="Page Title">
            <sukiUi:SukiSideMenuItem.Icon>
                <PathIcon Data="..."/>
            </sukiUi:SukiSideMenuItem.Icon>
            <sukiUi:SukiSideMenuItem.PageContent>
                <ScrollViewer>
                    <StackPanel Spacing="16" Margin="24">
                        <!-- Page Content (Required) -->
                    </StackPanel>
                </ScrollViewer>
            </sukiUi:SukiSideMenuItem.PageContent>
        </sukiUi:SukiSideMenuItem>
    </sukiUi:SukiSideMenu.Items>
</sukiUi:SukiSideMenu>
```

> **注意**: `SukiSideMenuItem.PageContent` 不能为空，否则会触发异常。

---

## GlassCard

玻璃卡片：
```xml
<GlassCard>
    <TextBlock Text="Default" />
</GlassCard>

<GlassCard Classes="Primary" Width="150" Height="80">
    <TextBlock Text="Primary" />
</GlassCard>

<GlassCard Classes="Accent" Width="150" Height="80">
    <TextBlock Text="Accent" />
</GlassCard>

<GlassCard IsOpaque="True" Width="150" Height="80">
    <TextBlock Text="Opaque" />
</GlassCard>

<GlassCard IsInteractive="True" Width="150" Height="80">
    <TextBlock Text="Interactive" />
</GlassCard>
```

| 属性/Class | 说明 |
|------------|------|
| `Classes="Primary"` | 主色调 |
| `Classes="Accent"` | 强调色 |
| `IsOpaque="True"` | 不透明 |
| `IsInteractive="True"` | 可交互（悬停效果） |
| `IsAnimated="True"` | 默认开启动画 |

---

## SettingsLayout

设置布局：
```xml
<sukiUi:SettingsLayout>
    <sukiUi:SettingsLayout.Items>
        <sukiUi:SettingsLayoutItem Header="Setting Part 1">
            <sukiUi:SettingsLayoutItem.Content>
                <StackPanel Spacing="8" Margin="0,8">
                    <TextBox Watermark="Enter value..." />
                    <CheckBox Content="Enable feature" IsChecked="True" />
                </StackPanel>
            </sukiUi:SettingsLayoutItem.Content>
        </sukiUi:SettingsLayoutItem>

        <sukiUi:SettingsLayoutItem Header="Setting Part 2">
            <sukiUi:SettingsLayoutItem.Content>
                <StackPanel Spacing="8" Margin="0,8">
                    <!-- Content -->
                </StackPanel>
            </sukiUi:SettingsLayoutItem.Content>
        </sukiUi:SettingsLayoutItem>
    </sukiUi:SettingsLayout.Items>
</sukiUi:SettingsLayout>
```

---

## SukiWindow

窗口背景样式：
```xml
<sukiUi:SukiWindow BackgroundStyle="Bubble">
    <!-- Bubble (default), Flat, Gradient, Dark, Light -->
</sukiUi:SukiWindow>
```

Logo：
```xml
<sukiUi:SukiWindow>
    <sukiUi:SukiWindow.LogoContent>
        <!-- Logo -->
    </sukiUi:SukiWindow.LogoContent>
</sukiUi:SukiWindow>
```

菜单：
```xml
<sukiUi:SukiWindow IsMenuVisible="True">
    <sukiUi:SukiWindow.MenuItems>
        <MenuItem Header="Menu 1" />
        <MenuItem Header="Menu 2" />
    </sukiUi:SukiWindow.MenuItems>
</sukiUi:SukiWindow>
```

右侧标题栏控件：
```xml
<sukiUi:SukiWindow>
    <sukiUi:SukiWindow.RightWindowTitleBarControls>
        <!-- Controls show on the right of title bar -->
    </sukiUi:SukiWindow.RightWindowTitleBarControls>
</sukiUi:SukiWindow>
```

---

## Dock

安装 `SukiUI.Dock` NuGet 包，并在 App.axaml 中引用：
```xml
<StyleInclude Source="avares://SukiUI.Dock/Index.axaml" />
```

---

## 版本兼容性

| SukiUI | Avalonia |
|--------|----------|
| 6.0.3 (稳定版) | 11.3.4 |
| 6.0.4-nightly | Avalonia 12.x（可能不兼容） |

建议使用 SukiUI 6.0.3 + Avalonia 11.3.4 稳定组合。
