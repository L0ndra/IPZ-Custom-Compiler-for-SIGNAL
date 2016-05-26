using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Compiler
{
    public class TableElem
    {
        public string Name { get; set; }
        public string Oper { get; set; }
        public string TrueReturn { get; set; }
        public string FalseReturn { get; set; }
        public string EmptyValue { get; set; }

        public TableElem()
        {

        }

        public TableElem(string name, string oper, string trueReturn, string falseReturn, string emptyValue)
        {
            Name = name;
            Oper = oper;
            TrueReturn = trueReturn;
            FalseReturn = falseReturn;
            EmptyValue = emptyValue;
        }
    }



    public class KnutTable
    {
        public KnutTable()
        {
            Table = new List<TableElem>
            {
                new TableElem("signal-program", "program", "T", "F", ""),
                new TableElem("program", "401", "2", "F", ""),
                new TableElem("", "identifier", "3", "F", ""),
                new TableElem("", "1", "4", "F", ""),
                new TableElem("", "block", "5", "F", ""),
                new TableElem("", "1", "T", "F", ""),
                new TableElem("block", "402", "7", "F", ""),
                new TableElem("", "statement-list", "8", "F", ""),
                new TableElem("", "403", "T", "F", ""),
                new TableElem("statement-list", "statement", "10", "11", ""),
                new TableElem("", "statement-list", "T", "F", ""),
                new TableElem("", "empty", "T", "F", "403"),
                new TableElem("statement", "unsigned-integer", "13", "15", ""),
                new TableElem("", "2", "14", "15", ""),
                new TableElem("", "statement", "T", "15", ""),
                new TableElem("", "identifier", "16", "19", ""),
                new TableElem("", "301", "17", "19", ""),
                new TableElem("", "unsigned-integer", "18", "19", ""),
                new TableElem("", "1", "T", "19", ""),
                new TableElem("", "identifier", "20", "22", ""),
                new TableElem("", "actual-arguments", "21", "22", ""),
                new TableElem("", "1", "T", "22", ""),
                new TableElem("", "404", "23", "25", ""),
                new TableElem("", "unsigned-integer", "24", "25", ""),
                new TableElem("", "1", "T", "25", ""),
                new TableElem("", "405", "26", "30", ""),
                new TableElem("", "identifier", "27", "30", ""),
                new TableElem("", "5", "28", "30", ""),
                new TableElem("", "unsigned-integer", "29", "30", ""),
                new TableElem("", "1", "T", "30", ""),
                new TableElem("", "406", "31", "33", ""),
                new TableElem("", "unsigned-integer", "32", "33", ""),
                new TableElem("", "1", "T", "33", ""),
                new TableElem("", "407", "34", "36", ""),
                new TableElem("", "unsigned-integer", "35", "36", ""),
                new TableElem("", "1", "T", "33", ""),
                new TableElem("", "408", "37", "38", ""),
                new TableElem("", "1", "T", "38", ""),
                new TableElem("", "1", "T", "39", ""),
                new TableElem("", "302", "40", "F", ""),
                new TableElem("", "identifier", "41", "F", ""),
                new TableElem("", "303", "T", "F", ""),
                new TableElem("actual-arguments", "3", "43", "46", ""),
                new TableElem("", "identifier", "44", "F", ""),
                new TableElem("", "actual-arguments-list", "45", "F", ""),
                new TableElem("", "4", "T", "F", ""),
                new TableElem("", "empty", "T", "F", "1"),
                new TableElem("actual-arguments-list", "5", "48", "50", ""),
                new TableElem("", "identifier", "49", "F", ""),
                new TableElem("", "actual-arguments-list", "T", "F", ""),
                new TableElem("", "empty", "T", "F", "4"),
            };
        }

        public List<TableElem> Table { get; set; }

        public TableElem GetElemByName(string name)
        {
            return Table.First(a => a.Name == name);
        }

        public TableElem GetElemByIndex(int indx)
        {
            return Table[indx];
        }
    }

    public class Tree
    {
        public string Name { get; set; }
        public List<Tree> InnerTree { get; set; }
    }

    public class StackElement
    {
        public TableElem TableElem { get; set; }
        public Tree Tree { get; set; }
        public int LexNum { get; set; }
        public bool InnerCall { get; set; }
    }

    public class SyntAnal
    {

        public List<Table> Tables = new List<Table>();
        public Tree finalTree;
        public int ErrorLex;

        public void SaveXml(string folder)
        {
            using (StreamWriter sw = new StreamWriter(folder + "/program.xml"))
            {
                sw.Write(GeneretaNode(finalTree));
            }
        }

        private StringBuilder GeneretaNode(Tree tree)
        {
            var str = new StringBuilder("<"+tree.Name+">"+"\n");
            if (tree.InnerTree == null) return new StringBuilder(tree.Name + "\n");
            foreach (var subnode in tree.InnerTree)
            {
                str.Append(GeneretaNode(subnode));
            }
            str.Append("</" + tree.Name + ">" + "\n");
            return str;
        }
        private string GetTableElemByCode(int code)
        {
            foreach (var table in Tables)
            {

                if (table.Dictionary.ContainsValue(code))
                {
                    foreach (var node in table.Dictionary)
                    {
                        if (node.Value == code)
                        {
                            return node.Key;
                        }
                    }
                }
            }
            return "";

        }

        public bool Start(Table sdetTable, Table cdetTable, Table keysTable, Table constTable, Table identTable,
            List<int> lexems)
        {
            Tables.Add(sdetTable);
            Tables.Add(cdetTable);
            Tables.Add(keysTable);
            Tables.Add(constTable);
            Tables.Add(identTable);
            bool hasResult = false;
            bool result = false;
            KnutTable knutTable = new KnutTable();
            Stack<StackElement> stack = new Stack<StackElement>();
            finalTree = new Tree {Name = "signal-program"};
            stack.Push(new StackElement {TableElem = knutTable.GetElemByIndex(0), Tree = finalTree, LexNum = 0});
            while (stack.Count > 0)
            {
                StackElement elem = stack.Peek();
                if (!hasResult)
                {
                    int oper;
                    if (Int32.TryParse(elem.TableElem.Oper, out oper))
                    {
                        string lex = GetTableElemByCode(oper);
                        elem.Tree.Name = lex;
                        if (lexems[elem.LexNum] == oper)
                        {
                            int next;
                            if (Int32.TryParse(elem.TableElem.TrueReturn, out next))
                            {
                                stack.Push(new StackElement
                                {
                                    TableElem = knutTable.GetElemByIndex(next),
                                    Tree = new Tree
                                    {
                                        Name = knutTable.GetElemByIndex(next).Oper
                                    },
                                    LexNum = elem.LexNum + 1
                                });
                            }
                            else
                            {
                                hasResult = true;
                                result = true;
                            }
                        }
                        else
                        {
                            int next;
                            if (Int32.TryParse(elem.TableElem.FalseReturn, out next))
                            {
                                while (!stack.Peek().InnerCall)
                                {
                                    stack.Pop();
                                }
                                stack.Push(new StackElement
                                {
                                    TableElem = knutTable.GetElemByIndex(next),
                                    Tree = new Tree
                                    {
                                        Name = knutTable.GetElemByIndex(next).Oper
                                    },
                                    LexNum = stack.Peek().LexNum
                                });
                            }
                            else
                            {
                                hasResult = true;
                                result = false;
                                ErrorLex = elem.LexNum;
                            }
                        }
                    }
                    else
                    {
                        if (elem.TableElem.Oper == "unsigned-integer")
                        {
                            if (lexems[elem.LexNum] > 500 && lexems[elem.LexNum] <= 1000)
                            {
                                string lex = GetTableElemByCode(lexems[elem.LexNum]);
                                elem.Tree.InnerTree = new List<Tree>();
                                elem.Tree.InnerTree.Add(new Tree
                                {
                                    Name = lex
                                });
                                int next;
                                if (Int32.TryParse(elem.TableElem.TrueReturn, out next))
                                {
                                    stack.Push(new StackElement
                                    {
                                        TableElem = knutTable.GetElemByIndex(next),
                                        Tree = new Tree
                                        {
                                            Name = knutTable.GetElemByIndex(next).Oper
                                        },
                                        LexNum = elem.LexNum + 1
                                    });
                                }
                                else
                                {
                                    hasResult = true;
                                    result = true;
                                }
                            }
                            else
                            {
                                int next;
                                if (Int32.TryParse(elem.TableElem.FalseReturn, out next))
                                {
                                    while (!stack.Peek().InnerCall)
                                    {
                                        stack.Pop();
                                    }
                                    stack.Push(new StackElement
                                    {
                                        TableElem = knutTable.GetElemByIndex(next),
                                        Tree = new Tree
                                        {
                                            Name = knutTable.GetElemByIndex(next).Oper
                                        },
                                        LexNum = stack.Peek().LexNum
                                    });
                                }
                                else
                                {
                                    hasResult = true;
                                    result = false;
                                    ErrorLex = elem.LexNum;
                                }
                            }
                        }
                        else
                        {
                            if (elem.TableElem.Oper == "identifier")
                            {
                                if (lexems[elem.LexNum] > 1000)
                                {
                                    string lex = GetTableElemByCode(lexems[elem.LexNum]);
                                    elem.Tree.InnerTree = new List<Tree>();
                                    elem.Tree.InnerTree.Add(new Tree
                                    {
                                        Name = lex
                                    });
                                    int next;
                                    if (Int32.TryParse(elem.TableElem.TrueReturn, out next))
                                    {
                                        stack.Push(new StackElement
                                        {
                                            TableElem = knutTable.GetElemByIndex(next),
                                            Tree = new Tree
                                            {
                                                Name = knutTable.GetElemByIndex(next).Oper
                                            },
                                            LexNum = elem.LexNum + 1
                                        });
                                    }
                                    else
                                    {
                                        hasResult = true;
                                        result = true;
                                    }
                                }
                                else
                                {
                                    int next;
                                    if (Int32.TryParse(elem.TableElem.FalseReturn, out next))
                                    {
                                        while (!stack.Peek().InnerCall)
                                        {
                                            stack.Pop();
                                        }
                                        stack.Push(new StackElement
                                        {
                                            TableElem = knutTable.GetElemByIndex(next),
                                            Tree = new Tree
                                            {
                                                Name = knutTable.GetElemByIndex(next).Oper
                                            },
                                            LexNum = stack.Peek().LexNum
                                        });
                                    }
                                    else
                                    {
                                        hasResult = true;
                                        result = false;
                                        ErrorLex = elem.LexNum;
                                    }
                                }
                            }
                            else
                            {
                                if (elem.TableElem.Oper == "empty")
                                {
                                    if (lexems[elem.LexNum] == Convert.ToInt32(elem.TableElem.EmptyValue))
                                    {
                                        elem.LexNum = elem.LexNum - 1;
                                        hasResult = true;
                                        result = true;
                                    }
                                    else
                                    {
                                        hasResult = true;
                                        result = false;
                                        ErrorLex = elem.LexNum;
                                    }
                                }
                                else
                                {
                                    stack.Push(new StackElement
                                    {
                                        TableElem = knutTable.GetElemByName(elem.TableElem.Oper),
                                        Tree = new Tree
                                        {
                                            Name = knutTable.GetElemByName(elem.TableElem.Oper).Oper
                                        },
                                        LexNum = elem.LexNum
                                    });
                                    elem.InnerCall = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    hasResult = false;
                    if (result)
                    {
                        List<Tree> treeNode = new List<Tree>();
                        elem = stack.Peek();
                        int nextLex = elem.LexNum;
                        while (!elem.InnerCall)
                        {
                            stack.Pop();
                            treeNode.Add(elem.Tree);
                            elem = stack.Peek();
                        }
                        treeNode.Reverse();
                        elem.Tree.InnerTree = treeNode;
                        elem.InnerCall = false;
                        int nextRow;
                        if (Int32.TryParse(elem.TableElem.TrueReturn, out nextRow))
                        {
                            stack.Push(new StackElement
                            {
                                TableElem = knutTable.GetElemByIndex(nextRow),
                                Tree = new Tree {Name = knutTable.GetElemByIndex(nextRow).Oper},
                                LexNum = nextLex+1
                            });
                        }
                        else
                        {
                            elem.LexNum = nextLex;
                            hasResult = true;
                            result = true;
                        }
                    }
                    else
                    {
                        elem = stack.Peek();
                        while (!elem.InnerCall)
                        {
                            stack.Pop();
                            elem = stack.Peek();
                        }
                        elem.InnerCall = false;
                        int nextRow;
                        if (Int32.TryParse(elem.TableElem.FalseReturn, out nextRow))
                        {
                            stack.Push(new StackElement()
                            {
                                TableElem = knutTable.GetElemByIndex(nextRow),
                                Tree = new Tree() {Name = knutTable.GetElemByIndex(nextRow).Oper},
                                LexNum = stack.Peek().LexNum
                            });
                        }
                        else
                        {
                            hasResult = true;
                            result = false;
                        }
                    }

                }
                if (stack.Count == 1)
                {
                    break;
                }
            }
            return result;
        }
    }
}
