// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Aspire.Hosting;

internal static partial class VisualStudioExtensions
{
    /// <summary>
    /// This is a collection of the interfaces used to attach a debugger. We included them manually instead of referencing DTE so we don't
    /// have it as a dependency. Since they're all COM interfaces, we can just define them here. Importantly:
    /// - We only need a small subset of the DTE interfaces, so this keeps the dependency surface small.
    /// - We must include members in the type up until the ones we need
    /// - The members we don't need can be of type object
    /// </summary>
    private static class EnvDTE
    {
        [ComImport]
        [Guid("95AC1923-6EAA-427C-B43E-6274A8CA6C95")]
        [DefaultMember("Name")]
        public interface Process2
        {
            [DispId(1)]
            void Attach();

            [DispId(2)]
            void Detach([In] bool WaitForBreakOrEnd = true);

            [DispId(3)]
            void Break([In] bool WaitForBreakMode = true);

            [DispId(4)]
            void Terminate([In] bool WaitForBreakOrEnd = true);

            [DispId(0)]
            string Name
            {
                [DispId(0)]
                [return: MarshalAs(UnmanagedType.BStr)]
                get;
            }

            [DispId(100)]
            int ProcessID
            {
                [DispId(100)]
                get;
            }

            [DispId(101)]
            object stub_Programs { get; }

            [DispId(200)]
            DTE DTE
            {
                [DispId(200)]
                [return: MarshalAs(UnmanagedType.Interface)]
                get;
            }

            [DispId(201)]
            object stub_Parent { get; }

            [DispId(202)]
            object stub_Collection { get; }

            //
            // Parameters:
            //   Engines:
            //     Single System.String or an array of either System.String or EnvDTE80.Engine objects
            [DispId(1001)]
            void Attach2([In][MarshalAs(UnmanagedType.Struct)] object Engines);
        }

        [ComImport]
        [ComVisible(false)]
        [DefaultMember("Name")]
        [Guid("04A72314-32E9-48E2-9B87-A63603454F3E")]
        [CoClass(typeof(DTEClass))]
        public interface _DTE : DTE
        {
        }

        [ComImport]
        [Guid("3C9CFE1E-389F-4118-9FAD-365385190329")]
        [ComVisible(false)]
        [ClassInterface(ClassInterfaceType.None)]
        [DefaultMember("Name")]
        public class DTEClass
        {
        }


        [ComImport]
        [DefaultMember("Name")]
        [Guid("04A72314-32E9-48E2-9B87-A63603454F3E")]
        [TypeLibType(4160)]
        public interface DTE
        {
            object Stub_Name { get; }
            object Stub_FileName { get; }
            object Stub_Version { get; }
            object Stub_CommandBars { get; }
            object Stub_Windows { get; }
            object Stub_Events { get; }
            object Stub_AddIns { get; }
            object Stub_MainWindow { get; }
            object Stub_ActiveWindow { get; }
            object Stub_Quit();
            object Stub_DisplayMode { get; set; }
            object Stub_Solution { get; }
            object Stub_Commands { get; }
            object Stub_GetObject();
            object Stub_Properties { get; }
            object Stub_SelectedItems { get; }
            object Stub_CommandLineArguments { get; }
            object Stub_OpenFile();
            object Stub_IsOpenFile { get; }
            object Stub_DTE { get; }
            object Stub_LocaleID { get; }
            object Stub_WindowConfigurations { get; }
            object Stub_Documents { get; }
            object Stub_ActiveDocument { get; }
            object Stub_ExecuteCommand();
            object Stub_Globals { get; }
            object Stub_StatusBar { get; }
            object Stub_FullName { get; }
            object Stub_UserControl { get; set; }
            object Stub_ObjectExtenders { get; }
            object Stub_Find { get; }
            object Stub_Mode { get; }
            object Stub_LaunchWizard();
            object Stub_ItemOperations { get; }
            object Stub_UndoContext { get; }
            object Stub_Macros { get; }
            object Stub_ActiveSolutionProjects { get; }
            object Stub_MacrosIDE { get; }
            object Stub_RegistryRoot { get; }
            object Stub_Application { get; }
            object Stub_ContextAttributes { get; }
            object Stub_SourceControl { get; }
            object Stub_SuppressUI { get; set; }

            Debugger Debugger
            {
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                [return: MarshalAs(UnmanagedType.Interface)]
                get;
            }
        }

        [ComImport]
        [Guid("338FB9A0-BAE5-11D2-8AD1-00C04F79E479")]
        public interface Debugger
        {
            object Stub_GetExpression();
            object Stub_DetachAll();
            object Stub_StepInto();
            object Stub_StepOver();
            object Stub_StepOut();
            object Stub_Go();
            object Stub_Break();
            object Stub_Stop();
            object Stub_SetNextStatement();
            object Stub_RunToCursor();
            object Stub_ExecuteStatement();
            object Stub_Breakpoints { get; }
            object Stub_Languages { get; }
            object Stub_CurrentMode { get; }
            object Stub_CurrentProcess { get; set; }
            object Stub_CurrentProgram { get; set; }
            object Stub_CurrentThread { get; set; }
            object Stub_CurrentStackFrame { get; set; }
            object Stub_HexDisplayMode { get; set; }
            object Stub_HexInputMode { get; set; }
            object Stub_LastBreakReason { get; }
            object Stub_BreakpointLastHit { get; }
            object Stub_AllBreakpointsLastHit { get; }

            Processes DebuggedProcesses
            {
                [return: MarshalAs(UnmanagedType.Interface)]
                get;
            }

            Processes LocalProcesses
            {
                [return: MarshalAs(UnmanagedType.Interface)]
                get;
            }
        }

        [ComImport]
        [DefaultMember("Item")]
        [Guid("9F379969-5EAC-4A54-B2BC-6946CFFB56EF")]
        public interface Processes : IEnumerable
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            Process2 Item([In][MarshalAs(UnmanagedType.Struct)] object index);

            [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler, CustomMarshalers, Version=2.0.0.0, Culture=neutral, publicKeyToken=b03f5f7f11d50a3a")]
            new IEnumerator GetEnumerator();
        }
    }
}
