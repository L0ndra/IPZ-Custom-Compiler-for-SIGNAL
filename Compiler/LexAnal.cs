using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        private int _iterator;
        private List<int> _result;
        public string ResultText { get; set; }
        public bool Error { get; set; }

        public LexAnal(string folder)
        {
            _simpleDet = new Table(folder + simpleFile, simplOffset);
            _compDet = new Table(folder + compFile, compOffset);
            _keys = new Table(folder + keysFile, keysOffset);
            _consts = new Table(folder + constsFile, constsOffset);
            _idents = new Table(folder + identsFile, identsOffset);
            StreamReader sr = new StreamReader(folder + "/program.txt");
            Program = sr.ReadToEnd();
        }

        public void Compile()
        {
            _result = new List<int>();
            while (_iterator < Program.Length)
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
                                        ResultText += Program.Substring(_iterator);
                                        Error = true;
                                        return;
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
            char symb = Program[_iterator];
            if (symb >= 'A' && symb <= 'Z')
            {
                string lex = symb.ToString();
                _iterator++;
                symb = Program[_iterator];
                while ((_iterator < Program.Length) && ((symb >= 'A' && symb <= 'Z') || (symb >= '0' && symb <= '9')))
                {
                    lex += symb;
                    _iterator++;
                    symb = Program[_iterator];
                }
                int offset = _keys.Check(lex) ? _keys.AddItem(lex) : _idents.AddItem(lex);
                _result.Add(offset);
                return true;
            }
            return false;
        }

        public bool TryGetConst()
        {
            char symb = Program[_iterator];
            if (symb > '0' && symb < '9')
            {
                string lex = symb.ToString();
                _iterator++;
                symb = Program[_iterator];
                while (_iterator < Program.Length && symb >= '0' && symb <= '9')
                {
                    lex += symb;
                    _iterator++;
                    symb = Program[_iterator];
                }
                int offset = _consts.AddItem(lex);
                _result.Add(offset);
                return true;
            }
            return false;
        }

        public bool TryGetCompDet()
        {
            if (_iterator < Program.Length - 1)
            {
                string lex = Program[_iterator].ToString() + Program[_iterator + 1];
                if (_compDet.Check(lex))
                {
                    _result.Add(_compDet.AddItem(lex));
                    _iterator += 2;
                    return true;
                }
            }
            return false;
        }

        public bool TryGetSimplDet()
        {
            char symb = Program[_iterator];
            if (_simpleDet.Check(symb.ToString()))
            {
                _result.Add(_simpleDet.AddItem(symb.ToString()));
                _iterator++;
                return true;
            }
            return false;
        }

        public bool TryDeleteWhiteSpace()
        {
            char symb = Program[_iterator];
            if (_ws.Any(c => symb == c))
            {
                _iterator++;
                return true;
            }
            return false;
        }

        public bool TryDeleteComment()
        {
            if (_iterator < Program.Length - 1)
            {
                int j = _iterator;
                string lex = Program[j].ToString() + Program[j+1];
                if (lex == CommentStart)
                {
                    j++;
                    while (j < Program.Length - 1)
                    {
                        lex = Program[j].ToString() + Program[j+1];
                        if (lex == CommentEnd)
                        {
                            _iterator = j + 2;
                            return true;
                        }
                        j++;
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
