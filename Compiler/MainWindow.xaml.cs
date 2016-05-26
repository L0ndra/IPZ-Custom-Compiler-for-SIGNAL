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
        private SyntAnal syntAnal;
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
                }

            }
            _lexAnal = new LexAnal(_path, _file);
            textBlock.Text = _lexAnal.Program;
            Compile.IsEnabled = true;
        }

        private async void Compile_OnClick(object sender, RoutedEventArgs e)
        {
            _lexAnal.Compile(_file);
            textBlock1.Text = _lexAnal.ResultText;
            if (!_lexAnal.Error)
            {
                syntAnal = new SyntAnal();
                var result = syntAnal.Start(_lexAnal._simpleDet, _lexAnal._compDet, _lexAnal._keys, _lexAnal._consts,
                    _lexAnal._idents, _lexAnal.ResultList);
                if (result)
                {
                    Save.IsEnabled = true;
                }
                else
                {
                    textBlock2.Text = "SyntAnal error in lex num" + syntAnal.ErrorLex;
                }
            }
            else
            {
                textBlock2.Text = "LexAnal error";
            }
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
                    syntAnal.SaveXml(_path);
                }
            }
        }
    }
}
