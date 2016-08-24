using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace RH.HeadShop.IO
{
    /// <summary> Skeletno for enhanced dialogue open / save files </summary>
    public abstract class FileDialogEx : IDisposable
    {
        /// <summary> Constructor </summary>
        public FileDialogEx()
        {

        }
        /// <summary>Constructor </summary>
        /// <param name="title">Title for dialog</param>
        public FileDialogEx(string title)
        {
            Title = title;
            Filter = "All files (*.*)|*.*";
        }
        /// <summary> Constructor </summary>
        /// <param name="title">Title for dialog</param>
        /// <param name="filter">Filter for showing files. Example: "All files (*.*)|*.*". First part - display for user, second - go to OS</param>
        public FileDialogEx(string title, string filter)
        {
            Title = title;
            Filter = filter;
        }
     
        private FileDialog _dlg;
        /// <summary> Standart filedialog </summary>
        protected FileDialog dlg
        {
            get { return _dlg ?? (_dlg = CreateDlg()); }
        }
        /// <summary> Create standart dialog </summary>
        /// <returns>Return copy of dialog</returns>
        protected abstract FileDialog CreateDlg();
        /// <summary> Show dialog for user </summary>
        /// <returns>return user selection (Ok, Cancel) </returns>
        public DialogResult ShowDialog(bool needAllFiles = true)
        {
            if (needAllFiles && !dlg.Filter.Contains("*.*"))
                dlg.Filter = string.Format("{0}All files (*.*)|*.*", (string.IsNullOrEmpty(dlg.Filter) ? "" : dlg.Filter + "|"));
            var result = dlg.ShowDialog();
            return result;
        }
       

        #region Differences

        // Summary:
        //     Gets or sets a value indicating whether the dialog box automatically adds
        //     an extension to a file name if the user omits the extension.
        //
        // Returns:
        //     true if the dialog box adds an extension to a file name if the user omits
        //     the extension; otherwise, false. The default value is true.
        [DefaultValue(true)]
        public bool AddExtension { get { return dlg.AddExtension; } set { dlg.AddExtension = value; } }
        //
        // Summary:
        //     Gets or sets a value indicating whether this System.Windows.Forms.FileDialog
        //     instance should automatically upgrade appearance and behavior when running
        //     on Windows Vista.
        //
        // Returns:
        //     true if this System.Windows.Forms.FileDialog instance should automatically
        //     upgrade appearance and behavior when running on Windows Vista; otherwise,
        //     false. The default is true.
        [DefaultValue(true)]
        public bool AutoUpgradeEnabled { get { return dlg.AutoUpgradeEnabled; } set { dlg.AutoUpgradeEnabled = value; } }
        //
        // Summary:
        //     Gets or sets a value indicating whether the dialog box displays a warning
        //     if the user specifies a file name that does not exist.
        //
        // Returns:
        //     true if the dialog box displays a warning if the user specifies a file name
        //     that does not exist; otherwise, false. The default value is false.
        [DefaultValue(false)]
        public virtual bool CheckFileExists { get { return dlg.CheckFileExists; } set { dlg.CheckFileExists = value; } }
        //
        // Summary:
        //     Gets or sets a value indicating whether the dialog box displays a warning
        //     if the user specifies a path that does not exist.
        //
        // Returns:
        //     true if the dialog box displays a warning when the user specifies a path
        //     that does not exist; otherwise, false. The default value is true.
        [DefaultValue(true)]
        public bool CheckPathExists { get { return dlg.CheckPathExists; } set { dlg.CheckPathExists = value; } }
        //
        // Summary:
        //     Gets the custom places collection for this System.Windows.Forms.FileDialog
        //     instance.
        //
        // Returns:
        //     The custom places collection for this System.Windows.Forms.FileDialog instance.
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public FileDialogCustomPlacesCollection CustomPlaces { get { return dlg.CustomPlaces; } }
        //
        // Summary:
        //     Gets or sets the default file name extension.
        //
        // Returns:
        //     The default file name extension. The returned string does not include the
        //     period. The default value is an empty string ("").
        [DefaultValue("")]
        public string DefaultExt { get { return dlg.DefaultExt; } set { dlg.DefaultExt = value; } }
        //
        // Summary:
        //     Gets or sets a value indicating whether the dialog box returns the location
        //     of the file referenced by the shortcut or whether it returns the location
        //     of the shortcut (.lnk).
        //
        // Returns:
        //     true if the dialog box returns the location of the file referenced by the
        //     shortcut; otherwise, false. The default value is true.
        [DefaultValue(true)]
        public bool DereferenceLinks { get { return dlg.DereferenceLinks; } set { dlg.DereferenceLinks = value; } }
        //
        // Summary:
        //     Gets or sets a string containing the file name selected in the file dialog
        //     box.
        //
        // Returns:
        //     The file name selected in the file dialog box. The default value is an empty
        //     string ("").
        [DefaultValue("")]
        public string FileName { get { return dlg.FileName; } set { dlg.FileName = value; } }
        //
        // Summary:
        //     Gets the file names of all selected files in the dialog box.
        //
        // Returns:
        //     An array of type System.String, containing the file names of all selected
        //     files in the dialog box.
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string[] FileNames { get { return dlg.FileNames; } }
        //
        // Summary:
        //     Gets or sets the current file name filter string, which determines the choices
        //     that appear in the "Save as file type" or "Files of type" box in the dialog
        //     box.
        //
        // Returns:
        //     The file filtering options available in the dialog box.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     Filter format is invalid.
        [Localizable(false), DefaultValue("")]
        public string Filter { get { return dlg.Filter; } set { dlg.Filter = value; } }
        //
        // Summary:
        //     Gets or sets the index of the filter currently selected in the file dialog
        //     box.
        //
        // Returns:
        //     A value containing the index of the filter currently selected in the file
        //     dialog box. The default value is 1.
        [DefaultValue(1)]
        public int FilterIndex { get { return dlg.FilterIndex; } set { dlg.FilterIndex = value; } }
        //
        // Summary:
        //     Gets or sets the initial directory displayed by the file dialog box.
        //
        // Returns:
        //     The initial directory displayed by the file dialog box. The default is an
        //     empty string ("").
        [DefaultValue("")]
        public string InitialDirectory { get { return dlg.InitialDirectory; } set { dlg.InitialDirectory = value; } }
        //
        // Summary:
        //     Gets or sets whether the dialog box supports displaying and saving files
        //     that have multiple file name extensions.
        //
        // Returns:
        //     true if the dialog box supports multiple file name extensions; otherwise,
        //     false. The default is false.
        [DefaultValue(false)]
        public bool SupportMultiDottedExtensions { get { return dlg.SupportMultiDottedExtensions; } set { dlg.SupportMultiDottedExtensions = value; } }
        //
        // Summary:
        //     Gets or sets the file dialog box title.
        //
        // Returns:
        //     The file dialog box title. The default value is an empty string ("").
        [Localizable(false), DefaultValue("")]
        public string Title { get { return dlg.Title; } set { dlg.Title = value; } }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            dlg.Dispose();
        }

        #endregion
    }

    public class OpenFileDialogEx : FileDialogEx
    {
        public OpenFileDialogEx()
        {
        }
        public OpenFileDialogEx(string title)
            : base(title)
        {

        }
        public OpenFileDialogEx(string title, string filter)
            : base(title, filter)
        {

        }
        protected override FileDialog CreateDlg()
        {
            return new OpenFileDialog();
        }
        //
        // Summary:
        //     Gets or sets a value indicating whether the dialog box allows multiple files
        //     to be selected.
        //
        // Returns:
        //     true if the dialog box allows multiple files to be selected together or concurrently;
        //     otherwise, false. The default value is false.
        [DefaultValue(false)]
        public bool Multiselect
        {
            get
            {
                return (dlg as OpenFileDialog).Multiselect;
            }
            set
            {
                (dlg as OpenFileDialog).Multiselect = value;
            }
        }
    }
    public class SaveFileDialogEx : FileDialogEx
    {
        public SaveFileDialogEx()
        {
            OverwritePrompt = true;
        }
        public SaveFileDialogEx(string title)
            : base(title)
        {

        }
        public SaveFileDialogEx(string title, string filter)
            : base(title, filter)
        {

        }
        protected override FileDialog CreateDlg()
        {
            return new SaveFileDialog();
        }
        //
        // Summary:
        //     Gets or sets a value indicating whether the Save As dialog box displays a
        //     warning if the user specifies a file name that already exists.
        //
        // Returns:
        //     true if the dialog box prompts the user before overwriting an existing file
        //     if the user specifies a file name that already exists; false if the dialog
        //     box automatically overwrites the existing file without prompting the user
        //     for permission. The default value is true.
        [DefaultValue(true)]
        public bool OverwritePrompt
        {
            get
            {
                return (dlg as SaveFileDialog).OverwritePrompt;
            }
            set
            {
                (dlg as SaveFileDialog).OverwritePrompt = value;
            }
        }
    }
}
