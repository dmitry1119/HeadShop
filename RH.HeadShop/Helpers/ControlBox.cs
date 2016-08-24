using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using RH.HeadShop.Controls;

namespace RH.HeadShop.Helpers
{
   public class ControlBox
    {
        private const int Margin = 4;
        private static void AddButtons(Form box, Button[] buttons, int defaultButton)
        {
            double v;
            buttons = (from button in buttons
                       orderby (button.Tag != null && (double.TryParse(button.Tag.ToString(), out v)) ? double.Parse(button.Tag.ToString()) : 1 + double.Epsilon)
                       select button).ToArray();

            var left = box.ClientSize.Width - Margin - (box.FormBorderStyle == FormBorderStyle.Sizable || box.FormBorderStyle == FormBorderStyle.SizableToolWindow ? 10 : 0);
            for (var i = buttons.Length - 1; i >= 0; i--)
            {
                buttons[i].Location = new Point(left - buttons[i].Width, box.ClientSize.Height - buttons[i].Height - Margin);
                buttons[i].Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                buttons[i].Parent = box;
                left -= buttons[i].Width + Margin;
            }
            for (var i = 0; i < buttons.Length; i++)
            {
                buttons[i].TabIndex = i + 1;
            }
            if (0 <= defaultButton && defaultButton < buttons.Length)
                box.AcceptButton = buttons[defaultButton];

            box.ClientSize = new Size(Math.Max(box.ClientSize.Width - left + 2 * Margin, box.ClientSize.Width), box.ClientSize.Height);
        }

        private static Button CreateButton(string text, DialogResult dialogResult, double order = 0)
        {
            var result = new Button { Tag = order, Text = text, DialogResult = dialogResult };
            return result;
        }
        private static List<Button> GetControlButtons(Control control)
        {
            var fields = control.GetType().GetFields();

            var buttons = new List<Button>();
            foreach (var field in fields)
            {
                if (field.IsPublic)
                {
                    var button = field.GetValue(control) as Button;
                    if (button != null)
                        buttons.Add(button);
                }
            }
            return buttons;
        }
        private static void AppendButton(List<Button> controlButtons, string text, DialogResult dialogResult, double order)
        {
            if (!controlButtons.Exists(btn => btn.DialogResult == dialogResult))
                controlButtons.Add(CreateButton(text, dialogResult, order));
        }

        private static void AppendButtons(Form box, Control control, MessageBoxButtons buttons, int defaultButton)
        {
            var controlButtons = GetControlButtons(control);
            Button cancelButton = null;

            var oKpresent = controlButtons.Any(t => t.DialogResult == DialogResult.OK);

            switch (buttons)
            {
                case MessageBoxButtons.AbortRetryIgnore:
                    AppendButton(controlButtons, "Abort", DialogResult.Abort, 1 + double.Epsilon);
                    AppendButton(controlButtons, "Repeat", DialogResult.Retry, 2 + double.Epsilon);
                    AppendButton(controlButtons, "Ignore", DialogResult.Ignore, 3 + double.Epsilon);
                    cancelButton = controlButtons[controlButtons.Count - 1];
                    break;
                case MessageBoxButtons.OK:
                    if (oKpresent)
                        AppendButton(controlButtons, "Close", DialogResult.Cancel, 1 + double.Epsilon);
                    else
                        AppendButton(controlButtons, "OK", DialogResult.OK, 1 + double.Epsilon);
                    cancelButton = controlButtons[controlButtons.Count - 1];
                    break;
                case MessageBoxButtons.OKCancel:
                    AppendButton(controlButtons, "OK", DialogResult.OK, 1 + double.Epsilon);
                    AppendButton(controlButtons, "Cancel", DialogResult.Cancel, 2 + double.Epsilon);
                    cancelButton = controlButtons[controlButtons.Count - 1];
                    break;
                case MessageBoxButtons.RetryCancel:
                    AppendButton(controlButtons, "Repeat", DialogResult.Retry, 1 + double.Epsilon);
                    AppendButton(controlButtons, "Cancel", DialogResult.Cancel, 2 + double.Epsilon);
                    cancelButton = controlButtons[controlButtons.Count - 1];
                    break;
                case MessageBoxButtons.YesNo:
                    AppendButton(controlButtons, "Yes", DialogResult.Yes, 1 + double.Epsilon);
                    AppendButton(controlButtons, "No", DialogResult.No, 2 + double.Epsilon);
                    cancelButton = controlButtons[controlButtons.Count - 1];
                    break;
                case MessageBoxButtons.YesNoCancel:
                    AppendButton(controlButtons, "Yes", DialogResult.Yes, 1 + double.Epsilon);
                    AppendButton(controlButtons, "No", DialogResult.No, 2 + double.Epsilon);
                    AppendButton(controlButtons, "Cancel", DialogResult.Cancel, 3 + double.Epsilon);
                    cancelButton = controlButtons[controlButtons.Count - 1];
                    break;
            }

            AddButtons(box, controlButtons.ToArray(), defaultButton);

            if (cancelButton != null)
                box.CancelButton = cancelButton;
        }

        public static DialogResult Show(IWin32Window owner, Control control, string caption, MessageBoxButtons buttons, int defaultButton, bool canResize)
        {
            int btnHeight;
            using (var btn = CreateButton("test", DialogResult.None))
            {
                btnHeight = btn.Height;
            }
            var box = new frmControlBox
            {
                Text = caption,
                FormBorderStyle = canResize ? FormBorderStyle.Sizable : FormBorderStyle.FixedDialog
            };
            if (canResize)
            {
                box.MaximizeBox = true;
            }
            if (!control.MinimumSize.IsEmpty)
                box.MinimumSize = new Size(control.MinimumSize.Width + 2 * Margin, control.MinimumSize.Height + btnHeight + 3 * Margin) + (box.Size - box.ClientSize);
            if (!control.MaximumSize.IsEmpty)
                box.MaximumSize = new Size(control.MaximumSize.Width + 2 * Margin, control.MaximumSize.Height + btnHeight + 3 * Margin) + (box.Size - box.ClientSize);
            box.ClientSize = new Size(control.Width + 2 * Margin, control.Height + btnHeight + 3 * Margin);
            control.Location = new Point(Margin, Margin);
            control.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            AppendButtons(box, control, buttons, defaultButton);

            control.Parent = box;

            return owner != null ? box.ShowDialog(owner) : box.ShowDialog();
        }
        public static DialogResult Show(IWin32Window owner, Control control, string caption, bool canResize)
        {
            int btnHeight;
            using (var btn = CreateButton("test", DialogResult.None))
            {
                btnHeight = btn.Height;
            }
            var box = new frmControlBox
            {
                Text = caption,
                FormBorderStyle = canResize ? FormBorderStyle.Sizable : FormBorderStyle.FixedDialog
            };
            if (canResize)
            {
                box.MaximizeBox = true;
            }
            if (!control.MinimumSize.IsEmpty)
                box.MinimumSize = new Size(control.MinimumSize.Width + 2 * Margin, control.MinimumSize.Height + btnHeight + 3 * Margin) + (box.Size - box.ClientSize);
            if (!control.MaximumSize.IsEmpty)
                box.MaximumSize = new Size(control.MaximumSize.Width + 2 * Margin, control.MaximumSize.Height + btnHeight + 3 * Margin) + (box.Size - box.ClientSize);
            box.ClientSize = new Size(control.Width + 2 * Margin, control.Height + btnHeight + 3 * Margin);
            control.Location = new Point(Margin, Margin);
            control.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            control.Parent = box;

            var buttons = GetControlButtons(control);
            AddButtons(box, buttons.ToArray(), -1);

            foreach (var button in buttons)
            {
                if (button.DialogResult == DialogResult.OK)
                {
                    box.AcceptButton = button;
                }
                else if (button.DialogResult == DialogResult.Cancel)
                {
                    box.CancelButton = button;
                }
            }

            return owner != null ? box.ShowDialog(owner) : box.ShowDialog();
        }

        public static void ShowFloatWindow(IWin32Window owner, Control control, string caption, bool canResize, bool allowMaximize = false)
        {
            #region Dialog creation

            int btnHeight;
            using (var btn = CreateButton("test", DialogResult.None))
            {
                btnHeight = btn.Height;
            }
            var box = new frmControlBox { Owner = owner as Form, ShowInTaskbar = true, Text = caption };
            //box.StartPosition = FormStartPosition.CenterParent;
            if (canResize && allowMaximize)
            {
                box.FormBorderStyle = FormBorderStyle.Sizable;
                box.MaximizeBox = true;
                box.MinimizeBox = false;
            }
            else if (canResize)
            {
                box.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            }
            else
            {
                box.FormBorderStyle = FormBorderStyle.FixedDialog;
            }
            //box.FormBorderStyle = canResize ? FormBorderStyle.Sizable: FormBorderStyle.FixedDialog;
            if (!control.MinimumSize.IsEmpty)
                box.MinimumSize = new Size(control.MinimumSize.Width + 2 * Margin, control.MinimumSize.Height + btnHeight + 3 * Margin) + (box.Size - box.ClientSize);
            if (!control.MaximumSize.IsEmpty)
                box.MaximumSize = new Size(control.MaximumSize.Width + 2 * Margin, control.MaximumSize.Height + btnHeight + 3 * Margin) + (box.Size - box.ClientSize);
            box.ClientSize = new Size(control.Width + 2 * Margin, control.Height + btnHeight + 3 * Margin);
            control.Location = new Point(Margin, Margin);
            control.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            control.Parent = box;
            var controlButtons = GetControlButtons(control);
            if (controlButtons.Count == 0)
            {
                controlButtons.Add(CreateButton("Close", DialogResult.Cancel));
                box.CancelButton = controlButtons[0];
                controlButtons[0].Click += box_Disposed;
            }
            else
            {
                foreach (var btn in controlButtons)
                {
                    if (btn.DialogResult == DialogResult.Cancel)
                    {
                        btn.Click += box_Disposed;
                        box.CancelButton = btn;
                        break;
                    }
                }
            }
            AddButtons(box, controlButtons.ToArray(), 0);
            #endregion
            box.Disposed += box_Disposed;
            if (owner != null)
                box.Show(owner);
            else
                box.Show();
        }
        private static void box_Disposed(object sender, EventArgs e)
        {
            Control form;
            if (sender is Button)
                form = (sender as Button).Parent;
            else
                form = (sender as frmControlBox);
            if (form != null)
            {
                form.Disposed -= box_Disposed;
                form.Dispose();
            }
        }
    }
}
