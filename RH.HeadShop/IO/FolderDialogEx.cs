using System;
using System.Windows.Forms;
using RH.HeadShop.Helpers;

namespace RH.HeadShop.IO
{
    /// <summary>
    /// Wraps System.Windows.Forms.OpenFileDialog to make it present
    /// a vista-style dialog.
    /// </summary>
    public class FolderDialogEx : IDisposable
    {
        #region Var

        /// <summary>
        /// Gets/Sets the initial folder to be selected. A null value selects the current directory.
        /// </summary>
        public string InitialDirectory
        {
            get
            {
                return ofd.InitialDirectory;
            }
            set
            {
                ofd.InitialDirectory = string.IsNullOrEmpty(value) ? Environment.CurrentDirectory : value;
            }
        }
        /// <summary>
        /// Gets/Sets the title to show in the dialog
        /// </summary>
        public string Title
        {
            get { return ofd.Title; }
            set { ofd.Title = value ?? "Select folder"; }
        }
        /// <summary> List of selected folders </summary>
        public string[] SelectedFolder
        {
            get { return ofd.FileNames; }
        }
        public Environment.SpecialFolder RootFolder = Environment.SpecialFolder.MyComputer;

        // Wrapped dialog
        private readonly OpenFileDialog ofd;

        #endregion

        /// <summary> Constructor </summary>
        public FolderDialogEx()
        {
            ofd = new OpenFileDialog { Filter = @"Folders|\n", AddExtension = false, CheckFileExists = false, DereferenceLinks = true, Multiselect = true };            // initialize standart open file dialog
        }

        /// <summary>
        /// Shows the dialog
        /// </summary>
        /// <returns>True if the user presses OK else false</returns>
        public DialogResult ShowDialog()
        {
            bool flag;

            if (Environment.OSVersion.Version.Major >= 6)            // if current OS is windows and version Vista or higher - then use extended dialogs
            {
                var r = new Reflector("System.Windows.Forms");

                uint num = 0;
                var typeIFileDialog = r.GetType("FileDialogNative.IFileDialog");
                var dialog = r.Call(ofd, "CreateVistaDialog");
                r.Call(ofd, "OnBeforeVistaDialog", dialog);

                var options = (uint)r.CallAs(typeof(FileDialog), ofd, "GetOptions");
                options |= (uint)r.GetEnum("FileDialogNative.FOS", "FOS_PICKFOLDERS");
                r.CallAs(typeIFileDialog, dialog, "SetOptions", options);

                var pfde = r.New("FileDialog.VistaDialogEvents", ofd);
                var parameters = new[] { pfde, num };
                r.CallAs2(typeIFileDialog, dialog, "Advise", parameters);
                num = (uint)parameters[1];
                try
                {
                    var num2 = (int)r.CallAs(typeIFileDialog, dialog, "Show", IntPtr.Zero);
                    flag = 0 == num2;
                }
                finally
                {
                    r.CallAs(typeIFileDialog, dialog, "Unadvise", num);
                    GC.KeepAlive(pfde);
                }
            }
            else                        // in another case - use standart dialog
            {
                using (var fbd = new FolderBrowserDialog { Description = Title, SelectedPath = InitialDirectory, ShowNewFolderButton = false, RootFolder = RootFolder })
                {
                    if (fbd.ShowDialog() != DialogResult.OK)
                        return DialogResult.Cancel;
                    ofd.FileName = fbd.SelectedPath;
                    flag = true;
                }
            }

            return flag ? DialogResult.OK : DialogResult.Cancel;
        }

        /// <summary> Do dialog disposable, to use in "using" </summary>
        public void Dispose()
        {

        }
    }
}
