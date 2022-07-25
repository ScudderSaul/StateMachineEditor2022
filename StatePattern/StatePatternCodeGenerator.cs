using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using Microsoft.CSharp;
using SmSimpleData;

namespace StatePattern
{
    public class StatePatternCodeGenerator
    {
        private string _targetDir;
        private string _stateMachineName;
        private string _underDir;
        private string _propertiesDir;

        #region Fields

        #endregion


        #region ctor

        #endregion

        #region properties
        CompilerInfo[] _allCompilerInfo { get; set; }

        public List<string> ErrorList { get; set; }

        public string TargetDir
        {
            get { return _targetDir; }
            set
            {
                _targetDir = value;


            }
        }

        public string Language { get; set; }

        public Dictionary<string, Int64> StateNames
        {
            get; set;
        }

        public Dictionary<string, Int64> ActionNames
        {
            get; set;
        }

        public Dictionary<string, Int64> StateEventNames
        {
            get; set;
        }

        public Dictionary<string, Dictionary<string, Tranval>> Transitions
        {
            get; set;
        }

        public static string SmAppName { get; set; }
        public static string SmAppNameSpace { get; set; }
        public static string SmSmName { get; set; }
        public static string SmProjFiles { get; set; }
        public static string SmProjGuid { get; set; }

        public string WinCodeName { get; private set; }
        public string WindowXamlName { get; private set; }
        public string CfgFileName { get; set; }
        public string AppXamlName { get; private set; }
        public string AppCsName { get; private set; }
        public string AsmbFileName { get; private set; }
        //     private string errlogname = "errLog.txt";
        public string ProjectFileName { get; private set; }
        public string SettingsDesignerName { get; private set; }
        public string SettingsFileName { get; private set; }
        public string ResourceDesignerName { get; private set; }
        public string ResourceFileName { get; private set; }

        public string StateMachineName
        {
            get { return _stateMachineName; }
            set
            {
                _stateMachineName = value;
                _underDir = _targetDir + "\\" + StateMachineName;
                DirectoryInfo info = new DirectoryInfo(_underDir);
                if (info.Exists == false)
                {
                    info.Create();
                }
                _propertiesDir = _targetDir + "\\Properties";
                DirectoryInfo info2 = new DirectoryInfo(_propertiesDir);
                if (info2.Exists == false)
                {
                    info2.Create();
                }

                AppXamlName = _targetDir + "\\App.xaml";
                AppCsName = _targetDir + "\\App.xaml.cs";
                WinCodeName = _targetDir + "\\MainWindow.xaml";
                WindowXamlName = _targetDir + "\\MainWindow.xaml";
                CfgFileName = _targetDir + "\\App.Config";
                AsmbFileName = _propertiesDir + "\\AssemblyInfo.cs";
                //  Errlogname = _targetDir + "\\errLog.Txt";
                ProjectFileName = _targetDir + "\\" + StateMachineName + ".csproj";
                SettingsDesignerName = _propertiesDir + "\\Settings.Designer.cs";
                SettingsFileName = _propertiesDir + "\\Settings.settings";
                ResourceDesignerName = _propertiesDir + "\\Resources.Designer.cs";
                ResourceFileName = _propertiesDir + "\\Resources.resx";

            }
        }

        private string UnderDir
        {
            get { return _underDir; }
            set { _underDir = value; }
        }

        public long StateMachineGuid
        {
            get;
            set;
        }

        public string InstanceGuid
        {
            get;
            set;
        }


        public bool CreateWinProject { get; set; }

        public string InitialState { get; set; }

        #endregion

        #region methods

        #region Generation utility

        List<string> StateEventNamesUsedByState(string statename)
        {
            List<string> StateEventNamesList = new List<string>();
            List<string> UniqueStateEventNamesList = new List<string>();
            if (Transitions.ContainsKey(statename))
            {
                Dictionary<string, Tranval> stateeventdata = Transitions[statename];
                StateEventNamesList = stateeventdata.Keys.ToList<string>();
            }
            foreach (string st in StateEventNamesList)
            {
                if (UniqueStateEventNamesList.Contains(st) == false)
                {
                    UniqueStateEventNamesList.Add(st);
                }
            }

            return (UniqueStateEventNamesList);
        }

        string StateEventActionNameUsedByState(string statename, string stateeventname)
        {
            string actionname = string.Empty;
            if (Transitions.ContainsKey(statename))
            {
                Dictionary<string, Tranval> stateeventdata = Transitions[statename];
                Tranval tranval = stateeventdata[stateeventname];
                actionname = tranval.Action;
            }
            return (actionname);
        }

        private List<string> AllActionsUsedByState(string statename)
        {
            List<string> retlist = new List<string>();
            if (Transitions.ContainsKey(statename))
            {
                Dictionary<string, Tranval> stateeventdata = Transitions[statename];
                foreach (var vv in stateeventdata)
                {
                    if (retlist.Contains(vv.Value.Action) == false)
                    {
                        retlist.Add(vv.Value.Action);
                    }
                }

            }
            return (retlist);
        }

        string StateEventDestinationNameUsedByState(string statename, string stateeventname)
        {
            string destinationname = string.Empty;
            if (Transitions.ContainsKey(statename))
            {
                Dictionary<string, Tranval> stateeventdata = Transitions[statename];
                Tranval tranval = stateeventdata[stateeventname];
                destinationname = tranval.Endstate;
            }
            return (destinationname);
        }

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
                string.Format("{0} machine {1}.", StateMachineName, comment)));
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
                string.Format("{0} machine {1}.", StateMachineName, comment)));
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

        public CodeMemberEvent CreateSomeEvent(string stateevent, string handlername)
        {
            CodeMemberEvent eventa = new CodeMemberEvent();
            eventa.Name = stateevent;
            eventa.Type = new CodeTypeReference(handlername);
            eventa.Attributes = MemberAttributes.Public;
            return (eventa);
        }

        #endregion



        public CodeMemberMethod CreateRunActionForState(string statename, string stateeventname)
        {
            string strunaction = string.Format("Run{0}Action{1}", statename, StateEventActionNameUsedByState(statename, stateeventname));
            string stactionhandler = "StateActionHandler";
            string staction = string.Format("{0}Action{1}", statename, StateEventActionNameUsedByState(statename, stateeventname));
            string aevargs = "ActionsEventArgs";

            CodeMemberMethod doaction = new CodeMemberMethod();
            doaction.Name = strunaction;
            doaction.ReturnType = new CodeTypeReference("System.Boolean");
            doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            CodeVariableDeclarationStatement ash = new CodeVariableDeclarationStatement(
              stactionhandler,
              "actionEventHandler",
              new CodeVariableReferenceExpression(staction));
            doaction.Statements.Add(ash);

            CodeConditionStatement theif = new CodeConditionStatement();  // if
            CodeBinaryOperatorExpression atest = new CodeBinaryOperatorExpression();  // (actioneventhandler != null)
            atest.Right = new CodePrimitiveExpression(null);
            atest.Left = new CodeVariableReferenceExpression("actionEventHandler");
            atest.Operator = CodeBinaryOperatorType.IdentityInequality;
            theif.Condition = atest;

            CodeMethodReturnStatement aret = new CodeMethodReturnStatement();   // return 
            CodeDelegateInvokeExpression exsh = new CodeDelegateInvokeExpression();   // ehv(this, new {0}ActionsEventArgs(smid, instance_id, sm_instguid));

            exsh.TargetObject = new CodeVariableReferenceExpression("actionEventHandler");

            CodeObjectCreateExpression evarg = new CodeObjectCreateExpression();
            evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "StateMachineName"));
            evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "StateMachineId"));
            evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "InstanceGuid"));
            evarg.CreateType = new CodeTypeReference(aevargs);  // {0}ActionsEventArgs(_sm_id, instance_id, sm_instguid)

            exsh.Parameters.Add(new CodeThisReferenceExpression());
            exsh.Parameters.Add(evarg);

            aret.Expression = exsh;

            theif.TrueStatements.Add(aret);
            doaction.Statements.Add(theif);
            doaction.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));

            return (doaction);

        }

        #region BaseInterface

        public void CreateBaseInterface()
        {
            CodeCompileUnit StateBaseUnit = new CodeCompileUnit();
            CodeNamespace codeNamespace = new CodeNamespace(StateMachineName);
            //codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            //codeNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            //if (Language == "c#")
            //{
            //    codeNamespace.Imports.Add(new CodeNamespaceImport("System.Linq"));
            //}
            //codeNamespace.Imports.Add(new CodeNamespaceImport("System.Text"));
            //codeNamespace.Imports.Add(new CodeNamespaceImport("System.IO"));

            string machineClassName = "IState";

            CodeTypeDeclaration stateClassType = new CodeTypeDeclaration(machineClassName);
            stateClassType.IsInterface = true;
            //     stateClassType.TypeAttributes = TypeAttributes.Public;
            codeNamespace.Types.Add(stateClassType);
            StateBaseUnit.Namespaces.Add(codeNamespace);

            var dx = new CodeMemberProperty();
            dx.HasGet = true;
            dx.HasSet = true;
            dx.Name = "StateMachineGuid";
            dx.Type = new CodeTypeReference("System.String");

            stateClassType.Members.Add(dx);

            dx = new CodeMemberProperty();
            dx.HasGet = true;
            dx.HasSet = true;
            dx.Name = "InstanceGuid";
            dx.Type = new CodeTypeReference("System.Guid");

            stateClassType.Members.Add(dx);

            dx = new CodeMemberProperty();
            dx.HasGet = true;
            dx.HasSet = true;
            dx.Name = "StateMachineName";
            dx.Type = new CodeTypeReference("System.String");

            stateClassType.Members.Add(dx);

            dx = new CodeMemberProperty();
            dx.HasGet = true;
            dx.HasSet = true;
            dx.Name = "StateName";
            dx.Type = new CodeTypeReference("System.String");

            stateClassType.Members.Add(dx);

            dx = new CodeMemberProperty();
            dx.HasGet = true;
            dx.HasSet = true;
            dx.Name = "LastAction";
            dx.Type = new CodeTypeReference("System.String");

            stateClassType.Members.Add(dx);

            var cc = new CodeMemberMethod();
            cc.Name = "StateEventCheck";
            cc.ReturnType = new CodeTypeReference("System.String");
            cc.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "stateeventname"));
            stateClassType.Members.Add(cc);

            var cc1 = new CodeMemberMethod();
            cc1.Name = "OnEntry";
            cc1.ReturnType = new CodeTypeReference("System.Boolean");
            cc1.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "stateeventname"));
            stateClassType.Members.Add(cc1);

            var cc2 = new CodeMemberMethod();
            cc2.Name = "OnExit";
            cc2.ReturnType = new CodeTypeReference("System.Boolean");
            cc2.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "stateeventname"));
            stateClassType.Members.Add(cc2);

            GenerateCode(UnderDir + "\\" + "IState", StateBaseUnit);
        }

        #endregion

        #region StateClass

        public void CreateStateClassFields(string statename, ref CodeTypeDeclaration stateClassType)
        {
            // stateClassType.Members.Add(CreateField(typeof(Dictionary<string, object>),
            //"EventActionDictionary",
            // string.Format("Dictionary for state {0} event actions", statename)));

            stateClassType.Members.Add(CreateField(typeof(Dictionary<string, string>),
            "_finalDictionary",
            string.Format("Dictionary for state {0} action success state", statename)));

            stateClassType.Members.Add(CreateField(typeof(System.String),
          "_stateMachineGuid",
           "State machine Guid"));

            stateClassType.Members.Add(CreateField(typeof(System.Guid),
                       "_instanceGuid",
                        "State Machine Instance Guid"));


            stateClassType.Members.Add(CreateField(typeof(System.String),
                   "_stateMachineName",
                    "State Machine Name"));

            stateClassType.Members.Add(CreateField(typeof(System.String),
                 "_stateName",
                  "State Name"));

            stateClassType.Members.Add(CreateField(typeof(System.String),
               "_lastAction",
                "Last Action tried"));


            stateClassType.Members.Add(
                CreateFieldProperty("System.String", "_stateMachineGuid", "StateMachineGuid", "the State Machine's Guid"));

            stateClassType.Members.Add(
                CreateFieldProperty("System.Guid", "_instanceGuid", "InstanceGuid", "the State Machine's Instance Guid"));

            stateClassType.Members.Add(
                CreateFieldProperty("System.String", "_stateMachineName", "StateMachineName", "the State Machine's Name"));

            stateClassType.Members.Add(
              CreateFieldProperty("System.String", "_stateName", "StateName", "the State's Name"));

            stateClassType.Members.Add(
          CreateFieldProperty("System.String", "_lastAction", "LastAction", "the Last Action tried"));

            stateClassType.Members.Add(
               CreateFieldProperty("Dictionary<string, string>", "_finalDictionary", "FinalDictionary", " StateEvent to Final State"));

            stateClassType.Members.Add(CreateStateActionHandler(statename));

        }

        public CodeConstructor CreateStateClassCtor(string statename)
        {
            CodeConstructor StateCtor = new CodeConstructor();
            StateCtor.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            //CodeObjectCreateExpression edictcreate = new CodeObjectCreateExpression("Dictionary<string, object>", new CodeExpression[] { });
            //CodeAssignStatement ecreate = new CodeAssignStatement(
            //    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "EventActionDictionary"),
            //    edictcreate);
            //StateCtor.Statements.Add(ecreate);

            CodeObjectCreateExpression fdictcreate = new CodeObjectCreateExpression("Dictionary<string, string>", new CodeExpression[] { });
            CodeAssignStatement fcreate = new CodeAssignStatement(
             new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "FinalDictionary"),
             fdictcreate);
            StateCtor.Statements.Add(fcreate);

            CodeAssignStatement gcreate = new CodeAssignStatement(
             new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "StateName"),
             new CodePrimitiveExpression(statename));
            StateCtor.Statements.Add(gcreate);

            CodeMethodInvokeExpression adddicts = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "FillStateEventToActionsDictionary", new CodeExpression[] { });
            StateCtor.Statements.Add(adddicts);
            return (StateCtor);
        }

        public void CreateStateClassMembers(string statename, ref CodeTypeDeclaration stateClassType)
        {
            List<string> actionnames = AllActionsUsedByState(statename);
            foreach (string actionname in actionnames)
            {
                stateClassType.Members.Add(CreateSomeEvent(actionname, "StateActionHandler"));
            }

            stateClassType.Members.Add(CreateSomeEvent("StateEntry", "StateActionHandler"));
            stateClassType.Members.Add(CreateSomeEvent("StateExit", "StateActionHandler"));

            stateClassType.Members.Add(CreateStateClassCtor(statename));

            stateClassType.Members.Add(CreateEntryMethod());

            stateClassType.Members.Add(CreateExitMethod());

            stateClassType.Members.Add(CreateFillStateEventToActionsDictionary(statename));
        }

        public CodeMemberMethod CreateEntryMethod()
        {
            CodeMemberMethod doaction = new CodeMemberMethod();
            doaction.Name = "OnEntry";

            doaction.ReturnType = new CodeTypeReference("System.Boolean");
            doaction.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "stateevent"));
            doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            CodeConditionStatement zif = new CodeConditionStatement();  // if
            CodeBinaryOperatorExpression ztest = new CodeBinaryOperatorExpression();  // (sact == null)
            ztest.Right = new CodePrimitiveExpression(null);
            ztest.Left = new CodeEventReferenceExpression(new CodeThisReferenceExpression(), "StateEntry");
            ztest.Operator = CodeBinaryOperatorType.IdentityInequality;
            zif.Condition = ztest;

            CodeMethodReturnStatement aret = new CodeMethodReturnStatement();   // return 
            CodeDelegateInvokeExpression exsh = new CodeDelegateInvokeExpression();

            exsh.TargetObject = new CodeEventReferenceExpression(new CodeThisReferenceExpression(), "StateEntry");

            CodeObjectCreateExpression evarg = new CodeObjectCreateExpression();
            evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "StateMachineName"));
            evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "StateMachineGuid"));
            evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "InstanceGuid"));
            evarg.CreateType = new CodeTypeReference("ActionsEventArgs");

            exsh.Parameters.Add(new CodeThisReferenceExpression());
            exsh.Parameters.Add(evarg);

            CodeVariableDeclarationStatement decl = new CodeVariableDeclarationStatement(
                "System.Boolean",
                "result",
                exsh);

            zif.TrueStatements.Add(decl);

            aret.Expression = new CodePrimitiveExpression(true);
            zif.TrueStatements.Add(aret);

            zif.FalseStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));
            doaction.Statements.Add(zif);
            return (doaction);
        }

        public CodeMemberMethod CreateExitMethod()
        {
            CodeMemberMethod doaction = new CodeMemberMethod();
            doaction.Name = "OnExit";
            doaction.ReturnType = new CodeTypeReference("System.Boolean");
            doaction.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "stateevent"));
            doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            CodeConditionStatement zif = new CodeConditionStatement();  // if
            CodeBinaryOperatorExpression ztest = new CodeBinaryOperatorExpression();  // (sact == null)
            ztest.Right = new CodePrimitiveExpression(null);
            ztest.Left = new CodeEventReferenceExpression(new CodeThisReferenceExpression(), "StateExit");
            ztest.Operator = CodeBinaryOperatorType.IdentityInequality;
            zif.Condition = ztest;

            CodeMethodReturnStatement aret = new CodeMethodReturnStatement();   // return 
            CodeDelegateInvokeExpression exsh = new CodeDelegateInvokeExpression();

            exsh.TargetObject = new CodeEventReferenceExpression(new CodeThisReferenceExpression(), "StateExit");

            CodeObjectCreateExpression evarg = new CodeObjectCreateExpression();
            evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "StateMachineName"));
            evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "StateMachineGuid"));
            evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "InstanceGuid"));
            evarg.CreateType = new CodeTypeReference("ActionsEventArgs");

            exsh.Parameters.Add(new CodeThisReferenceExpression());
            exsh.Parameters.Add(evarg);

            CodeVariableDeclarationStatement decl = new CodeVariableDeclarationStatement(
                "System.Boolean",
                "result",
                exsh);

            zif.TrueStatements.Add(decl);

            aret.Expression = new CodePrimitiveExpression(true);
            zif.TrueStatements.Add(aret);

            zif.FalseStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));

            doaction.Statements.Add(zif);
            return (doaction);
        }

        public CodeMemberMethod CreateStateActionHandler(string statename)
        {
            // string eventactiondict = "EventActionDictionary";
            string ststatedict = "FinalDictionary";

            CodeMemberMethod doaction = new CodeMemberMethod();
            doaction.Name = "StateEventCheck";
            doaction.ReturnType = new CodeTypeReference("System.String");
            doaction.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "stateevent"));
            doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            CodeFieldReferenceExpression dictfield = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                "FinalDictionary");

            List<string> allevents = StateEventNamesUsedByState(statename);
            foreach (string st in allevents)
            {
                string act = StateEventActionNameUsedByState(statename, st);

                CodeConditionStatement zif = new CodeConditionStatement();  // if
                CodeBinaryOperatorExpression ztest = new CodeBinaryOperatorExpression();  // (sact == null)
                ztest.Right = new CodePrimitiveExpression(st);
                ztest.Left = new CodeArgumentReferenceExpression("stateevent");
                ztest.Operator = CodeBinaryOperatorType.IdentityEquality;
                zif.Condition = ztest;

                CodeEventReferenceExpression cev = new CodeEventReferenceExpression(new CodeThisReferenceExpression(), act);

                CodeConditionStatement pif = new CodeConditionStatement();  // if
                CodeBinaryOperatorExpression utest = new CodeBinaryOperatorExpression();
                utest.Right = cev;
                utest.Left = new CodePrimitiveExpression(null);
                utest.Operator = CodeBinaryOperatorType.IdentityEquality;
                pif.Condition = utest;
                pif.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
                zif.TrueStatements.Add(pif);

                zif.TrueStatements.Add(
                    new CodeAssignStatement(
                        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "LastAction"),
                        new CodePrimitiveExpression(act)));

                CodeObjectCreateExpression evarg = new CodeObjectCreateExpression();
                evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "StateMachineName"));
                evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "StateMachineGuid"));
                evarg.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "InstanceGuid"));
                evarg.CreateType = new CodeTypeReference("ActionsEventArgs");


                CodeArrayIndexerExpression ci2 = new CodeArrayIndexerExpression(
                    new CodeVariableReferenceExpression(ststatedict), new CodeArgumentReferenceExpression("stateevent"));
                CodeCastExpression castExpression2 = new CodeCastExpression("System.String", ci2);
                CodeVariableDeclarationStatement ash2 = new CodeVariableDeclarationStatement(
                  "System.String",
                  "endstate",
                  castExpression2);
                zif.TrueStatements.Add(ash2);


                CodeConditionStatement inif = new CodeConditionStatement();  // if
                CodeBinaryOperatorExpression ctest = new CodeBinaryOperatorExpression();
                ctest.Right = new CodePrimitiveExpression(true);
                CodeDelegateInvokeExpression invokesact = new CodeDelegateInvokeExpression(
                    cev,
                    new CodeExpression[] { new CodeThisReferenceExpression(), evarg });
                ctest.Left = invokesact;
                ctest.Operator = CodeBinaryOperatorType.IdentityEquality;
                inif.Condition = ctest;
                inif.TrueStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("endstate")));
                zif.TrueStatements.Add(inif);
                doaction.Statements.Add(zif);
            }

            doaction.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
            doaction.Comments.Add(
                new CodeCommentStatement("Runs the action if it should and returns the new statename if the action is sucessfull"));

            return (doaction);
        }

        //    this.EventDictionary.Add("EnemySuspected", new StateAction(this.Monster_ev.RunMonsterActionLookAbout));
        //    this.MovingFinalDictionary.Add("EnemySuspected", "Searching");
        public CodeMemberMethod CreateFillStateEventToActionsDictionary(string statename)
        {
            CodeMemberMethod filldict = new CodeMemberMethod();
            filldict.Name = "FillStateEventToActionsDictionary";

            List<string> eventnames = StateEventNamesUsedByState(statename);

            foreach (string ename in eventnames)
            {
                //   string actionname = StateEventActionNameUsedByState(statename, ename);
                string finalstatename = StateEventDestinationNameUsedByState(statename, ename);

                //    string stmeventdict = "EventActionDictionary";

                //      string runaction = string.Format("Run{0}Action{1}", statename, actionname);
                //       string runaction = actionname;

                string stfinaldict = "FinalDictionary";

                // CodeObjectCreateExpression evarg = new CodeObjectCreateExpression();

                //    CodeCastExpression castExpression = new CodeCastExpression("StateActionHandler", 
                //        new CodeEventReferenceExpression(new CodeThisReferenceExpression(), runaction));
                //     evarg.Parameters.Add(castExpression);

                //   evarg.CreateType = new CodeTypeReference("StateEventAction");

                //CodeExpression[] ac = new CodeExpression[] { new CodePrimitiveExpression(ename), castExpression };

                //CodeFieldReferenceExpression dictfield = new CodeFieldReferenceExpression(
                //    new CodeThisReferenceExpression(), stmeventdict);
                //CodeMethodInvokeExpression exm = new CodeMethodInvokeExpression(dictfield, "Add", ac);

                //filldict.Statements.Add(exm);

                CodeExpression[] ae = new CodeExpression[] { new CodePrimitiveExpression(ename), new CodePrimitiveExpression(finalstatename) };

                CodeFieldReferenceExpression fdictfield = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), stfinaldict);
                CodeMethodInvokeExpression efa = new CodeMethodInvokeExpression(fdictfield, "Add", ae);

                filldict.Statements.Add(efa);
            }

            filldict.Comments.Add(new CodeCommentStatement("Fills the dictionaries"));
            return (filldict);
        }

        public void CreateStateClass(string statename)
        {
            CodeCompileUnit StateClassUnit = new CodeCompileUnit();
            CodeNamespace codeNamespace = new CodeNamespace(StateMachineName);
            codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            if (Language == "c#")
            {
                codeNamespace.Imports.Add(new CodeNamespaceImport("System.Linq"));
            }
            codeNamespace.Imports.Add(new CodeNamespaceImport("System.Text"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("System.IO"));

            CodeTypeDeclaration stateClassType = new CodeTypeDeclaration(statename);
            stateClassType.IsClass = true;
            stateClassType.BaseTypes.Add("IState");
            stateClassType.TypeAttributes = TypeAttributes.Public;
            codeNamespace.Types.Add(stateClassType);
            StateClassUnit.Namespaces.Add(codeNamespace);

            CreateStateClassFields(statename, ref stateClassType);

            CreateStateClassMembers(statename, ref stateClassType);

            GenerateCode(UnderDir + "\\" + statename, StateClassUnit);
        }

        #endregion

        #region ActionEventArgs
        public void CreateActionEventArgs()
        {
            CodeCompileUnit _actionsEventArgsUnit = new CodeCompileUnit();
            CodeNamespace Machine = new CodeNamespace(StateMachineName);
            Machine.Imports.Add(new CodeNamespaceImport("System"));
            Machine.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            if (Language == "c#")
            {
                Machine.Imports.Add(new CodeNamespaceImport("System.Linq"));
            }
            Machine.Imports.Add(new CodeNamespaceImport("System.Text"));
            Machine.Imports.Add(new CodeNamespaceImport("System.IO"));

            CodeTypeDeclaration argsClassTypeDeclaration = new CodeTypeDeclaration("ActionsEventArgs");
            argsClassTypeDeclaration.TypeAttributes = TypeAttributes.Public;
            argsClassTypeDeclaration.Attributes = MemberAttributes.Private;
            argsClassTypeDeclaration.IsClass = true;
            argsClassTypeDeclaration.BaseTypes.Add("EventArgs");
            Machine.Types.Add(argsClassTypeDeclaration);
            _actionsEventArgsUnit.Namespaces.Add(Machine);

            argsClassTypeDeclaration.Members.Add(CreateField(typeof(System.String),
          "_stateMachineGuid",
           "State machine Guid"));

            argsClassTypeDeclaration.Members.Add(CreateField(typeof(System.Guid),
                       "_instanceGuid",
                        "State Machine Instance Guid"));


            argsClassTypeDeclaration.Members.Add(CreateField(typeof(System.String),
                   "_stateMachineName",
                    "State Machine Name"));

            argsClassTypeDeclaration.Members.Add(
           CreateFieldProperty("System.String", "_stateMachineGuid", "StateMachineGuid", "the unique State Machine type id"));

            argsClassTypeDeclaration.Members.Add(
                CreateFieldProperty("System.Guid", "_instanceGuid", "InstanceGuid", "the State Machine Instance Guid"));

            argsClassTypeDeclaration.Members.Add(
                CreateFieldProperty("System.String", "_stateMachineName", "StateMachineName", "the State Machine Name"));
            CodeConstructor ActionsEventDataConstructor = new CodeConstructor();
            ActionsEventDataConstructor.Attributes = MemberAttributes.Public;

            ActionsEventDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "name"));
            ActionsEventDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "machineguid"));
            ActionsEventDataConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.Guid", "instguid"));
            ActionsEventDataConstructor.Statements.Add(CreateFieldAssign("_stateMachineName", "name"));
            ActionsEventDataConstructor.Statements.Add(CreateFieldAssign("_stateMachineGuid", "machineguid"));
            ActionsEventDataConstructor.Statements.Add(CreateFieldAssign("_instanceGuid", "instguid"));
            argsClassTypeDeclaration.Members.Add(ActionsEventDataConstructor);

            GenerateCode(UnderDir + "\\" + "ActionsEventArgs", _actionsEventArgsUnit);
        }

        #endregion

        #region StateMachineClass

        public CodeMemberMethod CreateEventArivalMethod()
        {
            CodeMemberMethod doaction = new CodeMemberMethod();
            doaction.Name = "MachineEventArrival";

            doaction.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "stateevent"));
            doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            //     CodeExpression[] qc = new CodeExpression[] { doaction.Parameters[0] };

            CodeVariableDeclarationStatement cvd =
                new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), "state",
                new CodeMethodInvokeExpression(
                    new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "CurrentState"),
                    "StateEventCheck", new CodeArgumentReferenceExpression("stateevent")));
            doaction.Statements.Add(cvd);

            //if (state == this.CurrentState.StateName)
            //{
            //    return;
            //}
            var vv = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "CurrentState");
            var vvv = new CodePropertyReferenceExpression(vv, "StateName");

            CodeConditionStatement bif = new CodeConditionStatement();
            CodeBinaryOperatorExpression btest = new CodeBinaryOperatorExpression();
            btest.Right = vvv;
            btest.Left = new CodeVariableReferenceExpression("state");
            btest.Operator = CodeBinaryOperatorType.IdentityEquality;
            bif.Condition = btest;
            bif.TrueStatements.Add(new CodeMethodReturnStatement());
            doaction.Statements.Add(bif);

            CodeConditionStatement zif = new CodeConditionStatement();
            CodeBinaryOperatorExpression ztest = new CodeBinaryOperatorExpression();
            ztest.Right = new CodePrimitiveExpression(null);
            ztest.Left = new CodeVariableReferenceExpression("state");
            ztest.Operator = CodeBinaryOperatorType.IdentityInequality;
            zif.Condition = ztest;

            zif.TrueStatements.Add(new CodeMethodInvokeExpression(
                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "CurrentState"),
                "OnExit", new CodeArgumentReferenceExpression("stateevent")));

            CodeExpression[] sc = new CodeExpression[] { new CodeVariableReferenceExpression("state") };

            zif.TrueStatements.Add(new CodeMethodInvokeExpression(
                new CodeThisReferenceExpression(), "StateFromStateName", sc));

            CodeEventReferenceExpression cee = new CodeEventReferenceExpression(new CodeThisReferenceExpression(), "StateChanged");

            CodeConditionStatement mif = new CodeConditionStatement();
            CodeBinaryOperatorExpression mtest = new CodeBinaryOperatorExpression();
            mtest.Right = new CodePrimitiveExpression(null);
            mtest.Left = cee;
            mtest.Operator = CodeBinaryOperatorType.IdentityInequality;
            mif.Condition = mtest;

            CodeDelegateInvokeExpression exsh = new CodeDelegateInvokeExpression();
            exsh.TargetObject = cee;
            CodeObjectCreateExpression evarg = new CodeObjectCreateExpression();
            evarg.CreateType = new CodeTypeReference("EventArgs");
            exsh.Parameters.Add(new CodeThisReferenceExpression());
            exsh.Parameters.Add(evarg);

            mif.TrueStatements.Add(exsh);

            zif.TrueStatements.Add(mif);

            zif.TrueStatements.Add(new CodeMethodInvokeExpression(
                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "CurrentState"),
                "OnEntry", new CodeArgumentReferenceExpression("stateevent")));
            doaction.Statements.Add(zif);

            return (doaction);
        }

        public CodeMemberMethod CreateStateFromStateNameMethod()
        {
            CodeMemberMethod doaction = new CodeMemberMethod();
            doaction.Name = "StateFromStateName";
            doaction.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "statename"));
            doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            foreach (string st in StateNames.Keys)
            {
                CodeConditionStatement zif = new CodeConditionStatement();
                CodeBinaryOperatorExpression ztest = new CodeBinaryOperatorExpression();
                ztest.Right = new CodeArgumentReferenceExpression("statename");
                ztest.Left = new CodePrimitiveExpression(st);
                ztest.Operator = CodeBinaryOperatorType.IdentityEquality;
                zif.Condition = ztest;

                CodeAssignStatement caas = new CodeAssignStatement(new CodePropertyReferenceExpression(
                    new CodeThisReferenceExpression(), "CurrentState"), new CodeObjectCreateExpression(st));
                zif.TrueStatements.Add(caas);

                CodePropertyReferenceExpression left =
                    new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "CurrentState");

                zif.TrueStatements.Add(new CodeAssignStatement(
                    new CodePropertyReferenceExpression(left, "StateMachineGuid"),
                    new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "StateMachineGuid")));

                zif.TrueStatements.Add(new CodeAssignStatement(
                   new CodePropertyReferenceExpression(left, "InstanceGuid"),
                   new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "InstanceGuid")));

                zif.TrueStatements.Add(new CodeAssignStatement(
                 new CodePropertyReferenceExpression(left, "StateMachineName"),
                 new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "StateMachineName")));

                doaction.Statements.Add(zif);
            }

            return (doaction);
        }

        public CodeConstructor CreateMachineClassCtor()
        {
            CodeConstructor MachineCtor = new CodeConstructor();
            MachineCtor.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            Guid gg = Guid.NewGuid();
            CodeAssignStatement ecreate = new CodeAssignStatement(
             new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_stateMachineGuid"),
             new CodePrimitiveExpression(gg.ToString()));
            MachineCtor.Statements.Add(ecreate);

            CodeExpression[] qc = new CodeExpression[] { };
            CodeAssignStatement fcreate = new CodeAssignStatement(
             new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_instanceGuid"),
             new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Guid"),
             "NewGuid", qc));
            MachineCtor.Statements.Add(fcreate);

            CodeAssignStatement gcreate = new CodeAssignStatement(
              new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_stateMachineName"),
              new CodePrimitiveExpression(StateMachineName));
            MachineCtor.Statements.Add(gcreate);

            CodeExpression[] qcc = new CodeExpression[] { new CodePrimitiveExpression(InitialState) };

            MachineCtor.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "StateFromStateName", qcc));

            return (MachineCtor);
        }

        public void CreateMachineClass()
        {
            CodeCompileUnit MachineClassUnit = new CodeCompileUnit();
            CodeNamespace codeNamespace = new CodeNamespace(StateMachineName);
            codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            if (Language == "c#")
            {
                codeNamespace.Imports.Add(new CodeNamespaceImport("System.Linq"));
            }
            codeNamespace.Imports.Add(new CodeNamespaceImport("System.Text"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("System.IO"));

            CodeTypeDelegate dCodeTypeDelegate = new CodeTypeDelegate("StateActionHandler");
            dCodeTypeDelegate.ReturnType = new CodeTypeReference(typeof(System.Boolean));
            dCodeTypeDelegate.Parameters.Add(new CodeParameterDeclarationExpression("System.Object", "sender"));
            dCodeTypeDelegate.Parameters.Add(new CodeParameterDeclarationExpression("ActionsEventArgs", "e"));
            codeNamespace.Types.Add(dCodeTypeDelegate);

            string machineClassName = "StateMachine" + StateMachineName;

            CodeTypeDeclaration stateClassType = new CodeTypeDeclaration(machineClassName);
            stateClassType.IsClass = true;
            stateClassType.TypeAttributes = TypeAttributes.Public;
            codeNamespace.Types.Add(stateClassType);
            MachineClassUnit.Namespaces.Add(codeNamespace);

            CodeMemberField fi = new CodeMemberField(new CodeTypeReference("IState"), "_currentState");
            stateClassType.Members.Add(fi);

            stateClassType.Members.Add(
            CreateFieldProperty("IState", "_currentState", "CurrentState", "the current state"));

            stateClassType.Members.Add(CreateField(typeof(System.String),
          "_stateMachineGuid",
           "State machine Guid"));

            stateClassType.Members.Add(CreateField(typeof(System.Guid),
                       "_instanceGuid",
                        "State Machine Instance Guid"));

            stateClassType.Members.Add(CreateField(typeof(System.String),
                   "_stateMachineName",
                    "State Machine Name"));

            CreateSomeEvent("StateChanged", "EventHandler");

            stateClassType.Members.Add(CreateSomeEvent("StateChanged", "EventHandler"));

            stateClassType.Members.Add(CreateMachineClassCtor());

            stateClassType.Members.Add(
                CreateFieldProperty("System.String", "_stateMachineGuid", "StateMachineGuid", "the unique State Machine Guid"));

            stateClassType.Members.Add(
                CreateFieldProperty("System.Guid", "_instanceGuid", "InstanceGuid", "the State Machine Instance Guid"));

            stateClassType.Members.Add(
                CreateFieldProperty("System.String", "_stateMachineName", "StateMachineName", "the State Machine Name"));

            stateClassType.Members.Add(CreateEventArivalMethod());

            stateClassType.Members.Add(CreateStateFromStateNameMethod());
            GenerateCode(UnderDir + "\\" + machineClassName, MachineClassUnit);
        }

        #endregion

        #region WinMain

        public void GenerateWinMain()
        {
            try
            {
                CodeCompileUnit MainWindowUnit = new CodeCompileUnit();
                CodeNamespace Machine = new CodeNamespace("Test" + StateMachineName);
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
                Machine.Imports.Add(new CodeNamespaceImport(StateMachineName));

                CodeTypeDeclaration MainWindowClassType = new CodeTypeDeclaration("MainWindow");
                MainWindowClassType.TypeAttributes = TypeAttributes.Public;
                MainWindowClassType.Attributes = MemberAttributes.Private;
                MainWindowClassType.IsClass = true;
                MainWindowClassType.IsPartial = true;

                Machine.Types.Add(MainWindowClassType);
                MainWindowUnit.Namespaces.Add(Machine);

                string isname = "_stateMachine" + StateMachineName;

                CodeMemberField mField = new CodeMemberField();
                mField.Attributes = MemberAttributes.Private;
                mField.Name = isname;
                mField.Type = new CodeTypeReference(StateMachineName + ".StateMachine" + StateMachineName);
                MainWindowClassType.Members.Add(mField);

                CodeMemberField mField2 = new CodeMemberField();
                mField2.Attributes = MemberAttributes.Private;
                mField2.Name = "LastEvent";
                mField2.Type = new CodeTypeReference(typeof(System.String));

                MainWindowClassType.Members.Add(mField2);

                CodeMemberProperty iidProperty = new CodeMemberProperty();
                iidProperty.Attributes =
                    MemberAttributes.Public | MemberAttributes.Final;
                iidProperty.Name = StateMachineName;
                iidProperty.HasGet = true;
                iidProperty.HasSet = true;
                iidProperty.Type = new CodeTypeReference(StateMachineName + ".StateMachine" + StateMachineName);
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

                TestConstructor.Statements.Add(
                    new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InitializeComponent"));

                TestConstructor.Statements.Add(
                    new CodeAttachEventStatement(new CodeThisReferenceExpression(),
                    "Loaded", new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "OnLoaded")));

                CodeObjectCreateExpression objectCreate =
                  new CodeObjectCreateExpression(
                  new CodeTypeReference("StateMachine" + StateMachineName));

                CodeFieldReferenceExpression refs = new
                    CodeFieldReferenceExpression(new CodeThisReferenceExpression(), isname);

                TestConstructor.Statements.Add(new CodeAssignStatement(
                    refs,
                    objectCreate));

                MainWindowClassType.Members.Add(TestConstructor);

                MainWindowClassType.Members.Add(GenerateOnLoaded());

                MainWindowClassType.Members.Add(GenerateWriter());

                MainWindowClassType.Members.Add(GenerateSetEvents());

                MainWindowClassType.Members.Add(GenerateAllActions());

                MainWindowClassType.Members.Add(GenerateAllEntry());

                MainWindowClassType.Members.Add(GenerateAllExit());

                MainWindowClassType.Members.Add(GenerateStateChanged());

                MainWindowClassType.Members.Add(GenerateEventButtonOnClick());

                MainWindowClassType.Members.Add(GenerateBuildButton());

                GenerateCode(WinCodeName, MainWindowUnit);
            }
            catch (Exception ee)
            {
                ErrorList.Add(ee.Message);
            }
        }

        public CodeMemberMethod GenerateSetEvents()
        {
            CodeMemberMethod doaction = new CodeMemberMethod();
            doaction.Name = "SetUpEvents";
            doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            CodeFieldReferenceExpression cfr = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_stateMachine" + StateMachineName);
            CodePropertyReferenceExpression cpr = new CodePropertyReferenceExpression(cfr, "CurrentState");
            CodePropertyReferenceExpression cnn = new CodePropertyReferenceExpression(cpr, "StateName");

            CodeVariableDeclarationStatement cvds =
                new CodeVariableDeclarationStatement(new CodeTypeReference("IState"), "baseState", cpr);

            doaction.Statements.Add(cvds);

            foreach (string st in StateNames.Keys)
            {
                CodeConditionStatement zif = new CodeConditionStatement();
                CodeBinaryOperatorExpression ztest = new CodeBinaryOperatorExpression();
                ztest.Right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("baseState"), "StateName");
                ztest.Left = new CodePrimitiveExpression(st);
                ztest.Operator = CodeBinaryOperatorType.IdentityEquality;
                zif.Condition = ztest;

                CodeVariableDeclarationStatement cvd = new CodeVariableDeclarationStatement();
                cvd.Name = "stateobj";
                cvd.Type = new CodeTypeReference(st);
                cvd.InitExpression = new CodeCastExpression(st, new CodeVariableReferenceExpression("baseState"));
                zif.TrueStatements.Add(cvd);

                List<string> actionnames = AllActionsUsedByState(st);

                //var ccc = new CodeEventReferenceExpression(new CodeVariableReferenceExpression("stateobj"), "StateEntry");

                //CodeConditionStatement quif = new CodeConditionStatement();
                //CodeBinaryOperatorExpression qtest = new CodeBinaryOperatorExpression();
                //qtest.Right = new CodePrimitiveExpression(null);
                //qtest.Left = ccc;
                //qtest.Operator = CodeBinaryOperatorType.IdentityEquality;
                //quif.Condition = qtest;


                foreach (string actionname in actionnames)
                {
                    zif.TrueStatements.Add(
                        new CodeAttachEventStatement(new CodeVariableReferenceExpression("stateobj"),
                            actionname, new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "AnyAction")));
                }
                zif.TrueStatements.Add(
                    new CodeAttachEventStatement(new CodeVariableReferenceExpression("stateobj"),
                        "StateEntry", new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "AnyEntry")));

                zif.TrueStatements.Add(
                    new CodeAttachEventStatement(new CodeVariableReferenceExpression("stateobj"),
                        "StateExit", new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "AnyExit")));


                zif.TrueStatements.Add(new CodeMethodReturnStatement());
                //  zif.TrueStatements.Add(quif);
                doaction.Statements.Add(zif);
            }
            return (doaction);
        }

        public CodeMemberMethod GenerateOnLoaded()
        {
            CodeMemberMethod doaction = new CodeMemberMethod();
            doaction.Name = "OnLoaded";
            doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            doaction.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.Object"), "obj"));
            doaction.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.EventArgs"), "e"));

            CodeFieldReferenceExpression cfr = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_stateMachine" + StateMachineName);
            CodePropertyReferenceExpression cpr = new CodePropertyReferenceExpression(cfr, "CurrentState");
            CodeAssignStatement cas = new CodeAssignStatement(cpr, new CodeObjectCreateExpression(InitialState));
            doaction.Statements.Add(cas);

            string mstart = string.Empty;
            mstart += "     var assembly = System.Reflection.Assembly.GetExecutingAssembly();\r\n";
            mstart += "     string[] lnst = assembly.GetManifestResourceNames();\r\n";
            mstart += string.Format("     var resourceName = \"{0}.{1}.xml\";\r\n", "Test" + StateMachineName, StateMachineName);
            mstart += "     using (Stream stream = assembly.GetManifestResourceStream(resourceName))\r\n";
            mstart += "     {\r\n";
            mstart += "         using (TextReader Tr = new StreamReader(stream))\r\n";
            mstart += "        {\r\n";
            mstart += "            string xml = Tr.ReadToEnd();\r\n";
            mstart += "           StateMachineXMLBlock.Text = xml;\r\n";
            mstart += "          Tr.Close();\r\n";
            mstart += "        }\r\n";
            mstart += "     }\r\n";
            CodeSnippetStatement snip = new CodeSnippetStatement(mstart);
            doaction.Statements.Add(snip);



            CodeMethodInvokeExpression cmo = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "SetUpEvents");
            doaction.Statements.Add(cmo);

            doaction.Statements.Add(
                       new CodeAttachEventStatement(cfr,
                           "StateChanged", new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "OnStateChanged")));

            foreach (string sn in Transitions.Keys)
            {
                List<string> elist = StateEventNamesUsedByState(sn);

                string rs = string.Format("\r\nState {0} : ", sn);
                foreach (string es in elist)
                {
                    rs += (es + " ");
                }
                var gg = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "InputBlock");
                var hh = new CodePropertyReferenceExpression(gg, "Text");

                var kk = new CodeBinaryOperatorExpression(hh, CodeBinaryOperatorType.Add, new CodePrimitiveExpression(rs));
                doaction.Statements.Add(new CodeAssignStatement(hh, kk));
            }

            foreach (string ss in StateEventNames.Keys)
            {
                doaction.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "BuildButton",
                    new CodePrimitiveExpression(ss)));
            }

            return (doaction);
        }

        public CodeMemberMethod GenerateWriter()
        {
            CodeMemberMethod doaction = new CodeMemberMethod();
            doaction.Name = "Writer";
            doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            doaction.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.String"), "text"));

            CodePropertyReferenceExpression cpre = new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "OutputBlock"), "Text");

            CodeBinaryOperatorExpression cpo = new CodeBinaryOperatorExpression(cpre, CodeBinaryOperatorType.Add, new CodeArgumentReferenceExpression("text"));

            doaction.Statements.Add(new CodeAssignStatement(cpre, cpo));

            cpo = new CodeBinaryOperatorExpression(cpre, CodeBinaryOperatorType.Add, new CodePrimitiveExpression("\r\n"));
            doaction.Statements.Add(new CodeAssignStatement(cpre, cpo));
            doaction.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InvalidateVisual"));

            return (doaction);
        }

        public CodeMemberMethod GenerateAllActions()
        {
            CodeMemberMethod doaction = new CodeMemberMethod();
            doaction.Name = "AnyAction";
            doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            doaction.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.Object"), "obj"));
            doaction.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.EventArgs"), "e"));
            doaction.ReturnType = new CodeTypeReference("System.Boolean");

            CodePropertyReferenceExpression cpre = new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "EventTextBlock"), "Text");
            CodePropertyReferenceExpression cpstb = new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "StateTextBlock"), "Text");
            CodePropertyReferenceExpression cpac = new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "ActionTextBlock"), "Text");

            CodeFieldReferenceExpression cprr = new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(), "LastEvent");


            doaction.Statements.Add(new CodeAssignStatement(cpre, cprr));

            CodeCastExpression cce = new CodeCastExpression("IState", new CodeArgumentReferenceExpression("obj"));
            CodePropertyReferenceExpression cprst = new CodePropertyReferenceExpression(cce, "StateName");
            doaction.Statements.Add(new CodeAssignStatement(cpstb, cprst));

            CodePropertyReferenceExpression cprsaa = new CodePropertyReferenceExpression(cce, "LastAction");
            doaction.Statements.Add(new CodeAssignStatement(cpac, cprsaa));

            var vv = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("System.String"), "Format");
            vv.Parameters.Add(new CodePrimitiveExpression("In Action {0} from Event {1} in State {2}"));

            vv.Parameters.Add(cpac);
            vv.Parameters.Add(cpre);
            vv.Parameters.Add(cpstb);
            doaction.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Writer", vv));

            doaction.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));
            return (doaction);
        }

        public CodeMemberMethod GenerateAllEntry()
        {
            CodeMemberMethod doaction = new CodeMemberMethod();
            doaction.Name = "AnyEntry";
            doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            doaction.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.Object"), "obj"));
            doaction.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.EventArgs"), "e"));
            doaction.ReturnType = new CodeTypeReference("System.Boolean");

            CodePropertyReferenceExpression cpstb = new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "StateTextBlock"), "Text");
            CodeCastExpression cce = new CodeCastExpression("IState", new CodeArgumentReferenceExpression("obj"));
            CodePropertyReferenceExpression cprst = new CodePropertyReferenceExpression(cce, "StateName");
            doaction.Statements.Add(new CodeAssignStatement(cpstb, cprst));

            var vv = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("System.String"), "Format");
            vv.Parameters.Add(new CodePrimitiveExpression("In Entry to State {0}"));
            vv.Parameters.Add(cpstb);
            doaction.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Writer", vv)); doaction.Statements.Add(vv);


            doaction.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));
            return (doaction);
        }

        public CodeMemberMethod GenerateAllExit()
        {
            CodeMemberMethod doaction = new CodeMemberMethod();
            doaction.Name = "AnyExit";
            doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            doaction.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.Object"), "obj"));
            doaction.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.EventArgs"), "e"));
            doaction.ReturnType = new CodeTypeReference("System.Boolean");

            CodePropertyReferenceExpression cpstb = new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "StateTextBlock"), "Text");

            var vv = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("System.String"), "Format");
            vv.Parameters.Add(new CodePrimitiveExpression("In Exit from State {0}"));

            vv.Parameters.Add(cpstb);
            doaction.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Writer", vv));

            doaction.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));
            return (doaction);
        }

        public CodeMemberMethod GenerateStateChanged()
        {
            CodeMemberMethod doaction = new CodeMemberMethod();
            doaction.Name = "OnStateChanged";
            doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            doaction.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.Object"), "obj"));
            doaction.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.EventArgs"), "e"));

            CodePropertyReferenceExpression cpstb = new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "StateTextBlock"), "Text");
            CodeFieldReferenceExpression cfr = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_stateMachine" + StateMachineName);
            CodePropertyReferenceExpression cpr = new CodePropertyReferenceExpression(cfr, "CurrentState");
            CodePropertyReferenceExpression cnr = new CodePropertyReferenceExpression(cpr, "StateName");

            CodeConditionStatement quif = new CodeConditionStatement();
            CodeBinaryOperatorExpression qtest = new CodeBinaryOperatorExpression();
            qtest.Right = cnr;
            qtest.Left = cpstb;
            qtest.Operator = CodeBinaryOperatorType.IdentityInequality;
            quif.Condition = qtest;

            CodeMethodInvokeExpression cmo = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "SetUpEvents");
            quif.TrueStatements.Add(cmo);
            doaction.Statements.Add(quif);

            doaction.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Writer",
                new CodePrimitiveExpression("The current state has changed")));

            return (doaction);
        }

        public CodeMemberMethod GenerateEventButtonOnClick()
        {
            CodeMemberMethod doaction = new CodeMemberMethod();
            doaction.Name = "EventButtonOnClick";
            doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            doaction.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.Object"), "sender"));
            doaction.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.Windows.RoutedEventArgs"), "e"));

            CodeVariableDeclarationStatement cvd = new CodeVariableDeclarationStatement();
            cvd.Name = "button";
            cvd.Type = new CodeTypeReference("Button");
            cvd.InitExpression = new CodeCastExpression("Button", new CodeArgumentReferenceExpression("sender"));
            doaction.Statements.Add(cvd);

            CodeVariableDeclarationStatement cvvd = new CodeVariableDeclarationStatement();
            cvvd.Name = "stateeventname";
            cvvd.Type = new CodeTypeReference("System.String");
            cvvd.InitExpression = new CodeCastExpression("System.String", new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("button"), "Tag"));
            doaction.Statements.Add(cvvd);

            doaction.Statements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "LastEvent"),
                    new CodeVariableReferenceExpression("stateeventname")));

            doaction.Statements.Add(
                new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_stateMachineMonster"),
                    "MachineEventArrival", new CodeVariableReferenceExpression("stateeventname")));

            return (doaction);
        }

        public CodeMemberMethod GenerateBuildButton()
        {
            CodeMemberMethod doaction = new CodeMemberMethod();
            doaction.Name = "BuildButton";
            doaction.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            doaction.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.String"), "stateevent"));

            CodeVariableDeclarationStatement cvds = new CodeVariableDeclarationStatement(new CodeTypeReference("Button"), "button", new CodeObjectCreateExpression(new CodeTypeReference("Button")));
            doaction.Statements.Add(cvds);
            doaction.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("button"), "Tag"),
                new CodeArgumentReferenceExpression("stateevent")));
            doaction.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("button"), "Content"),
             new CodeArgumentReferenceExpression("stateevent")));

            doaction.Statements.Add(
                  new CodeAttachEventStatement(new CodeVariableReferenceExpression("button"),
                      "Click", new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "EventButtonOnClick")));

            doaction.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("InputWrapPanel"),
                        "Children"), "Add", new CodeVariableReferenceExpression("button")));

            return (doaction);
        }


        #endregion

        public void GenerateStatePattern()
        {

            CreateBaseInterface();
            CreateMachineClass();
            CreateActionEventArgs();
            foreach (string st in StateNames.Keys)
            {
                CreateStateClass(st);
            }
            //     GenerateWinMain();
            //     GenerateWpfProj();
        }


        public void GenerateStatePatternProject()
        {

            CreateBaseInterface();
            CreateMachineClass();
            CreateActionEventArgs();
            foreach (string st in StateNames.Keys)
            {
                CreateStateClass(st);
            }
            GenerateWinMain();
            GenerateWpfProj();
        }


        public void GenerateWpfProj()
        {
            //   _errList.Add("Started WPF Project Generation");
            try
            {
                SmAppName = "Test" + StateMachineName;
                SmAppNameSpace = SmAppName;
                SmSmName = StateMachineName;
                SmProjGuid = (Guid.NewGuid()).ToString("B");

                ErrorList = new List<string>();

                SmProjFiles = "";
                SmProjFiles += string.Format("<Compile Include = \"{0}\\StateMachine{0}.cs\" />\r\n ", StateMachineName);
                SmProjFiles += string.Format("<Compile Include = \"{0}\\ActionsEventArgs.cs\" />\r\n ", StateMachineName);
                SmProjFiles += string.Format("<Compile Include = \"{0}\\IState.cs\" />\r\n ", StateMachineName);

                foreach (String st in Transitions.Keys)
                {
                    SmProjFiles += string.Format("<Compile Include = \"{0}\\{1}.cs\" />\r\n ", StateMachineName, st);
                }


                char[] remchars = { ' ', '\n', '\r', };

                StatePattern.WpfTemplates.App genApp = new StatePattern.WpfTemplates.App();
                StreamWriter writer = new StreamWriter(AppXamlName);
                string xmltext = genApp.TransformText().TrimStart(remchars);
                writer.Write(xmltext);
                writer.Flush();
                writer.Close();

                StatePattern.WpfTemplates.Appxaml genAppcs = new StatePattern.WpfTemplates.Appxaml();
                StreamWriter writer2 = new StreamWriter(AppCsName);
                writer2.Write(genAppcs.TransformText());
                writer2.Flush();
                writer2.Close();

                StatePattern.WpfTemplates.Appconfig genAppconfig = new StatePattern.WpfTemplates.Appconfig();
                xmltext = genAppconfig.TransformText().TrimStart(remchars);
                StreamWriter writer3 = new StreamWriter(CfgFileName);
                writer3.Write(xmltext);
                writer3.Flush();
                writer3.Close();

                StatePattern.WpfTemplates.MainWindow genMainWindow = new StatePattern.WpfTemplates.MainWindow();
                xmltext = genMainWindow.TransformText().TrimStart(remchars);
                StreamWriter writer4 = new StreamWriter(WindowXamlName);
                writer4.Write(xmltext);
                writer4.Flush();
                writer4.Close();

                StatePattern.WpfTemplates.AssemblyInfo genAssemblyInfo = new StatePattern.WpfTemplates.AssemblyInfo();
                StreamWriter writer5 = new StreamWriter(AsmbFileName);
                writer5.Write(genAssemblyInfo.TransformText());
                writer5.Flush();
                writer5.Close();

                StatePattern.WpfTemplates.ProjectFile genProjectFile = new StatePattern.WpfTemplates.ProjectFile();
                xmltext = genProjectFile.TransformText().TrimStart(remchars);
                StreamWriter writer6 = new StreamWriter(ProjectFileName);
                writer6.Write(xmltext);
                writer6.Flush();
                writer6.Close();

                StatePattern.WpfTemplates.ResourceDesigner genResourceDesigner = new StatePattern.WpfTemplates.ResourceDesigner();
                StreamWriter writer7 = new StreamWriter(ResourceDesignerName);
                writer7.Write(genResourceDesigner.TransformText());
                writer7.Flush();
                writer7.Close();

                StatePattern.WpfTemplates.Resources genResources = new StatePattern.WpfTemplates.Resources();
                xmltext = genResources.TransformText().TrimStart(remchars);
                StreamWriter writer8 = new StreamWriter(ResourceFileName);
                writer8.Write(xmltext);
                writer8.Flush();
                writer8.Close();

                StatePattern.WpfTemplates.SettingsDesigner genSettingsDesigner = new StatePattern.WpfTemplates.SettingsDesigner();
                StreamWriter writer9 = new StreamWriter(SettingsDesignerName);
                writer9.Write(genSettingsDesigner.TransformText());
                writer9.Flush();
                writer9.Close();

                StatePattern.WpfTemplates.Settings genSettings = new StatePattern.WpfTemplates.Settings();
                xmltext = genSettings.TransformText().TrimStart(remchars);
                StreamWriter writer10 = new StreamWriter(SettingsFileName);
                writer10.Write(xmltext);
                writer10.Flush();
                writer10.Close();
            }
            catch (Exception ee)
            {
                ErrorList.Add(ee.Message);
            }
        }

        public void GenerateCode(string fileName, CodeCompileUnit cu)
        {
            CodeDomProvider provider;

            provider = CodeDomProvider.CreateProvider("c#");
            try
            {
                provider = CodeDomProvider.CreateProvider(Language);
            }
            catch (Exception ee)
            {
                ErrorList.Add(ee.Message);
            }

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
                    ErrorList.Add(ee.Message);
                }
            }
        }

        #endregion

    }
}
