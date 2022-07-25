using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.CodeDom.Compiler;
using System.IO;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using Microsoft.CSharp;
using Microsoft.SqlServer.Server;
using SmSimpleData;

namespace MVCStateMachineCodeGeneration
{
    public class MvcCodeGeneration
    {
        #region fields

     //    MvcStateMachineCodeDataContext _mvcStateMachineCodeDataContext;
        Int64 _smId;
        Guid _smCodeid;
        string _smName;
        string _smInitialstate;
        private string _language;

       

        string _targetdir;
 
        Dictionary<string, Int64> _stateDictionary;
        Dictionary<string, Int64> _evntdict;
        Dictionary<string, Int64> _actiondict;

     
        List<string> _errList;

        Dictionary<string, Dictionary<string, SmSimpleData.Tranval>> _transitions;

        CompilerInfo[] _allCompilerInfo;

        private string _controllerName = string.Empty;
        private string _controllerFileName = string.Empty;
        private CodeCompileUnit _controllerUnit;
        private CodeTypeDeclaration _controllerClassType;




        private CodeCompileUnit _modelUnit;
        private CodeTypeDeclaration _modelClassType;
        private string _modelName;
        private string _modelFileName;
        private string _auxName;
        private string _auxFileName;

        Dictionary<string, string> _stateFileNameDictionary = new Dictionary<string, string>();
        private string _viewdir;
        private string _modeldir;
        private string _controlerdir;

        #endregion

        #region ctor

          public MvcCodeGeneration()
        {
            _errList = new List<string>();
           // _smName = "unknown";
            _targetdir = "..\\..\\..\\TestStateb";
           
          //  SetFileNames();
            _smCodeid = Guid.NewGuid();
            _language = "c#";
            _allCompilerInfo = CodeDomProvider.GetAllCompilerInfo();
        }


          public MvcCodeGeneration(string smname)
        {
            _smName = smname;
            _errList = new List<string>();
            _targetdir = "..\\..\\..\\TestStateb";
         
         //   SetFileNames();
        //    StateMachineInfo(_smName);
            _language = "c#";
            _allCompilerInfo = CodeDomProvider.GetAllCompilerInfo();
        }

        #endregion

        #region properties

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

        public string Targetdir
        {
            get
            {
                return (_targetdir);
            }
            set
            {
                _targetdir = value;
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
                string vv = _smName;
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
   //             SetFileNames();
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

        public CodeCompileUnit ControllerUnit
        {
            get { return _controllerUnit; }
            set { _controllerUnit = value; }
        }

        public CodeTypeDeclaration ControllerClassType
        {
            get { return _controllerClassType; }
            set { _controllerClassType = value; }
        }


        public CodeCompileUnit ModelUnit
        {
            get { return _modelUnit; }
            set { _modelUnit = value; }
        }

        public CodeTypeDeclaration ModelClassType
        {
            get { return _modelClassType; }
            set { _modelClassType = value; }
        }

        public CodeCompileUnit AuxUnit { get; set; }

        public CodeTypeDeclaration AuxClassType
        {
            get; set; 
        }

        #endregion


        public bool GenerateMvcStateMachineFiles()
        {
            _errList.Clear();
            try
            {
                GenerateControllerClass();
                GenerateModelClass();
                GenerateAuxClass();
                GenerateViews();
            }
            catch (Exception ee)
            {
                _errList.Add(ee.Message);
            }

            if (_errList.Count > 0)
            {
                return (false);
            }
            return (true);
        }

#region GenerateController

        public void GenerateModelClass()
        {
            ModelUnit = new CodeCompileUnit();

            CodeNamespace model = new CodeNamespace("TheApp.Models");

            model.Imports.Add(new CodeNamespaceImport("System"));
            model.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            model.Imports.Add(new CodeNamespaceImport("System.Web.Mvc"));
            if (_language == "c#")
            {
                model.Imports.Add(new CodeNamespaceImport("System.Linq"));
            }

            ModelClassType = new CodeTypeDeclaration(SmName);
            ModelClassType.IsClass = true;
            ModelClassType.IsPartial = true;
            ModelClassType.TypeAttributes = (ControllerClassType.TypeAttributes & ~TypeAttributes.VisibilityMask) |
                                            TypeAttributes.Public;

            model.Types.Add(ModelClassType);
            ModelUnit.Namespaces.Add(model);

            // start SelectedEventName

            CodeMemberField afield = CreateField(typeof (string), "_selectedEventName",
                "This is the event mark that will tell the controller the input type of the user page");

            ModelClassType.Members.Add(afield);

            CodeMemberProperty meprop = CreateFieldProperty(typeof (string), "_selectedEventName",
                "SelectedEventName",
                "SelectedEventName" + " Will be set to the Events of the current state by the web page for that state: ");
            //foreach (string qq in SmEventNames.Keys)
            //{
            //    meprop.Comments.Add(new CodeCommentStatement(qq + ","));
            //}

          //  meprop.Attributes = MemberAttributes.Public ;
            meprop.Attributes = (meprop.Attributes & ~MemberAttributes.ScopeMask) | MemberAttributes.Public;
            CodeAttributeArgument[] args = new CodeAttributeArgument[1];
            args[0] = new CodeAttributeArgument("DisplayValue", new CodePrimitiveExpression(false));
            meprop.CustomAttributes.Add(new CodeAttributeDeclaration("HiddenInput", args));

            ModelClassType.Members.Add(meprop);

            // end SelectedEventName Property

            // START METHOD AddToExteralEventList
            CodeMemberMethod amethod = new CodeMemberMethod();
            amethod.Name = "AddToExteralEventList";
            amethod.Attributes = (amethod.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            amethod.Parameters.Add(new CodeParameterDeclarationExpression("System.string", "eventname"));

            CodeVariableDeclarationStatement variableDeclaration = new CodeVariableDeclarationStatement(
                "ExternalEventName", "ename",
                new CodeObjectCreateExpression("ExternalEventName", new CodeExpression[] {}));

            amethod.Statements.Add(variableDeclaration);

            CodeAssignStatement asc =
                new CodeAssignStatement(
                    new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("ename"), "Id"),
                    new CodeVariableReferenceExpression("eventname"));
            amethod.Statements.Add(asc);

            CodeAssignStatement bsc =
                new CodeAssignStatement(
                    new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("ename"), "Name"),
                    new CodeVariableReferenceExpression("eventname"));
            amethod.Statements.Add(bsc);



            CodeMethodReferenceExpression mref = new CodeMethodReferenceExpression(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_externalEventNames"),
                "Add",
                new CodeTypeReference[] { new CodeTypeReference("ExternalEventName") });

            CodeMethodInvokeExpression cmie = new CodeMethodInvokeExpression(mref,
                new CodeVariableReferenceExpression("ename"));

            amethod.Statements.Add(cmie);

            ModelClassType.Members.Add(amethod);
            // end Method AddToExteralEventList

            // start _externalEventNames fieldcmie
            CodeMemberField cfield = CreateField("List<ExternalEventName>", "_externalEventNames",
                "List of ExternalEventName objects");
            cfield.InitExpression = new CodeObjectCreateExpression("List<ExternalEventName>", new CodeExpression[] {});
            ModelClassType.Members.Add(cfield);
            // end _externalEventNames field

            // start ExternalEventNames Property
            CodeMemberProperty yprop = new CodeMemberProperty();
            yprop.Attributes =
                MemberAttributes.Public | MemberAttributes.Final;
            yprop.Name = "ExternalEventNames";
            yprop.HasGet = true;
            yprop.Type = new CodeTypeReference("IEnumerable<SelectListItem>");
            yprop.GetStatements.Add(new CodeMethodReturnStatement(
                new CodeObjectCreateExpression("SelectList", new CodeExpression[]
                {
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_externalEventNames"),
                    new CodePrimitiveExpression("Id"),
                    new CodePrimitiveExpression("Name")
                })));

            ModelClassType.Members.Add(yprop);
            GenerateCode(_modelFileName, ModelUnit);


            // end ExternalEventNames property
            //  public  class ExternamEventName
            //{
            //    public string Id { get; set; }
            //    public string Name { get; set; }

            //}
        }
        public void GenerateAuxClass()
        {

            AuxUnit = new CodeCompileUnit();

            CodeNamespace model = new CodeNamespace("TheApp.Models");

            model.Imports.Add(new CodeNamespaceImport("System"));
            model.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            model.Imports.Add(new CodeNamespaceImport("System.Web.Mvc"));
            if (_language == "c#")
            {
                model.Imports.Add(new CodeNamespaceImport("System.Linq"));
            }

            AuxUnit.Namespaces.Add(model);

            AuxClassType = new CodeTypeDeclaration("ExternalEventName");
            AuxClassType.IsClass = true;
            AuxClassType.TypeAttributes = (ControllerClassType.TypeAttributes & ~TypeAttributes.VisibilityMask) | TypeAttributes.Public;
            model.Types.Add(AuxClassType);

            CodeMemberField aafield = CreateField(typeof(string), "_id",
                        "the Event name as the value");

            CodeMemberField bfield = CreateField(typeof(string), "_name",
                      "the Event name as the name");

            AuxClassType.Members.Add(aafield);
            AuxClassType.Members.Add(bfield);

            CodeMemberProperty aprop = CreateFieldProperty(typeof(string), "_id",
                "Id", "A state machine event ");

            aprop.Attributes = MemberAttributes.Public;

            CodeMemberProperty bprop = CreateFieldProperty(typeof(string), "_name",
              "Name", "A state machine event Name ");

            bprop.Attributes = MemberAttributes.Public;
           
            AuxClassType.Members.Add(aprop);
            AuxClassType.Members.Add(bprop);

            GenerateCode(_auxFileName, AuxUnit);
        }

        public void GenerateViewClasses()
        {
            
        }

        public void GenerateControllerClass()
        {
            ControllerUnit = new CodeCompileUnit();

            CodeNamespace controller = new CodeNamespace("TheApp.Controllers");

            controller.Imports.Add(new CodeNamespaceImport("System"));
            controller.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            if (_language == "c#")
            {
                controller.Imports.Add(new CodeNamespaceImport("System.Linq"));
            }
            controller.Imports.Add(new CodeNamespaceImport("System.Text"));
            controller.Imports.Add(new CodeNamespaceImport("System.IO"));
            controller.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            controller.Imports.Add(new CodeNamespaceImport("System.Collections.ObjectModel"));
            controller.Imports.Add(new CodeNamespaceImport("System.Data"));
            controller.Imports.Add(new CodeNamespaceImport("System.Net"));
            controller.Imports.Add(new CodeNamespaceImport("System.Net.Http.Headers"));
            controller.Imports.Add(new CodeNamespaceImport("System.Security.Cryptography"));
            controller.Imports.Add(new CodeNamespaceImport("System.Web"));
            controller.Imports.Add(new CodeNamespaceImport("System.Web.Mvc"));
            controller.Imports.Add(new CodeNamespaceImport("System.Web.Routing"));

             ControllerClassType = new CodeTypeDeclaration(SmName + "Controller");
             ControllerClassType.IsClass = true;
             ControllerClassType.IsPartial = true;
             ControllerClassType.TypeAttributes = ( ControllerClassType.TypeAttributes & ~TypeAttributes.VisibilityMask) | TypeAttributes.Public;
             ControllerClassType.BaseTypes.Add(new CodeTypeReference("System.Web.Mvc.Controller"));

            controller.Types.Add(ControllerClassType);
            ControllerUnit.Namespaces.Add(controller);


            ControllerClassType.Members.Add(CreateField("TheApp.Models." + SmName, "_" + SmName,
                "An instanstance of the " + SmName + " model and event transport class"));

            ControllerClassType.Members.Add(CreateFieldProperty("TheApp.Models." + SmName, "_" + SmName, SmName + "Model",
                "The local model and event transport object"));

            GenerateControllerMembers();
            
            GenerateCode(_controllerFileName, ControllerUnit);
        }


        private void GenerateControllerMembers()
        {
            if (SmEventNames != null)
            {
                foreach (var smStateName in SmStateNames)
                {
                    string statename =  smStateName.Key;

                    string usname = statename.ToUpperInvariant();
                    statename = usname[0] + statename.Substring(1, statename.Length - 1);
                    
                     CodeRegionDirective regionStart = new CodeRegionDirective(CodeRegionMode.Start, statename);

                   
                    // Method HttpGet
                     CodeMemberMethod gmethod = new CodeMemberMethod();
                     gmethod.Name = statename;
                     gmethod.CustomAttributes.Add(new CodeAttributeDeclaration("HttpGet"));
                     gmethod.Attributes = (gmethod.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
                    
                     gmethod.StartDirectives.Add(regionStart);

                      CodeAssignStatement asi1 =  new CodeAssignStatement(
                          new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + SmName),
                          new CodeObjectCreateExpression(SmName, new CodeExpression[] {}));

                    gmethod.Statements.Add(asi1);

                    if (_transitions.ContainsKey(smStateName.Key))
                    {
                         Dictionary<string, Tranval> ee = _transitions[smStateName.Key];
                        foreach (Tranval tranval in ee.Values)
                        {
                            CodeMethodInvokeExpression me = new CodeMethodInvokeExpression(
                                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + SmName),
                                "AddToExteralEventList",
                                new CodeExpression[] { new CodePrimitiveExpression(tranval.Evntname) }
                                );
                            gmethod.Statements.Add(me);
                        }
                    }

                    AddViewModelReturn(ref gmethod);
                    ControllerClassType.Members.Add(gmethod);

                    // Method HttpPost

                     CodeMemberMethod method = new CodeMemberMethod();
                     method.Name = statename;
                     method.CustomAttributes.Add(new CodeAttributeDeclaration("HttpPost"));
                     method.Attributes = (method.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
                     string modelinstancename = (SmName + "Model").ToLower();

                     method.Parameters.Add(
                         new CodeParameterDeclarationExpression( SmName, modelinstancename));
                    
                     method.ReturnType = new CodeTypeReference("ActionResult");

                     if (_transitions.ContainsKey(smStateName.Key))
                    {
                        Dictionary<string, Tranval> dd = _transitions[smStateName.Key];
                        foreach (Tranval tranval in dd.Values)
                        {
                            //if (modelinstancename.MachineEvent == tranval.EventName)
                            //{
                            //    tranval.Action();
                            //    return RedirectToAction(tranval.Endstate, modelinstancename);
                            //}

                            CodePropertyReferenceExpression prop = 
                                new CodePropertyReferenceExpression();
                            
                            prop.TargetObject = new CodeVariableReferenceExpression(modelinstancename);
                            prop.PropertyName = "SelectedEventName";

                            CodeConditionStatement conditionalStatement =
                                new CodeConditionStatement();

                            CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression();
                            codeBinaryOperatorExpression.Left = prop;
                            codeBinaryOperatorExpression.Operator = CodeBinaryOperatorType.ValueEquality;
                            codeBinaryOperatorExpression.Right = new CodePrimitiveExpression(tranval.Evntname);

                            conditionalStatement.Condition = codeBinaryOperatorExpression;

                            string actname = tranval.Action;
                            string uactname = actname.ToUpperInvariant();
                            actname = uactname[0] + actname.Substring(1, actname.Length - 1);

                            conditionalStatement.TrueStatements.Add(new CodeMethodInvokeExpression(
                                new CodeThisReferenceExpression(),
                                actname
                                ));

                            string endstate = tranval.Endstate;
                            string uendstate = endstate.ToUpper();
                            endstate = uendstate[0] + endstate.Substring(1, endstate.Length - 1);

                            conditionalStatement.TrueStatements.Add(
                                new CodeMethodReturnStatement(
                                    new CodeMethodInvokeExpression(
                                        new CodeThisReferenceExpression(), "RedirectToAction",
                                        new CodePrimitiveExpression(endstate))));

                            method.Statements.Add(conditionalStatement);
                        }
                    }
                     method.Statements.Add( new CodeMethodReturnStatement(
                                    new CodeMethodInvokeExpression(
                                        new CodeThisReferenceExpression(), "RedirectToAction",
                                        new CodePrimitiveExpression(statename))));

                     CodeRegionDirective regionEnd = new CodeRegionDirective(CodeRegionMode.End, statename);
                     method.EndDirectives.Add(regionEnd);
                     ControllerClassType.Members.Add(method);
                }
                foreach (KeyValuePair<string, long> smActionName in SmActionNames)
                {
                    string aname = smActionName.Key;
                    string uaname = aname.ToUpperInvariant();
                    aname = uaname[0] + aname.Substring(1, aname.Length - 1);

                    CodeMemberMethod methodAct = new CodeMemberMethod();
                    methodAct.Name = aname;
                 
                    methodAct.Attributes = (methodAct.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Private;
                        

                    ControllerClassType.Members.Add(methodAct);
                }
            }
        }

        public void GenerateViews()
        {
            DirectoryInfo nf = new DirectoryInfo(_viewdir);
            string dirname3 = nf.FullName;
             _stateFileNameDictionary = new Dictionary<string, string>();

            foreach (string astate in _stateDictionary.Keys)
            {
                _stateFileNameDictionary.Add(astate, dirname3 + "\\" + astate + ".cshtml");

                string mstart = string.Format("@model TestStateCreations.Models.{0}\r\n", SmName);
                mstart += " \r\n";
                mstart += "     @{\r\n";
                mstart += string.Format("     ViewBag.Title = \"{0}\";\r\n", astate);
                mstart += "     }\r\n";
                mstart += " \r\n";
                mstart += string.Format("<h2>{0}</h2>\r\n", astate);
                mstart += "@using (Html.BeginForm())\r\n";
                mstart += " {\r\n";
                mstart += "      @Html.AntiForgeryToken()\r\n";
                mstart += " \r\n";
                mstart += "    <div class=\"form-horizontal\">\r\n";
                mstart += string.Format("          <h4>{0}</h4>\r\n", SmName);

                mstart += "          <hr />\r\n";
                mstart += "          @Html.ValidationSummary(true)\r\n";
                mstart += "          <div class=\"form-group\">\r\n";
                mstart += "      \r\n";
                mstart += "               <div class=\"col-md-10\">\r\n";
                mstart += "                    @Html.DropDownListFor(model => model.SelectedEventName, Model.ExternalEventNames)\r\n";
                mstart += "       \r\n";
                mstart += "               </div>\r\n";
                mstart += "         </div>\r\n";
                mstart += "         <div class=\"form-group\">\r\n";
                mstart += "              <div class=\"col-md-offset-2 col-md-10\">\r\n";
                mstart += "                   <input type=\"submit\" value=\"Go\" class=\"btn btn-default\" />\r\n";
                mstart += "              </div>\r\n";
                mstart += "         </div>\r\n";
                mstart += "     </div>\r\n";
                mstart += "}\r\n";
                mstart += "     \r\n";
                mstart += "@section Scripts {\r\n";
                mstart += "     @Scripts.Render(\"~/bundles/jqueryval\")\r\n";
                mstart += "}\r\n";
                mstart += "     \r\n";


                TextWriter rt = new StreamWriter(_stateFileNameDictionary[astate]);
                
                rt.Write(mstart);
                rt.Flush();
                rt.Close();


            }
        }

        private void GenerateControllerProperties()
        {
 	        throw new NotImplementedException();
        }

        private void GenerateControllerFields()
        {
 	        throw new NotImplementedException();
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

        public CodeMemberField CreateField(string fieldtype, string fieldname, string comment)
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
                string.Format("{0}: {1}.", _smName, comment)));
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
                string.Format("{0}Controller {1}.", _smName, comment)));
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

        void AddViewModelReturn(ref CodeMemberMethod method)
        {
             method.ReturnType = new CodeTypeReference("ActionResult");
             method.Statements.Add(
                        new CodeMethodReturnStatement(
                            new CodeMethodInvokeExpression(
                                new CodeThisReferenceExpression(), "View",  
                                 new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(), SmName + "Model")) ));
        }

#endregion



        #region file Names and write

        public void SetFileNames()
        {

            DirectoryInfo df = new DirectoryInfo(_targetdir);
            string dirname = df.FullName;

            _controllerFileName = dirname + "\\" + _smName + "Controller";

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
            _controlerdir = targ + "\\Controllers";
            if (Directory.Exists(_controlerdir) == false)
            {
                try
                {
                    Directory.CreateDirectory(_controlerdir);
                }
                catch (Exception ee)
                {
                    _errList.Add(ee.Message);
                    return;
                }
            }

            _modeldir = targ + "\\Models";
            if (Directory.Exists(_modeldir) == false)
            {
                try
                {
                    Directory.CreateDirectory(_modeldir);
                }
                catch (Exception ee)
                {
                    _errList.Add(ee.Message);
                    return;
                }
            }

            _viewdir = targ + "\\Views";
            if (Directory.Exists(_viewdir) == false)
            {
                try
                {
                    Directory.CreateDirectory(_viewdir);
                }
                catch (Exception ee)
                {
                    _errList.Add(ee.Message);
                    return;
                }
            }

            _controllerName = SmName + "Controller";
            _modelName = SmName + "Model";
            _auxName = "Utility";

            _targetdir = _controlerdir;
            DirectoryInfo df = new DirectoryInfo(_targetdir);
            string dirname = df.FullName;
            _controllerFileName = dirname + "\\" + _smName + "Controller";

        //    _targetdir = modeldir;
            DirectoryInfo mf = new DirectoryInfo(_modeldir);
            string dirname2 = mf.FullName;
            
            _modelFileName = dirname2 + "\\" + _modelName;
            _auxFileName = dirname2 + "\\" + _auxName;

            

        }

#endregion


#region Generate Code

    public void GenerateCode(string fileName, CodeCompileUnit cu)
        {
            CodeDomProvider provider;

            provider = CodeDomProvider.CreateProvider("c#");
            try
            {
                provider = CodeDomProvider.CreateProvider(_language);
            }
            catch(Exception ee)
            {
                _errList.Add(ee.Message);
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
                    _errList.Add(ee.Message);
                }
            }
        }

#endregion
    }
}
