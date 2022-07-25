using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Xml.Linq;
using MVCStateMachineCodeGeneration;
using SmSimpleData;
using StateMachineCodeGeneration;
using StateMachineEditor2019.Code;
using StatePattern;
using _ZenSoft.StateMachineEditor2017;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using PrintDialog = System.Windows.Controls.PrintDialog;
using RadioButton = System.Windows.Controls.RadioButton;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;

namespace StateMachineEditor2019.Controls
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class StateMachineEditorControl : UserControl
    {
        #region fields

        private Dictionary<string, Int64> _actiondict;
        private bool _dirty;
        private Dictionary<string, Int64> _evntdict;
        private string _lastprojectdirectory;
        private Int64 _mxindex;
        private string _smInitialState;
        private string _smName;
        private XElement _smTree;
        private SmCodeGeneration _smCodeGeneration;
        private MvcCodeGeneration _mvcCodeGeneration;
        private Dictionary<string, Int64> _statedict;
        private Dictionary<string, Dictionary<string, Tranval>> _transitions;
        private string _xmlfile;
        /*
                private string _lastproj;
        */


        // speech 
        //   SpeechSynthesizer _synthesizer = new SpeechSynthesizer();
        //     bool _isSpeechOn = true;
        //   string _selectedVoice = string.Empty;
        // VoiceInfo vi = new VoiceInfo();

        #endregion

        #region ctor

        public StateMachineEditorControl()
        {
            InitializeComponent();
            Init();
        }

        #endregion

        #region properties

        public Action<List<string>> OutputErrors { get; set; }

        #endregion

        #region Init

        private void Init()
        {
            _smCodeGeneration = new SmCodeGeneration();
            _mvcCodeGeneration = new MvcCodeGeneration();
            Unloaded += OnClosing;
            _mxindex = 0;
            _statedict = new Dictionary<string, long>();
            _evntdict = new Dictionary<string, long>();
            _actiondict = new Dictionary<string, long>();
            _transitions = new Dictionary<string, Dictionary<string, Tranval>>();
            _statedict.Add("State0", _mxindex++);
            _evntdict.Add("Event0", _mxindex++);
            _actiondict.Add("Action0", _mxindex);
            _smName = "Unknown";
            _smInitialState = "State0";
            _smTree = new XElement("StateMachine");
            _dirty = false;

            List<string> ls = _smCodeGeneration.Supported;
            foreach (string st in ls)
            {
                if (st != "js")
                {
                    OutputLanguageListBox.Items.Add(st);
                }
            }
            int index = OutputLanguageListBox.Items.IndexOf("c#");
            OutputLanguageListBox.SelectedIndex = index;
            GenerateAll();
        }


        //protected void SpeakText(string output)
        //{
        //    if (_isSpeechOn)
        //    {
        //        //   _synthesizer.SelectVoice(SelectedVoice);
        //        _synthesizer.SpeakAsync(output);
        //    }
        //}

        //public string SelectedVoice
        //{
        //    get { return _selectedVoice; }
        //    set { _selectedVoice = value; }
        //}

        #endregion

        #region State Machine State EventHandlers

        private static int _statecnt;

        private void OnAddStateClick(object sender, RoutedEventArgs e)
        {
            _statecnt++;
            string st = string.Format("{0}{1}", "NewState", _statecnt);
            if (_statedict.ContainsKey(st) == false)
            {
                _mxindex++;
                _statedict.Add(st, _mxindex);
                GenerateAll();
                _dirty = true;
            }
            else
            {
                MessageBox.Show("State " + st + " Already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDeleteStateClick(object sender, RoutedEventArgs e)
        {
            var wpi = StateListBox.SelectedItem as CheckItem;
            if (wpi == null)
            {
                MessageBox.Show("No state selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string ns = wpi.NameText.Text;
            if (_statedict.ContainsKey(ns))
            {
                if (MessageBox.Show(
                    "This will delete transitions that contain sm_State " + ns + " also\r\n Are you sure?",
                    "Warning Delete Transitions",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    _statedict.Remove(ns);
                    _transitions.Remove(ns);
                    foreach (var ev in _transitions)
                    {
                        Dictionary<string, Tranval> td = ev.Value;
                        var evwithfinal = new List<string>();
                        foreach (var tt in td)
                        {
                            if (tt.Value.Endstate == ns)
                            {
                                evwithfinal.Add(tt.Value.Evntname);
                            }
                        }
                        foreach (string evnt in evwithfinal)
                        {
                            ev.Value.Remove(evnt);
                        }
                        Dictionary<string, Tranval> kd = ev.Value;
                        if (kd.Count == 0)
                        {
                            _transitions.Remove(ev.Key);
                        }
                    }
                    _dirty = true;
                    GenerateAll();
                }
            }
        }

        private void StateName_LostFocus(object sender, RoutedEventArgs e)
        {
            var wpi = StateListBox.SelectedItem as CheckItem;
            if (wpi == null)
            {
                if (StateListBox.Items.Count > 0)
                {
                    StateListBox.SelectedIndex = 0;
                }

                return;
            }

            string now = wpi.OriginalText.Text;
            string after = wpi.NameText.Text.Trim();
            if (after == now)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(after))
            {
                MessageBox.Show("Is Empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_transitions.ContainsKey(after))
            {
                string mest = "There are existing transitions starting with sm_State " + after +
                              "\r\nThis will delete " + now +
                              "'s transitions when the event is the same\r\n Are you sure?";
                if (MessageBox.Show(mest, "Warning Common Initial sm_State and sm_Event in transitions ",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                {
                    return;
                }
            }
            if (_statedict.ContainsKey(now))
            {
                foreach (var ev in _transitions)
                {
                    Dictionary<string, Tranval> td = ev.Value;

                    foreach (var tt in td)
                    {
                        Tranval tran = tt.Value;
                        if (tran.Startstate == now)
                        {
                            tran.Startstate = after;
                        }
                        if (tt.Value.Endstate == now)
                        {
                            tran.Endstate = after;
                        }
                    }
                }

                if (_transitions.ContainsKey(after))
                {
                    Dictionary<string, Tranval> afterd = _transitions[after];
                    Dictionary<string, Tranval> nowd = _transitions[now];

                    foreach (var nd in nowd)
                    {
                        if (afterd.ContainsKey(nd.Key) == false)
                        {
                            afterd[nd.Key] = nd.Value;
                        }
                    }
                }
                else
                {
                    if (_transitions.ContainsKey(now))
                    {
                        _transitions.Add(after, _transitions[now]);
                    }
                }
                if (_transitions.ContainsKey(now))
                {
                    _transitions.Remove(now);
                }
                _statedict.Remove(now);
                if (_statedict.ContainsKey(after) == false)
                {
                    _mxindex++;
                    _statedict.Add(after, _mxindex);
                }
            }
            _dirty = true;
            GenerateAll();
        }

        private void StateListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (object item in StateListBox.Items)
            {
                (item as CheckItem).SetUnSelected();
                (item as CheckItem).NameText.IsReadOnly = true;
            }
            if (StateListBox.SelectedIndex >= 0)
            {
                (StateListBox.SelectedItem as CheckItem).SetSelected();
                (StateListBox.SelectedItem as CheckItem).NameText.IsReadOnly = false;
                (StateListBox.SelectedItem as CheckItem).NameText.Focus();
            }
        }


        private void FinalStateListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (object item in FinalStateListBox.Items)
            {
                (item as CheckItem).SetUnSelected();
                (item as CheckItem).NameText.IsReadOnly = true;
            }
            if (FinalStateListBox.SelectedIndex >= 0)
            {
                (FinalStateListBox.SelectedItem as CheckItem).SetSelected();
                (FinalStateListBox.SelectedItem as CheckItem).NameText.IsReadOnly = false;
            }
        }

        #endregion

        #region State Machine Event EventHandlers

        private static int _eventCnt;

        private void OnAddEventClick(object sender, RoutedEventArgs e)
        {
            _eventCnt++;
            string st = string.Format("{0}{1}", "NewEvent", _eventCnt);

            if (_evntdict.ContainsKey(st) == false)
            {
                _mxindex++;
                _evntdict.Add(st, _mxindex);
                GenerateAll();
                _dirty = true;
            }
            else
            {
                MessageBox.Show("Action " + st + " Already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDeleteEventClick(object sender, RoutedEventArgs e)
        {
            var wpi = EventListBox.SelectedItem as CheckItem;
            if (wpi == null)
            {
                MessageBox.Show("No Event Selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string ns = wpi.NameText.Text;
            if (_evntdict.ContainsKey(ns))
            {
                if (MessageBox.Show(
                    "This will delete transitions that contain sm_State " + ns + " also\r\n Are you sure?",
                    "Warning Delete Transitions",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    _evntdict.Remove(ns);
                    foreach (var ev in _transitions)
                    {
                        Dictionary<string, Tranval> td = ev.Value;
                        var evwithevnt = new List<string>();
                        foreach (var tt in td)
                        {
                            if (tt.Value.Evntname == ns)
                            {
                                evwithevnt.Add(tt.Value.Evntname);
                            }
                        }
                        foreach (string evnt in evwithevnt)
                        {
                            ev.Value.Remove(evnt);
                        }
                        Dictionary<string, Tranval> tk = ev.Value;
                        if (tk.Count == 0)
                        {
                            _transitions.Remove(ev.Key);
                        }
                    }
                    _dirty = true;
                    GenerateAll();
                }
            }
        }

        private void EventName_LostFocus(object sender, RoutedEventArgs e)
        {
            var wpi = EventListBox.SelectedItem as CheckItem;
            if (wpi == null)
            {
                if (EventListBox.Items.Count > 0)
                {
                    EventListBox.SelectedIndex = 0;
                }

                return;
            }
            string now = wpi.OriginalText.Text;
            string after = wpi.NameText.Text.Trim();
            if (after == now)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(after))
            {
                MessageBox.Show("Is Empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (_evntdict.ContainsKey(now))
            {
                foreach (var ev in _transitions)
                {
                    if (ev.Value.ContainsKey(after))
                    {
                        continue;
                    }
                    if (ev.Value.ContainsKey(now))
                    {
                        Dictionary<string, Tranval> nowed = ev.Value;
                        foreach (var tt in nowed)
                        {
                            Tranval tran = tt.Value;
                            if (tran.Evntname == now)
                            {
                                tran.Evntname = after;
                            }
                        }
                        ev.Value[after] = ev.Value[now];
                        ev.Value.Remove(now);
                    }
                }
                _evntdict.Remove(now);
                if (_evntdict.ContainsKey(after) == false)
                {
                    _mxindex++;
                    _evntdict[after] = _mxindex;
                }
            }
            _dirty = true;
            GenerateAll();
        }

        private void EventListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (object item in EventListBox.Items)
            {
                (item as CheckItem).SetUnSelected();
                (item as CheckItem).NameText.IsReadOnly = true;
            }
            if (EventListBox.SelectedIndex >= 0)
            {
                (EventListBox.SelectedItem as CheckItem).SetSelected();
                (EventListBox.SelectedItem as CheckItem).NameText.IsReadOnly = false;
                (EventListBox.SelectedItem as CheckItem).NameText.Focus();
            }
        }

        #endregion

        #region State Machine Action EventHandlers

        private static int _actionCnt;

        private void OnAddActionClick(object sender, RoutedEventArgs e)
        {
            _actionCnt++;
            string st = string.Format("{0}{1}", "NewAction", _actionCnt);
            if (_actiondict.ContainsKey(st) == false)
            {
                _mxindex++;
                _actiondict.Add(st, _mxindex);
                GenerateAll();
                _dirty = true;
            }
            else
            {
                MessageBox.Show("Action " + st + " Already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDeleteActionClick(object sender, RoutedEventArgs e)
        {
            var wpi = ActionListBox.SelectedItem as CheckItem;
            if (wpi == null)
            {
                MessageBox.Show("Is Empty or white space only", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string ns = wpi.NameText.Text;
            if (_actiondict.ContainsKey(ns))
            {
                if (MessageBox.Show(
                    "This will also delete transitions that contain sm_Action " + ns + "\r\n Are you sure?",
                    "Warning Delete Transitions",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)

                {
                    foreach (var ev in _transitions)
                    {
                        Dictionary<string, Tranval> td = ev.Value;
                        var evwithaction = new List<string>();
                        foreach (var tt in td)
                        {
                            if (tt.Value.Action == ns)
                            {
                                evwithaction.Add(tt.Value.Evntname);
                            }
                        }
                        foreach (string evnt in evwithaction)
                        {
                            ev.Value.Remove(evnt);
                        }
                        Dictionary<string, Tranval> kd = ev.Value;
                        if (kd.Count == 0)
                        {
                            _transitions.Remove(ev.Key);
                        }
                    }
                    _actiondict.Remove(ns);
                    _dirty = true;
                    GenerateAll();
                }
            }
        }

        private void ActionName_LostFocus(object sender, RoutedEventArgs e)
        {
            var wpi = ActionListBox.SelectedItem as CheckItem;

            if (wpi == null)
            {
                if (ActionListBox.Items.Count > 0)
                {
                    ActionListBox.SelectedIndex = 0;
                }

                return;
            }
            string now = wpi.OriginalText.Text;
            string after = wpi.NameText.Text.Trim();
            if (after == now)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(after))
            {
                MessageBox.Show("Is Empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_actiondict.ContainsKey(now))
            {
                foreach (var ev in _transitions)
                {
                    Dictionary<string, Tranval> td = ev.Value;
                    foreach (var tt in td)
                    {
                        Tranval tran = tt.Value;
                        if (tran.Action == now)
                        {
                            tran.Action = after;
                        }
                    }
                }
                _actiondict.Remove(now);
                if (_actiondict.ContainsKey(after) == false)
                {
                    _mxindex++;
                    _actiondict[after] = _mxindex;
                }
            }
            _dirty = true;
            GenerateAll();
        }

        private void ActionListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (object item in ActionListBox.Items)
            {
                (item as CheckItem).SetUnSelected();
                (item as CheckItem).NameText.IsReadOnly = true;
            }
            if (ActionListBox.SelectedIndex >= 0)
            {
                (ActionListBox.SelectedItem as CheckItem).SetSelected();
                (ActionListBox.SelectedItem as CheckItem).NameText.IsReadOnly = false;
                (ActionListBox.SelectedItem as CheckItem).NameText.Focus();
            }
        }

        #endregion

        #region State Machine Transition EventHandlers

        private void OnAddTransitionClick(object sender, RoutedEventArgs e)
        {
            string initial = string.Empty;
            string evnt = string.Empty;
            string action = string.Empty;
            string final = string.Empty;


            if (StateListBox.SelectedIndex < 0)
            {
                //   SpeakText("No Initial State is selected");
                MessageBox.Show("No Initial State is selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            initial = (StateListBox.SelectedItem as CheckItem).NameText.Text;

            if (EventListBox.SelectedIndex < 0)
            {
                //     SpeakText("No Event is selected");
                MessageBox.Show("No Event is selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            evnt = (EventListBox.SelectedItem as CheckItem).NameText.Text;

            if (ActionListBox.SelectedIndex < 0)
            {
                //     SpeakText("No Action is selected");
                MessageBox.Show("No Action is selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            action = (ActionListBox.SelectedItem as CheckItem).NameText.Text;

            if (FinalStateListBox.SelectedIndex < 0)
            {
                //      SpeakText("No Final State is selected");
                MessageBox.Show("No Final State is selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            final = (FinalStateListBox.SelectedItem as CheckItem).NameText.Text;

            var tt = new Tranval();
            tt.Endstate = final;
            tt.Evntname = evnt;
            tt.Startstate = initial;
            tt.Action = action;
            tt.Endstateid = _statedict[tt.Endstate];
            tt.Actionid = _actiondict[tt.Action];
            tt.Evntid = _evntdict[tt.Evntname];
            tt.Startstateid = _statedict[tt.Startstate];

            if (_transitions.ContainsKey(initial))
            {
                Dictionary<string, Tranval> dd = _transitions[initial];
                if (dd.ContainsKey(evnt) == false)
                {
                    dd[evnt] = tt;
                    _dirty = true;
                }
                else
                {
                    string stt = string.Format("A transition already starts in sm_state {0} for sm_event {1}", initial,
                        evnt);
                    //     SpeakText(stt);
                    MessageBox.Show(stt, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                var dd = new Dictionary<string, Tranval>();
                dd[evnt] = tt;
                _transitions[initial] = dd;
                _dirty = true;
            }

            GenerateAll();
        }

        private void OnDeleteTransitionClick(object sender, RoutedEventArgs e)
        {
            if (StateListBox.SelectedIndex < 0)
            {
                MessageBox.Show("No Initial State is selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (EventListBox.SelectedIndex < 0)
            {
                MessageBox.Show("No Event is selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //if (checkedListBox_Actions.SelectedIndex < 0)
            //{
            //    MessageBox.Show("No Final State is selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}
            //if (checkedListBox_Final.SelectedIndex < 0)
            //{
            //    MessageBox.Show("No Final State is selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}
            string initial = (StateListBox.SelectedItem as CheckItem).NameText.Text;
            string evnt = (EventListBox.SelectedItem as CheckItem).NameText.Text;
            //            string action = checkedListBox_Actions.SelectedItem.ToString();
            //            string final = checkedListBox_Final.SelectedItem.ToString();
            foreach (var ev in _transitions)
            {
                if (ev.Key == initial)
                {
                    Dictionary<string, Tranval> td = ev.Value;

                    //  var trwithevnt = new List<string>();
                    foreach (var tt in td)
                    {
                        if (tt.Key == evnt)
                        {
                            td.Remove(tt.Key);
                            break;
                            //  trwithevnt.Add(tt.Key);
                        }
                    }
                    //foreach (string st in trwithevnt)
                    //{
                    //    td.Remove(st);
                    //}

                    //  Dictionary<string, Tranval> tk = ev.Value;
                    if (td.Count == 0)
                    {
                        _transitions.Remove(ev.Key);
                        break;
                    }
                }
            }
            _dirty = true;
            GenerateAll();
        }

        #endregion

        #region Generate View

        private void GenerateAll()
        {
            try
            {
                GenerateXML();
                string buf = _smTree.ToString(SaveOptions.None);
                SetEditWindowText(buf);
                SetEvntItems();
                SetActionItems();
                SetStateItems();
                SetStartupItems();
                FillTransitionsPanel();
                InvalidateVisual();
                NeverUsed(false);

            }
            catch (Exception ee)
            {
                MessageBox.Show("Error:\r\n" + ee.Message + "\r\n" + ee.StackTrace, "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetEditWindowText(string buf)
        {
            XmlText.IsReadOnly = false;
            XmlText.Document = new FlowDocument(new Paragraph(new Run(buf)));
            XmlText.IsReadOnly = true;
        }


        private void SetStartupItems()
        {
            StateMachineNameBlock.Text = _smName;
            // this.Title = "State Machine Generator and Editor (" + _smName + ")";

            _smInitialState = StartupListBox.SelectedItem as string;

            StartupListBox.Items.Clear();

            foreach (var sg in _statedict)
            {
                StartupListBox.Items.Add(sg.Key);
            }
            int mm = StartupListBox.Items.IndexOf(_smInitialState);
            if (mm >= 0)
            {
                StartupListBox.SelectedIndex = mm;
            }
            else
            {
                if (StartupListBox.Items.Count > 0)
                {
                    StartupListBox.SelectedIndex = 0;
                }
            }
        }


        private void SetEvntItems()
        {
            //CheckItem item = EventListBox.SelectedItem as CheckItem;

            // if (EventListBox.Items.Count > 0 && item == null)
            // {
            //     item = EventListBox.Items[0] as CheckItem;
            // }
            int selected = -1;
            string text = string.Empty;
            if (EventListBox.Items.Count > 0)
            {
                if (EventListBox.SelectedIndex < 0)
                {
                    EventListBox.SelectedIndex = 0;
                }
                selected = EventListBox.SelectedIndex;
                text = (EventListBox.SelectedItem as CheckItem).OriginalText.Text;
            }
            EventListBox.Items.Clear();
            foreach (var sg in _evntdict)
            {
                var checkItem = new CheckItem();
                checkItem.SelectionAction = SelectionEventText;

                _eventCnt = Math.Max(_eventCnt, NumberAtEnd(8, sg.Key));
                checkItem.NameText.Text = sg.Key;
                checkItem.OriginalText.Text = sg.Key;
                checkItem.LostFocus += EventName_LostFocus;
                checkItem.SelectedCheckItemEvent += EventCheckItemSelectedCheckItemEvent;

                EventListBox.Items.Add(checkItem);
                if (_mxindex < sg.Value)
                {
                    _mxindex = sg.Value;
                }
            }


            foreach (object vv in EventListBox.Items)
            {
                var testitem = vv as CheckItem;
                if (testitem != null && testitem.NameText.Text == text)
                {
                    EventListBox.SelectedIndex = EventListBox.Items.IndexOf(vv);
                }
            }
        }

        private void EventCheckItemSelectedCheckItemEvent(object sender, SelectedCheckItemEventArgs ee)
        {
            foreach (object vv in EventListBox.Items)
            {
                var testitem = vv as CheckItem;
                if (testitem != null && ee.Item != null && testitem == ee.Item)
                {
                    if (EventListBox.SelectedIndex != EventListBox.Items.IndexOf(vv))
                    {
                        EventListBox.SelectedIndex = EventListBox.Items.IndexOf(vv);
                    }
                }
            }
        }

        private void SelectionEventText(CheckItem checkItem)
        {
            foreach (object it in EventListBox.Items)
            {
                var ititem = it as CheckItem;
                if (ititem.NameText == checkItem.NameText)
                {
                    if (ititem != EventListBox.SelectedItem)
                        EventListBox.SelectedItem = ititem;
                }
            }
        }

        private void SetActionItems()
        {
            //CheckItem item = ActionListBox.SelectedItem as CheckItem;

            //if (ActionListBox.Items.Count > 0 && item == null)
            //{
            //    item = ActionListBox.Items[0] as CheckItem;
            //}

            int selected = -1;
            string text = string.Empty;
            if (ActionListBox.Items.Count > 0)
            {
                if (ActionListBox.SelectedIndex < 0)
                {
                    ActionListBox.SelectedIndex = 0;
                }
                selected = ActionListBox.SelectedIndex;
                text = (ActionListBox.SelectedItem as CheckItem).OriginalText.Text;
            }

            ActionListBox.Items.Clear();
            foreach (var sg in _actiondict)
            {
                var checkItem = new CheckItem();
                checkItem.SelectionAction = SelectionActionText;
                _actionCnt = Math.Max(_actionCnt, NumberAtEnd(8, sg.Key));

                checkItem.NameText.Text = sg.Key;
                checkItem.OriginalText.Text = sg.Key;
                checkItem.LostFocus += ActionName_LostFocus;
                checkItem.SelectedCheckItemEvent += ActionCheckItemSelectedCheckItemEvent;
                ActionListBox.Items.Add(checkItem);
                if (_mxindex < sg.Value)
                {
                    _mxindex = sg.Value;
                }
            }

            foreach (object vv in ActionListBox.Items)
            {
                var testitem = vv as CheckItem;
                if (testitem != null && testitem.OriginalText.Text == text)
                {
                    ActionListBox.SelectedIndex = ActionListBox.Items.IndexOf(vv);
                }
            }
        }

        private void ActionCheckItemSelectedCheckItemEvent(object sender, SelectedCheckItemEventArgs ee)
        {
            foreach (object vv in ActionListBox.Items)
            {
                var testitem = vv as CheckItem;
                if (testitem != null && ee.Item != null && testitem == ee.Item)
                {
                    if (ActionListBox.SelectedIndex != ActionListBox.Items.IndexOf(vv))
                    {
                        ActionListBox.SelectedIndex = ActionListBox.Items.IndexOf(vv);
                    }
                }
            }
        }

        private void SelectionActionText(CheckItem checkItem)
        {
            foreach (object it in ActionListBox.Items)
            {
                var ititem = it as CheckItem;
                if (ititem.NameText == checkItem.NameText)
                {
                    if (ititem != ActionListBox.SelectedItem)
                        ActionListBox.SelectedItem = ititem;
                }
            }
        }

        private void SetStateItems()
        {
            //CheckItem item = StateListBox.SelectedItem as CheckItem;
            //if (StateListBox.Items.Count > 0 && item == null)
            //{
            //    item = StateListBox.Items[0] as CheckItem;
            //}


            //CheckItem finalitem = FinalStateListBox.SelectedItem as CheckItem;

            //if (FinalStateListBox.Items.Count > 0 && finalitem == null)
            //{
            //    finalitem = FinalStateListBox.Items[0] as CheckItem;
            //}

            int selected = -1;
            string text = string.Empty;
            if (StateListBox.Items.Count > 0)
            {
                if (StateListBox.SelectedIndex < 0)
                {
                    StateListBox.SelectedIndex = 0;
                }
                selected = StateListBox.SelectedIndex;
                text = (StateListBox.SelectedItem as CheckItem).OriginalText.Text;
            }

            int finalselected = -1;
            string finaltext = string.Empty;
            if (FinalStateListBox.Items.Count > 0)
            {
                if (FinalStateListBox.SelectedIndex < 0)
                {
                    FinalStateListBox.SelectedIndex = 0;
                }
                finalselected = FinalStateListBox.SelectedIndex;
                finaltext = (FinalStateListBox.SelectedItem as CheckItem).OriginalText.Text;
            }


            StateListBox.Items.Clear();
            FinalStateListBox.Items.Clear();
            foreach (var sg in _statedict)
            {
                var checkItem = new CheckItem();
                checkItem.SelectionAction = SelectionStateText;

                _statecnt = Math.Max(_statecnt, NumberAtEnd(8, sg.Key));

                checkItem.NameText.Text = sg.Key;
                checkItem.OriginalText.Text = sg.Key;
                checkItem.LostFocus += StateName_LostFocus;
                checkItem.SelectedCheckItemEvent += StateCheckItemSelectedCheckItemEvent;
                StateListBox.Items.Add(checkItem);

                var checkItem1 = new CheckItem();

                checkItem1.SelectionAction = SelectionFinalStateText;
                checkItem1.NameText.Text = sg.Key;
                checkItem1.NoEdit = true;

                checkItem1.OriginalText.Text = sg.Key;
                checkItem1.SelectedCheckItemEvent += FinalStateCheckItemSelectedCheckItemEvent;
                FinalStateListBox.Items.Add(checkItem1);
                if (_mxindex < sg.Value)
                {
                    _mxindex = sg.Value;
                }
            }

            foreach (object vv in StateListBox.Items)
            {
                var testitem = vv as CheckItem;
                if (testitem != null && testitem.OriginalText.Text == text)
                {
                    StateListBox.SelectedIndex = StateListBox.Items.IndexOf(vv);
                }
            }

            foreach (object vv in FinalStateListBox.Items)
            {
                var testitem = vv as CheckItem;
                if (testitem != null && testitem.OriginalText.Text == finaltext)
                {
                    FinalStateListBox.SelectedIndex = FinalStateListBox.Items.IndexOf(vv);
                }
            }
        }

        private void StateCheckItemSelectedCheckItemEvent(object sender, SelectedCheckItemEventArgs ee)
        {
            foreach (object vv in StateListBox.Items)
            {
                var testitem = vv as CheckItem;
                if (testitem != null && ee.Item != null && testitem == ee.Item)
                {
                    if (StateListBox.SelectedIndex != StateListBox.Items.IndexOf(vv))
                    {
                        StateListBox.SelectedIndex = StateListBox.Items.IndexOf(vv);
                    }
                }
            }
        }

        private void SelectionStateText(CheckItem checkItem)
        {
            foreach (object it in StateListBox.Items)
            {
                var ititem = it as CheckItem;
                if (ititem.NameText == checkItem.NameText)
                {
                    if (ititem != StateListBox.SelectedItem)
                        StateListBox.SelectedItem = ititem;
                }
            }
        }

        private void FinalStateCheckItemSelectedCheckItemEvent(object sender, SelectedCheckItemEventArgs ee)
        {
            foreach (object vv in FinalStateListBox.Items)
            {
                var testitem = vv as CheckItem;
                if (testitem != null && ee.Item != null && testitem == ee.Item)
                {
                    if (FinalStateListBox.SelectedIndex != FinalStateListBox.Items.IndexOf(vv))
                    {
                        FinalStateListBox.SelectedIndex = FinalStateListBox.Items.IndexOf(vv);
                    }
                }
            }
        }

        private void SelectionFinalStateText(CheckItem checkItem)
        {
            foreach (object it in FinalStateListBox.Items)
            {
                var ititem = it as CheckItem;
                if (ititem.NameText == checkItem.NameText)
                {
                    if (ititem != FinalStateListBox.SelectedItem)
                        FinalStateListBox.SelectedItem = ititem;
                }
            }
        }

        private void GenerateXML()
        {
            _smTree = new XElement("StateMachine");
            _smTree.Add(new XAttribute("Name", _smName));
            _smTree.Add(new XAttribute("Start", _smInitialState));
            _smTree.Add(new XAttribute("Current", _smInitialState));
            var Acts = new XElement("Actions");
            foreach (var ev in _actiondict)
            {
                var ActName = new XElement("Action");
                ActName.Add(new XAttribute("ActionName", ev.Key));
                Acts.Add(ActName);
            }
            var Evts = new XElement("Events");
            foreach (var ev in _evntdict)
            {
                var EveName = new XElement("Event");
                EveName.Add(new XAttribute("EventName", ev.Key));
                Evts.Add(EveName);
            }
            var Stas = new XElement("States");
            foreach (var ev in _statedict)
            {
                var StasName = new XElement("State");
                StasName.Add(new XAttribute("StateName", ev.Key));
                Stas.Add(StasName);
            }
            _smTree.Add(Acts);
            _smTree.Add(Evts);
            _smTree.Add(Stas);
            var trans = new XElement("Transitions");
            foreach (var ev in _transitions)
            {
                Dictionary<string, Tranval> td = ev.Value;
                foreach (var tt in td)
                {
                    var tran = new XElement("Transition");
                    Tranval vv = tt.Value;
                    tran.Add(new XAttribute("Initial", ev.Key));
                    tran.Add(new XAttribute("Event", vv.Evntname));
                    tran.Add(new XAttribute("Action", vv.Action));
                    tran.Add(new XAttribute("Final", vv.Endstate));
                    trans.Add(tran);
                }
            }
            _smTree.Add(trans);
            InvalidateVisual();
        }

        void FillTransitionsPanel()
        {
            TransitionsListPanel.Children.Clear();
            foreach (var ev in _transitions)
            {
                Dictionary<string, Tranval> td = ev.Value;
                foreach (var tt in td)
                {
                    RadioButton ch = new RadioButton();
                    Tranval vv = tt.Value;
                    ch.Tag = vv;
                    ch.Checked += Ch_Checked;
                    TextBlock txt = new TextBlock();

                    txt.Margin = new Thickness(1.0);
                    txt.Text = string.Format("{0} + {1} => {2} ==> {3}", ev.Key, vv.Evntname, vv.Action, vv.Endstate);
                    ch.Content = txt;
                    ch.GroupName = "TransitionChoice";
                    TransitionsListPanel.Children.Add(ch);
                }
            }
        }

        private void Ch_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton ch = sender as RadioButton;
            Tranval vv = ch.Tag as Tranval;
            if (vv != null)
            {
                foreach (object item in StateListBox.Items)
                {
                    if ((item as CheckItem).NameText.Text == vv.Startstate)
                    {
                        StateListBox.SelectedItem = item;
                    }
                }
                foreach (object item in EventListBox.Items)
                {
                    if ((item as CheckItem).NameText.Text == vv.Evntname)
                    {
                        EventListBox.SelectedItem = item;
                    }
                }

                foreach (object item in ActionListBox.Items)
                {
                    if ((item as CheckItem).NameText.Text == vv.Action)
                    {
                        ActionListBox.SelectedItem = item;
                    }
                }

                foreach (object item in FinalStateListBox.Items)
                {
                    if ((item as CheckItem).NameText.Text == vv.Endstate)
                    {
                        FinalStateListBox.SelectedItem = item;
                    }
                }

            }
        }

        private void NeverUsed(bool show)
        {
            IEnumerable<string> Initial =
                (from el in _smTree.Descendants("States").Elements("State").Attributes()
                 select (string)el)
                    .Except
                    (from el in _smTree.Descendants("Transition").Attributes("Initial")
                     select el.Value);

            List<string> StartNot = Initial.ToList();

            IEnumerable<string> Final =
                (from el in _smTree.Descendants("States").Elements("State").Attributes()
                 select (string)el)
                    .Except
                    (from el in _smTree.Descendants("Transition").Attributes("Final")
                     select el.Value);

            List<string> FinalNot = Final.ToList();

            IEnumerable<string> Action =
                (from el in _smTree.Descendants("Actions").Elements("Action").Attributes()
                 select (string)el)
                    .Except
                    (from el in _smTree.Descendants("Transition").Attributes("Action")
                     select el.Value);

            List<string> ActionNot = Action.ToList();

            IEnumerable<string> va =
                (from el in _smTree.Descendants("Events").Elements("Event").Attributes()
                 select (string)el)
                    .Except
                    (from el in _smTree.Descendants("Transition").Attributes("Event")
                     select el.Value);

            List<string> EvntNot = va.ToList();

            IEnumerable<string> vb =
                (from el in _smTree.Descendants("Transition").Attributes("Final")
                 select el.Value)
                    .Except
                    (from el in _smTree.Descendants("Transition").Attributes("Initial")
                     select el.Value);

            List<string> DeadEnd = vb.ToList();

            IEnumerable<string> vc =
                (from el in _smTree.Descendants("Transition").Attributes("Initial")
                 select el.Value)
                    .Except
                    (from el in _smTree.Descendants("Transition").Attributes("Final")
                     select el.Value);

            List<string> DeadStart = vc.ToList();

            if (show)
            {
                AnalizeExpander.Content = new ShowUnused(StartNot, FinalNot, ActionNot, EvntNot, DeadEnd, DeadStart);
                InvalidateVisual();
            }
        }

        #endregion

        #region State Machine Name

        private void StateMachineNameBlock_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var box = sender as TextBox;

            if (box != null)
            {
                string ns = box.Text.Trim().Split()[0];
                if (ns.Length == 0)
                {
                    MessageBox.Show("Is Empty or white space only", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _smName = ns;
            }
            if (_smInitialState != null && _smInitialState.Length > 0)
            {
                GenerateAll();
            }
            else
            {
                StateMachineNameBlock.Text = _smName;
            }
            _dirty = true;
        }

        #endregion

        #region Save Xml

        private void SaveXML()
        {
            try
            {
                var ff = new FileInfo(_xmlfile);
                _smCodeGeneration.Scriptdir = ff.DirectoryName;
                _smCodeGeneration.SmStateNames = _statedict;
                _smCodeGeneration.SmEventNames = _evntdict;
                _smCodeGeneration.SmActionNames = _actiondict;
                _smCodeGeneration.SmTransitionNames = _transitions;
                _smCodeGeneration.SmName = _smName;
                _smCodeGeneration.SmInitialState = _smInitialState;
                _smCodeGeneration.OutputNSMXmlFile(_xmlfile);
                _dirty = false;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                var listr = new List<string>();
                listr.Add(ee.Message);
                OutputErrors(listr);
            }
        }


        private void OnSaveXmlClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_xmlfile))
            {
                SaveXML();
            }
            else
            {
                var dlg = new SaveFileDialog();

                dlg.AddExtension = true;
                dlg.DefaultExt = "xml";
                dlg.Filter = "Text files (*.xml)|*.xml";
                dlg.InitialDirectory = Directory.GetCurrentDirectory();
                dlg.Title = "save State Machine .xml file";
                dlg.FileName = _smName + ".xml";
                if (dlg.ShowDialog() == true)
                {
                    _xmlfile = dlg.FileName;
                    SaveXML();
                }
            }
        }

        private void OnSaveAsXmlClick(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();

            dlg.AddExtension = true;
            dlg.DefaultExt = "xml";
            dlg.Filter = "Text files (*.xml)|*.xml";
            dlg.InitialDirectory = Directory.GetCurrentDirectory();
            dlg.Title = "save State Machine .xml file";
            dlg.FileName = _smName + ".xml";
            if (dlg.ShowDialog() == true)
            {
                _xmlfile = dlg.FileName;
                SaveXML();
            }

        }



        #endregion

        private void StartupListBox_OnSelectionChanged(object sender, EventArgs e)
        {
            if (StartupListBox.SelectedIndex < 0)
            {
                StartupListBox.SelectedIndex = 0;
            }
            else
            {
                _smInitialState = StartupListBox.SelectedItem as string;
                // GenerateAll();
                _dirty = true;
            }
        }

        private void OpenExample(object sender, RoutedEventArgs e)
        {
            string path = Path.GetTempPath() + "\\Monster.xml";
            _smCodeGeneration.GenerateExample(path);


            var ff = new FileInfo(path);
            _smCodeGeneration.Scriptdir = ff.DirectoryName;

            OnClosing(sender, e);

            Int32 count = 0;
            if (_smCodeGeneration.ParseNSMXmlFile(ff.Name, ref count))
            {
                _statedict = _smCodeGeneration.SmStateNames;
                _evntdict = _smCodeGeneration.SmEventNames;
                _actiondict = _smCodeGeneration.SmActionNames;
                _transitions = _smCodeGeneration.SmTransitionNames;
                _smName = _smCodeGeneration.SmName;
                _smInitialState = _smCodeGeneration.SmInitialState;
                if (count > 0)
                {
                    string elist = " ";
                    List<string> err = _smCodeGeneration.Errors;
                    for (int kk = 0; kk < count; kk++)
                    {
                        elist += err[err.Count - (1 + kk)] + "\r\n";
                    }
                    MessageBox.Show(elist, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                GenerateAll();
            }
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            try
            {
                var openFileDialog_xml = new OpenFileDialog();
                openFileDialog_xml.AddExtension = true;
                openFileDialog_xml.CheckFileExists = true;
                openFileDialog_xml.DefaultExt = "xml";
                openFileDialog_xml.Filter = "Text files (*.xml)|*.xml";
                openFileDialog_xml.InitialDirectory = Directory.GetCurrentDirectory();
                openFileDialog_xml.Title = "Open State Machine .xml file";
                openFileDialog_xml.FileName = "Unknown.xml";
                if (openFileDialog_xml.ShowDialog() == true)
                {
                    _xmlfile = openFileDialog_xml.FileName;
                    var ff = new FileInfo(_xmlfile);
                    _smCodeGeneration.Scriptdir = ff.DirectoryName;
                    Int32 count = 0;
                    if (_smCodeGeneration.ParseNSMXmlFile(ff.Name, ref count))
                    {
                        _statedict = _smCodeGeneration.SmStateNames;
                        _evntdict = _smCodeGeneration.SmEventNames;
                        _actiondict = _smCodeGeneration.SmActionNames;
                        _transitions = _smCodeGeneration.SmTransitionNames;
                        _smName = _smCodeGeneration.SmName;
                        _smInitialState = _smCodeGeneration.SmInitialState;
                        if (count > 0)
                        {
                            string elist = " ";
                            List<string> err = _smCodeGeneration.Errors;
                            for (int kk = 0; kk < count; kk++)
                            {
                                elist += err[err.Count - (1 + kk)] + "\r\n";
                            }
                            MessageBox.Show(elist, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        GenerateAll();
                    }
                    else
                    {
                        //      SpeakText(_smCodeGeneration.LastError);
                        MessageBox.Show(_smCodeGeneration.LastError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        var listg = new List<string>();
                        listg.Add(_smCodeGeneration.LastError);
                        OutputErrors(listg);
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                var listr = new List<string>();
                listr.Add(ee.Message);
                OutputErrors(listr);
            }
        }


        private void StateMachineEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private int NumberAtEnd(int start, string st)
        {
            var numbers = new char[10];

            int ii = 0;
            for (char cc = '0'; cc <= '9'; cc++)
            {
                numbers[ii] = cc;
                ii++;
            }

            int index = st.IndexOfAny(numbers, 0);
            int res = -1;

            if (index == start)
            {
                try
                {
                    res = Convert.ToInt32(st.Substring(start));
                }
                catch (Exception ee)
                {
                }
            }
            return res;
        }


        private void GenerateCodePlusWinProj_Click(object sender, RoutedEventArgs e)
        {
            if (OutputLanguageListBox.SelectedItem.ToString() == "c#")
            {
                _smCodeGeneration.CreateWinProject = true;
            }
            GenerateCodeButton_OnClick(sender, e);
            _smCodeGeneration.CreateWinProject = false;
        }

        private void GenerateCodePlusConsoleProj_Click(object sender, RoutedEventArgs e)
        {
            if (OutputLanguageListBox.SelectedItem.ToString() == "c#")
            {
                _smCodeGeneration.CreateConsoleProject = true;
            }
            GenerateCodeButton_OnClick(sender, e);
            _smCodeGeneration.CreateConsoleProject = false;
        }

        private void GenerateStatePatternCodeButton_OnClick(object sender, RoutedEventArgs e)
        {
            string _folderName = _lastprojectdirectory;

            _folderName = (Directory.Exists(_folderName)) ? _folderName : "";
            var dlg1 = new FolderBrowserDialogEx
            {
                Description = "Select a folder for the generated files:",
                ShowNewFolderButton = true,
                ShowEditBox = true,
                //NewStyle = false,
                SelectedPath = _folderName,
                ShowFullPathInEditBox = false,
            };
            dlg1.RootFolder = Environment.SpecialFolder.MyComputer;

            DialogResult result = dlg1.ShowDialog();

            if (result == DialogResult.OK)
            {
                _folderName = dlg1.SelectedPath;

                StatePatternCodeGenerator statePatternCodeGenerator = new StatePatternCodeGenerator();

                statePatternCodeGenerator.TargetDir = dlg1.SelectedPath;
                _lastprojectdirectory = statePatternCodeGenerator.TargetDir;

                string txml = _xmlfile;
                _xmlfile = statePatternCodeGenerator.TargetDir + "\\" + _smName + ".xml";
                SaveXML();
                _xmlfile = txml;

                statePatternCodeGenerator.Language = OutputLanguageListBox.SelectedItem.ToString();
                statePatternCodeGenerator.StateEventNames = _evntdict;
                statePatternCodeGenerator.ActionNames = _actiondict;
                statePatternCodeGenerator.StateNames = _statedict;
                statePatternCodeGenerator.Transitions = _transitions;
                statePatternCodeGenerator.StateMachineName = _smName;
                statePatternCodeGenerator.InitialState = _smInitialState;
                statePatternCodeGenerator.GenerateStatePattern();

                var zz = new Process();
                zz.StartInfo.FileName = "Explorer";
                zz.StartInfo.Arguments = statePatternCodeGenerator.TargetDir;
                zz.Start();
            }
        }

        private void GenerateStatePatternProjectCodeButton_OnClick(object sender, RoutedEventArgs e)
        {
            string _folderName = _lastprojectdirectory;

            _folderName = (Directory.Exists(_folderName)) ? _folderName : "";
            var dlg1 = new FolderBrowserDialogEx
            {
                Description = "Select a folder for the generated files:",
                ShowNewFolderButton = true,
                ShowEditBox = true,
                //NewStyle = false,
                SelectedPath = _folderName,
                ShowFullPathInEditBox = false,
            };
            dlg1.RootFolder = Environment.SpecialFolder.MyComputer;

            DialogResult result = dlg1.ShowDialog();

            if (result == DialogResult.OK)
            {
                _folderName = dlg1.SelectedPath;

                StatePatternCodeGenerator statePatternCodeGenerator = new StatePatternCodeGenerator();

                statePatternCodeGenerator.TargetDir = dlg1.SelectedPath;
                _lastprojectdirectory = statePatternCodeGenerator.TargetDir;

                string txml = _xmlfile;
                _xmlfile = statePatternCodeGenerator.TargetDir + "\\" + _smName + ".xml";
                SaveXML();
                _xmlfile = txml;

                statePatternCodeGenerator.Language = OutputLanguageListBox.SelectedItem.ToString();
                statePatternCodeGenerator.StateEventNames = _evntdict;
                statePatternCodeGenerator.ActionNames = _actiondict;
                statePatternCodeGenerator.StateNames = _statedict;
                statePatternCodeGenerator.Transitions = _transitions;
                statePatternCodeGenerator.StateMachineName = _smName;
                statePatternCodeGenerator.InitialState = _smInitialState;
                statePatternCodeGenerator.GenerateStatePatternProject();

                var zz = new Process();
                zz.StartInfo.FileName = "Explorer";
                zz.StartInfo.Arguments = statePatternCodeGenerator.TargetDir;
                zz.Start();
            }
        }

        private void GenerateCodeButton_OnClick(object sender, RoutedEventArgs e)
        {
            string _folderName = _lastprojectdirectory;

            _folderName = (Directory.Exists(_folderName)) ? _folderName : "";
            var dlg1 = new FolderBrowserDialogEx
            {
                Description = "Select a folder for the generated files:",
                ShowNewFolderButton = true,
                ShowEditBox = true,
                //NewStyle = false,
                SelectedPath = _folderName,
                ShowFullPathInEditBox = false,
            };
            dlg1.RootFolder = Environment.SpecialFolder.MyComputer;

            DialogResult result = dlg1.ShowDialog();

            if (result == DialogResult.OK)
            {
                _folderName = dlg1.SelectedPath;
                _smCodeGeneration.Targetdir = dlg1.SelectedPath;

                string txml = _xmlfile;
                _xmlfile = _smCodeGeneration.Targetdir + "\\" + _smName + ".xml";
                SaveXML();
                _xmlfile = txml;

                _lastprojectdirectory = _smCodeGeneration.Targetdir;
                _smCodeGeneration.SmStateNames = _statedict;
                _smCodeGeneration.SmEventNames = _evntdict;
                _smCodeGeneration.SmActionNames = _actiondict;
                _smCodeGeneration.SmTransitionNames = _transitions;
                _smCodeGeneration.SmLanguage = OutputLanguageListBox.SelectedItem.ToString();
                _smCodeGeneration.SmName = _smName;
                _smCodeGeneration.SmInitialState = _smInitialState;
                _smCodeGeneration.GenerateStateMachineFiles();

                if (_smCodeGeneration.Errors.Count > 0)
                {
                    //       GenerateResults.ErrorList.Items.Clear();
                    OutputErrors(_smCodeGeneration.Errors);

                    //foreach (string sr in _smCodeGeneration.Errors)
                    //{
                    //    TextBlock blk = new TextBlock();
                    //    blk.Text = sr;
                    //    GenerateResults.ErrorList.Items.Add(blk);
                    //    ResultsExpander.IsExpanded = true;
                    //}
                }
                var zz = new Process();
                zz.StartInfo.FileName = "Explorer";
                zz.StartInfo.Arguments = _smCodeGeneration.Targetdir;
                zz.Start();
            }
        }

        private void GenerateMvcButton_OnClick(object sender, RoutedEventArgs e)
        {
            string _folderName = _lastprojectdirectory;

            _folderName = (Directory.Exists(_folderName)) ? _folderName : "";
            var dlg1 = new FolderBrowserDialogEx
            {
                Description = "Select a folder for the generated files:",
                ShowNewFolderButton = true,
                ShowEditBox = true,
                //NewStyle = false,
                SelectedPath = _folderName,
                ShowFullPathInEditBox = false,
            };
            dlg1.RootFolder = Environment.SpecialFolder.MyComputer;

            DialogResult result = dlg1.ShowDialog();

            if (result == DialogResult.OK)
            {
                _folderName = dlg1.SelectedPath;
                _mvcCodeGeneration.Targetdir = dlg1.SelectedPath;
                _lastprojectdirectory = _mvcCodeGeneration.Targetdir;
                _mvcCodeGeneration.SmStateNames = _statedict;
                _mvcCodeGeneration.SmEventNames = _evntdict;
                _mvcCodeGeneration.SmActionNames = _actiondict;
                _mvcCodeGeneration.SmTransitionNames = _transitions;
                _mvcCodeGeneration.SmLanguage = OutputLanguageListBox.SelectedItem.ToString();
                _mvcCodeGeneration.SmName = _smName;
                _mvcCodeGeneration.SetFileNames(_mvcCodeGeneration.Targetdir);
                _mvcCodeGeneration.SmInitialState = _smInitialState;
                _mvcCodeGeneration.GenerateMvcStateMachineFiles();
                if (_mvcCodeGeneration.Errors.Count > 0)
                {
                    //       GenerateResults.ErrorList.Items.Clear();
                    OutputErrors(_mvcCodeGeneration.Errors);

                    //foreach (string sr in _smCodeGeneration.Errors)
                    //{
                    //    TextBlock blk = new TextBlock();
                    //    blk.Text = sr;
                    //    GenerateResults.ErrorList.Items.Add(blk);
                    //    ResultsExpander.IsExpanded = true;
                    //}
                }
                var zz = new Process();
                zz.StartInfo.FileName = "Explorer";
                zz.StartInfo.Arguments = _mvcCodeGeneration.Targetdir;
                zz.Start();
            }
        }

        private void FileOpenMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFile_Click(sender, e);
        }

        private void CodeMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            GenerateCodeButton_OnClick(sender, e);
        }

        private void PatternCodeMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            GenerateStatePatternCodeButton_OnClick(sender, e);
        }

        private void PatternProjectMenuItemOnClick(object sender, RoutedEventArgs e)
        {
            GenerateStatePatternProjectCodeButton_OnClick(sender, e);
        }

        private void AnalizeMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            AboutExpander.IsExpanded = true;
        }

        private void AnalizeExpander_Expanded(object sender, RoutedEventArgs e)
        {
            NeverUsed(true);
        }

        private void EventMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            OnAddEventClick(sender, e);
        }

        private void ActionMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            OnAddActionClick(sender, e);
        }

        private void StateMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            OnAddStateClick(sender, e);
        }


        private void OnClosing(object sender, RoutedEventArgs EventArgs)
        {
            if (_dirty)
            {
                MessageBoxResult vv =
                    MessageBox.Show(
                        "Warning this State Machine has been changed. Do you wish to save your changes first?",
                        "Changed Warning",
                        MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (vv == MessageBoxResult.Yes)
                {
                    var ee = new RoutedEventArgs();
                    OnSaveXmlClick(sender, ee);
                    _dirty = false;
                }
            }
            XmlExpander.IsExpanded = false;
        }

        private void FilePrintMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            // throw new NotImplementedException();
            PrintSm();
        }

        private string GetTextFromFlowDocument(FlowDocument flowDoc)
        {
            // Create a new TextRanage that takes the entire FlowDocument as the current selection.
            var flowDocSelection = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd);

            // Use the Text property to extract a string that contains the unformatted text contents  
            // of the FlowDocument. 
            return flowDocSelection.Text;
        }


        public static DocumentPaginator
            GetDocumentPaginator(FlowDocument flowDocument)
        {
            return ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
        }


        private void PrintSm()
        {
            try
            {
                string head = "Xml print of " + _smName;
                var dlg = new PrintDialog();
                dlg.PageRangeSelection = PageRangeSelection.AllPages;
                dlg.UserPageRangeEnabled = true;

                XmlText.Document.PagePadding = new Thickness(70, 100, 70, 10);
                DocumentPaginator pg = GetDocumentPaginator(XmlText.Document);

                bool? print = dlg.ShowDialog();
                if (print == true)
                {
                    dlg.PrintDocument(pg, head);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FileSaveMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            OnSaveXmlClick(sender, e);
        }

        private void FileSaveAsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            OnSaveAsXmlClick(sender, e);
        }

        private void FileNewMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBoxResult.Yes;
            if (_dirty)
            {
                result =
                    MessageBox.Show(
                        "Warning this State Machine has been changed. Do you wish to save your changes first?",
                        "Changed Warning",
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {
                    var ee = new RoutedEventArgs();
                    OnSaveXmlClick(sender, e);
                }
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            _smCodeGeneration = new SmCodeGeneration();
            _mxindex = 0;
            _statedict = new Dictionary<string, long>();
            _evntdict = new Dictionary<string, long>();
            _actiondict = new Dictionary<string, long>();
            _transitions = new Dictionary<string, Dictionary<string, Tranval>>();
            _statedict.Add("State0", _mxindex++);
            _evntdict.Add("Event0", _mxindex++);
            _actiondict.Add("Action0", _mxindex);
            _smName = "Unknown";
            _smInitialState = "State0";
            _smTree = new XElement("StateMachine");
            _dirty = false;
            GenerateAll();
        }

        //private void SoundMenuItem_OnClick(object sender, RoutedEventArgs e)
        //{
        //    if (_isSpeechOn == true)
        //    {
        //        _isSpeechOn = false;
        //        SoundMenuItem.Header = "Turn Sound On";
        //        SoundMenuItem.IsChecked = false;
        //    }
        //    else
        //    {
        //        _isSpeechOn = true;
        //        SoundMenuItem.Header = "Turn Sound Off";
        //        SoundMenuItem.IsChecked = true;
        //    }
        //}

        private void HelpMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            HelpExpander.IsExpanded = true;
            // MessageBox.Show("Forthcomming");
        }

        private void AboutMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            // throw new NotImplementedException();
            // MessageBox.Show("Forthcoming");
            AboutExpander.IsExpanded = true;
        }

        private void OutputLanguageListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OutputLanguageListBox.SelectedIndex >= 0)
            {
                GenerateCodeButton.Content = "Generate " + (OutputLanguageListBox.SelectedItem as string) + " Code (not-Pattern)";
                GenerateSmCodeButton.Content = "Generate " + (OutputLanguageListBox.SelectedItem as string) + " Pattern Code";
                CodeMenuItem.Header = "Generate " + (OutputLanguageListBox.SelectedItem as string) + " Code (not-Pattern)";
                PatternCodeMenuItem.Header = "Generate " + (OutputLanguageListBox.SelectedItem as string) + " Pattern Code";
            }
            else
            {
                OutputLanguageListBox.SelectedIndex = 0;
            }
        }

        private void HtmlButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var zz = new Process();
                zz.StartInfo.FileName = "iexplore";
                zz.StartInfo.Arguments = "http://dragonsawaken.net/exampleSM/statemachineeditor.html";
                zz.Start();
            }
            catch (Exception)
            {
                try
                {
                    var zz = new Process();
                    zz.StartInfo.FileName = "chrome";
                    zz.StartInfo.Arguments = "http://dragonsawaken.net/exampleSM/statemachineeditor.html";
                    zz.Start();
                }
                catch (Exception)
                {
                    try
                    {
                        var zz = new Process();
                        zz.StartInfo.FileName = "firefox";
                        zz.StartInfo.Arguments = "http://dragonsawaken.net/exampleSM/statemachineeditor.html";
                        zz.Start();
                    }
                    catch (Exception)
                    {


                    }
                }
            }

            //     ShowUrl("http://dragonsawaken.net/leho/exampleSM/statemachineeditor.html");

        }
    }

}