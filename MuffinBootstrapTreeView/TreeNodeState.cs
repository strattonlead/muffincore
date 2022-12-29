using Newtonsoft.Json;

namespace BootstrapTreeView
{
    public class TreeNodeState
    {
        /// <summary>
        /// Whether or not a node is checked, represented by a checkbox style glyphicon.
        /// </summary>
        [JsonProperty(PropertyName = "checked")]
        public bool Checked { get; set; }

        /// <summary>
        /// Whether or not a node is disabled (not selectable, expandable or checkable).
        /// </summary>
        [JsonProperty(PropertyName = "disabled")]
        public string Disabled { get; set; }

        /// <summary>
        /// Whether or not a node is expanded i.e. open. Takes precedence over global option levels.
        /// </summary>
        [JsonProperty(PropertyName = "expanded")]
        public string Expanded { get; set; }

        /// <summary>
        /// Whether or not a node is selected.
        /// </summary>
        [JsonProperty(PropertyName = "selected")]
        public string Selected { get; set; }
    }
}
