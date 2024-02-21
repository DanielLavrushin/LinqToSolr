using LinqToSolr.Tests.Models;

namespace LinqToSolr.Tests
{
    [TestClass]
    public class ConnectionTests
    {
        [TestMethod]
        public void InitServer()
        {
            var guidstr = "00000000-0000-0000-0000-000000000000";
            var guid = new Guid(guidstr);
            var date = DateTime.Now;
            var service = new LinqToSolrService();
            var list = service.AsQueryable<SolrDocument>().Where(x => x.IsActive || x.IsActive == false).ToList();


        }
    }
}