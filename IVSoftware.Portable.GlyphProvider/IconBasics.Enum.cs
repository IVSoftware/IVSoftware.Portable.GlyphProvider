using System.ComponentModel;

namespace IVSoftware.Portable
{
    /// <summary>
    /// [Careful] 
    /// This file must remain C# Compiler.
    /// The CreateEnumPrototype utility bases its
    /// code gen on this concrete definition.
    /// </summary>
    public enum IconBasics
    {
        [Description("add")]
        Add,

        [Description("delete")]
        Delete,

        [Description("edit")]
        Edit,

        [Description("ellipsis-horizontal")]
        EllipsisHorizontal,

        [Description("ellipsis-vertical")]
        EllipsisVertical,

        [Description("filter")]
        Filter,

        [Description("menu")]
        Menu,

        [Description("search")]
        Search,

        [Description("settings")]
        Settings,

        [Description("checked")]
        Checked,

        [Description("unchecked")]
        Unchecked,

        [Description("eye")]
        Eye,

        [Description("eye-off")]
        EyeOff,

        [Description("help-circled")]
        HelpCircled,

        [Description("help-circled-alt")]
        HelpCircledAlt,

        [Description("doc-empty")]
        DocEmpty,

        [Description("doc")]
        Doc,

        [Description("doc-new")]
        DocNew,
    }

}
