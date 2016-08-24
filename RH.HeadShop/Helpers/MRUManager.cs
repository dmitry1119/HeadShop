using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace RH.HeadShop.Helpers
{
    /// <summary>
    /// MRU manager - manages Most Recently Used Files list for Windows Form application.
    /// </summary>
    public class MRUManager
    {
        #region Members
        private Form ownerForm; // owner form

        private string registryPath; // Registry path to keep MRU list

        private int maxNumberOfFiles = 10; // maximum number of files in MRU list

        private int maxDisplayLength = 40; // maximum length of file name for display

        private string currentDirectory; // current directory

        public readonly ArrayList mruList; // MRU list (file names)

        private const string regEntryName = "file"; // entry name to keep MRU (file0, file1...)


        #endregion

        #region Windows API

        // BOOL PathCompactPathEx(          
        //    LPTSTR pszOut,
        //    LPCTSTR pszSrc,
        //    UINT cchMax,
        //    DWORD dwFlags
        //    );

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern bool PathCompactPathEx(
        StringBuilder pszOut,
        string pszPath,
        int cchMax,
        int reserved);

        #endregion

        #region Constructor

        public MRUManager()
        {
            mruList = new ArrayList();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Maximum length of displayed file name in menu (default is 40).
        ///
        /// Set this property to change default value (optional).
        /// </summary>
        public int MaxDisplayNameLength
        {
            set
            {
                maxDisplayLength = value;

                if (maxDisplayLength < 10)
                    maxDisplayLength = 10;
            }

            get
            {
                return maxDisplayLength;
            }
        }

        /// <summary>
        /// Maximum length of MRU list (default is 10).
        ///
        /// Set this property to change default value (optional).
        /// </summary>
        public int MaxMRULength
        {
            set
            {
                maxNumberOfFiles = value;

                if (maxNumberOfFiles < 1)
                    maxNumberOfFiles = 1;

                if (mruList.Count > maxNumberOfFiles)
                    mruList.RemoveRange(maxNumberOfFiles - 1, mruList.Count - maxNumberOfFiles);
            }

            get
            {
                return maxNumberOfFiles;
            }
        }

        /// <summary>
        /// Set current directory.
        ///
        /// Default value is program current directory which is set when
        /// Initialize function is called.
        ///
        /// Set this property to change default value (optional)
        /// after call to Initialize.
        /// </summary>
        public string CurrentDir
        {
            set
            {
                currentDirectory = value;
            }

            get
            {
                return currentDirectory;
            }
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Initialization. Call this function in form Load handler.
        /// </summary>
        /// <param name="owner">Owner form</param>
        /// <param name="mruItem">Recent Files menu item</param>
        /// <param name="regPath">Registry Path to keep MRU list</param>
        public void Initialize(Form owner, string regPath)
        {
            // keep Registry path adding MRU key to it
            registryPath = regPath;
            if (registryPath.EndsWith("\\"))
                registryPath += "MRU";
            else
                registryPath += "\\MRU";


            // keep current directory in the time of initialization
            currentDirectory = Directory.GetCurrentDirectory();

            ownerForm = owner;
            // subscribe to owner form Closing event
            ownerForm.Closing += OnOwnerClosing;

            // load MRU list from Registry
            LoadMRU();
        }

        /// <summary>
        /// Add file name to MRU list.
        /// Call this function when file is opened successfully.
        /// If file already exists in the list, it is moved to the first place.
        /// </summary>
        /// <param name="file">File Name</param>
        public void Add(string file)
        {
            Remove(file);

            // if array has maximum length, remove last element
            if (mruList.Count == maxNumberOfFiles)
                mruList.RemoveAt(maxNumberOfFiles - 1);

            // add new file name to the start of array
            mruList.Insert(0, file);
        }

        /// <summary>
        /// Remove file name from MRU list.
        /// Call this function when File - Open operation failed.
        /// </summary>
        /// <param name="file">File Name</param>
        public void Remove(string file)
        {
            var i = 0;

            var myEnumerator = mruList.GetEnumerator();

            while (myEnumerator.MoveNext())
            {
                if ((string)myEnumerator.Current == file)
                {
                    mruList.RemoveAt(i);
                    return;
                }

                i++;
            }
        }

        #endregion
        
        
        #region Event Handlers

        /// <summary>
        /// Save MRU list in Registry when owner form is closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOwnerClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                var key = Registry.CurrentUser.CreateSubKey(registryPath);

                if (key != null)
                {
                    var n = mruList.Count;

                    int i;
                    for (i = 0; i < maxNumberOfFiles; i++)
                    {
                        key.DeleteValue(regEntryName + i.ToString(CultureInfo.InvariantCulture), false);
                    }

                    for (i = 0; i < n; i++)
                    {
                        key.SetValue(regEntryName + i.ToString(CultureInfo.InvariantCulture), mruList[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Saving MRU to Registry failed: " + ex.Message);
            }
        }


        #endregion

        #region Private Functions

        /// <summary>
        /// Load MRU list from Registry.
        /// Called from Initialize.
        /// </summary>
        private void LoadMRU()
        {
            try
            {
                mruList.Clear();

                var key = Registry.CurrentUser.OpenSubKey(registryPath, true);

                if (key != null)
                {
                    for (var i = 0; i < maxNumberOfFiles; i++)
                    {
                        var sKey = regEntryName + i.ToString(CultureInfo.InvariantCulture);

                        var s = (string)key.GetValue(sKey, "");

                        if (s.Length == 0)
                            break;

                        if (!File.Exists(s))        // remove deleted files
                        {
                            key.DeleteValue(regEntryName + i.ToString(CultureInfo.InvariantCulture), false);
                            continue;
                        }

                        mruList.Add(s);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Loading MRU from Registry failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Get display file name from full name.
        /// </summary>
        /// <param name="fullName">Full file name</param>
        /// <returns>Short display name</returns>
        public string GetDisplayName(string fullName)
        {
            // if file is in current directory, show only file name
            var fileInfo = new FileInfo(fullName);

            if (fileInfo.DirectoryName == currentDirectory)
                return GetShortDisplayName(fileInfo.Name, maxDisplayLength);

            return GetShortDisplayName(fullName, maxDisplayLength);
        }

        /// <summary>
        /// Truncate a path to fit within a certain number of characters 
        /// by replacing path components with ellipses.
        ///
        /// This solution is provided by CodeProject and GotDotNet C# expert
        /// Richard Deeming.
        ///
        /// </summary>
        /// <param name="longName">Long file name</param>
        /// <param name="maxLen">Maximum length</param>
        /// <returns>Truncated file name</returns>
        private string GetShortDisplayName(string longName, int maxLen)
        {
            var pszOut = new StringBuilder(maxLen + maxLen + 2); // for safety

            if (PathCompactPathEx(pszOut, longName, maxLen, 0))
            {
                return pszOut.ToString();
            }
            return longName;
        }

        #endregion
    }
}
