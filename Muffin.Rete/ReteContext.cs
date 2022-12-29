using Muffin.Rete.Engine;
namespace Muffin.Rete
{
    public class ReteContext
    {
        public ReteEngine Engine { get; set; }
        public ReteModel Model { get; set; }
        public ReteEvent Event { get; set; }
    }

    public class ReteEvent
    {
        public int Id { get; set; }
    }
}
