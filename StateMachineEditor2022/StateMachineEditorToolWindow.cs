using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using StateMachineEditor2022.Controls;

namespace StateMachineEditor2022
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    /// [Guid("7205d4ce-cd59-42fe-9648-9d9427f5feed")]
    /// to  40d2e56c-2142-40cf-9025-5c9c948c1ae8
    [Guid("40d2e56c-2142-40cf-9025-5c9c948c1ae8")]
    public class StateMachineEditorToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachineEditorToolWindow"/> class.
        /// </summary>
        public StateMachineEditorToolWindow() : base(null)
        {
            this.Caption = "StateMachineEditorToolWindow";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            // this.Content = new StateMachineEditorToolWindowControl();
            this.Content = new StateMachineEditorControl();
        }
    }
}
