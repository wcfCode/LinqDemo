using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqDemo
{
    public class TreePermission
    {

        private static void ImplementPecursion()
        {
            //实现递归向上找权限
            List<Org> orgs = initializeOrg();
            Func<string, List<string>> Pecursion = null;
            List<string> bb = new List<string>();

            Pecursion = id =>
            {
                Org org = orgs.FirstOrDefault(x => x.Id == id);
                if (org.Permission != null && org.Permission.Count > 0)
                {
                    bb = org.Permission;
                }
                else if (org.ParentId != "0")
                {
                    Pecursion(org.ParentId);
                }
                return bb;
            };

            List<string> aa = Pecursion("3");

            List<Org> orgs2 = GetFatherList(orgs, "3").ToList();

        }

        private static List<Org> initializeOrg()
        {
            List<Org> orgs = new List<Org>();
            orgs.Add(new Org
            {
                Id = "1",
                Name = "1级组织",
                ParentId = "0",
                Permission = new List<string>() { "001", "002", "003" }
            });

            orgs.Add(new Org
            {
                Id = "2",
                Name = "2级组织",
                ParentId = "1",
                Permission = new List<string>()
            });

            orgs.Add(new Org
            {
                Id = "3",
                Name = "3级组织",
                ParentId = "2",
                Permission = new List<string>()
            });

            return orgs;
        }

        public static IEnumerable<Org> GetFatherList(IList<Org> list, string Id)
        {
            var query = list.Where(p => p.Id == Id).ToList();
            return query.ToList().Concat(query.ToList().SelectMany(t => GetFatherList(list, t.ParentId)));
        }
    }

    public class Org
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ParentId { get; set; }

        public List<string> Permission { get; set; }
    }
}
