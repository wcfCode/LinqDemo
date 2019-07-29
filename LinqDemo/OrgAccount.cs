using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqDemo
{
    public class OrgAccount
    {

        private static List<OrganizationAccount> organizationAccounts = new List<OrganizationAccount>();
        private static List<Organization> orgs = new List<Organization>();
        private static List<Account> accounts = new List<Account>();

        public static void StastTest()
        {
            initialize();
            string parentId = "0";//找组织id为1的下面的人员
            string tenantId = "001";//找组织id为1的下面的人员

            List<(Organization, Account)> result = (from o in orgs join oa in organizationAccounts on o.Id equals oa.OrgId join a in accounts on oa.UserId equals a.Id where o.TenantId == tenantId select ( o, a )).ToList();

            #region 逻辑一 转化为树

            List<OrganizationDTO> organizationDTOs = new List<OrganizationDTO>();
            foreach (var item in result)
            {
                OrganizationDTO organizationDTO = organizationDTOs.Where(x => x.Id == item.Item1.Id).FirstOrDefault();
                if (organizationDTO == null)
                {
                    organizationDTO = new OrganizationDTO()
                    {
                        Id = item.Item1.Id,
                        Name = item.Item1.Name,
                        ParentId = item.Item1.ParentId,
                        TenantId = item.Item1.TenantId,
                        Accounts = new List<Account>()
                    };
                    organizationDTO.Accounts.Add(item.Item2);
                    organizationDTOs.Add(organizationDTO);

                }
                else
                {
                    organizationDTO.Accounts.Add(item.Item2);
                }
            }

            //----------------------查看这个结果-----------------------------
            organizationDTOs = organizationDTOs.ToTree(parentId);

            #endregion

            #region  逻辑二 返回该组织的用户+子组织的用户
            //先筛选出该组织和子组织
            List<(Organization, Account)> ps = SelectChild(result, parentId);
            //再取出用户

            //------------------------------查看这个结局-----------------------------------
            List<Account> resultaccounts = new List<Account>();
            foreach (var item in ps)
            {
                if (!resultaccounts.Any(x => x.Id == item.Item2.Id))
                {
                    resultaccounts.Add(item.Item2);
                }
            }
            #endregion
        }

        private static List<(Organization, Account)> SelectChild(List<(Organization, Account)> list,string pid)
        {
            List<(Organization, Account)> resute = new List<(Organization, Account)>();
            List<(Organization, Account)> items = list.FindAll(t => pid==t.Item1.ParentId);
            if (items.Count > 0)
            {
                foreach ((Organization, Account) item in items)
                {
                    resute.Add(item);
                    resute.AddRange(SelectChild(list,item.Item1.Id));
                }
            }

            return resute;
        }

        private static List<Organization> initialize()
        {

            orgs.Add(new Organization
            {
                Id = "1",
                Name = "1级组织",
                ParentId = "0",
                TenantId = "001"

            });

            orgs.Add(new Organization
            {
                Id = "2",
                Name = "2级组织",
                ParentId = "1",
                TenantId = "001"
            });

            orgs.Add(new Organization
            {
                Id = "3",
                Name = "3级组织",
                ParentId = "2",
                TenantId = "001"
            });

            accounts.Add(new Account() { Id = "21", Name = "张三" });

            accounts.Add(new Account() { Id = "22", Name = "李四" });

            accounts.Add(new Account() { Id = "23", Name = "王五" });

            organizationAccounts.Add(new OrganizationAccount() { OrgId = "1", UserId = "21" });
            organizationAccounts.Add(new OrganizationAccount() { OrgId = "2", UserId = "22" });
            organizationAccounts.Add(new OrganizationAccount() { OrgId = "2", UserId = "23" });
            organizationAccounts.Add(new OrganizationAccount() { OrgId = "3", UserId = "23" });

            return orgs;
        }

    }


    public class Organization
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ParentId { get; set; }

        public string TenantId { get; set; }
    }

    public class OrganizationAccount
    {
        public string OrgId { get; set; }

        public string UserId { get; set; }
    }

    public class Account
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    public class OrganizationDTO
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ParentId { get; set; }

        public string TenantId { get; set; }

        public List<OrganizationDTO> Child { get; set; }

        public List<Account> Accounts { get; set; }
    }

    public static class TreeModule
    {

        public static List<OrganizationDTO> ToTree(this List<OrganizationDTO> list, string parentIds)
        {

            List<OrganizationDTO> strJson = new List<OrganizationDTO>();
            List<string> listIds = parentIds.Split(',').ToList();
            List<OrganizationDTO> items = list.FindAll(t => listIds.Contains(t.ParentId));
            if (items.Count > 0)
            {
                foreach (OrganizationDTO item in items)
                {
                    item.Child = ToTree(list, item.Id);
                    strJson.Add(item);
                }
            }

            return strJson.Distinct().ToList();
        }
    }
}
