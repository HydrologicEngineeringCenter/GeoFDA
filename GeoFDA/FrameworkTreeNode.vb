Public MustInherit Class FrameworkTreeNode
    Inherits TreeViewItem
    Private Shared _RootDirectory As String
    Private Shared _maptreeview As OpenGLMapping.MapTreeView
    Private Shared _IsExpanded As Boolean = True
    Private Shared _MainContextMenu As Menu
    Private Shared _MainWindow As MainWindow
    Private _firstlevelsubnodes As List(Of FrameworkTreeNode)
    Private _SavedState As Boolean = True
    'Private _UserDefinedName As String
    Private Shared tvitemmouseenter As FrameworkTreeNode = Nothing
    Private Shared MouseEnterCounter As Integer = 0
    Public Event Save(sender As Object, e As System.Windows.RoutedEventArgs)
    ''' <summary>
    ''' The base class constructor creates the treenode with the proper header name, and builds any required directories if they do not exist.
    ''' </summary>
    ''' <param name="headername">the name for the treeviewitem you would like to have displayed on the study tree.</param>
    ''' <remarks></remarks>
    Sub New(ByVal headername As String)
        MyBase.New()
        MyBase.IsExpanded = _IsExpanded
        _firstlevelsubnodes = New List(Of FrameworkTreeNode)
        MyBase.Header = headername
        MyBase.ToolTip = ""
        If System.IO.Directory.Exists(GetCurrentDirectory) Then
        Else
            System.IO.Directory.CreateDirectory(GetCurrentDirectory)
        End If
        SetContextMenu()
    End Sub
    ''' <summary>
    ''' This returns the current full directory which includes the root directory plus any sub directories.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property GetCurrentDirectory As String
        Get
            If GetSubDirectory = "" Then
                Return _RootDirectory
            Else
                Return _RootDirectory & GetSubDirectory
            End If
        End Get
    End Property

    ''' <summary>
    ''' This allows read access to the shared root directory.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property GetRootDirectory As String
        Get
            Return _RootDirectory
        End Get
    End Property
    ''' <summary>
    ''' this allows write access to the shared root directory
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    Public Shared WriteOnly Property SetRootDirectory As String
        Set(value As String)
            _RootDirectory = value
        End Set
    End Property
    ''' <summary>
    ''' Allows write access to set the shared maptreeview
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    Public Shared WriteOnly Property SetMapTreeView As OpenGLMapping.MapTreeView
        Set(value As OpenGLMapping.MapTreeView)
            _maptreeview = value
        End Set
    End Property
    ''' <summary>
    ''' Allows write access to the shared variable of "IsExpanded" on the base class TreeViewItem.  This must be set before the framework treenodes are added, but it is shared, so setting it for one, sets it for all.
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    Public Shared WriteOnly Property SetIsExpanded As Boolean
        Set(value As Boolean)
            _IsExpanded = value
        End Set
    End Property
    ''' <summary>
    ''' Allows write access to the shared Main context menu for the program.  this will allow any framework treenode to assign handlers to the click events for items in the main context menu.
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    Public Shared WriteOnly Property SetMainContextMenu As Menu
        Set(value As Menu)
            _MainContextMenu = value
        End Set
    End Property
    Public Shared WriteOnly Property SetMainWindow As Window
        Set(value As Window)
            _MainWindow = value
        End Set
    End Property
    ''' <summary>
    ''' Allows access to the private shared main context menu.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property GetMainContextMenu As Menu
        Get
            Return _MainContextMenu
        End Get
    End Property
    Public ReadOnly Property GetMainWindow As Window
        Get
            Return _MainWindow
        End Get
    End Property
    ''' <summary>
    ''' Returns all immediate decendents of the current treeviewitem
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property FirstLevelSubNodes As List(Of FrameworkTreeNode)
        Get
            Return _firstlevelsubnodes
        End Get
    End Property
    ''' <summary>
    ''' provides read access to the shared maptreeview so that feature nodes can be added from the study tree.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property GetMapTreeView As OpenGLMapping.MapTreeView
        Get
            Return _maptreeview
        End Get
    End Property
    ''' <summary>
    ''' A property that allows any framework tree node to declare if it is saved or if there are edits that are unsaved. True = saved False = unsaved. Also, it sets unsaved treenodes to have an asterisk in the header name.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Saved As Boolean
        Get
            Return _SavedState
        End Get
        Set(value As Boolean)
            _SavedState = value
            UpdateHeaderForSave()
        End Set
    End Property
    ''' <summary>
    ''' Returns a path relative to the current root directory of the project, unless the filepath is not in the root directory, then it returns the file path as is.
    ''' </summary>
    ''' <param name="FilePath"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ConvertToRelativePath(ByVal FilePath As String) As String
        'If GetRootDirectory <> "" AndAlso FilePath.Contains(GetRootDirectory) Then
        '    Return FilePath.Replace(GetRootDirectory, ".")
        'Else
        '    Return FilePath
        'End If
        Return FilePathManipulator.MakeRelative(FilePath, GetRootDirectory)
    End Function
    Public Function ConvertFromRelativePath(ByVal FilePath As String) As String
        Return FilePathManipulator.MakeAbsolute(FilePath, GetRootDirectory)

    End Function
    ''' <summary>
    ''' Sets a visual que for the user that this framework tree item has unsaved information
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub UpdateHeaderForSave()
        If _SavedState Then
            If MyBase.FontStyle = System.Windows.FontStyles.Italic Then
                MyBase.FontStyle = System.Windows.FontStyles.Normal
            Else

            End If
        Else
            If MyBase.FontStyle = System.Windows.FontStyles.Italic Then
                'previously not saved so still not saved
            Else
                'previously saved, so make it known it is not saved anymore.
                MyBase.FontStyle = System.Windows.FontStyles.Italic
            End If
        End If
    End Sub
    ''' <summary>
    ''' Adds the framework treenode to the firstlevel subnodes for its parent, adds the item to the visual study tree, and sends a message to the study tree node that a new child has been added to the tree.
    ''' </summary>
    ''' <param name="FTN">the framework treenode that is being added.</param>
    ''' <remarks></remarks>
    Public Overridable Sub AddFrameworkTreeNode(ByVal FTN As FrameworkTreeNode)
        'check for name conflicts
        'CheckForNameConflicts(Me) 'some of the various treenodes are copying data in their constructors. this causes issues with file copying, and name conflicts arent checked until the constructor is exited.
        'add to the list of current children
        _firstlevelsubnodes.Add(FTN)
        'add to the tree...
        MyBase.Items.Add(FTN)
        'tell your parent something happened.
        Bubble(FTN)
    End Sub
    ''' <summary>
    ''' This is intended to send a message to the studytreenode that a new child is being added, and to check for name conflicts before it is added. currently has issues...
    ''' </summary>
    ''' <param name="ftn"></param>
    ''' <remarks></remarks>
    Protected Overridable Function CheckForNameConflicts(ByVal ftn As FrameworkTreeNode) As Boolean
        If IsNothing(Parent) Then Return False
        Return CType(Parent, FrameworkTreeNode).CheckForNameConflicts(ftn)
    End Function


    ''' <summary>
    ''' This is to prevent the native inheritance of parent context menus if no menu is set for a treeviewitem
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub FrameworkTreeNode_MouseRightButtonUp(sender As Object, e As System.Windows.Input.MouseButtonEventArgs) Handles Me.MouseRightButtonUp
        If IsNothing(MyBase.ContextMenu) Then
            Dim c As New ContextMenu
            c.IsEnabled = False
            c.Visibility = Windows.Visibility.Collapsed
            MyBase.ContextMenu = c
        End If
    End Sub
    ''' <summary>
    ''' This sends a message to the studytreenode that a new frameworktreenode is being added to the study tree, and that it needs to be added to the treenode list for messaging.
    ''' </summary>
    ''' <param name="ftn"></param>
    ''' <remarks></remarks>
    Public Overridable Sub Bubble(ftn As FrameworkTreeNode)
        CType(Parent, FrameworkTreeNode).Bubble(ftn)
    End Sub
    ''' <summary>
    ''' Adding the ability to compare treenodes to determine if they are equivalent.  right now it is only checking type and header name. (if checkfornameconflict gets fixed, this will be enough to declare equivalency)
    ''' </summary>
    ''' <param name="left"></param>
    ''' <param name="right"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overloads Shared Operator =(left As FrameworkTreeNode, right As FrameworkTreeNode)
        If left.GetType = right.GetType Then
            'need to do some stricter comparison.  possibly have a hash capability on each concrete class
            If left.Header = right.Header Then
                Return True
            Else
                Return False
            End If
        Else
            Return False
        End If
    End Operator
    ''' <summary>
    ''' requried if the operator = is defined
    ''' </summary>
    ''' <param name="left"></param>
    ''' <param name="right"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overloads Shared Operator <>(left As FrameworkTreeNode, right As FrameworkTreeNode)
        Return Not (left = right)
    End Operator
    ''' <summary>
    ''' Allows a frameworktreenode to request for all current treenodes of a specific type. Returns a list of existing ftns of that type
    ''' </summary>
    ''' <param name="NodeTypeToGetInfoFrom">The type of treenode to search for</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Overridable Function GetAllFrameworkTreenodesOfType(ByVal NodeTypeToGetInfoFrom As System.Type) As List(Of FrameworkTreeNode)
        Return CType(Parent, FrameworkTreeNode).GetAllFrameworkTreenodesOfType(NodeTypeToGetInfoFrom)
    End Function
    ''' <summary>
    ''' do not use this for hydraulics child tree nodes!!!!!
    ''' </summary>
    ''' <param name="header"></param>
    ''' <param name="nodetype"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Overridable Function GetFrameworkTreenodeByNameAndType(ByVal header As String, ByVal nodetype As System.Type) As FrameworkTreeNode
        Dim list As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(nodetype)
        For i As Integer = 0 To list.Count - 1
            If list(i).Header = header Then Return list(i)
        Next
        Return Nothing
    End Function
    ''' <summary>
    ''' Enables treenodes with logical children that are not user specified to be added programatically.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub AddFrameworkChildren()
        'if no children then no children!
    End Sub
    ''' <summary>
    ''' Allows framework children to be added through an xml element, which is used when an existing study is opened.
    ''' </summary>
    ''' <param name="ele"></param>
    ''' <remarks></remarks>
    Public Overridable Sub AddFrameworkChildren(ByVal ele As XElement)
        'if no children then no children?
    End Sub
    ''' <summary>
    ''' This defines the subdirectory for the frameworktreenode.  For this to function properly, the string must be a const declaration because the function is called during the base class constructor.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public MustOverride ReadOnly Property GetSubDirectory As String
    ''' <summary>
    ''' This defines the node name for any framework treenode when it is being written to xlement.  (alternatively we could use the name of the class itself)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public MustOverride ReadOnly Property GetNodeName As String
    ''' <summary>
    ''' this is called during the base class constructor to add any context menu items that are unique to the specific frameworktreenode. IF no code is defined in this sub, the context menu is set to be collapsed.
    ''' </summary>
    ''' <remarks></remarks>
    Public MustOverride Sub SetContextMenu()
    Public Overridable Sub RemoveMainMenuContextOptions()
        'do nothing unless told otherwise.
        For Each tn As FrameworkTreeNode In _firstlevelsubnodes 'if any children have them tell em to remove em
            tn.RemoveMainMenuContextOptions()
        Next
    End Sub
    Public MustOverride Sub OnSaveEvent(sender As Object, e As System.Windows.RoutedEventArgs) Handles Me.Save
    ''' <summary>
    ''' allows a frameworktreenode to define its own saving location and data for any unique attributes that are not stored at the project file level.
    ''' </summary>
    ''' <remarks></remarks>
    Public MustOverride Sub WriteToXML()
    ''' <summary>
    ''' This allows a framework treenode to define the xelement that is written to the project directory.  If this is not overriden, a node of the "GetNodeName" will be written and all firstlevel treenodes will be written as well.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Function WriteToXMLElement() As XElement
        Dim ele As New XElement(GetNodeName)
        For i = 0 To FirstLevelSubNodes.Count - 1
            ele.Add(FirstLevelSubNodes(i).WriteToXMLElement)
        Next
        Return ele
    End Function
    ''' <summary>
    ''' this allows a framework treenode to define the way to construct itself from an xelement.  if this is not overriden, a parent must construct the treenode or it will not be added to the tree on open existing.
    ''' </summary>
    ''' <param name="xele"></param>
    ''' <remarks></remarks>
    Public Overridable Sub ReadFromXMLElement(xele As XElement)

    End Sub
    ''' <summary>
    ''' This allows a framework treenode the opporutnity to read anything it has written to xml that is unique to the treenode and not saved in the project study tree xelement node.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <remarks></remarks>
    Public MustOverride Sub ReadFromXMl(ByVal path As String)
    Public Overridable Sub ReportMessage(ByVal message As String)
        '_MainWindow.ReportMessage(message)
        CType(Parent, FrameworkTreeNode).ReportMessage(message)

    End Sub
    Private Sub FrameworkTreeNode_ToolTipOpening(sender As Object, e As ToolTipEventArgs) Handles Me.ToolTipOpening
        If IsNothing(Me.ToolTip) OrElse Me.ToolTip = "" Then
            e.Handled = True
        End If
    End Sub
End Class
