using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using Microsoft.CSharp;
using SmSimpleData;


namespace StateMachineCodeGeneration
{
    public class SmCodeGeneration
    {

        #region fields
        StateMachineCodeDataContext _smcdc;
        Int64 _smId;
        Guid _smCodeid;
        string _smName;
        string _smInitialstate;
        private string _language;

        string _targetdir;
        string _scriptdir;
        Dictionary<string, Int64> _stateDictionary;
        Dictionary<string, Int64> _evntdict;
        Dictionary<string, Int64> _actiondict;
        List<string> _errList;

        public delegate bool StateAction(long sm_id, long sm_inst);
        public delegate bool StateEvent(string evnt);


        CompilerInfo[] _allCompilerInfo;

        CodeCompileUnit MachineActionEventUnit;
        CodeTypeDeclaration MachineActionEventClassCodeType;

        CodeCompileUnit MachineUnit;
        CodeTypeDeclaration MachineClassType;

        CodeCompileUnit ActionsEventDataUnit;
        CodeTypeDeclaration ActionsEventDataClassType;

        CodeCompileUnit InputEventDataUnit;
        CodeTypeDeclaration InputEventDataClassType;

        CodeCompileUnit TestEventsReadyDataUnit;
        CodeTypeDeclaration TestEventsReadyDataClassType;

        CodeCompileUnit TestEventsReadyUnit;
        CodeTypeDeclaration TestEventsReadyClassType;

        CodeCompileUnit TestUnit;
        CodeTypeDeclaration TestClassType;

        CodeCompileUnit MainWindowUnit;
        CodeTypeDeclaration MainWindowClassType;

        private string _platformName;
        private string _machineFileName;
        private string ActionEventDataFileName;
        private string InputEventDataFileName;
        private string TestEventsReadyDataFileName;
        private string MachineActionFileName;
        private string TestEventsReadyFileName;
        private string TestFileName;

        private string ConsoleCodeName = "Program.cs";

        private string WinCodeName = "MainWindow.xaml";
        private string WindowXamlName = "MainWindow.xaml";
        private string cfgFileName = "App.Config";
        private string AppXamlName = "App.xaml";
        private string AppCsName = "App.xaml.cs";
        private string AsmbFileName = "AssemblyInfo.cs";
        private string Errlogname = "errLog.txt";
        private string ProjectFileName = "Smname.csproj";
        private string SettingsDesignerName = "Settings.Designer.cs";
        private string SettingsFileName = "Settings.settings";
        private string ResourceDesignerName = "Resources.Designer.cs";
        private string ResourceFileName = "Resources.resx";
        //   private string propFileName;



        private string dbFileName;
        //   private string projFileName;
        private string smRunDesignerFileName;
        //   private string SettingsSettingsFileName;
        private string SmRunDbmlFileName;
        private string SmRunLayoutFileName;

        Dictionary<string, Dictionary<string, Tranval>> _transitions;

        #endregion

        #region ctor

        public SmCodeGeneration()
        {
            _errList = new List<string>();
            _smName = "unknown";
            _targetdir = "..\\..\\..\\TestStateb";
            _scriptdir = @"..\..\..\scripts";
            SetFileNames();
            _smCodeid = Guid.NewGuid();
            _language = "c#";
            _allCompilerInfo = CodeDomProvider.GetAllCompilerInfo();
        }


        public SmCodeGeneration(string smname)
        {
            _smName = smname;
            _errList = new List<string>();
            _targetdir = "..\\..\\..\\TestStateb";
            _scriptdir = @"..\..\..\scripts";
            SetFileNames();
            StateMachineInfo(_smName);
            _language = "c#";
            _allCompilerInfo = CodeDomProvider.GetAllCompilerInfo();
        }

        #endregion

        #region properties

        public static string SmAppName { get; set; }
        public static string SmAppNameSpace { get; set; }
        public static string SmSmName { get; set; }
        public static string SmProjFiles { get; set; }
        public static string SmProjGuid { get; set; }

        public string SmLanguage
        {
            get
            {
                return (_language);
            }
            set
            {
                _language = value;
            }
        }

        public List<string> Supported
        {
            get
            {
                List<string> ab = new List<string>();

                foreach (CompilerInfo cm in _allCompilerInfo)
                {
                    try
                    {
                        CodeDomProvider provider = cm.CreateProvider();

                        string defaultExtension = provider.FileExtension;
                        if (defaultExtension[0] != '.')
                        {
                            defaultExtension = "." + defaultExtension;
                        }
                        ab.Add(CodeDomProvider.GetLanguageFromExtension(defaultExtension));

                        //foreach (String la in info.GetLanguages())
                        //{
                        //    ab.Add(la);
                        //}
                    }
                    catch (Exception ee)
                    {
                        string mm = ee.Message;
                    }

                }
                return (ab);
            }
        }

        public string SmName
        {
            get
            {
                return (_smName);
            }
            set
            {
                _smName = value;
                string ss = "";
                string vv = _smName + "Machine";
                Int64 smid = 0;
                try
                {
                    for (int qq = 1; qq < vv.Length; qq += 2)
                    {
                        Int32 iq = (Int32)vv[qq - 1];
                        ss = iq.ToString();
                        iq = (Int32)vv[qq];
                        ss += iq.ToString();

                        smid += (Convert.ToInt64(ss) * ((Int64)qq));
                    }
                }
                catch (Exception ee)
                {
                    _errList.Add(ee.Message);
                }
                _smId = smid;
                SetFileNames();
            }
        }

        public string SmInitialState
        {
            get
            {
                return (_smInitialstate);
            }
            set
            {
                _smInitialstate = value;
            }
        }



        public string Scriptdir
        {
            get
            {
                return (_scriptdir);
            }
            set
            {
                _scriptdir = value;
            }
        }

        public string PlatformName
        {
            get
            {
                return (_platformName);
            }
            set
            {
                _platformName = value;
            }
        }

        public string Targetdir
        {
            get
            {
                return (_targetdir);
            }
            set
            {
                _targetdir = value;
                SetFileNames(_targetdir);
            }
        }
        public string LastError
        {
            get
            {
                return (_errList[_errList.Count - 1]);
            }
        }

        public List<string> Errors
        {
            get
            {
                return (_errList);
            }
        }

        public Dictionary<string, Int64> SmStateNames
        {
            get
            {
                return (_stateDictionary);
            }
            set
            {
                _stateDictionary = value;
            }
        }

        public Dictionary<string, Int64> SmActionNames
        {
            get
            {
                return (_actiondict);
            }
            set
            {
                _actiondict = value;
            }
        }

        public Dictionary<string, Int64> SmEventNames
        {
            get
            {
                return (_evntdict);
            }
            set
            {
                _evntdict = value;
            }
        }

        public Dictionary<string, Dictionary<string, Tranval>> SmTransitionNames
        {
            get
            {
                return (_transitions);
            }
            set
            {
                _transitions = value;
            }
        }

        public bool CreateConsoleProject { get; set; }
        public bool CreateWinProject { get; set; }



        #endregion

        public bool GenerateStateMachineFiles()
        {
            _errList.Clear();
            try
            {
                SmAppName = _smName + "TestApp";
                SmAppNameSpace = "sm_" + _smName;
                SmSmName = _smName;

                SmProjFiles = "";
                SmProjFiles += string.Format("<Compile Include = \"{0}\\sm{0}.cs\" />\r\n ", _smName);
                SmProjFiles += string.Format("<Compile Include = \"{0}\\sm{0}Action.cs\" />\r\n ", _smName);
                SmProjFiles += string.Format("<Compile Include = \"{0}\\sm{0}Eventdata.cs\" />\r\n ", _smName);
                SmProjFiles += string.Format("<Compile Include = \"{0}\\sm{0}Inputdata.cs\" />\r\n ", _smName);
                SmProjFiles += string.Format("<Compile Include = \"{0}\\sm{0}TestEventsReady.cs\" />\r\n ", _smName);
                SmProjFiles += string.Format("<Compile Include = \"{0}\\sm{0}TestEventsReadyData.cs\" />\r\n ", _smName);
                if (CreateConsoleProject)
                {
                    SmProjFiles += string.Format("<Compile Include = \"{0}\\Test{0}.cs\" />\r\n ", _smName);
                }


                SmProjGuid = (Guid.NewGuid()).ToString("B");

                _errList = new List<string>();

                MachineActionEventClass();
                StateMachineClass();
                GenerateTestEventsReady();
                GenerateActionEventData();
                GenerateInputEventData();
                GenerateTestEventsReadyData();
                GenerateTestCode();

                if (CreateWinProject)
                {
                    GenerateWinMain();
                    GenerateWpfProj();
                }
                if (CreateConsoleProject)
                {
                    GenerateConsoleProj();
                }

            }
            catch (Exception ee)
            {
                _errList.Add(ee.Message);
            }

            if (_errList.Count > 0)
            {

                StreamWriter writer = new StreamWriter(Errlogname);
                foreach (string st in _errList)
                {
                    writer.Write(st);
                }

                writer.Flush();
                writer.Close();

                return (false);
            }
            return (true);
        }


        public void GenerateConsoleProj()
        {
            //   _errList.Add("Started WPF Project Generation");
            try
            {
                char[] remchars = { ' ', '\n', '\r', };

                StateMachineCodeGeneration.ConsoleTemplates.ConsoleAppconfig GenAppconfig = new StateMachineCodeGeneration.ConsoleTemplates.ConsoleAppconfig();
                string xmltext = GenAppconfig.TransformText().TrimStart(remchars);
                StreamWriter writer3 = new StreamWriter(cfgFileName);
                writer3.Write(xmltext);
                writer3.Flush();
                writer3.Close();


                StateMachineCodeGeneration.ConsoleTemplates.ConsoleAssemblyInfo GenAssemblyInfo = new StateMachineCodeGeneration.ConsoleTemplates.ConsoleAssemblyInfo();
                StreamWriter writer5 = new StreamWriter(AsmbFileName);
                writer5.Write(GenAssemblyInfo.TransformText());
                writer5.Flush();
                writer5.Close();

                StateMachineCodeGeneration.ConsoleTemplates.ConsoleFile GenProjectFile = new StateMachineCodeGeneration.ConsoleTemplates.ConsoleFile();
                xmltext = GenProjectFile.TransformText().TrimStart(remchars);
                StreamWriter writer6 = new StreamWriter(ProjectFileName);
                writer6.Write(xmltext);
                writer6.Flush();
                writer6.Close();

                StateMachineCodeGeneration.ConsoleTemplates.Program GenProgram = new StateMachineCodeGeneration.ConsoleTemplates.Program();
                xmltext = GenProgram.TransformText().TrimStart(remchars);
                StreamWriter writer10 = new StreamWriter(ConsoleCodeName);
                writer10.Write(xmltext);
                writer10.Flush();
                writer10.Close();
            }
            catch (Exception ee)
            {
                _errList.Add(ee.Message);
            }
        }


        // use a static string in FileGenerator;
        // call is like
        public void GenerateWpfProj()
        {
            //   _errList.Add("Started WPF Project Generation");
            try
            {
                char[] remchars = { ' ', '\n', '\r', };

                StateMachineCodeGeneration.WpfTemplates.App GenApp = new StateMachineCodeGeneration.WpfTemplates.App();
                StreamWriter writer = new StreamWriter(AppXamlName);
                string xmltext = GenApp.TransformText().TrimStart(remchars);
                writer.Write(xmltext);
                writer.Flush();
                writer.Close();

                StateMachineCodeGeneration.WpfTemplates.Appxaml GenAppcs = new StateMachineCodeGeneration.WpfTemplates.Appxaml();
                StreamWriter writer2 = new StreamWriter(AppCsName);
                writer2.Write(GenAppcs.TransformText());
                writer2.Flush();
                writer2.Close();

                StateMachineCodeGeneration.WpfTemplates.Appconfig GenAppconfig = new StateMachineCodeGeneration.WpfTemplates.Appconfig();
                xmltext = GenAppconfig.TransformText().TrimStart(remchars);
                StreamWriter writer3 = new StreamWriter(cfgFileName);
                writer3.Write(xmltext);
                writer3.Flush();
                writer3.Close();

                StateMachineCodeGeneration.WpfTemplates.MainWindow GenMainWindow = new StateMachineCodeGeneration.WpfTemplates.MainWindow();
                xmltext = GenMainWindow.TransformText().TrimStart(remchars);
                StreamWriter writer4 = new StreamWriter(WindowXamlName);
                writer4.Write(xmltext);
                writer4.Flush();
                writer4.Close();

                StateMachineCodeGeneration.WpfTemplates.AssemblyInfo GenAssemblyInfo = new StateMachineCodeGeneration.WpfTemplates.AssemblyInfo();
                StreamWriter writer5 = new StreamWriter(AsmbFileName);
                writer5.Write(GenAssemblyInfo.TransformText());
                writer5.Flush();
                writer5.Close();

                StateMachineCodeGeneration.WpfTemplates.ProjectFile GenProjectFile = new StateMachineCodeGeneration.WpfTemplates.ProjectFile();
                xmltext = GenProjectFile.TransformText().TrimStart(remchars);
                StreamWriter writer6 = new StreamWriter(ProjectFileName);
                writer6.Write(xmltext);
                writer6.Flush();
                writer6.Close();

                StateMachineCodeGeneration.WpfTemplates.ResourceDesigner GenResourceDesigner = new StateMachineCodeGeneration.WpfTemplates.ResourceDesigner();
                StreamWriter writer7 = new StreamWriter(ResourceDesignerName);
                writer7.Write(GenResourceDesigner.TransformText());
                writer7.Flush();
                writer7.Close();

                StateMachineCodeGeneration.WpfTemplates.Resources GenResources = new StateMachineCodeGeneration.WpfTemplates.Resources();
                xmltext = GenResources.TransformText().TrimStart(remchars);
                StreamWriter writer8 = new StreamWriter(ResourceFileName);
                writer8.Write(xmltext);
                writer8.Flush();
                writer8.Close();

                StateMachineCodeGeneration.WpfTemplates.SettingsDesigner GenSettingsDesigner = new StateMachineCodeGeneration.WpfTemplates.SettingsDesigner();
                StreamWriter writer9 = new StreamWriter(SettingsDesignerName);
                writer9.Write(GenSettingsDesigner.TransformText());
                writer9.Flush();
                writer9.Close();

                StateMachineCodeGeneration.WpfTemplates.Settings GenSettings = new StateMachineCodeGeneration.WpfTemplates.Settings();
                xmltext = GenSettings.TransformText().TrimStart(remchars);
                StreamWriter writer10 = new StreamWriter(SettingsFileName);
                writer10.Write(xmltext);
                writer10.Flush();
                writer10.Close();
            }
            catch (Exception ee)
            {
                _errList.Add(ee.Message);
            }
        }

        public void GenerateExample(string path)
        {
            char[] remchars = { ' ', '\n', '\r', };
            try
            {
                StateMachineCodeGeneration.Example.Monster Gen = new StateMachineCodeGeneration.Example.Monster();
                string xmltext = Gen.TransformText().TrimStart(remchars);
                StreamWriter writer10 = new StreamWriter(path);
                writer10.Write(xmltext);
                writer10.Flush();
                writer10.Close();

            }
            catch (Exception ee)
            {



            }
        }

        public bool CreateAssemblyLib()
        {
            GenerateStateMachineFiles();

            if (Directory.Exists(_targetdir + @"\bin") == false)
            {
                try
                {
                    Directory.CreateDirectory(_targetdir + @"\bin");
                }
                catch (Exception ee)
                {
                    _errList.Add(ee.Message);
                    return (false);
                }
            }
            if (Directory.Exists(_targetdir + @"\temp") == false)
            {
                try
                {
                    Directory.CreateDirectory(_targetdir + @"\temp");
                }
                catch (Exception ee)
                {
                    _errList.Add(ee.Message);
                    return (false);
                }
            }
            string smFilesDir = "\\" + _smName;

            if (Directory.Exists(_targetdir + smFilesDir) == false)
            {
                try
                {
                    Directory.CreateDirectory(_targetdir + smFilesDir);
                }
                catch (Exception ee)
                {
                    _errList.Add(ee.Message);
                    return (false);
                }
            }

            DirectoryInfo df = new DirectoryInfo(_targetdir);
            string dirname = df.FullName;

            _machineFileName = dirname + smFilesDir + "\\sm" + _smName;
            MachineActionFileName = dirname + smFilesDir + "\\sm" + _smName + "Action";
            ActionEventDataFileName = dirname + smFilesDir + "\\sm" + _smName + "EventData";
            InputEventDataFileName = dirname + smFilesDir + "\\sm" + _smName + "InputData";
            TestEventsReadyDataFileName = dirname + smFilesDir + "\\sm" + _smName + "TestEventsReadyData";
            TestEventsReadyFileName = dirname + smFilesDir + "\\sm" + _smName + "TestEventsReady";
            TestFileName = dirname + smFilesDir + "\\Test" + _smName;

            ConsoleCodeName = dirname + "\\Program.cs";
            AppXamlName = dirname + "\\App.xaml";
            AppCsName = dirname + "\\App.xaml.cs";
            WinCodeName = dirname + "\\MainWindow.xaml";
            WindowXamlName = dirname + "\\MainWindow.xaml";
            cfgFileName = dirname + "\\App.Config";
            AsmbFileName = dirname + "\\Properties\\AssemblyInfo.cs";
            Errlogname = dirname + "\\errLog.Txt";
            ProjectFileName = dirname + "\\" + _smName + ".csproj";
            SettingsDesignerName = dirname + "\\Properties\\Settings.Designer.cs";
            SettingsFileName = dirname + "\\Properties\\Settings.settings";
            ResourceDesignerName = dirname + "\\Properties\\Resources.Designer.cs";
            ResourceFileName = dirname + "\\Properties\\Resources.resx";

            dbFileName = dirname + "\\StateMachines.mdf";
            smRunDesignerFileName = dirname + "\\smRun.designer.cs";
            SmRunDbmlFileName = dirname + "\\smRun.dbml";
            SmRunLayoutFileName = dirname + "\\smRun.dbml.layout";

            CodeDomProvider provider = CodeDomProvider.CreateProvider("c#");
            CompilerParameters options = new CompilerParameters();

            options.GenerateExecutable = false;

            // Set the assembly file name to generate.
            options.OutputAssembly = dirname + @"\bin\sm" + _smName + ".dll";

            // Generate debug information.
            options.IncludeDebugInformation = false;

            // Add an assembly reference.
            //         options.ReferencedAssemblies.Add("System.dll");
            //         options.ReferencedAssemblies.Add("System.Core.dll");
            //         options.ReferencedAssemblies.Add("System.Data.dll");



            // Save the assembly as a physical file.
            options.GenerateInMemory = false;

            // Set the level at which the compiler 
            // should start displaying warnings.
            options.WarningLevel = 3;

            // Set whether to treat all warnings as errors.
            options.TreatWarningsAsErrors = false;

            // Set compiler argument to optimize output.
            options.CompilerOptions = "/optimize";

            // Set a temporary files collection.
            // The TempFileCollection stores the temporary files
            // generated during a build in the current directory,
            // and does not delete them after compilation.
            options.TempFiles = new TempFileCollection(dirname + "\\temp", true);

            if (provider.Supports(GeneratorSupport.EntryPointMethod))
            {

                // Specify the class that contains 
                // the main method of the executable.
                //    cp->MainClass = "Samples.Class1";
            }

            CompilerResults cr = provider.CompileAssemblyFromDom(options, new CodeCompileUnit[] {
                 MachineActionEventUnit,
                 MachineUnit,
                 ActionsEventDataUnit,
                 InputEventDataUnit,
                 TestUnit
            });

            // Invoke compilation.
            //CompilerResults cr = provider.CompileAssemblyFromFile(options, new string[] { 
            //    MachineFileName,         
            //    MachineActionFileName,   
            //    ActionEventDataFileName, 
            //    InputEventDataFileName,  
            //    TestFileName  }
            //    );       

            if (cr.Errors.Count > 0)
            {

                _errList.Add(string.Format("Errors building {0}", cr.PathToAssembly));
                foreach (CompilerError ce in cr.Errors)
                {
                    _errList.Add(ce.ToString());
                }
                return (false);
            }
            else
            {
                return (true);
            }
        }

        protected bool StateMachineInfo(string name)
        {
            bool retv = true;
            _smcdc = new StateMachineCodeDataContext();

            // Machine id 
            var av =
                from nn in _smcdc.smStateMachineDefinitions
                where nn.Name == _smName
                select new { id = nn.StateMachineDefinitionID, initialstate = nn.InitialState, aguid = nn.AsGuid.ToString() };
            foreach (var qq in av)
            {
                _smId = qq.id;
                _smInitialstate = qq.initialstate;
                _smCodeid = new Guid(qq.aguid);
                break;
            }

            // get states to id data
            var ss =
               from ww in _smcdc.smStates
               where ww.StateMachineDefinitionID == _smId
               select ww;

            _stateDictionary = ss.ToDictionary(e => e.StateName, e => e.StateID);


            // get action to id data
            var xx =
               from ww in _smcdc.smActions
               where ww.StateMachineDefinitionID == _smId
               select ww;

            _actiondict = xx.ToDictionary(e => e.ActionName, e => e.ActionID);

            // get action to id data
            var qw =
               from ee in _smcdc.smEvents
               where ee.StateMachineDefinitionID == _smId
               select ee;

            _evntdict = qw.ToDictionary(e => e.EventName, e => e.EventID);

            var nv =
                from nn in _smcdc.smTransitions
                where nn.StateMachineDefinitionID == _smId
                select new
                {
                    startstateID = nn.StateID,
                    endstateID = nn.EndStateID,
                    actionID = nn.ActionID,
                    evntID = nn.EventID,
                    tranID = nn.Transition
                };

            _transitions = new Dictionary<string, Dictionary<string, Tranval>>(); ;

            foreach (var qq in nv)
            {
                Tranval tr = new Tranval();
                tr.Actionid = qq.actionID;
                tr.Endstateid = qq.endstateID;
                tr.Evntid = qq.evntID;
                tr.Startstateid = qq.startstateID;
                foreach (KeyValuePair<string, Int64> sg in _actiondict)
                {
                    if (sg.Value == qq.actionID)
                    {
                        tr.Actionid = qq.actionID;
                        tr.Action = sg.Key;
                    }
                }
                foreach (KeyValuePair<string, Int64> sg in _evntdict)
                {
                    if (sg.Value == qq.evntID)
                    {
                        tr.Evntid = qq.evntID;
                        tr.Evntname = sg.Key;
                    }
                }
                foreach (KeyValuePair<string, Int64> sg in _stateDictionary)
                {
                    if (sg.Value == qq.startstateID)
                    {
                        tr.Startstateid = qq.startstateID;
                        tr.Startstate = sg.Key;
                    }
                    if (sg.Value == qq.endstateID)
                    {
                        tr.Endstateid = qq.endstateID;
                        tr.Endstate = sg.Key;
                    }
                }

                if (_transitions.ContainsKey(tr.Startstate))
                {
                    Dictionary<string, Tranval> tt = _transitions[tr.Startstate];
                    if (tt.ContainsKey(tr.Evntname) == false)  // state event data is unigue
                    {
                        tt[tr.Evntname] = tr;
                    }
                }
                else
                {
                    Dictionary<string, Tranval> evdict = new Dictionary<string, Tranval>();
                    evdict[tr.Evntname] = tr;
                    _transitions[tr.Startstate] = evdict;
                }
            }

            return (retv);
        }

        #region file Names and write

        public void SetFileNames()
        {
            string smFilesDir = "\\" + _smName;

            DirectoryInfo df = new DirectoryInfo(_targetdir);
            string dirname = df.FullName;

            //_machineFileName = dirname + "\\sm" + _smName;
            //MachineActionFileName = dirname + "\\sm" + _smName + "Action";
            //ActionEventDataFileName = dirname + "\\sm" + _smName + "Eventdata";
            //InputEventDataFileName = dirname + "\\sm" + _smName + "Inputdata";
            //TestEventsReadyDataFileName = dirname + "\\sm" + _smName + "TestEventsReadyData";
            //TestEventsReadyFileName = dirname + "\\sm" + _smName + "TestEventsReady";
            //TestFileName = dirname + "\\Test" + _smName;

            _machineFileName = dirname + smFilesDir + "\\sm" + _smName;
            MachineActionFileName = dirname + smFilesDir + "\\sm" + _smName + "Action";
            ActionEventDataFileName = dirname + smFilesDir + "\\sm" + _smName + "EventData";
            InputEventDataFileName = dirname + smFilesDir + "\\sm" + _smName + "InputData";
            TestEventsReadyDataFileName = dirname + smFilesDir + "\\sm" + _smName + "TestEventsReadyData";
            TestEventsReadyFileName = dirname + smFilesDir + "\\sm" + _smName + "TestEventsReady";
            TestFileName = dirname + smFilesDir + "\\Test" + _smName;

            ConsoleCodeName = dirname + "\\Program.cs";
            AppXamlName = dirname + "\\App.xaml";
            AppCsName = dirname + "\\App.xaml.cs";
            WinCodeName = dirname + "\\MainWindow.xaml";
            WindowXamlName = dirname + "\\MainWindow.xaml";
            cfgFileName = dirname + "\\App.Config";
            AsmbFileName = dirname + "\\Properties\\AssemblyInfo.cs";
            Errlogname = dirname + "\\errLog.Txt";
            ProjectFileName = dirname + "\\" + _smName + ".csproj";
            SettingsDesignerName = dirname + "\\Properties\\Settings.Designer.cs";
            SettingsFileName = dirname + "\\Properties\\Settings.settings";
            ResourceDesignerName = dirname + "\\Properties\\Resources.Designer.cs";
            ResourceFileName = dirname + "\\Properties\\Resources.resx";

            dbFileName = dirname + "\\StateMachines.mdf";
            smRunDesignerFileName = dirname + "\\smRun.designer.cs";
            SmRunDbmlFileName = dirname + "\\smRun.dbml";
            SmRunLayoutFileName = dirname + "\\smRun.dbml.layout";
        }

        public void SimpleWrite(string filename, string buf)
        {
            try
            {
                FileInfo af = new FileInfo(filename);
                if (af.Exists == false)
                {
                    FileStream fs = af.Create();
                    fs.Close();
                    using (StreamWriter sw = new StreamWriter(filename))
                    {
                        sw.Write(buf);
                    }
                }
            }
            catch (Exception ee)
            {
                _errList.Add(ee.Message);
            }
        }

        public void SetFileNames(string targ)
        {

            if (Directory.Exists(targ) == false)
            {
                try
                {
                    Directory.CreateDirectory(targ);
                }
                catch (Exception ee)
                {
                    _errList.Add(ee.Message);
                    return;
                }
            }
            if (Directory.Exists(targ + @"\Properties") == false)
            {
                try
                {
                    Directory.CreateDirectory(targ + @"\Properties");
                }
                catch (Exception ee)
                {
                    _errList.Add(ee.Message);
                    return;
                }
            }

            string smFilesDir = "\\" + _smName;

            if (Directory.Exists(_targetdir + smFilesDir) == false)
            {
                try
                {
                    Directory.CreateDirectory(_targetdir + smFilesDir);
                }
                catch (Exception ee)
                {
                    _errList.Add(ee.Message);
                }
            }

            _targetdir = targ;
            DirectoryInfo df = new DirectoryInfo(_targetdir);
            string dirname = df.FullName;

            //_machineFileName = dirname + "\\sm" + _smName;
            //MachineActionFileName = dirname + "\\sm" + _smName + "Action";
            //ActionEventDataFileName = dirname + "\\sm" + _smName + "Eventdata";
            //InputEventDataFileName = dirname + "\\sm" + _smName + "Inputdata";
            //TestEventsReadyDataFileName = dirname + "\\sm" + _smName + "TestEveentsReadyData";
            //TestEventsReadyFileName = dirname + "\\sm" + _smName + "TestEventsReady";
            //TestFileName = dirname + "\\Test" + _smName + ".cs";

            _machineFileName = dirname + smFilesDir + "\\sm" + _smName;
            MachineActionFileName = dirname + smFilesDir + "\\sm" + _smName + "Action";
            ActionEventDataFileName = dirname + smFilesDir + "\\sm" + _smName + "EventData";
            InputEventDataFileName = dirname + smFilesDir + "\\sm" + _smName + "InputData";
            TestEventsReadyDataFileName = dirname + smFilesDir + "\\sm" + _smName + "TestEventsReadyData";
            TestEventsReadyFileName = dirname + smFilesDir + "\\sm" + _smName + "TestEventsReady";
            TestFileName = dirname + smFilesDir + "\\Test" + _smName;

            ConsoleCodeName = dirname + "\\Program.cs";
            AppXamlName = dirname + "\\App.xaml";
            AppCsName = dirname + "\\App.xaml.cs";
            WinCodeName = dirname + "\\MainWindow.xaml";
            WindowXamlName = dirname + "\\MainWindow.xaml";
            cfgFileName = dirname + "\\App.Config";
            AsmbFileName = dirname + "\\Properties\\AssemblyInfo.cs";
            Errlogname = dirname + "\\errLog.Txt";
            ProjectFileName = dirname + "\\" + _smName + ".csproj";
            SettingsDesignerName = dirname + "\\Properties\\Settings.Designer.cs";
            SettingsFileName = dirname + "\\Properties\\Settings.settings";
            ResourceDesignerName = dirname + "\\Properties\\Resources.Designer.cs";
            ResourceFileName = dirname + "\\Properties\\Resources.resx";

            dbFileName = dirname + "\\StateMachines.mdf";
            smRunDesignerFileName = dirname + "\\smRun.designer.cs";
            SmRunDbmlFileName = dirname + "\\smRun.dbml";
            SmRunLayoutFileName = dirname + "\\smRun.dbml.layout";
        }

        #endregion

        #region Utilitys : Field, Field Property, CreateFieldAssign

        public CodeMemberField CreateField(System.Type fieldtype, string fieldname, string comment)
        {
            CodeMemberField evField = new CodeMemberField();
            evField.Attributes = MemberAttributes.Private;
            evField.Name = fieldname;
            evField.Type = new CodeTypeReference(fieldtype);
            evField.Comments.Add(new CodeCommentStatement(
                comment));
            return (evField);
        }

        public CodeMemberProperty CreateFieldProperty(System.Type proptype, string fieldname, string propname, string comment)
        {
            CodeMemberProperty idProperty = new CodeMemberProperty();
            idProperty.Attributes =
                MemberAttributes.Public | MemberAttributes.Final;
            idProperty.Name = propname;
            idProperty.HasGet = true;
            idProperty.HasSet = true;
            idProperty.Type = new CodeTypeReference(proptype);
            idProperty.Comments.Add(new CodeCommentStatement(
                string.Format("{0} machine {1}.", _smName, comment)));
            idProperty.GetStatements.Add(new CodeMethodReturnStatement(
                new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(), fieldname)));
            CodeAssignStatement asc = new CodeAssignStatement(
                           new CodeFieldReferenceExpression(
                                new CodeThisReferenceExpression(), fieldname),
                           new CodeVariableReferenceExpression("value"));
            idProperty.SetStatements.Add(asc);
            return (idProperty);
        }

        public CodeMemberProperty CreateFieldProperty(System.String proptype, string fieldname, string propname, string comment)
        {
            CodeMemberProperty idProperty = new CodeMemberProperty();
            idProperty.Attributes =
                MemberAttributes.Public | MemberAttributes.Final;
            idProperty.Name = propname;
            idProperty.HasGet = true;
            idProperty.HasSet = true;
            idProperty.Type = new CodeTypeReference(proptype);
            idProperty.Comments.Add(new CodeCommentStatement(
                string.Format("{0} machine {1}.", _smName, comment)));
            idProperty.GetStatements.Add(new CodeMethodReturnStatement(
                new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(), fieldname)));
            CodeAssignStatement asc = new CodeAssignStatement(
                           new CodeFieldReferenceExpression(
                                new CodeThisReferenceExpression(), fieldname),
                           new CodeVariableReferenceExpression("value"));
            idProperty.SetStatements.Add(asc);
            return (idProperty);
        }

        CodeAssignStatement CreateFieldAssign(string fieldname, string varname)
        {
            CodeAssignStatement as1 =
                new CodeAssignStatement(new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(), fieldname),
                new CodeVariableReferenceExpression(varname));
            return (as1);
        }

        #endregion

        #region Actions

        public void MachineActionEventClass()
        {
            MachineActionEventUnit = new CodeCompileUnit();
            CodeNamespace Machine = new CodeNamespace("sm_" + _smName);
            Machine.Imports.Add(new CodeNamespaceImport("System"));
            Machine.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            //if (language == "c#")
            //{
            //    Machine.Imports.Add(new CodeNamespaceImport("System.Linq"));
            //}
            Machine.Imports.Add(new CodeNamespaceImport("System.Text"));
            Machine.Imports.Add(new CodeNamespaceImport("System.IO"));

            CodeTypeDelegate StateAction = new CodeTypeDelegate("StateAction");
            StateAction.ReturnType = new CodeTypeReference(typeof(System.Boolean));
            StateAction.Parameters.Add(new CodeParameterDeclarationExpression("System.Int64", "sm_id"));
            StateAction.Parameters.Add(new CodeParameterDeclarationExpression("System.Int64", "sm_inst"));
            Machine.Types.Add(StateAction);

            CodeTypeDelegate StarShipEventHandler = new CodeTypeDelegate(_smName + "ActionEventHandler");
            StarShipEventHandler.ReturnType = new CodeTypeReference(typeof(System.Boolean));
            StarShipEventHandler.Parameters.Add(new CodeParameterDeclarationExpression("System.Object", "sender"));
            StarShipEventHandler.Parameters.Add(new CodeParameterDeclarationExpression(_smName + "ActionsEventArgs", "e"));
            Machine.Types.Add(StarShipEventHandler);

            MachineActionEventClassCodeType = new CodeTypeDeclaration(_smName + "ActionsEvents");
            MachineActionEventClassCodeType.IsClass = true;
            MachineActionEventClassCodeType.TypeAttributes = TypeAttributes.Public;
            Machine.Types.Add(MachineActionEventClassCodeType);
            MachineActionEventUnit.Namespaces.Add(Machine);
            MachineActionEventFields();
            MachineActionEventContructor();
            MachineActionEventProperties();
            MachineActionEventMembersA();
            MachineActionEventMembersB();
            GenerateCode(MachineActionFileName, MachineActionEventUnit);
        }

        public void MachineActionEventContructor()
        {
            CodeConstructor ActionsEventDataConstructor = new CodeConstructor();
            ActionsEventDataConstructor.Attributes = MemberAttributes.Public;
            ActionsEventDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.Int64", "smid"));
            ActionsEventDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.Int64", "sminst"));
            ActionsEventDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.Guid", "sminstguid"));

            ActionsEventDataConstructor.Statements.Add(CreateFieldAssign("_sm_id", "smid"));
            ActionsEventDataConstructor.Statements.Add(CreateFieldAssign("_sm_inst", "sminst"));
            ActionsEventDataConstructor.Statements.Add(CreateFieldAssign("_sm_instguid", "sminstguid"));

            MachineActionEventClassCodeType.Members.Add(ActionsEventDataConstructor);
        }

        public void MachineActionEventFields()
        {
            MachineActionEventClassCodeType.Members.Add(CreateField(typeof(System.Int64), "_sm_id", "the FSM id"));
            MachineActionEventClassCodeType.Members.Add(CreateField(typeof(System.Int64), "_sm_inst", "the FSM instance id"));
            MachineActionEventClassCodeType.Members.Add(CreateField(typeof(System.Guid), "_sm_instguid", "the FSM instance Guid"));

        }

        public void MachineActionEventProperties()
        {
            MachineActionEventClassCodeType.Members.Add(CreateFieldProperty(typeof(System.Int64), "_sm_id", "SM_ID", string.Format("The {0} FSM id property.", _smName)));
            MachineActionEventClassCodeType.Members.Add(CreateFieldProperty(typeof(System.Int64), "_sm_inst", "Instance_ID", string.Format("The {0} FSM instance id property.", _smName)));
            MachineActionEventClassCodeType.Members.Add(CreateFieldProperty(typeof(System.Guid), "_sm_instguid", "Guid_ID", string.Format("The {0} FSM Guid property.", _smName)));

            foreach (KeyValuePair<string, Int64> sg in _actiondict)
            {
                string aehand = string.Format("{0}ActionEventHandler", _smName);
                string ae = string.Format("{0}Action{1}", _smName, sg.Key);

                CodeMemberEvent eventa = new CodeMemberEvent();
                eventa.Name = ae;
                eventa.Type = new CodeTypeReference(aehand);
                eventa.Attributes = MemberAttributes.Public;
                MachineActionEventClassCodeType.Members.Add(eventa);
            }
        }

        public void MachineActionEventMembersA()
        {
            foreach (KeyValuePair<string, Int64> sg in _actiondict)
            {
                string strunaction = string.Format("Run{0}Action{1}", _smName, sg.Key);
                string stactionhandler = string.Format("{0}ActionEventHandler", _smName);
                string staction = string.Format("{0}Action{1}", _smName, sg.Key);
                string aevargs = string.Format("{0}ActionsEventArgs", _smName);

                CodeMemberMethod doaction = new CodeMemberMethod();
                doaction.Name = strunaction;
                doaction.ReturnType = new CodeTypeReference("System.Boolean");
                doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;

                CodeVariableDeclarationStatement ash = new CodeVariableDeclarationStatement(
                  stactionhandler,
                  "ehv",
                  new CodeVariableReferenceExpression(staction));
                doaction.Statements.Add(ash);

                CodeConditionStatement theif = new CodeConditionStatement();  // if
                CodeBinaryOperatorExpression atest = new CodeBinaryOperatorExpression();  // (ehv != null)
                atest.Right = new CodePrimitiveExpression(null);
                atest.Left = new CodeVariableReferenceExpression("ehv");
                atest.Operator = CodeBinaryOperatorType.IdentityInequality;
                theif.Condition = atest;

                CodeMethodReturnStatement aret = new CodeMethodReturnStatement();   // return 
                CodeDelegateInvokeExpression exsh = new CodeDelegateInvokeExpression();   // ehv(this, new {0}ActionsEventArgs(smid, instance_id, sm_instguid));

                exsh.TargetObject = new CodeVariableReferenceExpression("ehv");

                CodeObjectCreateExpression evarg = new CodeObjectCreateExpression();
                evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_sm_id"));
                evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_sm_inst"));
                evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_sm_instguid"));
                evarg.CreateType = new CodeTypeReference(aevargs);  // {0}ActionsEventArgs(_sm_id, instance_id, sm_instguid)

                exsh.Parameters.Add(new CodeThisReferenceExpression());
                exsh.Parameters.Add(evarg);

                aret.Expression = exsh;

                theif.TrueStatements.Add(aret);
                doaction.Statements.Add(theif);
                doaction.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));

                MachineActionEventClassCodeType.Members.Add(doaction);
            }
        }

        public void MachineActionEventMembersB()
        {
            foreach (KeyValuePair<string, Int64> sg in _actiondict)
            {
                string strunaction = string.Format("Run{0}Action{1}", _smName, sg.Key);
                string stactionhandler = string.Format("{0}ActionEventHandler", _smName);
                string staction = string.Format("{0}Action{1}", _smName, sg.Key);
                string aevargs = string.Format("{0}ActionsEventArgs", _smName);

                CodeMemberMethod doaction = new CodeMemberMethod();
                doaction.Name = strunaction;
                doaction.ReturnType = new CodeTypeReference("System.Boolean");
                doaction.Parameters.Add(new CodeParameterDeclarationExpression("System.Int64", "id"));
                doaction.Parameters.Add(new CodeParameterDeclarationExpression("System.Int64", "inst"));
                doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;

                CodeVariableDeclarationStatement ash = new CodeVariableDeclarationStatement(
                  stactionhandler,
                  "ehv",
                  new CodeVariableReferenceExpression(staction));
                doaction.Statements.Add(ash);

                CodeConditionStatement theif = new CodeConditionStatement();  // if

                CodeBinaryOperatorExpression atest = new CodeBinaryOperatorExpression();  // (ehv != null)
                atest.Right = new CodePrimitiveExpression(null);
                atest.Left = new CodeVariableReferenceExpression("ehv");
                atest.Operator = CodeBinaryOperatorType.IdentityInequality;
                theif.Condition = atest;

                CodeMethodReturnStatement aret = new CodeMethodReturnStatement();   // return 
                CodeDelegateInvokeExpression exsh = new CodeDelegateInvokeExpression();   // ehv(this, new {0}ActionsEventArgs(smid, instance_id, sm_instguid));

                exsh.TargetObject = new CodeVariableReferenceExpression("ehv");

                CodeObjectCreateExpression evarg = new CodeObjectCreateExpression();
                evarg.Parameters.Add(new CodeArgumentReferenceExpression("id"));
                evarg.Parameters.Add(new CodeArgumentReferenceExpression("inst"));
                evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_sm_instguid"));
                evarg.CreateType = new CodeTypeReference(aevargs);  // {0}ActionsEventArgs(smid, instance_id, sm_instguid)

                exsh.Parameters.Add(new CodeThisReferenceExpression());
                exsh.Parameters.Add(evarg);

                aret.Expression = exsh;

                theif.TrueStatements.Add(aret);
                doaction.Statements.Add(theif);

                doaction.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));

                MachineActionEventClassCodeType.Members.Add(doaction);
            }
        }

        #endregion

        #region State Machine Unit

        public void StateMachineClass()
        {
            MachineUnit = new CodeCompileUnit();
            CodeNamespace Machine = new CodeNamespace("sm_" + _smName);
            Machine.Imports.Add(new CodeNamespaceImport("System"));
            Machine.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            if (_language == "c#")
            {
                Machine.Imports.Add(new CodeNamespaceImport("System.Linq"));
            }
            Machine.Imports.Add(new CodeNamespaceImport("System.Text"));
            Machine.Imports.Add(new CodeNamespaceImport("System.IO"));

            CodeTypeDelegate StateEvent = new CodeTypeDelegate("StateEvent");
            StateEvent.ReturnType = new CodeTypeReference(typeof(System.Boolean));
            StateEvent.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "evnt"));
            Machine.Types.Add(StateEvent);

            CodeTypeDelegate StarShipEventHandler = new CodeTypeDelegate(_smName + "InputEventHandler");
            StarShipEventHandler.ReturnType = new CodeTypeReference(typeof(System.Boolean));
            StarShipEventHandler.Parameters.Add(new CodeParameterDeclarationExpression("System.Object", "sender"));
            StarShipEventHandler.Parameters.Add(new CodeParameterDeclarationExpression(_smName + "InputEventArgs", "e"));
            Machine.Types.Add(StarShipEventHandler);

            MachineClassType = new CodeTypeDeclaration(_smName);
            MachineClassType.IsClass = true;
            MachineClassType.TypeAttributes = TypeAttributes.Public;
            Machine.Types.Add(MachineClassType);
            MachineUnit.Namespaces.Add(Machine);
            MachineFields();
            MachineProperties();
            MachineMembers();
            GenerateCode(_machineFileName, MachineUnit);
        }

        public void MachineProperties()
        {

            MachineClassType.Members.Add(CreateFieldProperty(_smName + "ActionsEvents", _smName + "_ev", _smName + "Actions", "The Transition ActionsEvents property"));
            MachineClassType.Members.Add(CreateFieldProperty(_smName + "TestEventsReady",
                string.Format("{0}InputReadyNow", _smName), _smName + "InputReady", "The Check for input Events property"));
            //           MachineClassType.Members.Add(CreateFieldProperty(typeof(System.String), sm_currentstate, "CurrentState", "the current state"));


            //CodeMemberProperty kkProperty = new CodeMemberProperty();
            //kkProperty.Attributes =
            //    MemberAttributes.Public | MemberAttributes.Final;
            //kkProperty.Name = sm_name + "Actions";
            //kkProperty.HasGet = true;
            //kkProperty.HasSet = true;
            //kkProperty.Type = new CodeTypeReference(sm_name + "ActionsEvents");
            //kkProperty.Comments.Add(new CodeCommentStatement(
            //    "The Transition Actions property."));
            //kkProperty.GetStatements.Add(new CodeMethodReturnStatement(
            //    new CodeFieldReferenceExpression(
            //    new CodeThisReferenceExpression(), sm_name + "_ev")));

            //CodeAssignStatement asc = new CodeAssignStatement(
            //               new CodeVariableReferenceExpression(sm_name + "_ev"),
            //               new CodeVariableReferenceExpression("value"));

            //kkProperty.SetStatements.Add(asc);
            //MachineClassType.Members.Add(kkProperty);
        }

        public void MachineFields()
        {

            MachineClassType.Members.Add(CreateField(typeof(System.Int64), "sm_id", "the FSM id."));
            MachineClassType.Members.Add(CreateField(typeof(System.String), "sm_currentstate", "the current state"));
            MachineClassType.Members.Add(CreateField(typeof(System.Boolean), "smuse_db", "true if a database is used for current state and to communicate with other state machines"));
            MachineClassType.Members.Add(CreateField(typeof(System.Int64), "instance_id", "the FSM instance id"));
            MachineClassType.Members.Add(CreateField(typeof(System.Guid), "sm_instguid", "the FSM instance Guid"));

            CodeMemberField nameField = new CodeMemberField();
            nameField.Attributes = MemberAttributes.Private | MemberAttributes.Const;
            nameField.Name = "sm_name";
            nameField.Type = new CodeTypeReference(typeof(System.String));
            nameField.Comments.Add(new CodeCommentStatement(
                "The name"));
            nameField.InitExpression = new CodePrimitiveExpression(string.Format("{0}", _smName));
            MachineClassType.Members.Add(nameField);

            CodeMemberField aefiled = new CodeMemberField();
            aefiled.Attributes = MemberAttributes.Private;
            aefiled.Name = string.Format("{0}_ev", _smName);
            aefiled.Type = new CodeTypeReference(string.Format("{0}ActionsEvents", _smName));
            aefiled.Comments.Add(new CodeCommentStatement(
                string.Format("{0}_ActionsEvents class instance", _smName)));
            MachineClassType.Members.Add(aefiled);

            CodeMemberField befiled = new CodeMemberField();
            befiled.Attributes = MemberAttributes.Private;
            befiled.Name = string.Format("{0}InputReadyNow", _smName);
            befiled.Type = new CodeTypeReference(string.Format("{0}TestEventsReady", _smName));
            befiled.Comments.Add(new CodeCommentStatement(
                string.Format("{0}TestEventsReady class instance", _smName)));
            MachineClassType.Members.Add(befiled);

            foreach (KeyValuePair<string, Dictionary<string, Tranval>> tran in _transitions)
            {

                //  MachineClassType.Members.Add(CreateField(typeof(Dictionary<string, StateAction>),
                MachineClassType.Members.Add(CreateField(typeof(Dictionary<string, object>),
               string.Format("{0}EventDictionary", tran.Key),
               string.Format("Dictionary for state {0} event actions", tran.Key)));

                MachineClassType.Members.Add(CreateField(typeof(Dictionary<string, string>),
                  string.Format("{0}FinalDictionary", tran.Key),
                  string.Format("Dictionary for state {0} action success state", tran.Key)));

                //MachineClassType.Members.Add(CreateField(typeof(System.Collections.Hashtable), 
                //    string.Format("{0}evnthash", tran.Key), 
                //    string.Format("Hastable for state {0} event actions", tran.Key)));

                //MachineClassType.Members.Add(CreateField(typeof(System.Collections.Hashtable),
                //  string.Format("{0}finalhash", tran.Key),
                //  string.Format("Hastable for state {0} action success state", tran.Key)));
            }

            //   MachineClassType.Members.Add(CreateField(typeof(Dictionary<string, StateEvent>),
            MachineClassType.Members.Add(CreateField(typeof(Dictionary<string, object>),
                    string.Format("{0}StateDictionary", _smName),
                   string.Format("Dictionary on event in state methods")));

            //MachineClassType.Members.Add(CreateField(typeof(System.Collections.Hashtable),
            //        string.Format("{0}statehash", sm_name),
            //        string.Format("Hastable on event in state methods")));
        }

        public void MachineDictionary()
        {
            CodeMemberMethod domach = new CodeMemberMethod();
            domach.Name = "BuildStateEventToActionsDictionary";
            foreach (KeyValuePair<string, Dictionary<string, Tranval>> tran in _transitions)
            {
                string st = tran.Key;
                Dictionary<string, Tranval> tt = tran.Value;

                //   string ststatehash = string.Format("{0}statehash", sm_name);
                string ststatedict = string.Format("{0}StateDictionary", _smName);

                string sttrankey = string.Format("st_{0}", tran.Key);

                foreach (KeyValuePair<string, Tranval> tranev in tt)
                {
                    Tranval tv = tranev.Value;
                    //       string stmevnthash = string.Format("{0}evnthash", st);
                    string stmeventdict = string.Format("{0}EventDictionary", st);

                    string ststateaction = string.Format("StateAction({0}ev", _smName);
                    string ststatemeth = string.Format("Run{0}Action{1}", _smName, tv.Action);

                    //        string stfinalhash = string.Format("{0}finalhash", tran.Key);

                    string stfinaldict = string.Format("{0}FinalDictionary", tran.Key);

                    CodeObjectCreateExpression evarg = new CodeObjectCreateExpression();  // new StateAction({2}_ev.Run_{2}Action_{3})
                    evarg.Parameters.Add(new CodeFieldReferenceExpression(
                     new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), _smName + "_ev"),
                     ststatemeth
                     ));

                    evarg.CreateType = new CodeTypeReference("StateAction");

                    CodeExpression[] ac = new CodeExpression[] { new CodePrimitiveExpression(tv.Evntname), evarg };

                    //      CodeFieldReferenceExpression hashfield = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), stmevnthash);
                    //      CodeMethodInvokeExpression exm = new CodeMethodInvokeExpression(hashfield, "Add", ac);

                    CodeFieldReferenceExpression dictfield = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), stmeventdict);
                    CodeMethodInvokeExpression exm = new CodeMethodInvokeExpression(dictfield, "Add", ac);

                    domach.Statements.Add(exm);

                    CodeExpression[] ae = new CodeExpression[] { new CodePrimitiveExpression(tv.Evntname), new CodePrimitiveExpression(tv.Endstate) };

                    CodeFieldReferenceExpression fdictfield = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), stfinaldict);
                    CodeMethodInvokeExpression efa = new CodeMethodInvokeExpression(fdictfield, "Add", ae);

                    //     CodeFieldReferenceExpression fhashfield = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), stfinalhash);
                    //     CodeMethodInvokeExpression efa = new CodeMethodInvokeExpression(fhashfield, "Add", ae);

                    domach.Statements.Add(efa);
                }
                CodeObjectCreateExpression evargb = new CodeObjectCreateExpression();  // new StateAction({2}_ev.Run_{2}Action_{3})
                CodeFieldReferenceExpression stevent = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), sttrankey);
                evargb.Parameters.Add(stevent);
                evargb.CreateType = new CodeTypeReference("StateEvent");

                CodeExpression[] acb = new CodeExpression[] { new CodePrimitiveExpression(tran.Key), evargb };


                // CodeFieldReferenceExpression ghashfield = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), ststatehash);
                //    CodeMethodInvokeExpression exma = new CodeMethodInvokeExpression(ghashfield, "Add", acb);

                CodeFieldReferenceExpression gdictfield = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), ststatedict);
                CodeMethodInvokeExpression exma = new CodeMethodInvokeExpression(gdictfield, "Add", acb);


                domach.Statements.Add(exma);
            }
            domach.Comments.Add(new CodeCommentStatement("Fills the dictionaries"));
            MachineClassType.Members.Add(domach);
        }

        public void CreateStateActionHandler()
        {
            foreach (KeyValuePair<string, Dictionary<string, Tranval>> tran in _transitions)
            {
                string st = tran.Key;
                Dictionary<string, Tranval> tt = tran.Value;
                string stname = string.Format("st_{0}", st);
                string steventdict = string.Format("{0}EventDictionary", tran.Key);
                string ststatedict = string.Format("{0}FinalDictionary", tran.Key);

                //string stevnthash = string.Format("{0}evnthash", tran.Key);
                //string ststatehash = string.Format("{0}finalhash", tran.Key);

                CodeMemberMethod doaction = new CodeMemberMethod();
                doaction.Name = stname;
                doaction.ReturnType = new CodeTypeReference("System.Boolean");
                doaction.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "evnt"));
                doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;

                //      CodeArrayIndexerExpression ci1 = new CodeArrayIndexerExpression(new CodeVariableReferenceExpression(stevnthash), new CodeArgumentReferenceExpression("evnt"));

                CodeArrayIndexerExpression ci1 = new CodeArrayIndexerExpression(new CodeVariableReferenceExpression(steventdict), new CodeArgumentReferenceExpression("evnt"));
                CodeCastExpression castExpression = new CodeCastExpression("StateAction", ci1);
                CodeVariableDeclarationStatement ash = new CodeVariableDeclarationStatement(
                  "StateAction",
                  "sact",
                  castExpression);
                doaction.Statements.Add(ash);

                // CodeArrayIndexerExpression ci2 = new CodeArrayIndexerExpression(new CodeVariableReferenceExpression(ststatehash), new CodeArgumentReferenceExpression("evnt"));
                CodeArrayIndexerExpression ci2 = new CodeArrayIndexerExpression(new CodeVariableReferenceExpression(ststatedict), new CodeArgumentReferenceExpression("evnt"));
                CodeCastExpression castExpression2 = new CodeCastExpression("System.String", ci2);
                CodeVariableDeclarationStatement ash2 = new CodeVariableDeclarationStatement(
                  "System.String",
                  "endstate",
                  castExpression2);
                doaction.Statements.Add(ash2);

                CodeConditionStatement outif = new CodeConditionStatement();  // if
                CodeBinaryOperatorExpression atest = new CodeBinaryOperatorExpression();  // (sact == null)
                atest.Right = new CodePrimitiveExpression(null);
                atest.Left = new CodeVariableReferenceExpression("sact");
                atest.Operator = CodeBinaryOperatorType.IdentityEquality;
                outif.Condition = atest;

                //CodeArrayIndexerExpression ci3 = new CodeArrayIndexerExpression(
                //    new CodeVariableReferenceExpression(stevnthash),
                //    new CodePrimitiveExpression("default"));
                CodeArrayIndexerExpression ci3 = new CodeArrayIndexerExpression(
                    new CodeVariableReferenceExpression(steventdict),
                    new CodePrimitiveExpression("default"));

                CodeCastExpression castExpression3 = new CodeCastExpression("StateAction", ci3);
                CodeAssignStatement ash3 = new CodeAssignStatement(new CodeVariableReferenceExpression("sact"), castExpression3);
                outif.TrueStatements.Add(ash3);

                //CodeArrayIndexerExpression ci4 = new CodeArrayIndexerExpression(
                //    new CodeVariableReferenceExpression(ststatehash),
                //    new CodePrimitiveExpression("default"));

                CodeArrayIndexerExpression ci4 = new CodeArrayIndexerExpression(
                    new CodeVariableReferenceExpression(ststatedict),
                    new CodePrimitiveExpression("default"));


                CodeCastExpression castExpression4 = new CodeCastExpression("System.String", ci4);
                CodeAssignStatement ash4 = new CodeAssignStatement(new CodeVariableReferenceExpression("endstate"), castExpression4);
                outif.TrueStatements.Add(ash4);
                doaction.Statements.Add(outif);

                CodeConditionStatement nif = new CodeConditionStatement();  // if
                CodeBinaryOperatorExpression btest = new CodeBinaryOperatorExpression();  // (sact != null)
                btest.Right = new CodePrimitiveExpression(null);
                btest.Left = new CodeVariableReferenceExpression("sact");
                btest.Operator = CodeBinaryOperatorType.IdentityInequality;
                nif.Condition = btest;

                CodeConditionStatement inif = new CodeConditionStatement();  // if
                CodeBinaryOperatorExpression ctest = new CodeBinaryOperatorExpression();  // (sact(sm_id, instance_id) == true)
                ctest.Right = new CodePrimitiveExpression(true);
                CodeDelegateInvokeExpression invokesact = new CodeDelegateInvokeExpression(new CodeVariableReferenceExpression("sact"),
                    new CodeExpression[] { new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_id"), new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "instance_id") });
                ctest.Left = invokesact;
                ctest.Operator = CodeBinaryOperatorType.IdentityEquality;
                inif.Condition = ctest;

                CodeMethodReferenceExpression setstate = new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "SetCurrentState");
                CodeMethodInvokeExpression invokesetst = new CodeMethodInvokeExpression(setstate, new CodeExpression[] { new CodeVariableReferenceExpression("endstate") });
                inif.TrueStatements.Add(invokesetst);
                inif.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));
                nif.TrueStatements.Add(inif);
                doaction.Statements.Add(nif);
                doaction.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));
                doaction.Comments.Add(new CodeCommentStatement(string.Format("Runs the action defined for state {0} and an input", tran.Key)));
                MachineClassType.Members.Add(doaction);
            }
        }

        public void CreateInputEventHandler()
        {
            foreach (KeyValuePair<string, Int64> ev in _evntdict)
            {
                CodeMemberEvent event1 = new CodeMemberEvent();
                event1.Name = string.Format("{0}Event{1}", _smName, ev.Key);
                event1.Type = new CodeTypeReference(_smName + "InputEventHandler");
                event1.Attributes = MemberAttributes.Public;
                MachineClassType.Members.Add(event1);

                string inputname = string.Format("Run{0}Event{1}", _smName, ev.Key);
                string inputeventhand = string.Format("{0}InputEventHandler", _smName);
                string inputevent = string.Format("{0}Event{1}", _smName, ev.Key);
                string inputargs = string.Format("{0}InputEventArgs", _smName);

                CodeMemberMethod doactionevent = new CodeMemberMethod();
                doactionevent.Name = inputname;
                doactionevent.ReturnType = new CodeTypeReference("System.Boolean");
                doactionevent.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "st"));
                doactionevent.Attributes = MemberAttributes.Public | MemberAttributes.Final;

                CodeVariableDeclarationStatement declare_ehv = new CodeVariableDeclarationStatement(
                   inputeventhand,
                   "ehv",
                   new CodeEventReferenceExpression(new CodeThisReferenceExpression(), inputevent));

                doactionevent.Statements.Add(declare_ehv);

                CodeConditionStatement outif = new CodeConditionStatement();  // if
                CodeBinaryOperatorExpression ehvtest = new CodeBinaryOperatorExpression();  // (ehv != null)
                ehvtest.Right = new CodePrimitiveExpression(null);
                ehvtest.Left = new CodeVariableReferenceExpression("ehv");
                ehvtest.Operator = CodeBinaryOperatorType.IdentityInequality;
                outif.Condition = ehvtest;

                CodeObjectCreateExpression CreateInputEvent = new CodeObjectCreateExpression(inputargs,
                  new CodeExpression[]
                       {
                         new CodeArgumentReferenceExpression("st"),
                         new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_id"),
                         new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "instance_id"),
                         new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_instguid"),
                       });

                CodeVariableDeclarationStatement setupe = new CodeVariableDeclarationStatement(
                inputargs,
                "e",
                CreateInputEvent);

                outif.TrueStatements.Add(setupe);

                CodeDelegateInvokeExpression invokeehv = new CodeDelegateInvokeExpression(new CodeVariableReferenceExpression("ehv"),
                  new CodeExpression[]
                     {
                         new CodeThisReferenceExpression(),
                         new CodeVariableReferenceExpression("e")
                     });

                CodeVariableDeclarationStatement setupgo = new CodeVariableDeclarationStatement(
                "System.Boolean",
                "go",
                invokeehv);

                outif.TrueStatements.Add(setupgo);

                CodeConditionStatement inif = new CodeConditionStatement();  // if
                CodeBinaryOperatorExpression intest = new CodeBinaryOperatorExpression();  // (go == true)
                intest.Right = new CodePrimitiveExpression(true);
                intest.Left = new CodeVariableReferenceExpression("go");
                intest.Operator = CodeBinaryOperatorType.IdentityEquality;
                inif.Condition = intest;

                CodeMethodInvokeExpression CallEventFromState = new CodeMethodInvokeExpression(
                    new CodeThisReferenceExpression(),
                    "RunEventFromState",
                    new CodeExpression[]
                     {
                         new CodePrimitiveExpression(ev.Key)
                     });

                inif.TrueStatements.Add(new CodeMethodReturnStatement(CallEventFromState));
                outif.TrueStatements.Add(inif);


                doactionevent.Statements.Add(outif);
                doactionevent.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));
                doactionevent.Comments.Add(new CodeCommentStatement(string.Format("If input event returns true passes input on to state:{0} and it's actions ", ev.Key)));
                MachineClassType.Members.Add(doactionevent);
            }
        }

        public void CreateRunEventFromState()
        {
            //string hashname = string.Format("{0}statehash", sm_name);

            string dictname = string.Format("{0}StateDictionary", _smName);

            CodeMemberMethod doFromState = new CodeMemberMethod();
            doFromState.Name = "RunEventFromState";
            doFromState.ReturnType = new CodeTypeReference("System.Boolean");
            doFromState.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "evnt"));
            doFromState.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            CodeMethodInvokeExpression callgetstate = new CodeMethodInvokeExpression(
                 new CodeThisReferenceExpression(),
                "GetCurrentState",
                new CodeExpression[] { });


            //   CodeArrayIndexerExpression hasv = new CodeArrayIndexerExpression(new CodeVariableReferenceExpression(hashname), callgetstate);

            CodeArrayIndexerExpression hasv = new CodeArrayIndexerExpression(new CodeVariableReferenceExpression(dictname), callgetstate);
            CodeCastExpression castExpression = new CodeCastExpression("StateEvent", hasv);
            CodeVariableDeclarationStatement declare_sev = new CodeVariableDeclarationStatement(
               "StateEvent",
               "sev",
               castExpression);

            doFromState.Statements.Add(declare_sev);

            CodeConditionStatement outif = new CodeConditionStatement();  // if
            CodeBinaryOperatorExpression sevtest = new CodeBinaryOperatorExpression();  // (sev != null)
            sevtest.Right = new CodePrimitiveExpression(null);
            sevtest.Left = new CodeVariableReferenceExpression("sev");
            sevtest.Operator = CodeBinaryOperatorType.IdentityInequality;
            outif.Condition = sevtest;

            CodeDelegateInvokeExpression invokesev = new CodeDelegateInvokeExpression(new CodeVariableReferenceExpression("sev"),
                     new CodeExpression[]
                     {
                         new CodeArgumentReferenceExpression("evnt")
                     });


            CodeConditionStatement inif = new CodeConditionStatement();  // if
            CodeBinaryOperatorExpression runsevtest = new CodeBinaryOperatorExpression();  // (sev(evnt) == true)
            runsevtest.Right = new CodePrimitiveExpression(true);
            runsevtest.Left = invokesev;
            runsevtest.Operator = CodeBinaryOperatorType.IdentityEquality;
            inif.Condition = runsevtest;
            inif.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));

            outif.TrueStatements.Add(inif);
            doFromState.Statements.Add(outif);
            doFromState.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));

            MachineClassType.Members.Add(doFromState);
        }

        public void CreateGetState()
        {
            CodeMemberMethod doGetState = new CodeMemberMethod();
            doGetState.Name = "GetCurrentState";
            doGetState.ReturnType = new CodeTypeReference("System.String");
            doGetState.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            if (_language == "c#")
            {
                string mstart = "";
                mstart += "#if USE_RUNDB\r\n";
                mstart += string.Format("          sm_{0}.smRunDataContext smdc = new sm_{0}.smRunDataContext();\r\n", _smName);
                mstart += "          if (smuse_db == true && smdc.DatabaseExists() == true)\r\n";
                mstart += "          {\r\n";
                mstart += "               var qq = from p in smdc.smInstances\r\n";
                mstart += "                     where p.smInstanceID == instance_id\r\n";
                mstart += "                     select p;\r\n";
                mstart += "            foreach (var vv in qq)\r\n";
                mstart += "            {\r\n";
                mstart += "               if (vv.smName == sm_name)\r\n";
                mstart += "               {\r\n";
                mstart += "                   sm_currentstate = vv.smCurrentState;\r\n";
                mstart += "               }\r\n";
                mstart += "            }\r\n";
                mstart += "          }\r\n";
                mstart += "#endif\r\n";
                CodeSnippetStatement cne = new CodeSnippetStatement(mstart);
                doGetState.Statements.Add(cne);
            }

            CodeFieldReferenceExpression anexpr = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_currentstate");
            doGetState.Statements.Add(new CodeMethodReturnStatement(anexpr));
            doGetState.Comments.Add(new CodeCommentStatement("Returns the current state "));
            doGetState.Comments.Add(new CodeCommentStatement("In c# will add linq to keep state the database if USE_RUNDB is defined"));
            MachineClassType.Members.Add(doGetState);
        }

        public void CreateSetState()
        {
            CodeMemberMethod doSetState = new CodeMemberMethod();
            doSetState.Name = "SetCurrentState";
            doSetState.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "st"));
            doSetState.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            if (_language == "c#")
            {
                string mstart = "";
                mstart += "#if USE_RUNDB\r\n";
                mstart += string.Format("          sm_{0}.smRunDataContext smdc = new sm_{0}.smRunDataContext();\r\n", _smName);
                mstart += "          if (smuse_db == true && smdc.DatabaseExists() == true)\r\n";
                mstart += "          {\r\n";
                mstart += "               var qq = from p in smdc.smInstances\r\n";
                mstart += "                     where p.smInstanceID == instance_id\r\n";
                mstart += "                     select p;\r\n";
                mstart += "            foreach (var vv in qq)\r\n";
                mstart += "            {\r\n";
                mstart += "               if (vv.smName == sm_name)\r\n";
                mstart += "               {\r\n";
                mstart += "                   vv.smCurrentState = st;\r\n";
                mstart += "                   vv.smLastTranTime = DateTime.Now;\r\n";
                mstart += "                   smdc.SubmitChanges();\r\n";
                mstart += "               }\r\n";
                mstart += "            }\r\n";
                mstart += "          }\r\n";
                mstart += "#endif\r\n";
                CodeSnippetStatement cne = new CodeSnippetStatement(mstart);
                doSetState.Statements.Add(cne);
            }

            CodeFieldReferenceExpression anexpr = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_currentstate");
            CodeAssignStatement asst = new CodeAssignStatement(anexpr, new CodeArgumentReferenceExpression("st"));
            doSetState.Statements.Add(asst);
            doSetState.Comments.Add(new CodeCommentStatement("Returns the current state "));
            doSetState.Comments.Add(new CodeCommentStatement("In c# will add linq to keep state in a database if USE_RUNDB is defined"));
            MachineClassType.Members.Add(doSetState);
        }

        public void CreateEmptyMachineConstructor()
        {
            CodeConstructor doMachine = new CodeConstructor();
            doMachine.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            CodeAssignStatement asdb = new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "smuse_db"), new CodePrimitiveExpression(false));
            doMachine.Statements.Add(asdb);

            CodeObjectCreateExpression rndcreate = new CodeObjectCreateExpression("System.Random", new CodeExpression[] { });
            CodeVariableDeclarationStatement isrnd = new CodeVariableDeclarationStatement(
                  "System.Random",
                  "rnd",
                  rndcreate);
            doMachine.Statements.Add(isrnd);

            CodeAssignStatement asinst = new CodeAssignStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "instance_id"),
                new CodeMethodInvokeExpression(
                     new CodeVariableReferenceExpression("rnd"),
                     "Next",
                     new CodeExpression[] { }
                     )
                );
            doMachine.Statements.Add(asinst);

            CodeAssignStatement asguid = new CodeAssignStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_instguid"),
                new CodeMethodInvokeExpression(
                     new CodeTypeReferenceExpression("System.Guid"),
                     "NewGuid",
                     new CodeExpression[] { }
                     )
                );
            doMachine.Statements.Add(asguid);



            CodeAssignStatement asid = new CodeAssignStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_id"), new CodePrimitiveExpression(_smId));
            doMachine.Statements.Add(asid);

            CodeAssignStatement ascurrent = new CodeAssignStatement(
               new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_currentstate"), new CodePrimitiveExpression(_smInitialstate));
            doMachine.Statements.Add(ascurrent);

            foreach (KeyValuePair<string, Dictionary<string, Tranval>> tran in _transitions)
            {
                string st = tran.Key;
                //    string ehash = string.Format("{0}evnthash", st);
                // CodeObjectCreateExpression ehashcreate = new CodeObjectCreateExpression("System.Collections.Hashtable", new CodeExpression[] { });

                //CodeAssignStatement ecreate = new CodeAssignStatement(
                //    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), ehash),
                //    ehashcreate);
                string edict = string.Format("{0}EventDictionary", st);
                //   CodeObjectCreateExpression edictcreate = new CodeObjectCreateExpression("Dictionary<string, StateAction>", new CodeExpression[] { });
                CodeObjectCreateExpression edictcreate = new CodeObjectCreateExpression("Dictionary<string, object>", new CodeExpression[] { });
                CodeAssignStatement ecreate = new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), edict),
                    edictcreate);

                doMachine.Statements.Add(ecreate);

                // string fhash = string.Format("{0}finalhash", st);
                //   CodeObjectCreateExpression fhashcreate = new CodeObjectCreateExpression("System.Collections.Hashtable", new CodeExpression[] { });
                //CodeAssignStatement fcreate = new CodeAssignStatement(
                //    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fhash),
                //    fhashcreate);

                string fdict = string.Format("{0}FinalDictionary", st);
                CodeObjectCreateExpression fdictcreate = new CodeObjectCreateExpression("Dictionary<string, string>", new CodeExpression[] { });
                CodeAssignStatement fcreate = new CodeAssignStatement(
                 new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fdict),
                 fdictcreate);
                doMachine.Statements.Add(fcreate);
            }

            //string shash = string.Format("{0}statehash", sm_name);
            //CodeObjectCreateExpression shashcreate = new CodeObjectCreateExpression("System.Collections.Hashtable", new CodeExpression[] { });
            //CodeAssignStatement screate = new CodeAssignStatement(
            //    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), shash),
            //    shashcreate);


            string sdict = string.Format("{0}StateDictionary", _smName);
            //   CodeObjectCreateExpression sdictcreate = new CodeObjectCreateExpression("Dictionary<string, StateEvent>", new CodeExpression[] { });
            CodeObjectCreateExpression sdictcreate = new CodeObjectCreateExpression("Dictionary<string, object>", new CodeExpression[] { });
            CodeAssignStatement screate = new CodeAssignStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), sdict),
                sdictcreate);
            doMachine.Statements.Add(screate);

            string nev = string.Format("{0}_ev", _smName);
            string acev = string.Format("{0}ActionsEvents", _smName);

            CodeObjectCreateExpression ssh = new CodeObjectCreateExpression(
                acev,
                new CodeExpression[]
                {
                   new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_id"),
                   new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "instance_id"),
                   new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_instguid")
                });

            CodeAssignStatement aev = new CodeAssignStatement(
               new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), nev),
               ssh);

            doMachine.Statements.Add(aev);

            // private StarShipTestEvntsReady StarShipInputReadyNow;

            string bnev = string.Format("{0}InputReadyNow", _smName);
            string bacev = string.Format("{0}TestEventsReady", _smName);

            CodeObjectCreateExpression bssh = new CodeObjectCreateExpression(
                bacev,
                new CodeExpression[]
                {
                   new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_id"),
                   new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "instance_id"),
                   new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_instguid")
                });

            CodeAssignStatement baev = new CodeAssignStatement(
               new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), bnev),
               bssh);

            doMachine.Statements.Add(baev);





            //  CodeMethodInvokeExpression addhashs = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "BuildStateEventToActionsHash", new CodeExpression[] { });
            //  doMachine.Statements.Add(addhashs);

            CodeMethodInvokeExpression adddicts = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "BuildStateEventToActionsDictionary", new CodeExpression[] { });
            doMachine.Statements.Add(adddicts);

            doMachine.Comments.Add(new CodeCommentStatement("The empty constructor will not use the database for state storage"));
            MachineClassType.Members.Add(doMachine);
        }

        public void CreateBoolMachineConstructor()
        {
            CodeConstructor doMachine = new CodeConstructor();
            doMachine.Parameters.Add(new CodeParameterDeclarationExpression("System.Boolean", "use_db"));
            doMachine.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            CodeAssignStatement asdb = new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "smuse_db"), new CodeArgumentReferenceExpression("use_db"));
            doMachine.Statements.Add(asdb);

            CodeObjectCreateExpression rndcreate = new CodeObjectCreateExpression("System.Random", new CodeExpression[] { });
            CodeVariableDeclarationStatement isrnd = new CodeVariableDeclarationStatement(
                  "System.Random",
                  "rnd",
                  rndcreate);
            doMachine.Statements.Add(isrnd);

            CodeAssignStatement asinst = new CodeAssignStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "instance_id"),
                new CodeMethodInvokeExpression(
                     new CodeVariableReferenceExpression("rnd"),
                     "Next",
                     new CodeExpression[] { }
                     )
                );
            doMachine.Statements.Add(asinst);

            CodeAssignStatement asguid = new CodeAssignStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_instguid"),
                new CodeMethodInvokeExpression(
                     new CodeTypeReferenceExpression("System.Guid"),
                     "NewGuid",
                     new CodeExpression[] { }
                     )
                );
            doMachine.Statements.Add(asguid);

            CodeAssignStatement asid = new CodeAssignStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_id"), new CodePrimitiveExpression(_smId));
            doMachine.Statements.Add(asid);

            CodeAssignStatement ascurrent = new CodeAssignStatement(
               new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_currentstate"), new CodePrimitiveExpression(_smInitialstate));
            doMachine.Statements.Add(ascurrent);

            foreach (KeyValuePair<string, Dictionary<string, Tranval>> tran in _transitions)
            {
                string st = tran.Key;
                //string ehash = string.Format("{0}evnthash", st);

                //CodeObjectCreateExpression ehashcreate = new CodeObjectCreateExpression("System.Collections.Hashtable", new CodeExpression[] { });
                //CodeAssignStatement ecreate = new CodeAssignStatement(
                //    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), ehash),
                //    ehashcreate);

                string edict = string.Format("{0}EventDictionary", st);
                //  CodeObjectCreateExpression edictcreate = new CodeObjectCreateExpression("Dictionary<string, StateAction>", new CodeExpression[] { });
                CodeObjectCreateExpression edictcreate = new CodeObjectCreateExpression("Dictionary<string, object>", new CodeExpression[] { });
                CodeAssignStatement ecreate = new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), edict),
                    edictcreate);
                doMachine.Statements.Add(ecreate);

                //string fhash = string.Format("{0}finalhash", st);
                //CodeObjectCreateExpression fhashcreate = new CodeObjectCreateExpression("System.Collections.Hashtable", new CodeExpression[] { });
                //CodeAssignStatement fcreate = new CodeAssignStatement(
                //    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fhash),
                //    fhashcreate);


                string fdict = string.Format("{0}FinalDictionary", st);
                CodeObjectCreateExpression fdictcreate = new CodeObjectCreateExpression("Dictionary<string, string>", new CodeExpression[] { });
                CodeAssignStatement fcreate = new CodeAssignStatement(
                 new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fdict),
                 fdictcreate);

                doMachine.Statements.Add(fcreate);
            }
            //string shash = string.Format("{0}statehash", sm_name);
            //CodeObjectCreateExpression shashcreate = new CodeObjectCreateExpression("System.Collections.Hashtable", new CodeExpression[] { });
            //CodeAssignStatement screate = new CodeAssignStatement(
            //    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), shash),
            //    shashcreate);

            string sdict = string.Format("{0}StateDictionary", _smName);
            //   CodeObjectCreateExpression sdictcreate = new CodeObjectCreateExpression("Dictionary<string, StateEvent>", new CodeExpression[] { });
            CodeObjectCreateExpression sdictcreate = new CodeObjectCreateExpression("Dictionary<string, object>", new CodeExpression[] { });
            CodeAssignStatement screate = new CodeAssignStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), sdict),
                sdictcreate);
            doMachine.Statements.Add(screate);

            string mstart = "";
            if (_language == "C#")
            {
                mstart += "#if USE_RUNDB\r\n";
                mstart += "         if(usedb)\r\n";
                mstart += "          {\r\n";
                mstart += "  //    change the database to the the location desired, it will be created if it does not exist\r\n";
                mstart += "  //        string conn = \"Data Source=DRAGON-NAP\\SQLEXPRESS;Initial Catalog=sm_run;Integrated Security=True;Pooling=False\";\r\n";
                mstart += string.Format("            sm_{0}.smRunDataContext smdc = new sm_{0}.smRunDataContext();\r\n", _smName);
                mstart += "            if (smdc.DatabaseExists() == false)\r\n";
                mstart += "            {\r\n";
                mstart += "                 smdc.CreateDatabase();\r\n";
                mstart += "            }\r\n";
                mstart += "            System.Nullable<Int64> qe = (from p in smdc.smInstances\r\n";
                mstart += "              where p.smID == sm_id\r\n";
                mstart += "              select p.smInstanceID)\r\n";
                mstart += "              .Max();\r\n";
                mstart += "            if(qe.HasValue)\r\n";
                mstart += "            {\r\n";
                mstart += "                instance_id = qe.Value;\r\n";
                mstart += "                GetCurrentState();\r\n";
                mstart += "            }\r\n";
                mstart += "            else\r\n";
                mstart += "            {\r\n";
                mstart += "                DateTime starttime = DateTime.Now;\r\n";
                mstart += "                smInstance sinst = new smInstance();\r\n";
                mstart += string.Format("                sinst.smName = \"{0}\"; \r\n", _smName);
                mstart += string.Format("                sinst.smLastTranTime = starttime;\r\n");
                mstart += string.Format("                sinst.smID = {0}; \r\n", _smId);
                mstart += string.Format("                sinst.smCurrentState = \"{0}\";\r\n", _smInitialstate);
                mstart += "                sm_currentstate = sinst.smCurrentState;\r\n";
                mstart += string.Format("                sinst.smDataTableName = \"{0}\"; \r\n", "unknown");
                mstart += "                smdc.smInstances.InsertOnSubmit(sinst);\r\n";
                mstart += "                smdc.SubmitChanges();\r\n";
                mstart += "                var qq = from p in smdc.smInstances\r\n";
                mstart += "                       where p.smLastTranTime == starttime\r\n";
                mstart += "                       select p.smInstanceID;\r\n";
                mstart += "                instance_id = qq.ToList()[0];\r\n";
                mstart += "            }\r\n";
                mstart += "         }\r\n";
                mstart += "#endif\r\n";

                CodeSnippetStatement cne = new CodeSnippetStatement(mstart);
                doMachine.Statements.Add(cne);
            }

            string nev = string.Format("{0}_ev", _smName);
            string acev = string.Format("{0}ActionsEvents", _smName);

            CodeObjectCreateExpression ssh = new CodeObjectCreateExpression(
                acev,
                new CodeExpression[]
                {
                   new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_id"),
                   new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "instance_id"),
                   new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_instguid")
                });

            CodeAssignStatement asshash = new CodeAssignStatement(
               new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), nev),
               ssh);

            doMachine.Statements.Add(asshash);

            string bnev = string.Format("{0}InputReadyNow", _smName);
            string bacev = string.Format("{0}TestEventsReady", _smName);

            CodeObjectCreateExpression bssh = new CodeObjectCreateExpression(
                bacev,
                new CodeExpression[]
                {
                   new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_id"),
                   new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "instance_id"),
                   new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sm_instguid")
                });

            CodeAssignStatement baev = new CodeAssignStatement(
               new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), bnev),
               bssh);

            doMachine.Statements.Add(baev);

            //  CodeMethodInvokeExpression addhashs = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "BuildStateEventToActionsHash", new CodeExpression[] { });
            // doMachine.Statements.Add(addhashs);

            CodeMethodInvokeExpression adddicts = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "BuildStateEventToActionsDictionary", new CodeExpression[] { });
            doMachine.Statements.Add(adddicts);

            doMachine.Comments.Add(new CodeCommentStatement("The bool constructor can use the database for state storage if use_db is true"));
            MachineClassType.Members.Add(doMachine);
        }


        public void MachineMembers()
        {
            CreateEmptyMachineConstructor();
            CreateBoolMachineConstructor();
            CreateGetState();
            CreateSetState();
            MachineDictionary();
            CreateStateActionHandler();
            CreateInputEventHandler();
            CreateRunEventFromState();
        }

        #endregion

        public static String GenerateCSharpCode(CodeCompileUnit compileunit)
        {
            // Generate the code with the C# code provider.
            CSharpCodeProvider provider = new CSharpCodeProvider();

            // Build the output file name.
            String sourceFile;
            if (provider.FileExtension[0] == '.')
            {
                sourceFile = "Testout" + provider.FileExtension;
            }
            else
            {
                sourceFile = "testout." + provider.FileExtension;
            }

            // Create a TextWriter to a StreamWriter to the output file.
            IndentedTextWriter tw = new IndentedTextWriter(
                    new StreamWriter(sourceFile, false), "    ");

            // Generate source code using the code provider.
            provider.GenerateCodeFromCompileUnit(compileunit, tw,
                    new CodeGeneratorOptions());

            // Close the output file.
            tw.Close();

            return sourceFile;
        }

        #region TestEvents 

        public void TestEventsReadyDictionary()
        {
            CodeMemberMethod domach = new CodeMemberMethod();
            //   domach.Name = "BuildStateTestEvntsReadyHash";
            domach.Name = "BuildStateTestEventsReadyDictionary";
            //    string ststatehash = "test_eventsready";

            string ststatedict = "_testEventsReady";
            foreach (KeyValuePair<string, Dictionary<string, Tranval>> tran in _transitions)
            {
                string st = tran.Key;
                string fistatemeth = string.Format("Run{0}CheckEventReady", st);

                CodeObjectCreateExpression evarg = new CodeObjectCreateExpression();
                evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                 fistatemeth
                 ));

                evarg.CreateType = new CodeTypeReference(_smName + "TestEventsReadyHandler");

                CodeExpression[] ac = new CodeExpression[] { new CodePrimitiveExpression(st), evarg };

                //    CodeFieldReferenceExpression hashfield = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), ststatehash);
                //   CodeMethodInvokeExpression exm = new CodeMethodInvokeExpression(hashfield, "Add", ac);
                CodeFieldReferenceExpression dictfield = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), ststatedict);
                CodeMethodInvokeExpression exm = new CodeMethodInvokeExpression(dictfield, "Add", ac);


                domach.Statements.Add(exm);
            }
            //  domach.Comments.Add(new CodeCommentStatement("Fills the Test For Event hashtable"));
            domach.Comments.Add(new CodeCommentStatement("Fills the Test For Event Dictionary"));
            TestEventsReadyClassType.Members.Add(domach);
        }

        public void TestEventsListDictionary()
        {
            CodeMemberMethod domach = new CodeMemberMethod();
            //   domach.Name = "BuildListEvntsHash";
            domach.Name = "BuildListEventsDictionary";
            string ststatedict = "_listEvents";
            foreach (KeyValuePair<string, Dictionary<string, Tranval>> tran in _transitions)
            {
                string st = tran.Key;
                string fistatemeth = string.Format("Run{0}ListEvent", st);

                CodeObjectCreateExpression evarg = new CodeObjectCreateExpression();
                evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                 fistatemeth
                 ));

                evarg.CreateType = new CodeTypeReference(_smName + "ListEventsHandlerDelegate");

                CodeExpression[] ac = new CodeExpression[] { new CodePrimitiveExpression(st), evarg };
                //CodeFieldReferenceExpression hashfield = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), ststatehash);
                //CodeMethodInvokeExpression exm = new CodeMethodInvokeExpression(hashfield, "Add", ac);
                CodeFieldReferenceExpression dictfield = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), ststatedict);
                CodeMethodInvokeExpression exm = new CodeMethodInvokeExpression(dictfield, "Add", ac);
                domach.Statements.Add(exm);
            }
            //     domach.Comments.Add(new CodeCommentStatement("Fills the List Events for state methods hashtable"));
            domach.Comments.Add(new CodeCommentStatement("Fills the List Events for state methods dictionary"));
            TestEventsReadyClassType.Members.Add(domach);
        }

        public void CreateTestEventsReadyConstructor()
        {
            CodeConstructor doMachine = new CodeConstructor();
            doMachine.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            doMachine.Parameters.Add(new CodeParameterDeclarationExpression("System.Int64", "smid"));
            doMachine.Parameters.Add(new CodeParameterDeclarationExpression("System.Int64", "sminst"));
            doMachine.Parameters.Add(new CodeParameterDeclarationExpression("System.Guid", "sminstguid"));

            doMachine.Statements.Add(CreateFieldAssign("_sm_id", "smid"));
            doMachine.Statements.Add(CreateFieldAssign("_sm_inst", "sminst"));
            doMachine.Statements.Add(CreateFieldAssign("_sm_instguid", "sminstguid"));


            //  string ehash = "test_eventsready";

            //CodeObjectCreateExpression ehashcreate = new CodeObjectCreateExpression("System.Collections.Hashtable", new CodeExpression[] { });
            //CodeAssignStatement ecreate = new CodeAssignStatement(
            //    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), ehash),
            //    ehashcreate);

            string edict = "_testEventsReady";
            //   CodeObjectCreateExpression edictcreate = new CodeObjectCreateExpression("Dictionary<string, TestEventReadyHandler>", new CodeExpression[] { });
            CodeObjectCreateExpression edictcreate = new CodeObjectCreateExpression("Dictionary<string, object>", new CodeExpression[] { });
            CodeAssignStatement ecreate = new CodeAssignStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), edict),
                edictcreate);
            doMachine.Statements.Add(ecreate);

            // string zehash = "list_events";

            string zedict = "_listEvents";

            //CodeObjectCreateExpression zehashcreate = new CodeObjectCreateExpression("System.Collections.Hashtable", new CodeExpression[] { });
            //CodeAssignStatement zecreate = new CodeAssignStatement(
            //    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), zehash),
            //    zehashcreate);

            CodeObjectCreateExpression zedictcreate = new CodeObjectCreateExpression("Dictionary<string, object>", new CodeExpression[] { });
            CodeAssignStatement zecreate = new CodeAssignStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), zedict),
                zedictcreate);


            doMachine.Statements.Add(zecreate);

            //   CodeMethodInvokeExpression addhashs = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "BuildStateTestEvntsReadyHash", new CodeExpression[] { });

            CodeMethodInvokeExpression adddicts = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "BuildStateTestEventsReadyDictionary", new CodeExpression[] { });
            doMachine.Statements.Add(adddicts);

            //  CodeMethodInvokeExpression zaddhashs = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "BuildListEvntsHash", new CodeExpression[] { });

            CodeMethodInvokeExpression zadddicts = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "BuildListEventsDictionary", new CodeExpression[] { });
            doMachine.Statements.Add(zadddicts);
            TestEventsReadyClassType.Members.Add(doMachine);
        }

        public void CreateEventReadyHandlers()
        {
            foreach (KeyValuePair<string, Dictionary<string, Tranval>> tran in _transitions)
            {
                string st = tran.Key;
                string ststatehash = string.Format("{0}testfor", st);

                string fistatemeth = string.Format("Run{0}CheckEventReady", st);

                CodeMemberEvent event1 = new CodeMemberEvent();
                event1.Name = string.Format("{0}CheckEventReady", st);
                event1.Type = new CodeTypeReference(_smName + "TestStateEventsReady");
                event1.Attributes = MemberAttributes.Public;
                TestEventsReadyClassType.Members.Add(event1);

                string inputname = string.Format("Run{0}CheckEventReady", st);
                string inputeventhand = _smName + "TestStateEventsReady";
                string inputevent = string.Format("{0}CheckEventReady", st);
                string inputargs = string.Format("{0}TestEventsReadyArgs", _smName);

                CodeMemberMethod doactionevent = new CodeMemberMethod();
                doactionevent.Name = inputname;
                doactionevent.ReturnType = new CodeTypeReference("System.String");
                doactionevent.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "st"));
                doactionevent.Attributes = MemberAttributes.Public | MemberAttributes.Final;

                CodeVariableDeclarationStatement declare_ehv = new CodeVariableDeclarationStatement(
                   inputeventhand,
                   "ehv",
                   new CodeEventReferenceExpression(new CodeThisReferenceExpression(), inputevent));

                doactionevent.Statements.Add(declare_ehv);

                CodeConditionStatement outif = new CodeConditionStatement();  // if
                CodeBinaryOperatorExpression ehvtest = new CodeBinaryOperatorExpression();  // (ehv != null)
                ehvtest.Right = new CodePrimitiveExpression(null);
                ehvtest.Left = new CodeVariableReferenceExpression("ehv");
                ehvtest.Operator = CodeBinaryOperatorType.IdentityInequality;
                outif.Condition = ehvtest;

                CodeObjectCreateExpression CreateEventReadyEvent = new CodeObjectCreateExpression(inputargs,
                  new CodeExpression[]
                   {
                         new CodeArgumentReferenceExpression("st"),
                         new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_sm_id"),
                         new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_sm_inst"),
                         new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_sm_instguid"),
                   });

                CodeVariableDeclarationStatement setupe = new CodeVariableDeclarationStatement(
                inputargs,
                "e",
                CreateEventReadyEvent);

                outif.TrueStatements.Add(setupe);

                CodeDelegateInvokeExpression invokeehv = new CodeDelegateInvokeExpression(new CodeVariableReferenceExpression("ehv"),
                  new CodeExpression[]
                 {
                         new CodeThisReferenceExpression(),
                         new CodeVariableReferenceExpression("e")
                 });

                CodeVariableDeclarationStatement setupgo = new CodeVariableDeclarationStatement(
                "System.String",
                "go",
                invokeehv);

                outif.TrueStatements.Add(setupgo);
                outif.TrueStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("go")));

                doactionevent.Statements.Add(outif);
                doactionevent.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
                doactionevent.Comments.Add(new CodeCommentStatement(string.Format("Designed to test for ready events in state {0} ", tran.Key)));
                TestEventsReadyClassType.Members.Add(doactionevent);
            }
        }

        public void CreateListEventHandlers()
        {

            foreach (KeyValuePair<string, Dictionary<string, Tranval>> tran in _transitions)
            {
                string st = tran.Key;
                Dictionary<string, Tranval> tv = tran.Value;

                string fistatemeth = string.Format("Run{0}ListEvent", st);

                CodeMemberMethod doactionevent = new CodeMemberMethod();
                doactionevent.Name = fistatemeth;
                doactionevent.ReturnType = new CodeTypeReference("Dictionary<string, int>");
                doactionevent.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "st"));
                doactionevent.Attributes = MemberAttributes.Public | MemberAttributes.Final;

                CodeVariableDeclarationStatement declare_ehv = new CodeVariableDeclarationStatement(
                   typeof(Dictionary<string, int>),
                   "retDictionary",
                   new CodeObjectCreateExpression(typeof(Dictionary<string, int>), new CodeExpression[] { }));

                doactionevent.Statements.Add(declare_ehv);
                int ii = 0;

                foreach (KeyValuePair<string, Tranval> tt in tv)
                {
                    doactionevent.Statements.Add(new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("retDictionary"),
                        "Add",
                        new CodeExpression[] { new CodePrimitiveExpression(tt.Key), new CodePrimitiveExpression(ii) }));
                    ii++;
                }

                doactionevent.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("retDictionary")));
                doactionevent.Comments.Add(new CodeCommentStatement(string.Format("Designed to test for ready events in state {0} ", tran.Key)));
                TestEventsReadyClassType.Members.Add(doactionevent);
            }
        }

        public void CreateEventsReadyFromState()
        {
            string dictname = "_testEventsReady";

            CodeMemberMethod doFromState = new CodeMemberMethod();
            doFromState.Name = "EventsReadyFromState";
            doFromState.ReturnType = new CodeTypeReference("System.String");
            doFromState.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "st"));
            doFromState.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            CodeArrayIndexerExpression hasv = new CodeArrayIndexerExpression(new CodeVariableReferenceExpression(dictname),
                new CodeArgumentReferenceExpression("st"));
            CodeCastExpression castExpression = new CodeCastExpression(_smName + "TestEventsReadyHandler", hasv);
            CodeVariableDeclarationStatement declare_sev = new CodeVariableDeclarationStatement(
               _smName + "TestEventsReadyHandler",
               "sst",
               castExpression);

            doFromState.Statements.Add(declare_sev);

            CodeConditionStatement outif = new CodeConditionStatement();  // if
            CodeBinaryOperatorExpression sevtest = new CodeBinaryOperatorExpression();  // (sev != null)
            sevtest.Right = new CodePrimitiveExpression(null);
            sevtest.Left = new CodeVariableReferenceExpression("sst");
            sevtest.Operator = CodeBinaryOperatorType.IdentityInequality;
            outif.Condition = sevtest;

            CodeDelegateInvokeExpression invokesev = new CodeDelegateInvokeExpression(new CodeVariableReferenceExpression("sst"),
                     new CodeExpression[]
                     {
                         new CodeArgumentReferenceExpression("st")
                     });

            outif.TrueStatements.Add(new CodeMethodReturnStatement(invokesev));

            doFromState.Statements.Add(outif);
            doFromState.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));

            TestEventsReadyClassType.Members.Add(doFromState);
        }

        public void CreateListEventsFromState()
        {
            string dictname = "_listEvents";

            CodeMemberMethod doFromState = new CodeMemberMethod();
            doFromState.Name = "ListEventsFromState";
            doFromState.ReturnType = new CodeTypeReference("Dictionary<string, int>");
            doFromState.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "st"));
            doFromState.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            CodeArrayIndexerExpression hasv = new CodeArrayIndexerExpression(new CodeVariableReferenceExpression(dictname),
                new CodeArgumentReferenceExpression("st"));
            CodeCastExpression castExpression = new CodeCastExpression(_smName + "ListEventsHandlerDelegate", hasv);
            CodeVariableDeclarationStatement declare_sev = new CodeVariableDeclarationStatement(
               _smName + "ListEventsHandlerDelegate",
               "sst",
               castExpression);

            doFromState.Statements.Add(declare_sev);

            CodeConditionStatement outif = new CodeConditionStatement();  // if
            CodeBinaryOperatorExpression sevtest = new CodeBinaryOperatorExpression();  // (sev != null)
            sevtest.Right = new CodePrimitiveExpression(null);
            sevtest.Left = new CodeVariableReferenceExpression("sst");
            sevtest.Operator = CodeBinaryOperatorType.IdentityInequality;
            outif.Condition = sevtest;

            CodeDelegateInvokeExpression invokesev = new CodeDelegateInvokeExpression(new CodeVariableReferenceExpression("sst"),
                     new CodeExpression[]
                     {
                         new CodeArgumentReferenceExpression("st")
                     });

            outif.TrueStatements.Add(new CodeMethodReturnStatement(invokesev));

            doFromState.Statements.Add(outif);
            doFromState.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));

            TestEventsReadyClassType.Members.Add(doFromState);
        }

        public void GenerateTestEventsReady()
        {
            TestEventsReadyUnit = new CodeCompileUnit();
            CodeNamespace Machine = new CodeNamespace("sm_" + _smName);
            Machine.Imports.Add(new CodeNamespaceImport("System"));
            Machine.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            if (_language == "c#")
            {
                Machine.Imports.Add(new CodeNamespaceImport("System.Linq"));
            }
            Machine.Imports.Add(new CodeNamespaceImport("System.Text"));
            Machine.Imports.Add(new CodeNamespaceImport("System.IO"));

            CodeTypeDelegate StateEventsReadyHandler = new CodeTypeDelegate(_smName + "TestStateEventsReady");
            StateEventsReadyHandler.ReturnType = new CodeTypeReference(typeof(System.String));
            StateEventsReadyHandler.Parameters.Add(new CodeParameterDeclarationExpression("System.Object", "sender"));
            StateEventsReadyHandler.Parameters.Add(new CodeParameterDeclarationExpression(_smName + "TestEventsReadyArgs", "e"));
            Machine.Types.Add(StateEventsReadyHandler);

            CodeTypeDelegate TestEventsReadyHandler = new CodeTypeDelegate(_smName + "TestEventsReadyHandler");
            TestEventsReadyHandler.ReturnType = new CodeTypeReference(typeof(System.String));
            TestEventsReadyHandler.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "st"));
            Machine.Types.Add(TestEventsReadyHandler);

            CodeTypeDelegate ListEventsHandler = new CodeTypeDelegate(_smName + "ListEventsHandlerDelegate");
            ListEventsHandler.ReturnType = new CodeTypeReference(typeof(Dictionary<string, int>));
            ListEventsHandler.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "st"));
            Machine.Types.Add(ListEventsHandler);

            TestEventsReadyClassType = new CodeTypeDeclaration(_smName + "TestEventsReady");
            TestEventsReadyClassType.TypeAttributes = TypeAttributes.Public;
            TestEventsReadyClassType.Attributes = MemberAttributes.Private;
            TestEventsReadyClassType.IsClass = true;
            Machine.Types.Add(TestEventsReadyClassType);
            TestEventsReadyUnit.Namespaces.Add(Machine);

            TestEventsReadyClassType.Members.Add(CreateField(typeof(System.Int64), "_sm_id", "The state machine id."));
            TestEventsReadyClassType.Members.Add(CreateField(typeof(System.Int64), "_sm_inst", "The state machine instance."));
            TestEventsReadyClassType.Members.Add(CreateField(typeof(System.Guid), "_sm_instguid", "The state machine instance Guid."));

            //TestEvntsReadyClassType.Members.Add(CreateField(typeof(System.Collections.Hashtable),
            //    "test_eventsready", 
            //    string.Format("Hastable methods to determine if events are ready for each state")));

            //TestEvntsReadyClassType.Members.Add(CreateField(typeof(System.Collections.Hashtable),
            //    "list_events",
            //    string.Format("Hastable methods to list events for each state")));

            // TestEvntsReadyClassType.Members.Add(CreateField(typeof(Dictionary<string, ATestEvntsReadyHandler> ),
            TestEventsReadyClassType.Members.Add(CreateField(typeof(Dictionary<string, object>),
                "_testEventsReady",
                string.Format("Dictionary methods to determine if events are ready for each state")));

            //  TestEventsReadyClassType.Members.Add(CreateField(typeof(Dictionary<string, ATestEvntsReadyHandler>),
            TestEventsReadyClassType.Members.Add(CreateField(typeof(Dictionary<string, object>),
                "_listEvents",
                string.Format("Dictionary methods to list events for each state")));



            CreateTestEventsReadyConstructor();
            TestEventsReadyDictionary();
            TestEventsListDictionary();
            CreateEventReadyHandlers();
            CreateEventsReadyFromState();
            CreateListEventHandlers();
            CreateListEventsFromState();

            GenerateCode(TestEventsReadyFileName, TestEventsReadyUnit);
        }

        public void GenerateTestEventsReadyData()
        {
            TestEventsReadyDataUnit = new CodeCompileUnit();
            CodeNamespace Machine = new CodeNamespace("sm_" + _smName);
            Machine.Imports.Add(new CodeNamespaceImport("System"));
            Machine.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            if (_language == "c#")
            {
                Machine.Imports.Add(new CodeNamespaceImport("System.Linq"));
            }
            Machine.Imports.Add(new CodeNamespaceImport("System.Text"));
            Machine.Imports.Add(new CodeNamespaceImport("System.IO"));

            TestEventsReadyDataClassType = new CodeTypeDeclaration(_smName + "TestEventsReadyArgs");
            TestEventsReadyDataClassType.TypeAttributes = TypeAttributes.Public;
            TestEventsReadyDataClassType.Attributes = MemberAttributes.Private;
            TestEventsReadyDataClassType.IsClass = true;
            TestEventsReadyDataClassType.BaseTypes.Add("EventArgs");
            Machine.Types.Add(TestEventsReadyDataClassType);
            TestEventsReadyDataUnit.Namespaces.Add(Machine);

            //           TestEvntsReadyDataClassType.Members.Add(CreateField(typeof(System.String), "_eventname", "The Event Name."));
            TestEventsReadyDataClassType.Members.Add(CreateField(typeof(System.String), "_statename", "The State Name."));
            TestEventsReadyDataClassType.Members.Add(CreateField(typeof(System.Int64), "_sm_id", "The state machine id."));
            TestEventsReadyDataClassType.Members.Add(CreateField(typeof(System.Int64), "_sm_inst", "The state machine instance."));
            TestEventsReadyDataClassType.Members.Add(CreateField(typeof(System.Guid), "_sm_instguid", "The state machine instance Guid."));

            CodeConstructor TestEventsReadyDataConstructor = new CodeConstructor();
            TestEventsReadyDataConstructor.Attributes = MemberAttributes.Public;
            //           TestEvntsReadyDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "evnt"));
            TestEventsReadyDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "state"));
            TestEventsReadyDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.Int64", "smid"));
            TestEventsReadyDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.Int64", "inst"));
            TestEventsReadyDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.Guid", "instguid"));
            //         TestEvntsReadyDataConstructor.Statements.Add(CreateFieldAssign("_eventname", "evnt"));
            TestEventsReadyDataConstructor.Statements.Add(CreateFieldAssign("_statename", "state"));
            TestEventsReadyDataConstructor.Statements.Add(CreateFieldAssign("_sm_id", "smid"));
            TestEventsReadyDataConstructor.Statements.Add(CreateFieldAssign("_sm_inst", "inst"));
            TestEventsReadyDataConstructor.Statements.Add(CreateFieldAssign("_sm_instguid", "instguid"));
            TestEventsReadyDataClassType.Members.Add(TestEventsReadyDataConstructor);

            //        TestEvntsReadyDataClassType.Members.Add(CreateFieldProperty(typeof(System.String), "_eventname", "EventName", "The Tested Event Name"));
            TestEventsReadyDataClassType.Members.Add(CreateFieldProperty(typeof(System.String), "_statename", "StateName", "The Current State Name"));
            TestEventsReadyDataClassType.Members.Add(CreateFieldProperty(typeof(System.Int64), "_sm_id", "Sm_ID", "The State machine type id"));
            TestEventsReadyDataClassType.Members.Add(CreateFieldProperty(typeof(System.Int64), "_sm_inst", "Sm_Instance", "The State machine instance id"));
            TestEventsReadyDataClassType.Members.Add(CreateFieldProperty(typeof(System.Guid), "_sm_instguid", "Sm_InstGuid", "The State machine instance Guid"));

            GenerateCode(TestEventsReadyDataFileName, TestEventsReadyDataUnit);
        }

        #endregion

        #region imput event data


        public void GenerateInputEventData()
        {
            InputEventDataUnit = new CodeCompileUnit();
            CodeNamespace Machine = new CodeNamespace("sm_" + _smName);
            Machine.Imports.Add(new CodeNamespaceImport("System"));
            Machine.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            if (_language == "c#")
            {
                Machine.Imports.Add(new CodeNamespaceImport("System.Linq"));
            }
            Machine.Imports.Add(new CodeNamespaceImport("System.Text"));
            Machine.Imports.Add(new CodeNamespaceImport("System.IO"));

            InputEventDataClassType = new CodeTypeDeclaration(_smName + "InputEventArgs");
            InputEventDataClassType.TypeAttributes = TypeAttributes.Public;
            InputEventDataClassType.Attributes = MemberAttributes.Private;
            InputEventDataClassType.IsClass = true;
            InputEventDataClassType.BaseTypes.Add("EventArgs");
            Machine.Types.Add(InputEventDataClassType);
            InputEventDataUnit.Namespaces.Add(Machine);

            InputEventDataClassType.Members.Add(CreateField(typeof(System.String), "_comment", "The debug comment."));
            InputEventDataClassType.Members.Add(CreateField(typeof(System.Int64), "_sm_id", "The state machine id."));
            InputEventDataClassType.Members.Add(CreateField(typeof(System.Int64), "_sm_inst", "The state machine instance."));
            InputEventDataClassType.Members.Add(CreateField(typeof(System.Guid), "_sm_instguid", "The state machine instance Guid."));

            CodeConstructor InputEventDataConstructor = new CodeConstructor();
            InputEventDataConstructor.Attributes = MemberAttributes.Public;
            InputEventDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "cmnt"));
            InputEventDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.Int64", "smid"));
            InputEventDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.Int64", "inst"));
            InputEventDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.Guid", "instguid"));
            InputEventDataConstructor.Statements.Add(CreateFieldAssign("_comment", "cmnt"));
            InputEventDataConstructor.Statements.Add(CreateFieldAssign("_sm_id", "smid"));
            InputEventDataConstructor.Statements.Add(CreateFieldAssign("_sm_inst", "inst"));
            InputEventDataConstructor.Statements.Add(CreateFieldAssign("_sm_instguid", "instguid"));
            InputEventDataClassType.Members.Add(InputEventDataConstructor);

            InputEventDataClassType.Members.Add(CreateFieldProperty(typeof(System.String), "_comment", "Comment", "The debug comment"));
            InputEventDataClassType.Members.Add(CreateFieldProperty(typeof(System.Int64), "_sm_id", "Sm_ID", "The State machine type id"));
            InputEventDataClassType.Members.Add(CreateFieldProperty(typeof(System.Int64), "_sm_inst", "Sm_Instance", "The State machine instance id"));
            InputEventDataClassType.Members.Add(CreateFieldProperty(typeof(System.Guid), "_sm_instguid", "Sm_InstGuid", "The State machine instance Guid"));

            GenerateCode(InputEventDataFileName, InputEventDataUnit);
        }

        public void GenerateActionEventData()
        {
            ActionsEventDataUnit = new CodeCompileUnit();
            CodeNamespace Machine = new CodeNamespace("sm_" + _smName);
            Machine.Imports.Add(new CodeNamespaceImport("System"));
            Machine.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            if (_language == "c#")
            {
                Machine.Imports.Add(new CodeNamespaceImport("System.Linq"));
            }
            Machine.Imports.Add(new CodeNamespaceImport("System.Text"));
            Machine.Imports.Add(new CodeNamespaceImport("System.IO"));

            ActionsEventDataClassType = new CodeTypeDeclaration(_smName + "ActionsEventArgs");
            ActionsEventDataClassType.TypeAttributes = TypeAttributes.Public;
            ActionsEventDataClassType.Attributes = MemberAttributes.Private;
            ActionsEventDataClassType.IsClass = true;
            ActionsEventDataClassType.BaseTypes.Add("EventArgs");
            Machine.Types.Add(ActionsEventDataClassType);
            ActionsEventDataUnit.Namespaces.Add(Machine);

            ActionsEventDataClassType.Members.Add(CreateField(typeof(System.Int64), "_sm_id", "The state machine id."));
            ActionsEventDataClassType.Members.Add(CreateField(typeof(System.Int64), "_sm_inst", "The state machine instance."));
            ActionsEventDataClassType.Members.Add(CreateField(typeof(System.Guid), "_sm_instguid", "The state machine instance Guid."));

            CodeConstructor ActionsEventDataConstructor = new CodeConstructor();
            ActionsEventDataConstructor.Attributes = MemberAttributes.Public;

            ActionsEventDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.Int64", "smid"));
            ActionsEventDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.Int64", "inst"));
            ActionsEventDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.Guid", "instguid"));
            ActionsEventDataConstructor.Statements.Add(CreateFieldAssign("_sm_id", "smid"));
            ActionsEventDataConstructor.Statements.Add(CreateFieldAssign("_sm_inst", "inst"));
            ActionsEventDataConstructor.Statements.Add(CreateFieldAssign("_sm_instguid", "instguid"));
            ActionsEventDataClassType.Members.Add(ActionsEventDataConstructor);

            ActionsEventDataClassType.Members.Add(CreateFieldProperty(typeof(System.Int64), "_sm_id", "Sm_ID", "The State machine type id"));
            ActionsEventDataClassType.Members.Add(CreateFieldProperty(typeof(System.Int64), "_sm_inst", "Sm_Instance", "The State machine instance id"));
            ActionsEventDataClassType.Members.Add(CreateFieldProperty(typeof(System.Guid), "_sm_instguid", "Sm_InstGuid", "The State machine instance Guid"));
            GenerateCode(ActionEventDataFileName, ActionsEventDataUnit);
        }

        #endregion

        #region Test Unit

        public void GenerateNoCsharpRunTest(ref CodeTypeDeclaration TestClassType)
        {
            string isname = "_" + _smName.ToLower();

            CodeMemberMethod doTest = new CodeMemberMethod();
            doTest.Name = "RunTest";
            doTest.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            CodeVariableDeclarationStatement vardec = new CodeVariableDeclarationStatement(
                typeof(System.String),
                "show",
                 new CodePrimitiveExpression(""));
            doTest.Statements.Add(vardec);

            Dictionary<Int64, string> evlist = new Dictionary<Int64, string>();
            int ii = 0;

            foreach (KeyValuePair<string, Int64> ev in _evntdict)
            {
                evlist[ii] = ev.Key;
                string vn = string.Format("Input {0} to run {1}\r\n", ii, ev.Key);
                CodeAssignStatement cs = new CodeAssignStatement(
                    new CodeVariableReferenceExpression("show"),
                    new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("show"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(vn)));
                doTest.Statements.Add(cs);
                ii++;
            }

            CodeMethodInvokeExpression m5 = new CodeMethodInvokeExpression(
               new CodeTypeReferenceExpression("System.Console"), "WriteLine", new CodeVariableReferenceExpression("show"));
            doTest.Statements.Add(m5);
            CodeMethodInvokeExpression m6 = new CodeMethodInvokeExpression(
               new CodeTypeReferenceExpression("System.Console"), "WriteLine", new CodePrimitiveExpression("100 to exit, 101 show input list"));
            doTest.Statements.Add(m6);

            CodeVariableDeclarationStatement vardec2 = new CodeVariableDeclarationStatement(
                typeof(System.Boolean),
                "retv",
                 new CodePrimitiveExpression(false));

            CodeLabeledStatement labelstart = new CodeLabeledStatement("IsStart", vardec2);
            doTest.Statements.Add(labelstart);
            // Convert.ToInt32(Console.ReadLine())
            CodeVariableDeclarationStatement vardec5 = new CodeVariableDeclarationStatement(
                typeof(System.Int32),
                "nn",
                 new CodeMethodInvokeExpression(
                     new CodeTypeReferenceExpression("System.Convert"), "ToInt32", new CodeExpression[]
                     {
                         new CodeMethodInvokeExpression(
                           new CodeTypeReferenceExpression("System.Console"), "ReadLine", new CodeExpression[] { } )
                     })
                 );
            doTest.Statements.Add(vardec5);

            CodeVariableDeclarationStatement vardec3 = new CodeVariableDeclarationStatement(
               typeof(System.DateTime),
               "tmsg",
                new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("System.DateTime"), "Now"));
            doTest.Statements.Add(vardec3);
            foreach (KeyValuePair<Int64, string> ev in evlist)
            {
                CodeConditionStatement anif = new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("nn"), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(ev.Key)));

                string st = string.Format("Run{0}Event{1}", _smName, ev.Value);
                CodeAssignStatement evs = new CodeAssignStatement(
                    new CodeVariableReferenceExpression("retv"),
                    new CodeMethodInvokeExpression(
                         new CodeTypeReferenceExpression(isname),
                         st,
                         new CodeMethodInvokeExpression(
                            new CodeVariableReferenceExpression("tmsg"),
                            "ToString",
                            new CodeExpression[] { })));
                anif.TrueStatements.Add(evs);

                // Console.WriteLine(\"state is now \" + {0}.GetCurrentState()... isname

                CodeMethodInvokeExpression strfmt = new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression("System.String"),
                    "Format",
                    new CodeExpression[]
                    {
                        new CodePrimitiveExpression("state is now {0}"),
                        new CodeMethodInvokeExpression(
                             new CodeTypeReferenceExpression(isname),
                             "GetCurrentState",
                             new CodeExpression[]  { } )
                    });


                anif.TrueStatements.Add(
                         new CodeMethodInvokeExpression(
                             new CodeTypeReferenceExpression("System.Console"),
                                 "WriteLine",
                                 new CodeExpression[] { strfmt }));


                // Console.WriteLine(\"state is now \" + {0}.GetCurrentState());\r\n", isname);
                anif.TrueStatements.Add(new CodeGotoStatement("IsStart"));
                doTest.Statements.Add(anif);
            }

            CodeConditionStatement anif2 = new CodeConditionStatement(
                   new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("nn"), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(100)));
            anif2.TrueStatements.Add(new CodeGotoStatement("IsDone"));
            doTest.Statements.Add(anif2);

            CodeMethodInvokeExpression m1 = new CodeMethodInvokeExpression(
               new CodeTypeReferenceExpression("System.Console"), "WriteLine", new CodeVariableReferenceExpression("show"));
            doTest.Statements.Add(m1);
            CodeMethodInvokeExpression m2 = new CodeMethodInvokeExpression(
               new CodeTypeReferenceExpression("System.Console"), "WriteLine", new CodePrimitiveExpression("100 to exit, 101 show input list"));
            doTest.Statements.Add(m2);
            doTest.Statements.Add(new CodeGotoStatement("IsStart"));

            CodeMethodInvokeExpression m4 = new CodeMethodInvokeExpression(
            new CodeTypeReferenceExpression("System.Console"), "WriteLine", new CodePrimitiveExpression("Done Test"));
            CodeLabeledStatement labeldone = new CodeLabeledStatement("IsDone", new CodeExpressionStatement(m4));
            doTest.Statements.Add(labeldone);
            TestClassType.Members.Add(doTest);
        }

        public void GenerateTestCode()
        {
            TestUnit = new CodeCompileUnit();
            CodeNamespace Machine = new CodeNamespace("sm_" + _smName);
            Machine.Imports.Add(new CodeNamespaceImport("System"));
            Machine.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            if (_language == "c#")
            {
                Machine.Imports.Add(new CodeNamespaceImport("System.Linq"));
                //   Machine.Imports.Add(new CodeNamespaceImport("System.Data.Linq"));
            }
            Machine.Imports.Add(new CodeNamespaceImport("System.Text"));
            Machine.Imports.Add(new CodeNamespaceImport("System.IO"));

            TestClassType = new CodeTypeDeclaration("Test" + _smName);
            TestClassType.TypeAttributes = TypeAttributes.Public;
            TestClassType.Attributes = MemberAttributes.Private;
            TestClassType.IsClass = true;



            Machine.Types.Add(TestClassType);
            TestUnit.Namespaces.Add(Machine);

            string isname = "_" + _smName.ToLower();

            CodeMemberField mField = new CodeMemberField();
            mField.Attributes = MemberAttributes.Private;
            mField.Name = isname;
            mField.Type = new CodeTypeReference("sm_" + _smName + "." + _smName);
            mField.Comments.Add(new CodeCommentStatement(
                "ToDo: Add Console Main to test the state machine " + _smName));

            mField.Comments.Add(new CodeCommentStatement(@"In c# add something like
// *******
namespace sm_Beast
{
    class Program
    {
        static void Main(string[] args)
        {
            TestBeast testbeast = new TestBeast();
            testbeast.StartTest();
            testbeast.ShowList();
            testbeast.ChooseEvent();
        }
    }
}
// *******  "));

            TestClassType.Members.Add(mField);

            CodeMemberField mField2 = new CodeMemberField();
            mField2.Attributes = MemberAttributes.Private;
            mField2.Name = "chevnt";
            mField2.Type = new CodeTypeReference(typeof(System.String));
            mField2.Comments.Add(new CodeCommentStatement(
                "User's event to try running in current state"));
            TestClassType.Members.Add(mField2);

            CodeMemberProperty iidProperty = new CodeMemberProperty();
            iidProperty.Attributes =
                MemberAttributes.Public | MemberAttributes.Final;
            iidProperty.Name = _smName;
            iidProperty.HasGet = true;
            iidProperty.HasSet = true;
            iidProperty.Type = new CodeTypeReference("sm_" + _smName + "." + _smName);
            iidProperty.Comments.Add(new CodeCommentStatement("The test instance."));
            iidProperty.GetStatements.Add(new CodeMethodReturnStatement(
                new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(), isname)));

            CodeAssignStatement asd = new CodeAssignStatement(
                            new CodeVariableReferenceExpression(isname),
                            new CodeVariableReferenceExpression("value"));

            iidProperty.SetStatements.Add(asd);
            TestClassType.Members.Add(iidProperty);

            // constructor
            CodeConstructor TestConstructor = new CodeConstructor();
            TestConstructor.Attributes = MemberAttributes.Public;

            CodeObjectCreateExpression objectCreate =
               new CodeObjectCreateExpression(
               new CodeTypeReference(_smName));


            CodeFieldReferenceExpression refs = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), isname);

            TestConstructor.Statements.Add(new CodeAssignStatement(
                refs,
                objectCreate));

            TestClassType.Members.Add(TestConstructor);
            TestClassType.Members.Add(GenerateStartTest());
            TestClassType.Members.Add(GenerateEndTest());
            CreateTestInputMethods(ref TestClassType);
            CreateTestActionMethods(ref TestClassType);
            CreateTestEventReadyMethods(ref TestClassType);

            string mstart;

            if (_language != "c#" && _language != "c++")
            {
                GenerateNoCsharpRunTest(ref TestClassType);
            }
            else
            {
                if (_language != "c#")
                {
                    mstart = " public: void RunTest()\r\n";
                    mstart += "   {\r\n";
                    Dictionary<Int64, string> evlist = new Dictionary<Int64, string>();
                    Dictionary<string, Int64> tmpev = new Dictionary<string, Int64>();
                    int ii = 0;
                    mstart += "   string^ show = \"\";\r\n";
                    foreach (KeyValuePair<string, Int64> ev in _evntdict)
                    {
                        evlist[ii] = ev.Key;
                        mstart += string.Format("       show += \"Input {0} to run {1}\\r\\n\";\r\n", ii, ev.Key);
                        ii++;
                    }

                    foreach (KeyValuePair<string, Int64> ev in _evntdict)
                    {
                        evlist[ii] = ev.Key;
                        tmpev[ev.Key] = ii;
                        ii++;
                    }
                    foreach (KeyValuePair<string, Int64> sta in _stateDictionary)
                    {
                        mstart += string.Format("       show += \"state {0} -- \";\r\n", sta.Key);
                        if (_transitions.ContainsKey(sta.Key))
                        {
                            Dictionary<string, Tranval> etrans = _transitions[sta.Key];
                            foreach (KeyValuePair<string, Tranval> tr in etrans)
                            {
                                mstart += string.Format("       show += \"{0} ({1}), \";\r\n", tr.Key, tmpev[tr.Key]);
                            }
                            mstart += "       show += \"\\r\\n\";\r\n";
                        }
                        else
                        {
                            mstart += string.Format("       show += \"{0} -- NoEvents\";\r\n", sta.Key);
                        }
                    }

                    mstart += "       Console->WriteLine(show);\r\n";
                    mstart += "       Console->WriteLine(\"100 to exit, 101 show input list\");\r\n";
                    mstart += "       while(true)\r\n";
                    mstart += "       {\r\n";
                    mstart += "          try\r\n";
                    mstart += "          {\r\n";
                    mstart += "             int nn = Convert->ToInt32(Console->ReadLine());\r\n";
                    mstart += "             bool retv;\r\n";
                    mstart += "             String ^evtst = nullptr;\r\n";
                    mstart += "             switch(nn)\r\n";
                    mstart += "             {\r\n";
                    foreach (KeyValuePair<Int64, string> ev in evlist)
                    {
                        mstart += string.Format("                case {0}:\r\n", ev.Key);
                        mstart += string.Format("                   chevnt = \"{0}\";\r\n", ev.Value);
                        mstart += string.Format("                   evtst = {0}->{1}InputReady->EventsReadyFromState({0}->GetCurrentState());\r\n", isname, _smName);
                        mstart += "                   if(evtst != nullptr)\r\n";
                        mstart += "                   {\r\n";
                        mstart += string.Format("                   retv = {0}->Run{1}Event{2}(DateTime->Now->ToString());\r\n", isname, _smName, ev.Value);
                        mstart += "                   }\r\n";
                        mstart += string.Format("                   Console->WriteLine(\"state is now \" + {0}->GetCurrentState());\r\n", isname);
                        mstart += "                   break;\r\n";
                    }
                    mstart += "             }\r\n";
                    mstart += "             if(nn == 101) Console->WriteLine(show);\r\n";
                    mstart += "             if(nn == 100) break;\r\n";
                    mstart += "          }\r\n";

                    mstart += "          catch(Exception^ ee)\r\n";
                    mstart += "          {\r\n";
                    mstart += "             Console->WriteLine(ee->Message);\r\n";
                    mstart += "          }\r\n";
                    mstart += "        }\r\n";
                    mstart += "   }\r\n";

                    CodeSnippetTypeMember dMember = new CodeSnippetTypeMember(mstart);
                    TestClassType.Members.Add(dMember);

                    //mstart += "          catch(ChangeConflictException^ e)\r\n";
                    //mstart += "          {\r\n";
                    //mstart += "             Console->WriteLine(e->Message);\r\n";
                    //mstart += "          }\r\n";
                }
                else
                {
                    //// ***** ShowList()
                    mstart = " public void ShowList()\r\n";
                    mstart += "   {\r\n";
                    Dictionary<Int64, string> evlist = new Dictionary<Int64, string>();
                    Dictionary<string, Int64> tmpev = new Dictionary<string, Int64>();
                    int ii = 0;
                    mstart += "       string show = \"\";\r\n";
                    foreach (KeyValuePair<string, Int64> ev in _evntdict)
                    {
                        evlist[ii] = ev.Key;
                        tmpev[ev.Key] = ii;
                        // mstart += string.Format("       show += \"Input {0} to run {1}\\r\\n\";\r\n", ii, ev.Key);
                        //     mstart += "       Console.WriteLine(show);\r\n";
                        ii++;
                    }
                    foreach (KeyValuePair<string, Int64> sta in _stateDictionary)
                    {
                        mstart += string.Format("       show += \"state {0} -- \";\r\n", sta.Key);
                        if (_transitions.ContainsKey(sta.Key))
                        {
                            Dictionary<string, Tranval> etrans = _transitions[sta.Key];
                            foreach (KeyValuePair<string, Tranval> tr in etrans)
                            {
                                mstart += string.Format("       show += \"{0} ({1}), \";\r\n", tr.Key, tmpev[tr.Key]);
                            }
                            mstart += "       show += \"\\r\\n\";\r\n";
                        }
                        else
                        {
                            mstart += string.Format("       show += \"{0} -- NoEvents\";\r\n", sta.Key);
                        }
                    }
                    mstart += "       Writer(show);\r\n";
                    mstart += "       Writer(\"-2 to exit, -1 to show state list\");\r\n";
                    mstart += "   }\r\n";

                    CodeSnippetTypeMember dMember = new CodeSnippetTypeMember(mstart);
                    TestClassType.Members.Add(dMember);

                    //// ****************** Writer
                    mstart = " public void Writer(string message)\r\n";
                    mstart += "   {\r\n";
                    mstart += "       Console.WriteLine(message);\r\n";
                    mstart += "   }\r\n";
                    CodeSnippetTypeMember fMember = new CodeSnippetTypeMember(mstart);
                    TestClassType.Members.Add(fMember);


                    //// ****** int ChooseEvent()
                    mstart = " public int ChooseEvent()\r\n";
                    mstart += "   {\r\n";
                    mstart += "      int nn = -4;\r\n";
                    mstart += "      while(nn != (-2))\r\n";
                    mstart += "       {\r\n";
                    mstart += "             nn = Convert.ToInt32(Console.ReadLine());\r\n";
                    mstart += "             RunTest(nn);\r\n";
                    mstart += "       }\r\n";
                    mstart += "      return(nn);\r\n";
                    mstart += "   }\r\n";

                    CodeSnippetTypeMember gMember = new CodeSnippetTypeMember(mstart);
                    TestClassType.Members.Add(gMember);

                    //// ***************** RunTest
                    mstart = " public void RunTest(int number)\r\n";
                    mstart += "   {\r\n";
                    ii = 0;
                    mstart += "          try\r\n";
                    mstart += "          {\r\n";
                    mstart += "             bool retv;\r\n";
                    mstart += "             string evtst = null;\r\n";
                    mstart += "             switch(number)\r\n";
                    mstart += "             {\r\n";
                    foreach (KeyValuePair<Int64, string> ev in evlist)
                    {
                        mstart += string.Format("                case {0}:\r\n", ev.Key);
                        mstart += string.Format("                   chevnt = \"{0}\";\r\n", ev.Value);
                        mstart += string.Format("                   evtst = {0}.{1}InputReady.EventsReadyFromState({0}.GetCurrentState());\r\n", isname, _smName);
                        mstart += "                   if(evtst != null)\r\n";
                        mstart += "                   {\r\n";
                        mstart += string.Format("                   retv = {0}.Run{1}Event{2}(DateTime.Now.ToString());\r\n", isname, _smName, ev.Value);
                        mstart += "                   }\r\n";
                        mstart += string.Format("                   Writer(\"state is now \" + {0}.GetCurrentState());\r\n", isname);

                        mstart += "                   break;\r\n";
                    }
                    mstart += "             }\r\n";
                    mstart += "             if(number == -1) ShowList();\r\n";

                    mstart += "          }\r\n";
                    mstart += "          catch(Exception ee)\r\n";
                    mstart += "          {\r\n";
                    mstart += "             Writer(ee.Message);\r\n";
                    mstart += "          }\r\n";
                    mstart += "   }\r\n";

                    CodeSnippetTypeMember eMember = new CodeSnippetTypeMember(mstart);
                    TestClassType.Members.Add(eMember);
                }
            }
            GenerateCode(TestFileName, TestUnit);
        }

        public CodeMemberMethod GenerateStartTest()
        {
            CodeMemberMethod doStartTest = new CodeMemberMethod();
            doStartTest.Name = "StartTest";
            doStartTest.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            string isname = "_" + _smName.ToLower();
            foreach (KeyValuePair<string, Int64> ev in _evntdict)
            {
                string evv = string.Format("{0}Event{1}", _smName, ev.Key);
                string ha = string.Format("TestInputEvent{0}", ev.Key);

                CodeAttachEventStatement att1 = new CodeAttachEventStatement(new CodeTypeReferenceExpression(isname), evv, new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), ha));
                doStartTest.Statements.Add(att1);
            }

            foreach (KeyValuePair<string, Int64> ev in _actiondict)
            {
                string acts = _smName + "Actions";
                string evv = string.Format("{2}.{0}Action{1}", _smName, ev.Key, acts);  // _starship.StarShipActions.StarShipAction_moveup
                string ha = string.Format("TestActionEvent{0}", ev.Key);
                CodeAttachEventStatement att2 = new CodeAttachEventStatement(new CodeTypeReferenceExpression(isname), evv, new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), ha));
                doStartTest.Statements.Add(att2);
            }

            foreach (KeyValuePair<string, Dictionary<string, Tranval>> tran in _transitions)
            {
                string st = tran.Key;
                string acts = _smName + "InputReady";
                string evv = string.Format("{1}.{0}CheckEventReady", st, acts);  // _starship.StarShipInputReady.destroyed_CheckEvntReady
                string ha = string.Format("CheckForEventInState{0}", st);
                CodeAttachEventStatement att2 = new CodeAttachEventStatement(
                    new CodeTypeReferenceExpression(isname),
                    evv,
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), ha));
                doStartTest.Statements.Add(att2);
            }
            doStartTest.Comments.Add(new CodeCommentStatement("Attach test methods to events"));
            return (doStartTest);
        }

        public CodeMemberMethod GenerateEndTest()
        {
            CodeMemberMethod doEndTest = new CodeMemberMethod();
            doEndTest.Name = "EndTest";
            doEndTest.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            string isname = "_" + _smName.ToLower();
            foreach (KeyValuePair<string, Int64> ev in _evntdict)
            {
                string evv = string.Format("{0}Event{1}", _smName, ev.Key);
                string ha = string.Format("TestInputEvent{0}", ev.Key);

                CodeRemoveEventStatement att1 = new CodeRemoveEventStatement(new CodeTypeReferenceExpression(isname), evv, new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), ha));
                doEndTest.Statements.Add(att1);
            }

            foreach (KeyValuePair<string, Int64> ev in _actiondict)
            {
                string acts = _smName + "Actions";
                string evv = string.Format("{2}.{0}Action{1}", _smName, ev.Key, acts);
                string ha = string.Format("TestActionEvent{0}", ev.Key);
                CodeRemoveEventStatement att2 = new CodeRemoveEventStatement(new CodeTypeReferenceExpression(isname), evv, new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), ha));
                doEndTest.Statements.Add(att2);
            }

            foreach (KeyValuePair<string, Dictionary<string, Tranval>> tran in _transitions)
            {
                string st = tran.Key;
                string acts = _smName + "InputReady";
                string evv = string.Format("{1}.{0}CheckEventReady", st, acts);  // _starship.StarShipInputReady.destroyed_CheckEvntReady
                string ha = string.Format("CheckForEventInState{0}", st);
                CodeRemoveEventStatement att2 = new CodeRemoveEventStatement(
                    new CodeTypeReferenceExpression(isname),
                    evv,
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), ha));
                doEndTest.Statements.Add(att2);
            }

            doEndTest.Comments.Add(new CodeCommentStatement("Remove test methods from events"));
            return (doEndTest);
        }

        private void CreateTestInputMethods(ref CodeTypeDeclaration TestClassType)
        {
            string isname = "_" + _smName.ToLower();
            foreach (KeyValuePair<string, Int64> ev in _evntdict)
            {
                CodeMemberMethod doTest = new CodeMemberMethod();
                doTest.Name = "TestInputEvent" + ev.Key;
                doTest.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                doTest.ReturnType = new CodeTypeReference("System.Boolean");
                CodeParameterDeclarationExpression p1 = new CodeParameterDeclarationExpression("System.Object", "sender");
                doTest.Parameters.Add(p1);
                CodeParameterDeclarationExpression p2 = new CodeParameterDeclarationExpression(_smName + "InputEventArgs", "e");
                doTest.Parameters.Add(p2);

                CodeMethodInvokeExpression mi = new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), isname),
                    "GetCurrentState",
                    new CodeExpression[] { });

                CodeVariableDeclarationStatement vdec = new CodeVariableDeclarationStatement(
                    typeof(System.String),
                   "nowstate",
                    mi);

                doTest.Statements.Add(vdec);

                string ts = string.Format(" running {0}Event{1}", _smName, ev.Key);
                CodeVariableDeclarationStatement vdec2 = new CodeVariableDeclarationStatement(
                   typeof(System.String),
                  "runs",
                   new CodePrimitiveExpression(ts));

                doTest.Statements.Add(vdec2);

                string fmt = "{0} In state {1} [{2}] for id = {3} instance = {4}";
                CodeMethodInvokeExpression mi2 = new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression("System.String"),
                    "Format",
                    new CodeExpression[]
                    {
                      new CodePrimitiveExpression(fmt),
                      new CodeVariableReferenceExpression("runs"),
                      new CodeVariableReferenceExpression("nowstate"),
                      new CodeTypeReferenceExpression("e.Comment"),
                      new CodeTypeReferenceExpression("e.Sm_ID"),
                      new CodeTypeReferenceExpression("e.Sm_Instance")
                    });
                CodeVariableDeclarationStatement vdec3 = new CodeVariableDeclarationStatement(
                   typeof(System.String),
                  "message",
                   mi2);

                doTest.Statements.Add(vdec3);

                if (_language != "c#")
                {
                    CodeMethodInvokeExpression mi3 = new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression("Console"),
                        "WriteLine",
                        new CodeExpression[] { new CodeVariableReferenceExpression("message") });
                    doTest.Statements.Add(mi3);
                }
                else
                {
                    CodeMethodInvokeExpression mi4 = new CodeMethodInvokeExpression(
                        new CodeThisReferenceExpression(),
                        "Writer",
                        new CodeExpression[] { new CodeVariableReferenceExpression("message") });
                    doTest.Statements.Add(mi4);
                }
                doTest.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));

                TestClassType.Members.Add(doTest);
            }
        }

        private void CreateTestActionMethods(ref CodeTypeDeclaration TestClassType)
        {
            string isname = "_" + _smName.ToLower();
            foreach (KeyValuePair<string, Int64> ev in _actiondict)
            {
                CodeMemberMethod doTest = new CodeMemberMethod();
                doTest.Name = "TestActionEvent" + ev.Key;
                doTest.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                doTest.ReturnType = new CodeTypeReference("System.Boolean");
                CodeParameterDeclarationExpression p1 = new CodeParameterDeclarationExpression("System.Object", "sender");
                doTest.Parameters.Add(p1);
                CodeParameterDeclarationExpression p2 = new CodeParameterDeclarationExpression(_smName + "ActionsEventArgs", "e");
                doTest.Parameters.Add(p2);

                CodeMethodInvokeExpression mi = new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), isname),
                    "GetCurrentState",
                    new CodeExpression[] { });

                CodeVariableDeclarationStatement vdec = new CodeVariableDeclarationStatement(
                    typeof(System.String),
                   "nowstate",
                    mi);

                doTest.Statements.Add(vdec);

                string ts = string.Format(" running Action {0}Event_{1}", _smName, ev.Key);
                CodeVariableDeclarationStatement vdec2 = new CodeVariableDeclarationStatement(
                   typeof(System.String),
                  "runs",
                   new CodePrimitiveExpression(ts));

                doTest.Statements.Add(vdec2);

                string fmt = "{0} In state {1} with id = {2} inst = {3}";
                CodeMethodInvokeExpression mi2 = new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression("System.String"),
                    "Format",
                    new CodeExpression[]
                    {
                      new CodePrimitiveExpression(fmt),
                      new CodeVariableReferenceExpression("runs"),
                      new CodeVariableReferenceExpression("nowstate"),
                      new CodeTypeReferenceExpression("e.Sm_ID"),
                      new CodeTypeReferenceExpression("e.Sm_Instance"),
                    });
                CodeVariableDeclarationStatement vdec3 = new CodeVariableDeclarationStatement(
                   typeof(System.String),
                  "message",
                   mi2);

                doTest.Statements.Add(vdec3);

                if (_language != "c#")
                {
                    CodeMethodInvokeExpression mi3 = new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression("Console"),
                        "WriteLine",
                        new CodeExpression[] { new CodeVariableReferenceExpression("message") });
                    doTest.Statements.Add(mi3);
                }
                else
                {
                    CodeMethodInvokeExpression mi4 = new CodeMethodInvokeExpression(
                        new CodeThisReferenceExpression(),
                        "Writer",
                        new CodeExpression[] { new CodeVariableReferenceExpression("message") });
                    doTest.Statements.Add(mi4);
                }

                doTest.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));

                TestClassType.Members.Add(doTest);
            }
        }

        private void CreateTestEventReadyMethods(ref CodeTypeDeclaration TestClassType)
        {
            string isname = "_" + _smName.ToLower();
            foreach (KeyValuePair<string, Dictionary<string, Tranval>> tran in _transitions)
            {
                string st = tran.Key;
                CodeMemberMethod doTest = new CodeMemberMethod();
                doTest.Name = string.Format("CheckForEventInState{0}", st);
                doTest.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                doTest.ReturnType = new CodeTypeReference("System.String");
                CodeParameterDeclarationExpression p1 = new CodeParameterDeclarationExpression("System.Object", "sender");
                doTest.Parameters.Add(p1);
                CodeParameterDeclarationExpression p2 = new CodeParameterDeclarationExpression(_smName + "TestEventsReadyArgs", "e");
                doTest.Parameters.Add(p2);

                CodeMethodInvokeExpression mi = new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), isname),
                    "GetCurrentState",
                    new CodeExpression[] { });

                CodeVariableDeclarationStatement vdec = new CodeVariableDeclarationStatement(
                    typeof(System.String),
                   "nowstate",
                    mi);

                doTest.Statements.Add(vdec);

                string ts = string.Format(" running CheckForEventInState{0}", st);
                CodeVariableDeclarationStatement vdec2 = new CodeVariableDeclarationStatement(
                   typeof(System.String),
                  "runs",
                   new CodePrimitiveExpression(ts));

                doTest.Statements.Add(vdec2);

                string fmt = "{0} In state {1} with id = {2} inst = {3}";
                CodeMethodInvokeExpression mi2 = new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression("System.String"),
                    "Format",
                    new CodeExpression[]
                    {
                      new CodePrimitiveExpression(fmt),
                      new CodeVariableReferenceExpression("runs"),
                      new CodeVariableReferenceExpression("nowstate"),
                      new CodeTypeReferenceExpression("e.Sm_ID"),
                      new CodeTypeReferenceExpression("e.Sm_Instance"),
                    });
                CodeVariableDeclarationStatement vdec3 = new CodeVariableDeclarationStatement(
                   typeof(System.String),
                  "message",
                   mi2);

                doTest.Statements.Add(vdec3);

                //CodeMethodInvokeExpression mi3 = new CodeMethodInvokeExpression(
                //    new CodeTypeReferenceExpression("Console"),
                //    "WriteLine",
                //    new CodeExpression[] { new CodeVariableReferenceExpression("message") });
                //doTest.Statements.Add(mi3);

                if (_language != "c#")
                {
                    CodeMethodInvokeExpression mi3 = new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression("Console"),
                        "WriteLine",
                        new CodeExpression[] { new CodeVariableReferenceExpression("message") });
                    doTest.Statements.Add(mi3);
                }
                else
                {
                    CodeMethodInvokeExpression mi4 = new CodeMethodInvokeExpression(
                        new CodeThisReferenceExpression(),
                        "Writer",
                        new CodeExpression[] { new CodeVariableReferenceExpression("message") });
                    doTest.Statements.Add(mi4);
                }


                CodeMethodInvokeExpression mid = new CodeMethodInvokeExpression(
                   new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), isname),
                   _smName + "InputReady" + ".ListEventsFromState",
                   new CodeExpression[] { new CodePrimitiveExpression(st) });

                CodeVariableDeclarationStatement vdec4 = new CodeVariableDeclarationStatement(
                   typeof(Dictionary<string, int>),
                  "validevnts",
                   mid);

                doTest.Statements.Add(vdec4);

                CodeFieldReferenceExpression nref = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "chevnt");

                CodeConditionStatement outif = new CodeConditionStatement();
                outif.Condition = new CodeMethodInvokeExpression(
                   new CodeVariableReferenceExpression("validevnts"),
                   "ContainsKey",
                   new CodeExpression[] { nref });

                outif.TrueStatements.Add(new CodeMethodReturnStatement(nref));

                doTest.Statements.Add(outif);

                doTest.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
                TestClassType.Members.Add(doTest);
            }
        }


        #endregion

        #region Generate WinMain

        private void CreateWinMainInputMethods(ref CodeTypeDeclaration TestClassType)
        {
            string isname = "_" + _smName.ToLower();
            foreach (KeyValuePair<string, Int64> ev in _evntdict)
            {
                CodeMemberMethod doTest = new CodeMemberMethod();
                doTest.Name = "TestInputEvent" + ev.Key;
                doTest.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                doTest.ReturnType = new CodeTypeReference("System.Boolean");
                CodeParameterDeclarationExpression p1 = new CodeParameterDeclarationExpression("System.Object", "sender");
                doTest.Parameters.Add(p1);
                CodeParameterDeclarationExpression p2 = new CodeParameterDeclarationExpression(_smName + "InputEventArgs", "e");
                doTest.Parameters.Add(p2);

                string stt = string.Format("          EventTextBlock.Text = \"{0}\";\n\r", ev.Key);
                CodeSnippetStatement zzt = new CodeSnippetStatement(stt);
                doTest.Statements.Add(zzt);

                CodeMethodInvokeExpression mi = new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), isname),
                    "GetCurrentState",
                    new CodeExpression[] { });

                CodeVariableDeclarationStatement vdec = new CodeVariableDeclarationStatement(
                    typeof(System.String),
                   "nowstate",
                    mi);

                doTest.Statements.Add(vdec);

                string ts = string.Format(" running {0}Event{1}", _smName, ev.Key);
                CodeVariableDeclarationStatement vdec2 = new CodeVariableDeclarationStatement(
                   typeof(System.String),
                  "runs",
                   new CodePrimitiveExpression(ts));

                doTest.Statements.Add(vdec2);

                string fmt = "{0} In state {1} [{2}] for id = {3} instance = {4}";
                CodeMethodInvokeExpression mi2 = new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression("System.String"),
                    "Format",
                    new CodeExpression[]
                    {
                      new CodePrimitiveExpression(fmt),
                      new CodeVariableReferenceExpression("runs"),
                      new CodeVariableReferenceExpression("nowstate"),
                      new CodeTypeReferenceExpression("e.Comment"),
                      new CodeTypeReferenceExpression("e.Sm_ID"),
                      new CodeTypeReferenceExpression("e.Sm_Instance")
                    });
                CodeVariableDeclarationStatement vdec3 = new CodeVariableDeclarationStatement(
                   typeof(System.String),
                  "message",
                   mi2);

                doTest.Statements.Add(vdec3);

                if (_language != "c#")
                {
                    CodeMethodInvokeExpression mi3 = new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression("Console"),
                        "WriteLine",
                        new CodeExpression[] { new CodeVariableReferenceExpression("message") });
                    doTest.Statements.Add(mi3);
                }
                else
                {
                    CodeMethodInvokeExpression mi4 = new CodeMethodInvokeExpression(
                        new CodeThisReferenceExpression(),
                        "Writer",
                        new CodeExpression[] { new CodeVariableReferenceExpression("message") });
                    doTest.Statements.Add(mi4);
                }
                doTest.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));

                TestClassType.Members.Add(doTest);
            }
        }

        private void CreateWinMainActionMethods(ref CodeTypeDeclaration TestClassType)
        {
            string isname = "_" + _smName.ToLower();
            foreach (KeyValuePair<string, Int64> ev in _actiondict)
            {
                CodeMemberMethod doTest = new CodeMemberMethod();
                doTest.Name = "TestActionEvent" + ev.Key;
                doTest.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                doTest.ReturnType = new CodeTypeReference("System.Boolean");
                CodeParameterDeclarationExpression p1 = new CodeParameterDeclarationExpression("System.Object", "sender");
                doTest.Parameters.Add(p1);
                CodeParameterDeclarationExpression p2 = new CodeParameterDeclarationExpression(_smName + "ActionsEventArgs", "e");
                doTest.Parameters.Add(p2);


                string stt = string.Format("          ActionTextBlock.Text = \"{0}\";\n\r", ev.Key);
                CodeSnippetStatement zzt = new CodeSnippetStatement(stt);
                doTest.Statements.Add(zzt);


                CodeMethodInvokeExpression mi = new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), isname),
                    "GetCurrentState",
                    new CodeExpression[] { });

                CodeVariableDeclarationStatement vdec = new CodeVariableDeclarationStatement(
                    typeof(System.String),
                   "nowstate",
                    mi);

                doTest.Statements.Add(vdec);

                string ts = string.Format(" running Action {0}Event_{1}", _smName, ev.Key);
                CodeVariableDeclarationStatement vdec2 = new CodeVariableDeclarationStatement(
                   typeof(System.String),
                  "runs",
                   new CodePrimitiveExpression(ts));

                doTest.Statements.Add(vdec2);

                string fmt = "{0} In state {1} with id = {2} inst = {3}";
                CodeMethodInvokeExpression mi2 = new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression("System.String"),
                    "Format",
                    new CodeExpression[]
                    {
                      new CodePrimitiveExpression(fmt),
                      new CodeVariableReferenceExpression("runs"),
                      new CodeVariableReferenceExpression("nowstate"),
                      new CodeTypeReferenceExpression("e.Sm_ID"),
                      new CodeTypeReferenceExpression("e.Sm_Instance"),
                    });
                CodeVariableDeclarationStatement vdec3 = new CodeVariableDeclarationStatement(
                   typeof(System.String),
                  "message",
                   mi2);

                doTest.Statements.Add(vdec3);

                if (_language != "c#")
                {
                    CodeMethodInvokeExpression mi3 = new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression("Console"),
                        "WriteLine",
                        new CodeExpression[] { new CodeVariableReferenceExpression("message") });
                    doTest.Statements.Add(mi3);
                }
                else
                {
                    CodeMethodInvokeExpression mi4 = new CodeMethodInvokeExpression(
                        new CodeThisReferenceExpression(),
                        "Writer",
                        new CodeExpression[] { new CodeVariableReferenceExpression("message") });
                    doTest.Statements.Add(mi4);
                }

                doTest.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));

                TestClassType.Members.Add(doTest);
            }
        }

        public void GenerateWinMain()
        {
            try
            {
                MainWindowUnit = new CodeCompileUnit();
                CodeNamespace Machine = new CodeNamespace("sm_" + _smName);
                Machine.Imports.Add(new CodeNamespaceImport("System"));
                Machine.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
                Machine.Imports.Add(new CodeNamespaceImport("System.Linq"));

                Machine.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
                Machine.Imports.Add(new CodeNamespaceImport("System.Threading.Tasks"));
                Machine.Imports.Add(new CodeNamespaceImport("System.Windows"));
                Machine.Imports.Add(new CodeNamespaceImport("System.Windows.Controls"));
                Machine.Imports.Add(new CodeNamespaceImport("System.Windows.Data"));
                Machine.Imports.Add(new CodeNamespaceImport("System.Windows.Documents"));
                Machine.Imports.Add(new CodeNamespaceImport("System.Windows.Input"));
                Machine.Imports.Add(new CodeNamespaceImport("System.Windows.Media"));
                Machine.Imports.Add(new CodeNamespaceImport("System.Windows.Media.Imaging"));
                Machine.Imports.Add(new CodeNamespaceImport("System.Windows.Navigation"));
                Machine.Imports.Add(new CodeNamespaceImport("System.Windows.Shapes"));
                Machine.Imports.Add(new CodeNamespaceImport("System.IO"));
                Machine.Imports.Add(new CodeNamespaceImport("System.Xaml"));

                MainWindowClassType = new CodeTypeDeclaration("MainWindow");
                MainWindowClassType.TypeAttributes = TypeAttributes.Public;
                MainWindowClassType.Attributes = MemberAttributes.Private;
                MainWindowClassType.IsClass = true;
                MainWindowClassType.IsPartial = true;

                Machine.Types.Add(MainWindowClassType);
                MainWindowUnit.Namespaces.Add(Machine);

                string isname = "_" + _smName.ToLower();

                CodeMemberField mField = new CodeMemberField();
                mField.Attributes = MemberAttributes.Private;
                mField.Name = isname;
                mField.Type = new CodeTypeReference("sm_" + _smName + "." + _smName);
                MainWindowClassType.Members.Add(mField);

                CodeMemberField mField2 = new CodeMemberField();
                mField2.Attributes = MemberAttributes.Private;
                mField2.Name = "chevnt";
                mField2.Type = new CodeTypeReference(typeof(System.String));
                mField2.Comments.Add(new CodeCommentStatement(
                    "User's event to try running in current state"));
                MainWindowClassType.Members.Add(mField2);

                CodeMemberProperty iidProperty = new CodeMemberProperty();
                iidProperty.Attributes =
                    MemberAttributes.Public | MemberAttributes.Final;
                iidProperty.Name = _smName;
                iidProperty.HasGet = true;
                iidProperty.HasSet = true;
                iidProperty.Type = new CodeTypeReference("sm_" + _smName + "." + _smName);
                iidProperty.Comments.Add(new CodeCommentStatement("The test instance."));
                iidProperty.GetStatements.Add(new CodeMethodReturnStatement(
                    new CodeFieldReferenceExpression(
                    new CodeThisReferenceExpression(), isname)));

                CodeAssignStatement asd = new CodeAssignStatement(
                                new CodeVariableReferenceExpression(isname),
                                new CodeVariableReferenceExpression("value"));

                iidProperty.SetStatements.Add(asd);
                MainWindowClassType.Members.Add(iidProperty);

                // constructor
                CodeConstructor TestConstructor = new CodeConstructor();
                TestConstructor.Attributes = MemberAttributes.Public;



                string mev = "      InitializeComponent();\r\n";
                mev += "      Loaded += MainWindow_Loaded;\r\n";
                CodeSnippetStatement zst = new CodeSnippetStatement(mev);
                TestConstructor.Statements.Add(zst);

                CodeObjectCreateExpression objectCreate =
                  new CodeObjectCreateExpression(
                  new CodeTypeReference(_smName));

                CodeFieldReferenceExpression refs = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), isname);

                TestConstructor.Statements.Add(new CodeAssignStatement(
                    refs,
                    objectCreate));

                MainWindowClassType.Members.Add(TestConstructor);

                MainWindowClassType.Members.Add(GenerateStartTest());
                MainWindowClassType.Members.Add(GenerateEndTest());

                CreateWinMainInputMethods(ref MainWindowClassType);
                CreateWinMainActionMethods(ref MainWindowClassType);
                CreateTestEventReadyMethods(ref MainWindowClassType);

                string mstart;


                //// ***** ShowList()
                mstart = " public void MainWindow_Loaded(object sender, RoutedEventArgs e)\r\n";
                mstart += "   {\r\n";
                Dictionary<Int64, string> evlist = new Dictionary<Int64, string>();
                Dictionary<string, Int64> tmpev = new Dictionary<string, Int64>();

                mstart += "     var assembly = System.Reflection.Assembly.GetExecutingAssembly();\r\n";
                mstart += "     string[] lnst = assembly.GetManifestResourceNames();\r\n";
                mstart += string.Format("     var resourceName = \"{0}.{1}.xml\";\r\n", SmAppNameSpace, SmSmName);
                mstart += "     using (Stream stream = assembly.GetManifestResourceStream(resourceName))\r\n";
                mstart += "     {\r\n";
                mstart += "         using (TextReader Tr = new StreamReader(stream))\r\n";
                mstart += "        {\r\n";
                mstart += "            string xml = Tr.ReadToEnd();\r\n";
                mstart += "           StateMachineXMLBlock.Text = xml;\r\n";
                mstart += "          Tr.Close();\r\n";
                mstart += "       }\r\n";
                mstart += "      }\r\n";

                //    mstart += string.Format("     TextReader Tr = new StreamReader(\".\\\\{0}.Xml\");\r\n", SmSmName);
                //    mstart += "TextReader Tr = new StreamReader(\".\\\\Beast.Xml\");\r\n";
                //    mstart += "     string xml = Tr.ReadToEnd();\r\n";
                //   mstart += "     StateMachineXMLBlock.Text = xml;\r\n";
                //    mstart += "     Tr.Close();\r\n\r\n";

                mstart += "     StartTest();\r\n";

                int ii = 0;
                mstart += "       string statelist = \"Valid Events:\";\r\n";
                foreach (KeyValuePair<string, Int64> ev in _evntdict)
                {
                    evlist[ii] = ev.Key;
                    tmpev[ev.Key] = ii;
                    ii++;
                }

                foreach (KeyValuePair<string, Int64> sta in _stateDictionary)
                {
                    mstart += string.Format("       statelist += \"state {0} -- \";\r\n", sta.Key);
                    if (_transitions.ContainsKey(sta.Key))
                    {
                        Dictionary<string, Tranval> etrans = _transitions[sta.Key];
                        foreach (KeyValuePair<string, Tranval> tr in etrans)
                        {
                            mstart += string.Format("       statelist += \"{0}, \";\r\n", tr.Key);
                        }
                        mstart += "       statelist  += \"\\r\\n\";\r\n";
                    }
                    else
                    {
                        mstart += string.Format("       statelist += \"{0} -- NoEvents\";\r\n", sta.Key);
                    }

                }
                mstart += "     InputBlock.Text = statelist;\r\n\r\n";

                foreach (KeyValuePair<string, Int64> ev in _evntdict)
                {
                    mstart += "       BuildButton(\"" + ev.Key + "\");\r\n";
                }

                mstart += "   }\r\n";

                CodeSnippetTypeMember dMember = new CodeSnippetTypeMember(mstart);
                MainWindowClassType.Members.Add(dMember);

                //// buildButton
                mstart = "  private void BuildButton(string eventname)\r\n";
                mstart += "   {\r\n";
                mstart += "       Button button = new Button();\r\n";
                mstart += "        button.Tag = eventname;\r\n";
                mstart += "        button.Content = eventname;\r\n";
                mstart += "       button.Click += EventButtonOnClick;\r\n";
                mstart += "        InputWrapPanel.Children.Add(button);\r\n";
                mstart += "   }\r\n";
                CodeSnippetTypeMember fMember = new CodeSnippetTypeMember(mstart);
                MainWindowClassType.Members.Add(fMember);


                //mstart = "string Chevnt { get; set; }";
                //CodeSnippetTypeMember wwMember = new CodeSnippetTypeMember(mstart);
                //MainWindowClassType.Members.Add(wwMember);

                //// ****************** Writer
                mstart = "    public void Writer(string text)\r\n";
                mstart += "   {\r\n";
                mstart += "     OutputBlock.Text += text + \"\\r\\n\";\r\n";
                mstart += "     InvalidateVisual();\r\n";
                mstart += "   }\r\n";
                CodeSnippetTypeMember fccMember = new CodeSnippetTypeMember(mstart);
                MainWindowClassType.Members.Add(fccMember);

                //// ***************** EventButtonOnClick
                mstart = "    private void EventButtonOnClick(object sender, RoutedEventArgs routedEventArgs)\r\n";
                mstart += "   {\r\n";
                mstart += "      Button button = sender as Button;\r\n";
                mstart += "      string st = button.Tag as string;\r\n";

                ii = 0;
                mstart += "          try\r\n";
                mstart += "          {\r\n";
                mstart += "             bool retv;\r\n";
                mstart += "             string evtst = null;\r\n";
                mstart += "             switch(st)\r\n";
                mstart += "             {\r\n";
                foreach (KeyValuePair<Int64, string> ev in evlist)
                {
                    mstart += string.Format("                case \"{0}\":\r\n", ev.Value);
                    mstart += string.Format("                   this.chevnt = \"{0}\";\r\n", ev.Value);
                    mstart += string.Format("                   evtst = {0}.{1}InputReady.EventsReadyFromState({0}.GetCurrentState());\r\n", isname, _smName);
                    mstart += "                   if(evtst != null)\r\n";
                    mstart += "                   {\r\n";
                    mstart += string.Format("                   retv = {0}.Run{1}Event{2}(DateTime.Now.ToString());\r\n", isname, _smName, ev.Value);
                    mstart += "                   }\r\n";
                    mstart += "                   break;\r\n";
                }
                mstart += "             }\r\n";
                mstart += "          }\r\n";
                mstart += "          catch(Exception ee)\r\n";
                mstart += "          {\r\n";
                mstart += "             Writer(ee.Message);\r\n";
                mstart += "          }\r\n";
                mstart += string.Format("          StateTextBlock.Text = {0}.GetCurrentState();\r\n", isname);
                mstart += "   }\r\n";

                CodeSnippetTypeMember eMember = new CodeSnippetTypeMember(mstart);
                MainWindowClassType.Members.Add(eMember);

                GenerateCode(WinCodeName, MainWindowUnit);
            }
            catch (Exception ee)
            {
                _errList.Add(ee.Message);
            }
        }

        #endregion

        #region Project files (most Obselete)

        public bool GenerateProjectFiles()
        {
            if (_language == "c#")
            {
                GenerateSettingDesigner();
                GenerateProjFile();
                GenerateAssembly();
                GenerateSettingsSettingsFile();
                GenerateSM_designerFile();
                GenerateSmRunFiles();
                GenerateAppCfg();
                return (true);
            }
            else
            {
                _errList.Add("Only CSharp supported for project files");
                return (false);
            }
        }

        public void GenerateSettingDesigner()
        {
            FileInfo af = new FileInfo(SettingsDesignerName);
            if (af.Exists == false)
            {
                CodeCompileUnit SettingUnit = new CodeCompileUnit();
                CodeNamespace Machine = new CodeNamespace("sm_" + _smName + ".Properties");
                CodeTypeDeclaration SettingClassType = new CodeTypeDeclaration("Settings");
                SettingClassType.TypeAttributes = TypeAttributes.Sealed | TypeAttributes.NestedAssembly;
                SettingClassType.CustomAttributes.Add(new CodeAttributeDeclaration(
                    "global::System.Runtime.CompilerServices.CompilerGeneratedAttribute"));

                CodeAttributeDeclaration anat = new CodeAttributeDeclaration("global::System.CodeDom.Compiler.GeneratedCodeAttribute");
                anat.Arguments.Add(
                    new CodeAttributeArgument(new CodePrimitiveExpression(
                "Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator")));
                anat.Arguments.Add(
                    new CodeAttributeArgument(
                        new CodePrimitiveExpression("9.0.0.0")));

                SettingClassType.CustomAttributes.Add(anat);

                SettingClassType.Attributes = MemberAttributes.Private;
                SettingClassType.IsClass = true;
                SettingClassType.IsPartial = true;
                SettingClassType.BaseTypes.Add(new CodeTypeReference("global::System.Configuration.ApplicationSettingsBase"));

                string mstart = " private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));\r\n";
                CodeSnippetTypeMember dMember = new CodeSnippetTypeMember(mstart);
                SettingClassType.Members.Add(dMember);

                mstart = " public static Settings Default {\r\n";
                mstart += "     get {\r\n";
                mstart += "    return defaultInstance;\r\n";
                mstart += "   }\r\n";
                mstart += " }\r\n";
                CodeSnippetTypeMember kMember = new CodeSnippetTypeMember(mstart);
                SettingClassType.Members.Add(kMember);

                CodeMemberProperty kkProperty = new CodeMemberProperty();
                kkProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                kkProperty.Name = "sm_runConnectionString";
                kkProperty.HasGet = true;
                kkProperty.HasSet = false;
                kkProperty.Type = new CodeTypeReference(typeof(System.String));

                kkProperty.CustomAttributes.Add(new CodeAttributeDeclaration(
                    "global::System.Configuration.ApplicationScopedSettingAttribute"));
                kkProperty.CustomAttributes.Add(new CodeAttributeDeclaration(
                   "global::System.Diagnostics.DebuggerNonUserCodeAttribute"));
                kkProperty.CustomAttributes.Add(new CodeAttributeDeclaration(
                  "global::System.Configuration.SpecialSettingAttribute",
                  new CodeAttributeArgument(
                      new CodeSnippetExpression("global::System.Configuration.SpecialSetting.ConnectionString"))));
                kkProperty.CustomAttributes.Add(new CodeAttributeDeclaration(
                  "global::System.Configuration.DefaultSettingValueAttribute",
                  new CodeAttributeArgument(new CodePrimitiveExpression(
                      "Data Source=DRAGON-NAP\\SQLEXPRESS;Initial Catalog=sm_run;Integrated Security=True;Pooling=False"))));
                kkProperty.GetStatements.Add(
                    new CodeMethodReturnStatement(
                        new CodeCastExpression("System.String",
                             new CodeArrayIndexerExpression(
                               new CodeSnippetExpression("this"),
                     new CodePrimitiveExpression("sm_runConnectionString")))));
                SettingClassType.Members.Add(kkProperty);

                Machine.Types.Add(SettingClassType);
                SettingUnit.Namespaces.Add(Machine);
                GenerateCode(SettingsDesignerName, SettingUnit);
            }
        }

        public void GenerateProjFile()
        {
            FileInfo af = new FileInfo(ProjectFileName);
            //           if (af.Exists == false)
            {
                XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
                XElement xml = new XElement(ns + "Project",
                    new XAttribute("ToolsVersion", "3.5"),
                    new XAttribute("DefaultTargets", "Build"),
                    new XAttribute("xmlns", ns),
                    new XElement(ns + "PropertyGroup",
                        new XElement(ns + "Configuration",
                            new XAttribute("Condition", " '$(Configuration)' == '' "),
                            "Debug"
                        ),
                        new XElement(ns + "Platform",
                            new XAttribute("Condition", " '$(Platform)' == '' "),
                            "AnyCPU"
                        ),
                        new XElement(ns + "ProductVersion", "9.0.21022"),
                        new XElement(ns + "SchemaVersion", "2.0"),
                        new XElement(ns + "ProjectGuid", "{8AEA9F97-8F96-44F9-BD42-75192A4E1626}"),
                        new XElement(ns + "OutputType", "Library"),
                        new XElement(ns + "AppDesignerFolder", "Properties"),
                        new XElement(ns + "RootNamespace", "sm_StarShip"),
                        new XElement(ns + "AssemblyName", "sm_StarShip"),
                        new XElement(ns + "TargetFrameworkVersion", "v3.5"),
                        new XElement(ns + "FileAlignment", "512")
                    ),
                    new XElement(ns + "PropertyGroup",
                        new XAttribute("Condition", " '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "),
                        new XElement(ns + "DebugSymbols", "true"),
                        new XElement(ns + "DebugType", "full"),
                        new XElement(ns + "Optimize", "false"),
                        new XElement(ns + "OutputPath", "bin\\Debug\\"),
                        new XElement(ns + "DefineConstants", "DEBUG;TRACE"),
                        new XElement(ns + "ErrorReport", "prompt"),
                        new XElement(ns + "WarningLevel", "4")
                    ),
                    new XElement(ns + "PropertyGroup",
                        new XAttribute("Condition", " '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "),
                        new XElement(ns + "DebugType", "pdbonly"),
                        new XElement(ns + "Optimize", "true"),
                        new XElement(ns + "OutputPath", "bin\\Release\\"),
                        new XElement(ns + "DefineConstants", "TRACE"),
                        new XElement(ns + "ErrorReport", "prompt"),
                        new XElement(ns + "WarningLevel", "4")
                    ),
                    new XElement(ns + "ItemGroup",
                        new XElement(ns + "Reference",
                            new XAttribute("Include", "System")
                        ),
                        new XElement(ns + "Reference",
                            new XAttribute("Include", "System.Core"),
                            new XElement(ns + "RequiredTargetFramework", "3.5")
                        ),
                        new XElement(ns + "Reference",
                            new XAttribute("Include", "System.Data.Linq"),
                            new XElement(ns + "RequiredTargetFramework", "3.5")
                        ),
                        new XElement(ns + "Reference",
                            new XAttribute("Include", "System.Xml.Linq"),
                            new XElement(ns + "RequiredTargetFramework", "3.5")
                        ),
                        new XElement(ns + "Reference",
                            new XAttribute("Include", "System.Data.DataSetExtensions"),
                            new XElement(ns + "RequiredTargetFramework", "3.5")
                        ),
                        new XElement(ns + "Reference",
                            new XAttribute("Include", "System.Data")
                        ),
                        new XElement(ns + "Reference",
                            new XAttribute("Include", "System.Xml")
                        )
                    ),
                    new XElement(ns + "ItemGroup",
                        new XElement(ns + "Compile",
                            new XAttribute("Include", "Properties\\AssemblyInfo.cs")
                        ),
                        new XElement(ns + "Compile",
                            new XAttribute("Include", "Properties\\Settings.Designer.cs"),
                            new XElement(ns + "DesignTimeSharedInput", "True"),
                            new XElement(ns + "AutoGen", "True")
                        ),
                        new XElement(ns + "Compile",
                            new XAttribute("Include", "smRun.designer.cs"),
                            new XElement(ns + "AutoGen", "True"),
                            new XElement(ns + "DesignTime", "True"),
                            new XElement(ns + "DependentUpon", "smRun.dbml")
                        ),
                        new XElement(ns + "Compile",
                            new XAttribute("Include", "smStarShip.cs")
                        ),
                        new XElement(ns + "Compile",
                            new XAttribute("Include", "smStarShipAction.cs")
                        ),
                        new XElement(ns + "Compile",
                            new XAttribute("Include", "smStarShipEventdata.cs")
                        ),
                        new XElement(ns + "Compile",
                            new XAttribute("Include", "smStarShipInputdata.cs")
                        ),
                        new XElement(ns + "Compile",
                            new XAttribute("Include", "smStarShipTestEvntsReady.cs")
                        ),
                         new XElement(ns + "Compile",
                            new XAttribute("Include", "smStarShipTestEvntsReadyData.cs")
                        ),
                        new XElement(ns + "Compile",
                            new XAttribute("Include", "TestStarShip.cs")
                        )
                    ),
                    new XElement(ns + "ItemGroup",
                        new XElement(ns + "None",
                            new XAttribute("Include", "smRun.dbml"),
                            new XElement(ns + "Generator", "MSLinqToSQLGenerator"),
                            new XElement(ns + "LastGenOutput", "smRun.designer.cs"),
                            new XElement(ns + "SubType", "Designer")
                        )
                    ),
                    new XElement(ns + "ItemGroup",
                        new XElement(ns + "Service",
                            new XAttribute("Include", "{3259AA49-8AA1-44D3-9025-A0B520596A8C}")
                        )
                    ),
                    new XElement(ns + "ItemGroup",
                        new XElement(ns + "None",
                            new XAttribute("Include", "app.config")
                        ),
                        new XElement(ns + "None",
                            new XAttribute("Include", "Properties\\Settings.settings"),
                            new XElement(ns + "Generator", "SettingsSingleFileGenerator"),
                            new XElement(ns + "LastGenOutput", "Settings.Designer.cs")
                        ),
                        new XElement(ns + "None",
                            new XAttribute("Include", "smRun.dbml.layout"),
                            new XElement(ns + "DependentUpon", "smRun.dbml")
                        )
                    ),
                    new XElement(ns + "Import",
                        new XAttribute("Project", "$(MSBuildToolsPath)\\Microsoft.CSharp.targets")
                    ),
                    new XComment(
                        " To modify your build process, add your task inside one of the targets below and uncomment it. \n" +
                        "       Other similar extension points exist, see Microsoft.Common.targets.\n" +
                        "  <Target Name=\"BeforeBuild\">\n" +
                        "  </Target>\n" +
                        "  <Target Name=\"AfterBuild\">\n" +
                        "  </Target>\n" +
                        "  "
                    )
                );

                IEnumerable<XElement> de =
                    from el in xml.Descendants(ns + "Compile")
                    select el;

                foreach (XElement el in de)
                {
                    string s2 = (string)el.Attributes("Include").First();
                    el.Attributes("Include").First().SetValue(s2.Replace("StarShip", _smName));
                    string s3 = (string)el.Attributes("Include").First();
                }

                //  new XElement(ns + "RootNamespace", "sm_starship"),
                //  new XElement(ns + "AssemblyName", "sm_starship"),

                IEnumerable<XElement> ve =
                    from el in xml.Descendants(ns + "RootNamespace")
                    select el;

                foreach (XElement el in ve)
                {
                    string s2 = (string)el;
                    el.SetValue(s2.Replace("StarShip", _smName));
                    string s3 = (string)el;
                }

                IEnumerable<XElement> ye =
                                 from el in xml.Descendants(ns + "AssemblyName")
                                 select el;

                foreach (XElement el in ye)
                {
                    string s2 = (string)el;
                    el.SetValue(s2.Replace("StarShip", _smName));
                    string s3 = (string)el;
                }

                IEnumerable<XElement> xe =
                                 from el in xml.Descendants(ns + "ProjectGuid")
                                 select el;

                foreach (XElement el in xe)
                {
                    string s2 = (string)el;

                    string s4 = _smCodeid.ToString().ToUpper();
                    el.SetValue(s4);
                    string s3 = (string)el;
                }

                xml.Save(ProjectFileName, SaveOptions.None);

            }
        }

        public void GenerateAssembly()
        {
            string sa = @"
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
";
            sa += "[assembly: AssemblyTitle(\"sm_Starship\")]\r\n";
            sa += "[assembly: AssemblyDescription(\"\")]\r\n";
            sa += "[assembly: AssemblyConfiguration(\"\")]\r\n";
            sa += "[assembly: AssemblyCompany(\"Unknown\")]\r\n";
            sa += "[assembly: AssemblyProduct(\"sm_Starship\")]\r\n";
            sa += "[assembly: AssemblyCopyright(\"Copyright © Unknown 2008\")]\r\n";
            sa += "[assembly: AssemblyTrademark(\"\")]\r\n";
            sa += "[assembly: AssemblyCulture(\"\")]\r\n";
            sa += @"
// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
";
            sa += "[assembly: Guid(\"3ac45725-fa0a-4f1b-8105-96939c3aee43\")]\r\n";
            sa += @"
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
";
            sa += "// [assembly: AssemblyVersion(\"1.0.*\")]\r\n";
            sa += "[assembly: AssemblyVersion(\"1.0.0.0\")]\r\n";
            sa += "[assembly: AssemblyFileVersion(\"1.0.0.0\")]\r\n";

            sa = sa.Replace("Starship", _smName);
            SimpleWrite(AsmbFileName, sa);
        }

        public void GenerateSM_designerFile()
        {


            FileInfo af = new FileInfo(smRunDesignerFileName);
            if (af.Exists == true)
            {
                //          return;
            }

            string qq = @"#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1433
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace sm_StarShip
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[System.Data.Linq.Mapping.DatabaseAttribute(Name=Quixoricsm_runQuixoric)]
	public partial class smRunDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void Insertsm_Associate(sm_Associate instance);
    partial void Updatesm_Associate(sm_Associate instance);
    partial void Deletesm_Associate(sm_Associate instance);
    partial void InsertsmInstance(smInstance instance);
    partial void UpdatesmInstance(smInstance instance);
    partial void DeletesmInstance(smInstance instance);
    #endregion
		
		public smRunDataContext() : 
				base(global::sm_StarShip.Properties.Settings.Default.sm_runConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public smRunDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public smRunDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public smRunDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public smRunDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<sm_Associate> sm_Associates
		{
			get
			{
				return this.GetTable<sm_Associate>();
			}
		}
		
		public System.Data.Linq.Table<smInstance> smInstances
		{
			get
			{
				return this.GetTable<smInstance>();
			}
		}
	}
	
	[Table(Name=Quixoricdbo.sm_AssociateQuixoric)]
	public partial class sm_Associate : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _smInstanceID;
		
		private long _smAssociateID;
		
		private long _smAssocIndex;
		
		private EntityRef<smInstance> _smInstance;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnsmInstanceIDChanging(long value);
    partial void OnsmInstanceIDChanged();
    partial void OnsmAssociateIDChanging(long value);
    partial void OnsmAssociateIDChanged();
    partial void OnsmAssocIndexChanging(long value);
    partial void OnsmAssocIndexChanged();
    #endregion
		
		public sm_Associate()
		{
			this._smInstance = default(EntityRef<smInstance>);
			OnCreated();
		}
		
		[Column(Storage=Quixoric_smInstanceIDQuixoric, DbType=QuixoricBigInt NOT NULLQuixoric)]
		public long smInstanceID
		{
			get
			{
				return this._smInstanceID;
			}
			set
			{
				if ((this._smInstanceID != value))
				{
					if (this._smInstance.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnsmInstanceIDChanging(value);
					this.SendPropertyChanging();
					this._smInstanceID = value;
					this.SendPropertyChanged(QuixoricsmInstanceIDQuixoric);
					this.OnsmInstanceIDChanged();
				}
			}
		}
		
		[Column(Storage=Quixoric_smAssociateIDQuixoric, DbType=QuixoricBigInt NOT NULLQuixoric)]
		public long smAssociateID
		{
			get
			{
				return this._smAssociateID;
			}
			set
			{
				if ((this._smAssociateID != value))
				{
					this.OnsmAssociateIDChanging(value);
					this.SendPropertyChanging();
					this._smAssociateID = value;
					this.SendPropertyChanged(QuixoricsmAssociateIDQuixoric);
					this.OnsmAssociateIDChanged();
				}
			}
		}
		
		[Column(Storage=Quixoric_smAssocIndexQuixoric, DbType=QuixoricBigInt NOT NULLQuixoric, IsPrimaryKey=true)]
		public long smAssocIndex
		{
			get
			{
				return this._smAssocIndex;
			}
			set
			{
				if ((this._smAssocIndex != value))
				{
					this.OnsmAssocIndexChanging(value);
					this.SendPropertyChanging();
					this._smAssocIndex = value;
					this.SendPropertyChanged(QuixoricsmAssocIndexQuixoric);
					this.OnsmAssocIndexChanged();
				}
			}
		}
		
		[Association(Name=QuixoricsmInstance_sm_AssociateQuixoric, Storage=Quixoric_smInstanceQuixoric, ThisKey=QuixoricsmInstanceIDQuixoric, IsForeignKey=true)]
		public smInstance smInstance
		{
			get
			{
				return this._smInstance.Entity;
			}
			set
			{
				smInstance previousValue = this._smInstance.Entity;
				if (((previousValue != value) 
							|| (this._smInstance.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._smInstance.Entity = null;
						previousValue.sm_Associates.Remove(this);
					}
					this._smInstance.Entity = value;
					if ((value != null))
					{
						value.sm_Associates.Add(this);
						this._smInstanceID = value.smInstanceID;
					}
					else
					{
						this._smInstanceID = default(long);
					}
					this.SendPropertyChanged(QuixoricsmInstanceQuixoric);
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name=Quixoricdbo.smInstanceQuixoric)]
	public partial class smInstance : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _smInstanceID;
		
		private string _smName;
		
		private string _smDataTableName;
		
		private System.DateTime _smLastTranTime;
		
		private long _smID;
		
		private string _smCurrentState;
		
		private EntitySet<sm_Associate> _sm_Associates;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnsmInstanceIDChanging(long value);
    partial void OnsmInstanceIDChanged();
    partial void OnsmNameChanging(string value);
    partial void OnsmNameChanged();
    partial void OnsmDataTableNameChanging(string value);
    partial void OnsmDataTableNameChanged();
    partial void OnsmLastTranTimeChanging(System.DateTime value);
    partial void OnsmLastTranTimeChanged();
    partial void OnsmIDChanging(long value);
    partial void OnsmIDChanged();
    partial void OnsmCurrentStateChanging(string value);
    partial void OnsmCurrentStateChanged();
    #endregion
		
		public smInstance()
		{
			this._sm_Associates = new EntitySet<sm_Associate>(new Action<sm_Associate>(this.attach_sm_Associates), new Action<sm_Associate>(this.detach_sm_Associates));
			OnCreated();
		}
		
		[Column(Storage=Quixoric_smInstanceIDQuixoric, AutoSync=AutoSync.OnInsert, DbType=QuixoricBigInt NOT NULL IDENTITYQuixoric, IsPrimaryKey=true, IsDbGenerated=true)]
		public long smInstanceID
		{
			get
			{
				return this._smInstanceID;
			}
			set
			{
				if ((this._smInstanceID != value))
				{
					this.OnsmInstanceIDChanging(value);
					this.SendPropertyChanging();
					this._smInstanceID = value;
					this.SendPropertyChanged(QuixoricsmInstanceIDQuixoric);
					this.OnsmInstanceIDChanged();
				}
			}
		}
		
		[Column(Storage=Quixoric_smNameQuixoric, DbType=QuixoricNVarChar(50) NOT NULLQuixoric, CanBeNull=false)]
		public string smName
		{
			get
			{
				return this._smName;
			}
			set
			{
				if ((this._smName != value))
				{
					this.OnsmNameChanging(value);
					this.SendPropertyChanging();
					this._smName = value;
					this.SendPropertyChanged(QuixoricsmNameQuixoric);
					this.OnsmNameChanged();
				}
			}
		}
		
		[Column(Storage=Quixoric_smDataTableNameQuixoric, DbType=QuixoricNVarChar(50)Quixoric)]
		public string smDataTableName
		{
			get
			{
				return this._smDataTableName;
			}
			set
			{
				if ((this._smDataTableName != value))
				{
					this.OnsmDataTableNameChanging(value);
					this.SendPropertyChanging();
					this._smDataTableName = value;
					this.SendPropertyChanged(QuixoricsmDataTableNameQuixoric);
					this.OnsmDataTableNameChanged();
				}
			}
		}
		
		[Column(Storage=Quixoric_smLastTranTimeQuixoric, DbType=QuixoricDateTime NOT NULLQuixoric)]
		public System.DateTime smLastTranTime
		{
			get
			{
				return this._smLastTranTime;
			}
			set
			{
				if ((this._smLastTranTime != value))
				{
					this.OnsmLastTranTimeChanging(value);
					this.SendPropertyChanging();
					this._smLastTranTime = value;
					this.SendPropertyChanged(QuixoricsmLastTranTimeQuixoric);
					this.OnsmLastTranTimeChanged();
				}
			}
		}
		
		[Column(Storage=Quixoric_smIDQuixoric, DbType=QuixoricBigInt NOT NULLQuixoric)]
		public long smID
		{
			get
			{
				return this._smID;
			}
			set
			{
				if ((this._smID != value))
				{
					this.OnsmIDChanging(value);
					this.SendPropertyChanging();
					this._smID = value;
					this.SendPropertyChanged(QuixoricsmIDQuixoric);
					this.OnsmIDChanged();
				}
			}
		}
		
		[Column(Storage=Quixoric_smCurrentStateQuixoric, DbType=QuixoricNVarChar(50) NOT NULLQuixoric, CanBeNull=false)]
		public string smCurrentState
		{
			get
			{
				return this._smCurrentState;
			}
			set
			{
				if ((this._smCurrentState != value))
				{
					this.OnsmCurrentStateChanging(value);
					this.SendPropertyChanging();
					this._smCurrentState = value;
					this.SendPropertyChanged(QuixoricsmCurrentStateQuixoric);
					this.OnsmCurrentStateChanged();
				}
			}
		}
		
		[Association(Name=QuixoricsmInstance_sm_AssociateQuixoric, Storage=Quixoric_sm_AssociatesQuixoric, OtherKey=QuixoricsmInstanceIDQuixoric)]
		public EntitySet<sm_Associate> sm_Associates
		{
			get
			{
				return this._sm_Associates;
			}
			set
			{
				this._sm_Associates.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_sm_Associates(sm_Associate entity)
		{
			this.SendPropertyChanging();
			entity.smInstance = this;
		}
		
		private void detach_sm_Associates(sm_Associate entity)
		{
			this.SendPropertyChanging();
			entity.smInstance = null;
		}
	}
}
#pragma warning restore 1591";

            qq = qq.Replace("Quixoric", "\"");
            qq = qq.Replace("StarShip", _smName);
            SimpleWrite(smRunDesignerFileName, qq);

        }

        public void GenerateSettingsSettingsFile()
        {
            FileInfo af = new FileInfo(SettingsFileName);
            //         if (af.Exists == false)
            {
                XNamespace ns = "http://schemas.microsoft.com/VisualStudio/2004/01/settings";
                XElement xml = new XElement(ns + "SettingsFile",
                    new XAttribute("xmlns", ns),
                    new XAttribute("CurrentProfile", "(Default)"),
                    new XAttribute("GeneratedClassNamespace", "sm_StarShip.Properties"),
                    new XAttribute("GeneratedClassName", "Settings"),
                    new XElement(ns + "Profiles"),
                    new XElement(ns + "Settings",
                        new XElement(ns + "Setting",
                            new XAttribute("Name", "sm_runConnectionString"),
                            new XAttribute("Type", "(Connection string)"),
                            new XAttribute("Scope", "Application"),
                            new XElement(ns + "DesignTimeValue",
                                new XAttribute("Profile", "(Default)"),
                                "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n" +
                                "<SerializableConnectionString xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n" +
                                "  <ConnectionString>Data Source=DRAGON-NAP\\SQLEXPRESS;Initial Catalog=sm_run;Integrated Security=True;Pooling=False</ConnectionString>\n" +
                                "  <ProviderName>System.Data.SqlClient</ProviderName>\n" +
                                "</SerializableConnectionString>"
                            ),
                            new XElement(ns + "Value",
                                new XAttribute("Profile", "(Default)"),
                                "Data Source=DRAGON-NAP\\SQLEXPRESS;Initial Catalog=sm_run;Integrated Security=True;Pooling=False"
                            )
                        )
                    )
                );

                IEnumerable<XElement> xe =
                                       from el in xml.Descendants(ns + "SettingsFile")
                                       select el;

                foreach (XElement el in xe)
                {
                    string s2 = (string)el.Attributes("GeneratedClassNamespace").First();
                    el.Attributes("GeneratedClassNamespace").First().SetValue(s2.Replace("StarShip", _smName));
                    string s3 = (string)el.Attributes("GeneratedClassNamespace").First();
                }

                xml.Save(SettingsFileName, SaveOptions.None);
            }
        }

        public void GenerateSmRunFiles()
        {
            FileInfo af = new FileInfo(SmRunDbmlFileName);
            //        if (af.Exists == false)
            {
                XNamespace ns = "http://schemas.microsoft.com/linqtosql/dbml/2007";
                XElement xml = new XElement(ns + "Database",
                    new XAttribute("Name", "sm_run"),
                    new XAttribute("Class", "smRunDataContext"),
                    new XAttribute("xmlns", ns),
                    new XElement(ns + "Connection",
                        new XAttribute("Mode", "AppSettings"),
                        new XAttribute("ConnectionString", "Data Source=DRAGON-NAP\\SQLEXPRESS;Initial Catalog=sm_run;Integrated Security=True;Pooling=False"),
                        new XAttribute("SettingsObjectName", "sm_StarShip.Properties.Settings"),
                        new XAttribute("SettingsPropertyName", "sm_runConnectionString"),
                        new XAttribute("Provider", "System.Data.SqlClient")
                    ),
                    new XElement(ns + "Table",
                        new XAttribute("Name", "dbo.sm_Associate"),
                        new XAttribute("Member", "sm_Associates"),
                        new XElement(ns + "Type",
                            new XAttribute("Name", "sm_Associate"),
                            new XElement(ns + "Column",
                                new XAttribute("Name", "smInstanceID"),
                                new XAttribute("Type", "System.Int64"),
                                new XAttribute("DbType", "BigInt NOT NULL"),
                                new XAttribute("CanBeNull", "false")
                            ),
                            new XElement(ns + "Column",
                                new XAttribute("Name", "smAssociateID"),
                                new XAttribute("Type", "System.Int64"),
                                new XAttribute("DbType", "BigInt NOT NULL"),
                                new XAttribute("CanBeNull", "false")
                            ),
                            new XElement(ns + "Column",
                                new XAttribute("Name", "smAssocIndex"),
                                new XAttribute("Type", "System.Int64"),
                                new XAttribute("DbType", "BigInt NOT NULL"),
                                new XAttribute("IsPrimaryKey", "true"),
                                new XAttribute("CanBeNull", "false")
                            ),
                            new XElement(ns + "Association",
                                new XAttribute("Name", "smInstance_sm_Associate"),
                                new XAttribute("Member", "smInstance"),
                                new XAttribute("ThisKey", "smInstanceID"),
                                new XAttribute("Type", "smInstance"),
                                new XAttribute("IsForeignKey", "true")
                            )
                        )
                    ),
                    new XElement(ns + "Table",
                        new XAttribute("Name", "dbo.smInstance"),
                        new XAttribute("Member", "smInstances"),
                        new XElement(ns + "Type",
                            new XAttribute("Name", "smInstance"),
                            new XElement(ns + "Column",
                                new XAttribute("Name", "smInstanceID"),
                                new XAttribute("Type", "System.Int64"),
                                new XAttribute("DbType", "BigInt NOT NULL IDENTITY"),
                                new XAttribute("IsPrimaryKey", "true"),
                                new XAttribute("IsDbGenerated", "true"),
                                new XAttribute("CanBeNull", "false")
                            ),
                            new XElement(ns + "Column",
                                new XAttribute("Name", "smName"),
                                new XAttribute("Type", "System.String"),
                                new XAttribute("DbType", "NVarChar(50) NOT NULL"),
                                new XAttribute("CanBeNull", "false")
                            ),
                            new XElement(ns + "Column",
                                new XAttribute("Name", "smDataTableName"),
                                new XAttribute("Type", "System.String"),
                                new XAttribute("DbType", "NVarChar(50)"),
                                new XAttribute("CanBeNull", "true")
                            ),
                            new XElement(ns + "Column",
                                new XAttribute("Name", "smLastTranTime"),
                                new XAttribute("Type", "System.DateTime"),
                                new XAttribute("DbType", "DateTime NOT NULL"),
                                new XAttribute("CanBeNull", "false")
                            ),
                            new XElement(ns + "Column",
                                new XAttribute("Name", "smID"),
                                new XAttribute("Type", "System.Int64"),
                                new XAttribute("DbType", "BigInt NOT NULL"),
                                new XAttribute("CanBeNull", "false")
                            ),
                            new XElement(ns + "Column",
                                new XAttribute("Name", "smCurrentState"),
                                new XAttribute("Type", "System.String"),
                                new XAttribute("DbType", "NVarChar(50) NOT NULL"),
                                new XAttribute("CanBeNull", "false")
                            ),
                            new XElement(ns + "Association",
                                new XAttribute("Name", "smInstance_sm_Associate"),
                                new XAttribute("Member", "sm_Associates"),
                                new XAttribute("OtherKey", "smInstanceID"),
                                new XAttribute("Type", "sm_Associate")
                            )
                        )
                    )
                );

                IEnumerable<XElement> xe =
                       from el in xml.Descendants(ns + "Connection")
                       select el;

                foreach (XElement el in xe)
                {
                    string s2 = (string)el.Attributes("SettingsObjectName").First();
                    el.Attributes("SettingsObjectName").First().SetValue(s2.Replace("StarShip", _smName));
                    string s3 = (string)el.Attributes("SettingsObjectName").First();
                }


                xml.Save(SmRunDbmlFileName, SaveOptions.None);
            }

            af = new FileInfo(SmRunLayoutFileName);
            if (af.Exists == false)
            {

                XElement xml = new XElement("ordesignerObjectsDiagram",
                new XAttribute("dslVersion", "1.0.0.0"),
                new XAttribute("absoluteBounds", "0, 0, 11, 8.5"),
                new XAttribute("name", "smRun"),
                new XElement("DataContextMoniker",
                    new XAttribute("Name", "/smRunDataContext")
                ),
                new XElement("nestedChildShapes",
                    new XElement("classShape",
                        new XAttribute("Id", "4a53a0d3-b070-4b45-83df-ed41cde4c417"),
                        new XAttribute("absoluteBounds", "1.5, 1.75, 2, 1.3862939453125"),
                        new XElement("DataClassMoniker",
                            new XAttribute("Name", "/smRunDataContext/sm_Associate")
                        ),
                        new XElement("nestedChildShapes",
                            new XElement("elementListCompartment",
                                new XAttribute("Id", "33cb66bc-70c0-4b14-b671-38e27cb1951a"),
                                new XAttribute("absoluteBounds", "1.5150000000000001, 2.21, 1.9700000000000002, 0.8262939453125"),
                                new XAttribute("name", "DataPropertiesCompartment"),
                                new XAttribute("titleTextColor", "Black"),
                                new XAttribute("itemTextColor", "Black")
                            )
                        )
                    ),
                    new XElement("classShape",
                        new XAttribute("Id", "a711a423-a916-48e2-94af-f3ce4def2cf2"),
                        new XAttribute("absoluteBounds", "4.375, 3.375, 2, 1.9631982421875"),
                        new XElement("DataClassMoniker",
                            new XAttribute("Name", "/smRunDataContext/smInstance")
                        ),
                        new XElement("nestedChildShapes",
                            new XElement("elementListCompartment",
                                new XAttribute("Id", "2a9a10e3-10e0-4048-9405-50436a701eaf"),
                                new XAttribute("absoluteBounds", "4.39, 3.835, 1.9700000000000002, 1.4031982421875"),
                                new XAttribute("name", "DataPropertiesCompartment"),
                                new XAttribute("titleTextColor", "Black"),
                                new XAttribute("itemTextColor", "Black")
                            )
                        )
                    ),
                    new XElement("associationConnector",
                        new XAttribute("edgePoints", "[(4.375 : 4.35659912109375); (2.5 : 4.35659912109375); (2.5 : 3.1362939453125)]"),
                        new XAttribute("fixedFrom", "Algorithm"),
                        new XAttribute("fixedTo", "Algorithm"),
                        new XElement("AssociationMoniker",
                            new XAttribute("Name", "/smRunDataContext/smInstance/smInstance_sm_Associate")
                        ),
                        new XElement("nodes",
                            new XElement("classShapeMoniker",
                                new XAttribute("Id", "a711a423-a916-48e2-94af-f3ce4def2cf2")
                            ),
                            new XElement("classShapeMoniker",
                                new XAttribute("Id", "4a53a0d3-b070-4b45-83df-ed41cde4c417")
                            )
                        )
                    )
                )
            );

                xml.Save(SmRunLayoutFileName, SaveOptions.None);
            }
        }

        public void GenerateAppCfg()
        {
            string cfg = "   <?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                     "   <configuration>" +
                     "      <configSections>" +
                     "       </configSections>" +
                     "       <connectionStrings>" +
                     "          <add name=\"sm_StarShip.Properties.Settings.sm_runConnectionString\"" +
                     "              connectionString=\"Data Source=localhost\\SQLEXPRESS;Initial Catalog=sm_run;Integrated Security=True;Pooling=False\"" +
                     "             providerName=\"System.Data.SqlClient\" />" +
                     "      </connectionStrings>" +
                     "   </configuration> ";

            cfg.Replace("sm_StarShip", "sm_" + _smName);
            SimpleWrite(cfgFileName, cfg);
        }

        public void OutputNSMXmlFile(string fname)
        {

            if (Directory.Exists(_scriptdir) == false)
            {
                try
                {
                    Directory.CreateDirectory(_scriptdir);
                }
                catch (Exception ee)
                {
                    _errList.Add(ee.Message);
                    return;
                }
            }

            string scriptname;
            if (fname == null || fname.Length == 0)
            {

                DirectoryInfo df = new DirectoryInfo(_scriptdir);
                scriptname = df.FullName + "\\" + _smName + ".xml";
            }
            else
            {
                scriptname = fname;
            }

            XElement srcTree = new XElement("StateMachine");
            srcTree.Add(new XAttribute("Name", _smName));
            srcTree.Add(new XAttribute("Start", _smInitialstate));
            srcTree.Add(new XAttribute("Current", _smInitialstate));
            XElement Acts = new XElement("Actions");
            foreach (KeyValuePair<string, Int64> ev in _actiondict)
            {
                XElement ActName = new XElement("Action");
                ActName.Add(new XAttribute("ActionName", ev.Key));
                Acts.Add(ActName);
            }
            XElement Evts = new XElement("Events");
            foreach (KeyValuePair<string, Int64> ev in _evntdict)
            {
                XElement EveName = new XElement("Event");
                EveName.Add(new XAttribute("EventName", ev.Key));
                Evts.Add(EveName);
            }
            XElement Stas = new XElement("States");
            foreach (KeyValuePair<string, Int64> ev in _stateDictionary)
            {
                XElement StasName = new XElement("State");
                StasName.Add(new XAttribute("StateName", ev.Key));
                Stas.Add(StasName);
            }
            srcTree.Add(Acts);
            srcTree.Add(Evts);
            srcTree.Add(Stas);
            XElement trans = new XElement("Transitions");
            foreach (KeyValuePair<string, Dictionary<string, Tranval>> ev in _transitions)
            {

                Dictionary<string, Tranval> td = ev.Value;
                foreach (KeyValuePair<string, Tranval> tt in td)
                {
                    XElement tran = new XElement("Transition");
                    Tranval vv = tt.Value;
                    tran.Add(new XAttribute("Initial", ev.Key));
                    tran.Add(new XAttribute("Event", vv.Evntname));
                    tran.Add(new XAttribute("Action", vv.Action));
                    tran.Add(new XAttribute("Final", vv.Endstate));
                    trans.Add(tran);
                }
            }
            srcTree.Add(trans);
            srcTree.Save(scriptname, SaveOptions.None);
        }


        public void OutputSMXmlFile(string fname)
        {

            if (Directory.Exists(_scriptdir) == false)
            {
                try
                {
                    Directory.CreateDirectory(_scriptdir);
                }
                catch (Exception ee)
                {
                    _errList.Add(ee.Message);
                    return;
                }
            }

            string scriptname;
            if (fname == null || fname.Length == 0)
            {

                DirectoryInfo df = new DirectoryInfo(_scriptdir);
                scriptname = df.FullName + "\\" + _smName + ".xml";
            }
            else
            {
                scriptname = fname;
            }

            XElement srcTree = new XElement("StateMachine");
            srcTree.Add(new XAttribute("Name", _smName));
            srcTree.Add(new XAttribute("Start", _smInitialstate));
            srcTree.Add(new XAttribute("Current", _smInitialstate));
            XElement Acts = new XElement("Actions");
            foreach (KeyValuePair<string, Int64> ev in _actiondict)
            {
                Acts.Add(new XAttribute(ev.Key, ev.Value));
            }
            XElement Evts = new XElement("Events");
            foreach (KeyValuePair<string, Int64> ev in _evntdict)
            {
                Evts.Add(new XAttribute(ev.Key, ev.Value));
            }
            XElement Stas = new XElement("States");
            foreach (KeyValuePair<string, Int64> ev in _stateDictionary)
            {
                Stas.Add(new XAttribute(ev.Key, ev.Value));
            }
            srcTree.Add(Acts);
            srcTree.Add(Evts);
            srcTree.Add(Stas);
            XElement trans = new XElement("Transitions");
            foreach (KeyValuePair<string, Dictionary<string, Tranval>> ev in _transitions)
            {

                Dictionary<string, Tranval> td = ev.Value;
                foreach (KeyValuePair<string, Tranval> tt in td)
                {
                    XElement tran = new XElement("Transition");
                    Tranval vv = tt.Value;
                    tran.Add(new XAttribute("Initial", ev.Key));
                    tran.Add(new XAttribute("Event", vv.Evntname));
                    tran.Add(new XAttribute("Action", vv.Action));
                    tran.Add(new XAttribute("Final", vv.Endstate));
                    trans.Add(tran);
                }
            }
            srcTree.Add(trans);
            srcTree.Save(scriptname, SaveOptions.None);
        }


        public bool ParseSMXmlFile(string xml_name, ref Int32 errcount)
        {
            if (Directory.Exists(_scriptdir) == false)
            {
                _errList.Add("Directory not found " + _scriptdir);
                errcount++;
                return (false);
            }

            DirectoryInfo df = new DirectoryInfo(_scriptdir);
            string scriptname = df.FullName + "\\" + xml_name;
            errcount = 0;

            XElement srcTree;
            try
            {
                srcTree = XElement.Load(scriptname);
            }
            catch (Exception ee)
            {
                _errList.Add(ee.Message);
                errcount++;
                return (false);
            }

            if (srcTree.Name.LocalName != "StateMachine")
            {
                _errList.Add("The xml file is not a state machine");
                return (false);
            }
            try
            {
                _smName = (string)srcTree.Attribute("Name");

            }
            catch (Exception ee)
            {
                _errList.Add("Problem with Name or Start entry/r/n Error was " + ee.Message);
                if (_smName.Length == 0)
                {
                    _smName = "Unknown";
                }
                errcount++;
            }

            try
            {
                _smInitialstate = (string)srcTree.Attribute("Start");
            }
            catch (Exception ee)
            {
                _errList.Add("Problem with Start entry/r/n Error was " + ee.Message);
                if (_smName.Length == 0)
                {
                    _smInitialstate = "Unknown";
                }
                errcount++;
            }

            IEnumerable<XElement> xe =
                      from el in srcTree.Descendants("Actions")
                      select el;

            _actiondict = new Dictionary<string, long>();
            foreach (XElement zz in xe)
            {
                foreach (XAttribute aa in zz.Attributes())
                {
                    try
                    {
                        _actiondict.Add(aa.Name.LocalName, (Int64)aa);
                    }
                    catch (Exception ee)
                    {
                        string ng = ee.Message;
                        _errList.Add("Problem with Format of sm_Action name " + aa.Name.LocalName);
                        errcount++;
                    }
                }
            }

            IEnumerable<XElement> ye =
                       from el in srcTree.Descendants("Events")
                       select el;

            _evntdict = new Dictionary<string, long>();
            foreach (XElement zz in ye)
            {
                foreach (XAttribute aa in zz.Attributes())
                {
                    try
                    {
                        _evntdict.Add(aa.Name.LocalName, (Int64)aa);
                    }
                    catch (Exception ee)
                    {
                        string ng = ee.Message;
                        _errList.Add("Problem with Format of sm_Event name " + aa.Name.LocalName);
                        errcount++;
                    }
                }
            }

            IEnumerable<XElement> ze =
                       from el in srcTree.Descendants("States")
                       select el;

            _stateDictionary = new Dictionary<string, long>();
            foreach (XElement zz in ze)
            {
                foreach (XAttribute aa in zz.Attributes())
                {
                    try
                    {
                        _stateDictionary.Add(aa.Name.LocalName, (Int64)aa);
                    }
                    catch (Exception ee)
                    {
                        string ng = ee.Message;
                        _errList.Add("Problem with Format of sm_State name " + aa.Name.LocalName);
                        errcount++;
                    }
                }
            }

            IEnumerable<XElement> wwe =
                        from el in srcTree.Descendants("Transition")
                        select el;

            _transitions = new Dictionary<string, Dictionary<string, Tranval>>();
            foreach (XElement zz in wwe)
            {
                Tranval tt = new Tranval();
                try
                {
                    tt.Endstate = (string)zz.Attribute("Final");
                    tt.Evntname = (string)zz.Attribute("Event");
                    tt.Startstate = (string)zz.Attribute("Initial");
                    tt.Action = (string)zz.Attribute("Action");
                    tt.Endstateid = _stateDictionary[tt.Endstate];
                    tt.Actionid = _actiondict[tt.Action];
                    tt.Evntid = _evntdict[tt.Evntname];
                    tt.Startstateid = _stateDictionary[tt.Startstate];
                }
                catch (Exception ee)
                {
                    string dummy = ee.Message;
                    errcount++;
                    _errList.Add("Problem with sm_transition " + zz.ToString());
                    continue;
                }


                if (_transitions.ContainsKey(tt.Startstate))
                {
                    Dictionary<string, Tranval> ttmp = _transitions[tt.Startstate];
                    if (ttmp.ContainsKey(tt.Evntname) == false)  // state event data is unigue
                    {
                        ttmp[tt.Evntname] = tt;
                    }
                }
                else
                {
                    Dictionary<string, Tranval> evdict = new Dictionary<string, Tranval>();
                    evdict[tt.Evntname] = tt;
                    _transitions[tt.Startstate] = evdict;
                }
            }
            if (_evntdict.Count == 0)
            {
                errcount++;
                _errList.Add("No sm_Event entrys found");
            }
            if (_stateDictionary.Count == 0)
            {
                errcount++;
                _errList.Add("No sm_State entrys found");
            }
            if (_actiondict.Count == 0)
            {
                errcount++;
                _errList.Add("No sm_Action entrys found");
            }
            if (_transitions.Count == 0)
            {
                errcount++;
                _errList.Add("No sm_transition entrys found");
            }
            return (true);
        }

        public bool ParseNSMXmlFile(string xml_name, ref Int32 errcount)
        {
            if (Directory.Exists(_scriptdir) == false)
            {
                _errList.Add("Directory not found " + _scriptdir);
                errcount++;
                return (false);
            }

            DirectoryInfo df = new DirectoryInfo(_scriptdir);
            string scriptname = df.FullName + "\\" + xml_name;
            errcount = 0;

            XElement srcTree;
            try
            {
                srcTree = XElement.Load(scriptname);
            }
            catch (Exception ee)
            {
                _errList.Add(ee.Message);
                errcount++;
                return (false);
            }

            if (srcTree.Name.LocalName != "StateMachine")
            {
                _errList.Add("The xml file is not a state machine");
                return (false);
            }
            try
            {
                _smName = (string)srcTree.Attribute("Name");

            }
            catch (Exception ee)
            {
                _errList.Add("Problem with Name or Start entry/r/n Error was " + ee.Message);
                if (_smName.Length == 0)
                {
                    _smName = "Unknown";
                }
                errcount++;
            }

            try
            {
                _smInitialstate = (string)srcTree.Attribute("Start");
            }
            catch (Exception ee)
            {
                _errList.Add("Problem with Start entry/r/n Error was " + ee.Message);
                if (_smName.Length == 0)
                {
                    _smInitialstate = "Unknown";
                }
                errcount++;
            }

            IEnumerable<XElement> xe =
                      from el in srcTree.Descendants("Actions").Elements("Action")
                      select el;

            _actiondict = new Dictionary<string, long>();
            foreach (XElement zz in xe)
            {
                foreach (XAttribute aa in zz.Attributes())
                {
                    try
                    {
                        _actiondict.Add((string)aa, (Int64)0);
                        //  actiondict.Add(aa.Name.LocalName, (Int64)aa);
                    }
                    catch (Exception ee)
                    {
                        string ng = ee.Message;
                        _errList.Add("Problem with Format of sm_Action name " + aa.Name.LocalName);
                        errcount++;
                    }
                }
            }

            IEnumerable<XElement> ye =
                       from el in srcTree.Descendants("Events").Elements("Event")
                       select el;

            _evntdict = new Dictionary<string, long>();
            foreach (XElement zz in ye)
            {
                foreach (XAttribute aa in zz.Attributes())
                {
                    try
                    {
                        _evntdict.Add((string)aa, (Int64)0);
                        //_evntdict.Add(aa.Name.LocalName, (Int64)aa);
                    }
                    catch (Exception ee)
                    {
                        string ng = ee.Message;
                        _errList.Add("Problem with Format of sm_Event name " + aa.Name.LocalName);
                        errcount++;
                    }
                }
            }

            IEnumerable<XElement> ze =
                       from el in srcTree.Descendants("States").Elements("State")
                       select el;

            _stateDictionary = new Dictionary<string, long>();
            foreach (XElement zz in ze)
            {
                foreach (XAttribute aa in zz.Attributes())
                {
                    try
                    {
                        _stateDictionary.Add((string)aa, (Int64)0);
                        //   _stateDictionary.Add(aa.Name.LocalName, (Int64)aa);
                    }
                    catch (Exception ee)
                    {
                        string ng = ee.Message;
                        _errList.Add("Problem with Format of sm_State name " + aa.Name.LocalName);
                        errcount++;
                    }
                }
            }

            IEnumerable<XElement> wwe =
                        from el in srcTree.Descendants("Transition")
                        select el;

            _transitions = new Dictionary<string, Dictionary<string, Tranval>>();
            foreach (XElement zz in wwe)
            {
                Tranval tt = new Tranval();
                try
                {
                    tt.Endstate = (string)zz.Attribute("Final");
                    tt.Evntname = (string)zz.Attribute("Event");
                    tt.Startstate = (string)zz.Attribute("Initial");
                    tt.Action = (string)zz.Attribute("Action");
                    tt.Endstateid = _stateDictionary[tt.Endstate];
                    tt.Actionid = _actiondict[tt.Action];
                    tt.Evntid = _evntdict[tt.Evntname];
                    tt.Startstateid = _stateDictionary[tt.Startstate];
                }
                catch (Exception ee)
                {
                    string dummy = ee.Message;
                    errcount++;
                    _errList.Add("Problem with sm_transition " + zz.ToString());
                    continue;
                }


                if (_transitions.ContainsKey(tt.Startstate))
                {
                    Dictionary<string, Tranval> ttmp = _transitions[tt.Startstate];
                    if (ttmp.ContainsKey(tt.Evntname) == false)  // state event data is unigue
                    {
                        ttmp[tt.Evntname] = tt;
                    }
                }
                else
                {
                    Dictionary<string, Tranval> evdict = new Dictionary<string, Tranval>();
                    evdict[tt.Evntname] = tt;
                    _transitions[tt.Startstate] = evdict;
                }
            }
            if (_evntdict.Count == 0)
            {
                errcount++;
                _errList.Add("No sm_Event entrys found");
            }
            if (_stateDictionary.Count == 0)
            {
                errcount++;
                _errList.Add("No sm_State entrys found");
            }
            if (_actiondict.Count == 0)
            {
                errcount++;
                _errList.Add("No sm_Action entrys found");
            }
            if (_transitions.Count == 0)
            {
                errcount++;
                _errList.Add("No sm_transition entrys found");
            }
            return (true);
        }


        #endregion




        public void GenerateCode(string fileName, CodeCompileUnit cu)
        {
            CodeDomProvider provider;

            provider = CodeDomProvider.CreateProvider("c#");
            try
            {
                provider = CodeDomProvider.CreateProvider(_language);
            }
            catch (Exception ee)
            {
                _errList.Add(ee.Message);
            }

            //          switch (language)
            //          {
            //              case "c#":
            //                  provider = CodeDomProvider.CreateProvider("c#");
            ////                  fext = ".cs";
            //                  break;
            //              case "vb":
            //                  provider = CodeDomProvider.CreateProvider("VisualBasic");
            //  //                fext = ".vb";
            //                  break;
            //              case "JScript":
            //                  provider = CodeDomProvider.CreateProvider("JScript");
            //    //              fext = ".js";
            //                  break;
            //              case "C++":
            //                  provider = CodeDomProvider.CreateProvider("c++");  // new CppCodeProvider(); 
            //                  break;
            //              default:
            //                  provider = CodeDomProvider.CreateProvider("c#");
            //     //             fext = ".cs";
            //                  break;
            //          }

            if (provider.FileExtension[0] == '.')
            {
                fileName += provider.FileExtension;
            }
            else
            {
                fileName += ("." + provider.FileExtension);
            }

            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            options.BlankLinesBetweenMembers = false;
            using (StreamWriter sourceWriter = new StreamWriter(fileName))
            {
                try
                {
                    provider.GenerateCodeFromCompileUnit(
                        cu, sourceWriter, options);
                }
                catch (Exception ee)
                {
                    _errList.Add(ee.Message);
                }
            }
        }



    }
}
