using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using static Muffin.Common.Util.CollectionHelper;

namespace BootstrapTreeView
{
    public class TreeNode : ITreeItem<TreeNode>
    {
        #region ITreeItem

        [JsonIgnore]
        public long Id { get; set; }

        [JsonIgnore]
        public long ParentId { get; set; }

        [JsonIgnore]
        public List<TreeNode> Children { get; set; }

        #endregion

        /// <summary>
        /// The text value displayed for a given tree node, typically to the right of the nodes icon.
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// The icon displayed on a given node, typically to the left of the text.
        /// 
        /// For simplicity we directly leverage Bootstraps Glyphicons support and as such you should provide both the base class and individual icon class separated by a space.
        ///
        /// By providing the base class you retain full control over the icons used.If you want to use your own then just add your class to this icon field.
        /// </summary>
        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; set; }

        /// <summary>
        /// The icon displayed on a given node when selected, typically to the left of the text.
        /// </summary>
        [JsonProperty(PropertyName = "selectedIcon")]
        public string SelectedIcon { get; set; }

        /// <summary>
        /// The foreground color used on a given node, overrides global color option.
        /// </summary>
        [JsonProperty(PropertyName = "color")]
        public string Color { get; set; }

        /// <summary>
        /// The background color used on a given node, overrides global color option.
        /// </summary>
        [JsonProperty(PropertyName = "backColor")]
        public string BackColor { get; set; }

        /// <summary>
        /// Used in conjunction with global enableLinks option to specify anchor tag URL on a given node.
        /// </summary>
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }

        /// <summary>
        /// Whether or not a node is selectable in the tree. False indicates the node should act as an expansion heading and will not fire selection events.
        /// </summary>
        [JsonProperty(PropertyName = "selectable")]
        public bool Selectable { get; set; }

        /// <summary>
        /// Describes a node's initial state.
        /// </summary>
        [JsonProperty(PropertyName = "state")]
        public TreeNodeState State { get; set; }

        /// <summary>
        /// Used in conjunction with global showTags option to add additional information to the right of each node; using Bootstrap Badges
        /// </summary>
        [JsonProperty(PropertyName = "tags")]
        public List<string> Tags { get; set; }

        [JsonProperty(PropertyName = "nodes")]
        public List<TreeNode> Nodes { get; set; }
    }

    public static class TreeNodeExtensions
    {
        public static IEnumerable<T> Expand<T>(this IEnumerable<T> nodes, int level)
            where T : TreeNode
        {
            if (level <= 0)
            {
                return nodes;
            }
            _expand(nodes, level);
            return nodes;
        }

        private static void _expand<T>(this IEnumerable<T> nodes, int level)
            where T : TreeNode
        {
            foreach (var node in nodes)
            {
                if (node.State == null)
                {
                    node.State = new TreeNodeState();
                }
                node.State.Expanded = "true";

                if (node.Children != null && node.Children.Any() && level >= 2)
                {
                    _expand(node.Children, level - 1);
                }
            }
        }

        public static IEnumerable<T> Expand<T>(this IEnumerable<T> nodes, IEnumerable<long> expandedIds)
            where T : TreeNode
        {
            if (expandedIds != null && expandedIds.Any())
            {
                _expand(nodes, expandedIds);
            }
            return nodes;
        }

        public static void _expand<T>(this IEnumerable<T> nodes, IEnumerable<long> expandedIds)
            where T : TreeNode
        {
            foreach (var node in nodes)
            {
                if (expandedIds.Contains(node.Id))
                {
                    if (node.State == null)
                    {
                        node.State = new TreeNodeState();
                    }
                    node.State.Expanded = "true";
                }

                if (node.Children != null && node.Children.Any())
                {
                    _expand(node.Children, expandedIds);
                }
            }
        }
    }
}
