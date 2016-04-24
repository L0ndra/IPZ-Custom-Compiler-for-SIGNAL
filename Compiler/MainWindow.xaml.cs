using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace Compiler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private string _path;
        private string _file;
        private LexAnal _lexAnal;
        private async void Browse_OnClick(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.ShowNewFolderButton = false;
                dlg.SelectedPath = AppDomain.CurrentDomain.BaseDirectory;
                DialogResult result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    _path = dlg.SelectedPath;


                }
            }
            using (FileDialog dlg = new OpenFileDialog())
            {
                dlg.InitialDirectory = _path;
                DialogResult result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    _file = dlg.FileName;
                    var files = dlg.FileNames;
                }

            }
            _lexAnal = new LexAnal(_path);
            textBlock.Text = _lexAnal.Program;
            Compile.IsEnabled = true;
        }

        private async void Compile_OnClick(object sender, RoutedEventArgs e)
        {
            _lexAnal.Compile();
            textBlock1.Text = _lexAnal.ResultText;
            Save.IsEnabled = true;
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.ShowNewFolderButton = false;
                dlg.SelectedPath = AppDomain.CurrentDomain.BaseDirectory;
                DialogResult result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    _path = dlg.SelectedPath;
                    _lexAnal.Save(_path);
                }
            }
        }
    }
}
