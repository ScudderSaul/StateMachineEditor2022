﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 14.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace StatePattern.WpfTemplates
{
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "Q:\code\Code_2015\StateMachine2015\StatePattern\WpfTemplates\ProjectFile.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "14.0.0.0")]
    public partial class ProjectFile : ProjectFileBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""12.0"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"" Condition=""Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
    <ProjectGuid>
      ");
            
            #line 9 "Q:\code\Code_2015\StateMachine2015\StatePattern\WpfTemplates\ProjectFile.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(StatePatternCodeGenerator.SmProjGuid));
            
            #line default
            #line hidden
            this.Write("\r\n      </ProjectGuid>\r\n    <OutputType>WinExe</OutputType>\r\n    <AppDesignerFold" +
                    "er>Properties</AppDesignerFolder>\r\n    <RootNamespace>");
            
            #line 13 "Q:\code\Code_2015\StateMachine2015\StatePattern\WpfTemplates\ProjectFile.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(StatePatternCodeGenerator.SmAppNameSpace));
            
            #line default
            #line hidden
            this.Write("</RootNamespace>\r\n    <AssemblyName>");
            
            #line 14 "Q:\code\Code_2015\StateMachine2015\StatePattern\WpfTemplates\ProjectFile.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(StatePatternCodeGenerator.SmAppName));
            
            #line default
            #line hidden
            this.Write("</AssemblyName>\r\n    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>\r\n    <" +
                    "FileAlignment>512</FileAlignment>\r\n    <ProjectTypeGuids>{60dc8134-eba5-43b8-bcc" +
                    "9-bb4bc16c2548};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</ProjectTypeGuids>\r\n    <" +
                    "WarningLevel>4</WarningLevel>\r\n  </PropertyGroup>\r\n  <PropertyGroup Condition=\" " +
                    "\'$(Configuration)|$(Platform)\' == \'Debug|AnyCPU\' \">\r\n    <PlatformTarget>AnyCPU<" +
                    "/PlatformTarget>\r\n    <DebugSymbols>true</DebugSymbols>\r\n    <DebugType>full</De" +
                    "bugType>\r\n    <Optimize>false</Optimize>\r\n    <OutputPath>bin\\Debug\\</OutputPath" +
                    ">\r\n    <DefineConstants>DEBUG;TRACE</DefineConstants>\r\n    <ErrorReport>prompt</" +
                    "ErrorReport>\r\n    <WarningLevel>4</WarningLevel>\r\n  </PropertyGroup>\r\n  <Propert" +
                    "yGroup Condition=\" \'$(Configuration)|$(Platform)\' == \'Release|AnyCPU\' \">\r\n    <P" +
                    "latformTarget>AnyCPU</PlatformTarget>\r\n    <DebugType>pdbonly</DebugType>\r\n    <" +
                    "Optimize>true</Optimize>\r\n    <OutputPath>bin\\Release\\</OutputPath>\r\n    <Define" +
                    "Constants>TRACE</DefineConstants>\r\n    <ErrorReport>prompt</ErrorReport>\r\n    <W" +
                    "arningLevel>4</WarningLevel>\r\n  </PropertyGroup>\r\n  <ItemGroup>\r\n    <Reference " +
                    "Include=\"System\" />\r\n    <Reference Include=\"System.Data\" />\r\n    <Reference Inc" +
                    "lude=\"System.Xml\" />\r\n    <Reference Include=\"Microsoft.CSharp\" />\r\n    <Referen" +
                    "ce Include=\"System.Core\" />\r\n    <Reference Include=\"System.Xml.Linq\" />\r\n    <R" +
                    "eference Include=\"System.Data.DataSetExtensions\" />\r\n    <Reference Include=\"Sys" +
                    "tem.Xaml\">\r\n      <RequiredTargetFramework>4.0</RequiredTargetFramework>\r\n    </" +
                    "Reference>\r\n    <Reference Include=\"WindowsBase\" />\r\n    <Reference Include=\"Pre" +
                    "sentationCore\" />\r\n    <Reference Include=\"PresentationFramework\" />\r\n  </ItemGr" +
                    "oup>\r\n  <ItemGroup>\r\n    <ApplicationDefinition Include=\"App.xaml\">\r\n      <Gene" +
                    "rator>MSBuild:Compile</Generator>\r\n      <SubType>Designer</SubType>\r\n    </Appl" +
                    "icationDefinition>\r\n    <Page Include=\"MainWindow.xaml\">\r\n      <Generator>MSBui" +
                    "ld:Compile</Generator>\r\n      <SubType>Designer</SubType>\r\n    </Page>\r\n    <Com" +
                    "pile Include=\"App.xaml.cs\">\r\n      <DependentUpon>App.xaml</DependentUpon>\r\n    " +
                    "  <SubType>Code</SubType>\r\n    </Compile>\r\n\r\n\t");
            
            #line 68 "Q:\code\Code_2015\StateMachine2015\StatePattern\WpfTemplates\ProjectFile.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(StatePatternCodeGenerator.SmProjFiles));
            
            #line default
            #line hidden
            this.Write(@"

    <Compile Include=""MainWindow.xaml.cs"">
      <DependentUpon>MainWindow.xaml</DependentUpon>
      <SubType>Code</SubType>
    </Compile>
  </ItemGroup>
  <ItemGroup>
    <Compile Include=""Properties\AssemblyInfo.cs"">
      <SubType>Code</SubType>
    </Compile>
    <Compile Include=""Properties\Resources.Designer.cs"">
      <AutoGen>True</AutoGen>
      <DesignTime>True</DesignTime>
      <DependentUpon>Resources.resx</DependentUpon>
    </Compile>
    <Compile Include=""Properties\Settings.Designer.cs"">
      <AutoGen>True</AutoGen>
      <DependentUpon>Settings.settings</DependentUpon>
      <DesignTimeSharedInput>True</DesignTimeSharedInput>
    </Compile>
    <EmbeddedResource Include=""Properties\Resources.resx"">
      <Generator>ResXFileCodeGenerator</Generator>
      <LastGenOutput>Resources.Designer.cs</LastGenOutput>
    </EmbeddedResource>
    <None Include=""Properties\Settings.settings"">
      <Generator>SettingsSingleFileGenerator</Generator>
      <LastGenOutput>Settings.Designer.cs</LastGenOutput>
    </None>
    <AppDesigner Include=""Properties\"" />
  </ItemGroup>
  <ItemGroup>
    <None Include=""App.config"" />
  </ItemGroup>
  <ItemGroup>
    <EmbeddedResource Include=""");
            
            #line 103 "Q:\code\Code_2015\StateMachine2015\StatePattern\WpfTemplates\ProjectFile.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(StatePatternCodeGenerator.SmSmName));
            
            #line default
            #line hidden
            this.Write(@".xml"">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </EmbeddedResource>
  </ItemGroup>
  <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
  <!-- To modify your build process, add your task inside one of the targets below and uncomment it. 
       Other similar extension points exist, see Microsoft.Common.targets.
  <Target Name=""BeforeBuild"">
  </Target>
  <Target Name=""AfterBuild"">
  </Target>
  -->
</Project>");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "14.0.0.0")]
    public class ProjectFileBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        /// <summary>
        /// Helper to produce culture-oriented representation of an object as a string
        /// </summary>
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
