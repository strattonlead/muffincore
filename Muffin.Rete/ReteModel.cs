using Newtonsoft.Json;

namespace Muffin.Rete
{
    public class ReteModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "nodes")]
        public Dictionary<string, NodeData> Nodes { get; set; } = new Dictionary<string, NodeData>();

        public NodeData FindById(int nodeId)
        {
            Nodes.TryGetValue(nodeId.ToString(), out var node);
            return node;
        }

        public NodeData FindByEvent(ReteEvent @event)
        {
            return Nodes.Values.FirstOrDefault(x => x.name == "Event" && x.data.@event == @event.Id);
        }

        public string[] GetComponents()
        {
            if (Nodes == null)
            {
                return null;
            }

            return Nodes.Values.Select(x => x.name).Distinct().ToArray();
        }
    }
}
