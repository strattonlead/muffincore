using Newtonsoft.Json;
using System.Collections.Generic;

namespace BootstrapTreeView
{
    public class TreeViewOptions
    {
        /// <summary>
        /// Array of Objects. No default, expects data
        ///
        /// This is the core data to be displayed by the tree view.
        /// </summary>
        [JsonProperty(PropertyName = "date")]
        public List<TreeNode> Data { get; set; }

        /// <summary>
        /// String, any legal color value. Default: inherits from Bootstrap.css.
        ///
        ///Sets the default background color used by all nodes, except when overridden on a per node basis in data.
        /// </summary>
        [JsonProperty(PropertyName = "backColor")]
        public string BackColor { get; set; }

        /// <summary>
        /// String, any legal color value. Default: inherits from Bootstrap.css.
        ///
        /// Sets the border color for the component; set showBorder to false if you don't want a visible border.
        /// </summary>
        [JsonProperty(PropertyName = "borderColor")]
        public string BorderColor { get; set; }

        /// <summary>
        /// String, class names(s). Default: "glyphicon glyphicon-check" as defined by Bootstrap Glyphicons
        ///
        /// Sets the icon to be as a checked checkbox, used in conjunction with showCheckbox.
        /// </summary>
        [JsonProperty(PropertyName = "checkedIcon")]
        public string CheckedIcon { get; set; }

        /// <summary>
        /// String, class name(s). Default: "glyphicon glyphicon-minus" as defined by Bootstrap Glyphicons
        ///
        /// Sets the icon to be used on a collapsible tree node.
        /// </summary>
        [JsonProperty(PropertyName = "collapseIcon")]
        public string CollapseIcon { get; set; }

        /// <summary>
        /// String, any legal color value. Default: inherits from Bootstrap.css.
        ///
        /// Sets the default foreground color used by all nodes, except when overridden on a per node basis in data.
        /// </summary>
        [JsonProperty(PropertyName = "color")]
        public string Color { get; set; }

        /// <summary>
        /// String, class name(s). Default: "glyphicon" as defined by Bootstrap Glyphicons
        /// 
        /// Sets the icon to be used on a tree node with no child nodes.
        /// </summary>
        [JsonProperty(PropertyName = "emptyIcon")]
        public string EmptyIcon { get; set; }

        /// <summary>
        /// Boolean. Default: false
        ///
        ///Whether or not to present node text as a hyperlink.The href value of which must be provided in the data structure on a per node basis.
        /// </summary>
        [JsonProperty(PropertyName = "enableLinks")]
        public bool EnableLinks { get; set; }

        /// <summary>
        /// String, class name(s). Default: "glyphicon glyphicon-plus" as defined by Bootstrap Glyphicons
        ///
        ///Sets the icon to be used on an expandable tree node.
        /// </summary>
        [JsonProperty(PropertyName = "expandIcon")]
        public string ExpandIcon { get; set; }

        /// <summary>
        /// Boolean. Default: true
        ///
        /// Whether or not to highlight search results.
        /// </summary>
        [JsonProperty(PropertyName = "highlightSearchResults")]
        public bool HighlightSearchResults { get; set; } = true;

        /// <summary>
        /// Boolean. Default: true
        /// 
        /// Whether or not to highlight the selected node.
        /// </summary>
        [JsonProperty(PropertyName = "highlightSelected")]
        public bool HighlightSelected { get; set; } = true;

        /// <summary>
        /// Integer. Default: 2
        /// 
        /// Sets the number of hierarchical levels deep the tree will be expanded to by default.
        /// </summary>
        [JsonProperty(PropertyName = "levels")]
        public int Levels { get; set; } = 2;

        /// <summary>
        /// Boolean. Default: false
        ///
        /// Whether or not multiple nodes can be selected at the same time.
        /// </summary>
        [JsonProperty(PropertyName = "multiSelect")]
        public bool MultiSelect { get; set; }

        /// <summary>
        /// String, class name(s). Default: "glyphicon glyphicon-stop" as defined by Bootstrap Glyphicons
        /// 
        /// Sets the default icon to be used on all nodes, except when overridden on a per node basis in data.
        /// </summary>
        [JsonProperty(PropertyName = "nodeIcon")]
        public string NodeIcon { get; set; }

        /// <summary>
        /// String, any legal color value. Default: '#F5F5F5'.
        ///
        /// Sets the default background color activated when the users cursor hovers over a node.
        /// </summary>
        [JsonProperty(PropertyName = "onhoverColor")]
        public string OnhoverColor { get; set; }

        /// <summary>
        /// String, class name(s). Default: "glyphicon glyphicon-stop" as defined by Bootstrap Glyphicons
        ///
        /// Sets the default icon to be used on all selected nodes, except when overridden on a per node basis in data.
        /// </summary>
        [JsonProperty(PropertyName = "selectedIcon")]
        public string SelectedIcon { get; set; }

        /// <summary>
        /// String, any legal color value. Default: undefined, inherits.
        /// 
        /// Sets the background color of the selected node.
        /// </summary>
        [JsonProperty(PropertyName = "searchResultBackColor")]
        public string SearchResultBackColor { get; set; }

        /// <summary>
        /// String, any legal color value. Default: '#D9534F'.
        ///
        /// Sets the foreground color of the selected node.
        /// </summary>
        [JsonProperty(PropertyName = "searchResultColor")]
        public string SearchResultColor { get; set; }

        /// <summary>
        /// String, any legal color value. Default: '#428bca'.
        ///
        /// Sets the background color of the selected node.
        /// </summary>
        [JsonProperty(PropertyName = "selectedBackColor")]
        public string SelectedBackColor { get; set; }

        /// <summary>
        /// String, any legal color value. Default: '#FFFFFF'.
        ///
        /// Sets the foreground color of the selected node.
        /// </summary>
        [JsonProperty(PropertyName = "selectedColor")]
        public string SelectedColor { get; set; }

        /// <summary>
        /// Boolean. Default: true
        ///
        /// Whether or not to display a border around nodes.
        /// </summary>
        [JsonProperty(PropertyName = "showBorder")]
        public bool ShowBorder { get; set; } = true;

        /// <summary>
        /// Boolean. Default: false
        ///
        /// Whether or not to display checkboxes on nodes.
        /// </summary>
        [JsonProperty(PropertyName = "showCheckbox")]
        public bool ShowCheckbox { get; set; }

        /// <summary>
        /// Boolean. Default: true
        ///
        /// Whether or not to display a nodes icon.
        /// </summary>
        [JsonProperty(PropertyName = "showIcon")]
        public bool ShowIcon { get; set; } = true;

        /// <summary>
        /// Boolean. Default: false
        ///
        /// Whether or not to display tags to the right of each node.The values of which must be provided in the data structure on a per node basis.
        /// </summary>
        [JsonProperty(PropertyName = "showTags")]
        public bool ShowTags { get; set; }

        /// <summary>
        /// String, class names(s). Default: "glyphicon glyphicon-unchecked" as defined by Bootstrap Glyphicons
        ///
        /// Sets the icon to be as an unchecked checkbox, used in conjunction with showCheckbox.
        /// </summary>
        [JsonProperty(PropertyName = "uncheckedIcon")]
        public string UncheckedIcon { get; set; }
    }
}
