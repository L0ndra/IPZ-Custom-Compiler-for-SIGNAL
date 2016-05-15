using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Compiler
{
    public class LexAnal
    {
        public string Program { get; set; }
        private readonly Table _simpleDet;
        private readonly Table _compDet;
        private readonly Table _keys;
        private readonly Table _consts;
        private readonly Table _idents;
        private int simplOffset = 0;
        private int compOffset = 300;
        private int keysOffset = 400;
        private int constsOffset = 500;
        private int identsOffset = 1000;
        private string simpleFile = "/sdet.txt";
        private string compFile = "/cdet.txt";
        private string keysFile = "/keys.txt";
        private string constsFile = "/consts.txt";
        private string identsFile = "/idents.txt";
        private const string CommentStart = "(*";
        private const string CommentEnd = "*)";
        private readonly char[] _ws = {' ', '\r', '\n', '\t', '\v', '\f'};
        private List<int> _result;
        public string ResultText { get; set; }
        public bool Error { get; set; }
        private StreamReader _sr;
        private char _curChar;

        public LexAnal(string folder, string file)
        {
            _simpleDet = new Table(folder + simpleFile, simplOffset);
            _compDet = new Table(folder + compFile, compOffset);
            _keys = new Table(folder + keysFile, keysOffset);
            _consts = new Table(folder + constsFile, constsOffset);
            _idents = new Table(folder + identsFile, identsOffset);
            using (StreamReader sr = new StreamReader(file))
            {
                Program = sr.ReadToEnd();
            }
        }

        public void Compile(string file)
        {
            using (_sr = new StreamReader(file))
            {
                _result = new List<int>();
                _curChar = (char)_sr.Read();
                while (_curChar != -1)
                {
                    if (!TryGetIdentifier())
                    {
                        if (!TryGetConst())
                        {
                            if (!TryGetCompDet())
                            {
                                if (!TryDeleteComment())
                                {
                                    if (!TryDeleteWhiteSpace())
                                    {
                                        if (!TryGetSimplDet())
                                        {
                                            GenerateResultText();
                                            ResultText += _sr.ReadToEnd();
                                            Error = true;
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            GenerateResultText();
            Error = false;
        }

        public void Save(string folder)
        {
            using (StreamWriter sw = new StreamWriter(folder+"/program.txt"))
            {
                sw.Write(ResultText);
            }
            _consts.Save(folder+"/const.txt");
            _idents.Save(folder+"/idents.txt");
        }

        public bool TryGetIdentifier()
        {
            if (_curChar >= 'A' && _curChar <= 'Z')
            {
                string lex = _curChar.ToString();
                while ((_sr.Peek() != -1) && ((_sr.Peek() >= 'A' && _sr.Peek() <= 'Z') || (_sr.Peek() >= '0' && _sr.Peek() <= '9')))
                {
                    _curChar = (char)_sr.Read();
                    lex += _curChar;
                }
                _curChar = (char) _sr.Read();
                int offset = _keys.Check(lex) ? _keys.AddItem(lex) : _idents.AddItem(lex);
                _result.Add(offset);
                return true;
            }
            return false;
        }

        public bool TryGetConst()
        {
            if (_curChar > '0' && _curChar < '9')
            {
                string lex = _curChar.ToString();
                while (_sr.Peek()!=-1 && _sr.Peek() >= '0' && _sr.Peek() <= '9')
                {
                    _curChar = (char)_sr.Read();
                    lex += _curChar;
                }
                _curChar = (char) _sr.Read();
                int offset = _consts.AddItem(lex);
                _result.Add(offset);
                return true;
            }
            return false;
        }

        public bool TryGetCompDet()
        {
            if (_sr.Peek() != -1)
            {
                string lex = _curChar.ToString() + (char)_sr.Peek();
                if (_compDet.Check(lex))
                {
                    _result.Add(_compDet.AddItem(lex));
                    _curChar = (char) _sr.Read();
                    _curChar = (char)_sr.Read();
                    return true;
                }
            }
            return false;
        }

        public bool TryGetSimplDet()
        {
            if (_simpleDet.Check(_curChar.ToString()))
            {
                _result.Add(_simpleDet.AddItem(_curChar.ToString()));
                _curChar = (char)_sr.Read();
                return true;
            }
            return false;
        }

        public bool TryDeleteWhiteSpace()
        {
            if (_ws.Any(c => _curChar == c))
            {
                _curChar = (char)_sr.Read();
                return true;
            }
            return false;
        }

        public bool TryDeleteComment()
        {
            if (_sr.Peek() != -1)
            {
                string lex = _curChar.ToString() + (char)_sr.Peek();
                if (lex == CommentStart)
                {
                    _curChar = (char) _sr.Read();
                    while (_sr.Peek() != -1)
                    {
                        lex = _curChar.ToString() + (char)_sr.Peek();
                        if (lex == CommentEnd)
                        {
                            _curChar = (char)_sr.Read();
                            _curChar = (char)_sr.Read();
                            return true;
                        }
                        _curChar = (char)_sr.Read();
                    }
                }
            }
            return false;
        }

        public void GenerateResultText()
        {
            foreach (var lex in _result)
            {
                ResultText += lex+" ";
            }
        }
    }
}
