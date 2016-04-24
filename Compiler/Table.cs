using System.Collections.Generic;
using System.IO;

namespace Compiler
{
    public class Table
    {
        private int counter;
        public Dictionary<string, int> Dictionary; 
        public Table(string file, int offset)
        {
            counter = offset;
            Dictionary = new Dictionary<string, int>();
            using (StreamReader f = new StreamReader(file))
            {
                string line;
                while ((line = f.ReadLine()) != null)
                {
                    AddItem(line);
                }
            }
        }

        public int AddItem(string lex)
        {
            if (Dictionary.ContainsKey(lex))
            {
                return Dictionary[lex];
            }
            counter++;
            Dictionary.Add(lex,counter);
            return counter;
        }

        public void Save(string file)
        {
            using (StreamWriter sw = new StreamWriter(file))
            {
                foreach (var pair in Dictionary)
                {
                    sw.WriteLine("{0} {1}", pair.Value, pair.Key);
                }
            }
        }

        public bool Check(string lex)
        {
            return Dictionary.ContainsKey(lex);
        }
    }
}
