using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fluent_Terminal
{
    public sealed partial class TerminalPage : Page
    {
        private Process? _shellProcess;
        private StreamWriter? _inputWriter;
        private string? _currentShell;
        private List<string> _commandHistory = new();
        private int _historyIndex = -1;
        private int _lineCount = 0;

        public TerminalPage() { this.InitializeComponent(); }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            string shell = ShellSelector.SelectedIndex switch
            {
                0 => "powershell.exe",
                1 => "powershell.exe -Version 5.1",
                2 => "cmd.exe",
                3 => "wt.exe",
                _ => "powershell.exe"
            };
            StartShell(shell);
        }

        private void StartShell(string shellName)
        {
            try
            {
                ProgressRing.Visibility = Visibility.Visible;
                StartButton.IsEnabled = false;

                if (_shellProcess != null && !_shellProcess.HasExited)
                {
                    _shellProcess.Kill();
                    _shellProcess.Dispose();
                }

                _currentShell = shellName;
                _lineCount = 0;
                TerminalOutput.Text = $"--- {shellName} Session Started ---\n";
                UpdateStatus("Running", true);

                _shellProcess = new Process();
                _shellProcess.StartInfo = new ProcessStartInfo
                {
                    FileName = shellName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                };

                _shellProcess.Start();
                _inputWriter = _shellProcess.StandardInput;
                InputBox.Focus(FocusState.Programmatic);

                this.DispatcherQueue.TryEnqueue(() =>
                {
                    ProgressRing.Visibility = Visibility.Collapsed;
                    StartButton.IsEnabled = true;
                });

                Task.Run(() => ReadStream(_shellProcess.StandardOutput));
                Task.Run(() => ReadStream(_shellProcess.StandardError));
            }
            catch (Exception ex)
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    TerminalOutput.Text += $"\nError starting shell: {ex.Message}\n";
                    UpdateStatus("Error", false);
                    ProgressRing.Visibility = Visibility.Collapsed;
                    StartButton.IsEnabled = true;
                });
            }
        }

        private async Task ReadStream(StreamReader reader)
        {
            try
            {
                while (!reader.EndOfStream)
                {
                    string line = await reader.ReadLineAsync();
                    if (line != null)
                    {
                        this.DispatcherQueue.TryEnqueue(() =>
                        {
                            TerminalOutput.Text += line + "\n";
                            _lineCount++;
                            UpdateLineCount();
                            TerminalScroll.ChangeView(0, double.MaxValue, 1);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    TerminalOutput.Text += $"\nError reading stream: {ex.Message}\n";
                });
            }
        }

        private void InputBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && _inputWriter != null)
            {
                SendCommand();
                e.Handled = true;
            }
            else if (e.Key == Windows.System.VirtualKey.Up)
            {
                if (_commandHistory.Count > 0)
                {
                    _historyIndex = Math.Min(_historyIndex + 1, _commandHistory.Count - 1);
                    InputBox.Text = _commandHistory[_commandHistory.Count - 1 - _historyIndex];
                    InputBox.SelectAll();
                }
                e.Handled = true;
            }
            else if (e.Key == Windows.System.VirtualKey.Down)
            {
                if (_historyIndex > 0)
                {
                    _historyIndex--;
                    InputBox.Text = _commandHistory[_commandHistory.Count - 1 - _historyIndex];
                    InputBox.SelectAll();
                }
                else if (_historyIndex == 0)
                {
                    _historyIndex = -1;
                    InputBox.Text = "";
                }
                e.Handled = true;
            }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            SendCommand();
        }

        private void SendCommand()
        {
            if (_inputWriter != null && !string.IsNullOrEmpty(InputBox.Text))
            {
                try
                {
                    string command = InputBox.Text;
                    _inputWriter.WriteLine(command);
                    _inputWriter.Flush();

                    _commandHistory.Add(command);
                    _historyIndex = -1;
                    UpdateCommandHistory();

                    InputBox.Text = "";
                }
                catch (Exception ex)
                {
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        TerminalOutput.Text += $"\nError sending command: {ex.Message}\n";
                    });
                }
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            TerminalOutput.Text = string.Empty;
            _lineCount = 0;
            UpdateLineCount();
            UpdateStatus(_currentShell != null ? "Ready" : "Ready", _currentShell != null);
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
                dataPackage.SetText(TerminalOutput.Text);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            }
            catch { }
        }

        private void UpdateStatus(string status, bool isActive)
        {
            StatusText.Text = status;
            StatusIndicator.Fill = isActive 
                ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["SystemFillColorSuccessBrush"]
                : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["SystemFillColorCautionBrush"];
        }

        private void UpdateLineCount()
        {
            LineCountText.Text = $"Lines: {_lineCount}";
        }

        private void UpdateCommandHistory()
        {
            CommandHistory.Items.Clear();
            var recentCommands = _commandHistory.TakeLast(10).Reverse();
            foreach (var cmd in recentCommands)
            {
                CommandHistory.Items.Add(cmd);
            }
        }
    }
}