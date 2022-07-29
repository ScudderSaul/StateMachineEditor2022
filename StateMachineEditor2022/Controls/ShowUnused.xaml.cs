using System.Collections.Generic;
using System.Windows.Controls;

namespace _ZenSoft.StateMachineEditor2017
{
    /// <summary>
    /// Interaction logic for ShowUnused.xaml
    /// </summary>
    public partial class ShowUnused : UserControl
    {
        List<string> StartNot;
        List<string> FinalNot;
        List<string> ActionNot;
        List<string> EvntNot;
        List<string> DeadEnd;
        List<string> DeadStart;
        public ShowUnused()
        {
            InitializeComponent();
        }

        public ShowUnused(List<string> zStartNot, List<string> zFinalNot, List<string> zActionNot, List<string> zEvntNot, List<string> zDeadEnd, List<string> zDeadStart)
        {
            InitializeComponent();
            StartNot = zStartNot;
            if (StartNot == null)
            {
                StartNot = new List<string>();
            }
            FinalNot = zFinalNot;
            if (FinalNot == null)
            {
                FinalNot = new List<string>();
            }
            ActionNot = zActionNot;
            if (ActionNot == null)
            {
                ActionNot = new List<string>();
            }
            EvntNot = zEvntNot;
            if (EvntNot == null)
            {
                EvntNot = new List<string>();
            }
            DeadEnd = zDeadEnd;
            if (DeadEnd == null)
            {
                DeadEnd = new List<string>();
            }
            DeadStart = zDeadStart;
            if (DeadStart == null)
            {
                DeadStart = new List<string>();
            }
            foreach (string sg in StartNot)
            {
                NoTransitionStatesListBox.Items.Add(sg);
            }
            foreach (string sg in FinalNot)
            {
                StateNeverReachedListBox.Items.Add(sg);
            }
            foreach (string sg in ActionNot)
            {
                ActionsNeverUsedListBox.Items.Add(sg);
            }
            foreach (string sg in EvntNot)
            {
                EventsNeverUsedListBox.Items.Add(sg);
            }
            foreach (string sg in DeadStart)
            {
                NeverReturnedStatesListBox.Items.Add(sg);
            }
            foreach (string sg in DeadEnd)
            {
                DeadEndStatesListBox.Items.Add(sg);
            }
        }

    }
}
