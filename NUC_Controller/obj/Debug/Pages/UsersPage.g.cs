﻿#pragma checksum "..\..\..\Pages\UsersPage.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "77154B8711FEC8527401DA49374BB2E4E7F95DCE"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using NUC_Controller.Pages;
using NUC_Controller.UserControls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace NUC_Controller.Pages {
    
    
    /// <summary>
    /// UsersPage
    /// </summary>
    public partial class UsersPage : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 31 "..\..\..\Pages\UsersPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button buttonAdd;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\Pages\UsersPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button buttonRemove;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\..\Pages\UsersPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button buttonEdit;
        
        #line default
        #line hidden
        
        
        #line 39 "..\..\..\Pages\UsersPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox comboOrderBy;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\..\Pages\UsersPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image imgUp;
        
        #line default
        #line hidden
        
        
        #line 51 "..\..\..\Pages\UsersPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image imgDown;
        
        #line default
        #line hidden
        
        
        #line 62 "..\..\..\Pages\UsersPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox listboxUsers;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/NUC_Controller;component/pages/userspage.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Pages\UsersPage.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 10 "..\..\..\Pages\UsersPage.xaml"
            ((NUC_Controller.Pages.UsersPage)(target)).KeyUp += new System.Windows.Input.KeyEventHandler(this.PageKeyUp);
            
            #line default
            #line hidden
            
            #line 11 "..\..\..\Pages\UsersPage.xaml"
            ((NUC_Controller.Pages.UsersPage)(target)).Loaded += new System.Windows.RoutedEventHandler(this.PageLoaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.buttonAdd = ((System.Windows.Controls.Button)(target));
            
            #line 31 "..\..\..\Pages\UsersPage.xaml"
            this.buttonAdd.Click += new System.Windows.RoutedEventHandler(this.ButtonAddClick);
            
            #line default
            #line hidden
            return;
            case 3:
            this.buttonRemove = ((System.Windows.Controls.Button)(target));
            
            #line 32 "..\..\..\Pages\UsersPage.xaml"
            this.buttonRemove.Click += new System.Windows.RoutedEventHandler(this.ButtonRemoveClick);
            
            #line default
            #line hidden
            return;
            case 4:
            this.buttonEdit = ((System.Windows.Controls.Button)(target));
            
            #line 33 "..\..\..\Pages\UsersPage.xaml"
            this.buttonEdit.Click += new System.Windows.RoutedEventHandler(this.ButtonEditClick);
            
            #line default
            #line hidden
            return;
            case 5:
            this.comboOrderBy = ((System.Windows.Controls.ComboBox)(target));
            
            #line 40 "..\..\..\Pages\UsersPage.xaml"
            this.comboOrderBy.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.ComboOrderBySelectionChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 43 "..\..\..\Pages\UsersPage.xaml"
            ((System.Windows.Controls.Button)(target)).MouseEnter += new System.Windows.Input.MouseEventHandler(this.ButtonMouseEnter);
            
            #line default
            #line hidden
            
            #line 44 "..\..\..\Pages\UsersPage.xaml"
            ((System.Windows.Controls.Button)(target)).MouseLeave += new System.Windows.Input.MouseEventHandler(this.ButtonMouseLeave);
            
            #line default
            #line hidden
            
            #line 45 "..\..\..\Pages\UsersPage.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.ButtonOrderByClick);
            
            #line default
            #line hidden
            return;
            case 7:
            this.imgUp = ((System.Windows.Controls.Image)(target));
            return;
            case 8:
            this.imgDown = ((System.Windows.Controls.Image)(target));
            return;
            case 9:
            this.listboxUsers = ((System.Windows.Controls.ListBox)(target));
            
            #line 63 "..\..\..\Pages\UsersPage.xaml"
            this.listboxUsers.KeyDown += new System.Windows.Input.KeyEventHandler(this.ListboxUsersKeyDown);
            
            #line default
            #line hidden
            
            #line 64 "..\..\..\Pages\UsersPage.xaml"
            this.listboxUsers.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.LibtboxUsersMouseDoubleClick);
            
            #line default
            #line hidden
            
            #line 65 "..\..\..\Pages\UsersPage.xaml"
            this.listboxUsers.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.LibtboxUsersSelectionChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

