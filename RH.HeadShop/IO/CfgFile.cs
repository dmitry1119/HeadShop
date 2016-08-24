using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace RH.HeadShop.IO
{
    /// <summary> Class for working with CFG file format's (just for saving and loading program config) </summary>
    /// You can read in details about structure here:
    /// http://en.wikipedia.org/wiki/Configuration_file#Microsoft_Windows
    public class CfgFile : IDisposable
    {
        /// <summary> Struct for storage cfg line. Section - Key- Value </summary>
        private struct String3 : IComparable
        {
            public String3(string s1, string s2, string s3)
            {
                this.s1 = s1;
                this.s2 = s2;
                this.s3 = s3;
            }
            /// <summary> CFG section </summary>
            public readonly string s1;
            /// <summary> CFG Key </summary>
            public readonly string s2;
            /// <summary> CFG value </summary>
            public readonly string s3;
            public int CompareTo(object obj)
            {
                var item = (String3)obj;
                return String.CompareOrdinal((s1 + "\n" + s2), item.s1 + "\n" + item.s2);
            }
        }

        /// <summary> CFG file data </summary>
        private readonly List<String3> data = new List<String3>();

        /// <summary> Current cfg file path </summary>
        public readonly string FileName = "";

        /// <summary> Load cfg file</summary>
        private void LoadFromFile()
        {
            data.Clear();
            if (!File.Exists(FileName))         // if file not exists, exit
                return;

            using (var reader = new StreamReader(FileName, Encoding.Default))           // open stream for reading
            {
                var section = "";
                var keyWord = "";
                string keyValue;
                while (!reader.EndOfStream)
                {
                    var s = reader.ReadLine();
                    var i = 0;
                    while (i < s.Length)
                    {
                        if (keyWord == "" && s[i] == ';')
                            break;

                        if (keyWord == "" && s[i] == '#')               // section keyword
                        {
                            section = s.Substring(i + 1).Trim();        //read current section
                            break;
                        }

                        if (section != "" && s[i] != ' ')
                        {
                            if (keyWord == "")                                   // parse keyword
                            {
                                while (i < s.Length && s[i] != '=')
                                {
                                    keyWord += s[i];
                                    i++;
                                    while (i == s.Length)
                                    {
                                        if (reader.EndOfStream)
                                        {
                                            keyWord = "";
                                            break;
                                        }
                                        s = reader.ReadLine();
                                        i = 0;
                                    }
                                }
                                keyWord = keyWord.Trim();
                            }

                            if (keyWord != "")                  // parse keyvalue
                            {
                                keyValue = s.Substring(i + 1).TrimStart(new[] { ' ' });
                                i = s.Length - 1;
                                if (keyValue.Length > 0 && keyValue[0] == '"')
                                {
                                    s = keyValue.Substring(1);
                                    keyValue = "";
                                    i = 0;
                                    #region read empty lines
                                    while (i == s.Length)
                                    {
                                        if (reader.EndOfStream)
                                        {
                                            MessageBox.Show("File is corrupted", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            return;
                                        }

                                        keyValue += "\n";
                                        s = reader.ReadLine();
                                    }
                                    #endregion
                                    while (s[i] != '"')
                                    {
                                        keyValue += s[i];
                                        i++;
                                        while (i == s.Length)
                                        {
                                            if (reader.EndOfStream)
                                            {
                                                MessageBox.Show("File is corrupted", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                return;
                                            }

                                            keyValue += "\n";
                                            s = reader.ReadLine();
                                            i = 0;
                                        }
                                    }
                                }
                                data.Add(new String3(section, keyWord, keyValue));
                                keyWord = "";
                            }
                        }
                        i++;
                    }
                }
            }
        }

        /// <summary> Save data to cfg file </summary>
        public void SaveToFile()
        {
            data.Sort();
            var dir = Path.GetDirectoryName(FileName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (File.Exists(FileName))
                File.SetAttributes(FileName, FileAttributes.Normal);
            using (var writer = new StreamWriter(FileName, false, Encoding.Default))
            {
                var sectionName = "";
                foreach (var item in data)
                {
                    if (!item.s1.Equals(sectionName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        sectionName = item.s1;
                        writer.WriteLine();
                        writer.WriteLine("#" + sectionName);
                        writer.WriteLine();
                    }
                    var quoteNeed = item.s3.Contains("\r") || item.s3.Contains("\n");
                    writer.WriteLine(item.s2 + "=" + (quoteNeed ? "\"" : "") + item.s3 + (quoteNeed ? "\"" : ""));
                }
            }
        }

        /// <summary> Get cfg file value by section and key </summary>
        /// <param name="sectionName">Section title</param>
        /// <param name="keyName">Key title</param>
        /// <returns>String value</returns>
        public string this[string sectionName, string keyName]
        {
            get
            {
                return this[sectionName, keyName, null];
            }
            set
            {
                for (var i = 0; i < data.Count; i++)
                {
                    if (data[i].s1.Equals(sectionName, StringComparison.CurrentCultureIgnoreCase) &&
                        data[i].s2.Equals(keyName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        data[i] = new String3(sectionName, keyName, value);
                        SaveToFile();
                        return;
                    }
                }
                Add(sectionName, keyName, value);
            }
        }
        /// <summary> Get cfg file value by section and key </summary>
        /// <param name="sectionName">Section title</param>
        /// <param name="keyName">Key title</param>
        /// <param name="defaultValue">Value by default (if can't find it)</param>
        /// <returns>String value</returns>
        public string this[string sectionName, string keyName, string defaultValue]
        {
            get
            {
                foreach (var item in data)
                {
                    if (item.s1.Equals(sectionName, StringComparison.CurrentCultureIgnoreCase) &&
                        item.s2.Equals(keyName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return item.s3;
                    }
                }
                return defaultValue;
            }
        }

        /// <summary> Add new value to cfg file </summary>
        /// <param name="sectionName">Section title</param>
        /// <param name="keyName">Key title</param>
        /// <param name="keyValue">New value</param>
        public void Add(string sectionName, string keyName, string keyValue)
        {
            if (this[sectionName, keyName] == null)                     // if can't find section or key
                data.Add(new String3(sectionName, keyName, keyValue));  // create it
            else
                this[sectionName, keyName] = keyValue;                  // another - rewrite existing value
            SaveToFile();                                                    // Save changed file
        }

        /// <summary> Constructor </summary>
        /// <param name="fileName">Path to cfg file</param>
        public CfgFile(string fileName)
        {
            FileName = fileName;
            LoadFromFile();             // load cfg file
        }

        #region IDisposable Members

        public void Dispose()
        {
            data.Clear();
        }

        #endregion
    }
}
