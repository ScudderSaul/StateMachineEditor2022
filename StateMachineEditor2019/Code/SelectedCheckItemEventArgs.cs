using System;
using StateMachineEditor2019.Controls;
using _ZenSoft.StateMachineEditor2017;

namespace StateMachineEditor2019.Code
{
   public class SelectedCheckItemEventArgs : EventArgs
    {
       public CheckItem Item { get; set; }
       public string OriginalText { get; set; }
    }
}
