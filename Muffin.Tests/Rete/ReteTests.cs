using Microsoft.VisualStudio.TestTools.UnitTesting;
using Muffin.Rete;
using Muffin.Rete.Engine;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Muffin.Tests.Rete
{
    [TestClass]
    public class ReteTests
    {
        [TestMethod]
        public async Task SampleTest()
        {
            using (var stream = typeof(ReteTests).Assembly.GetManifestResourceStream("Muffin.Tests.Rete.model1.json"))
            using (var mem = new MemoryStream())
            {
                stream.CopyTo(mem);
                var bytes = mem.ToArray();
                var json = Encoding.UTF8.GetString(bytes);
                var model = JsonConvert.DeserializeObject<ReteModel>(json);
                var engine = new ReteEngine();
                var components = ReteEngine.GetSystemComponents();
                engine.AddComponents(components);
                //await engine.Run(model, 1, default);
                await engine.Run(model, new ReteEvent() { Id = 2 }, default);
            }

        }

        private Component _loadComponent(string name)
        {
            using (var stream = typeof(ReteTests).Assembly.GetManifestResourceStream($"Muffin.Tests.Rete.{name}"))
            using (var mem = new MemoryStream())
            {
                stream.CopyTo(mem);
                var bytes = mem.ToArray();
                var json = Encoding.UTF8.GetString(bytes);
                return JsonConvert.DeserializeObject<Component>(json);
            }
        }
    }
}
