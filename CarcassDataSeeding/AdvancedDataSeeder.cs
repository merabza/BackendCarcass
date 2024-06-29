using CarcassDb;
using CarcassMasterDataDom;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemToolsShared.Errors;

namespace CarcassDataSeeding;

public /*open*/ class AdvancedDataSeeder<TDst> : DataSeeder<TDst> where TDst : class, IDataType, IMyEquatable
{
    protected AdvancedDataSeeder(string dataSeedFolder, IDataSeederRepository repo) : base(dataSeedFolder, repo)
    {
    }

    public void CreateTempData()
    {
        var dataList = Repo.GetAll<TDst>();
        DataSeederTempData.Instance.SaveIntIdKeys<TDst>(dataList.ToDictionary(k => k.Key, v => v.Id));
    }

    protected override Option<Err[]> AdditionalCheck()
    {
        var (forAdd, forUpdate, forDelete) = CompareLists(Repo.GetAll<TDst>(), CreateMustList());

        if (!Repo.CreateEntities(forAdd))
            return new Err[]
            {
                new()
                {
                    ErrorCode = "CanNotCreateEntitiesInAdditionalCheck",
                    ErrorMessage = "Can Not Create Entities In Additional Check"
                }
            };

        if (!Repo.SetUpdates(forUpdate))
            return new Err[]
            {
                new()
                {
                    ErrorCode = "CanNotUpdateEntitiesInAdditionalCheck",
                    ErrorMessage = "Can Not Update Entities In Additional Check"
                }
            };

        if (!Repo.DeleteEntities(forDelete))
            return new Err[]
            {
                new()
                {
                    ErrorCode = "CanNotDeleteEntitiesInAdditionalCheck",
                    ErrorMessage = "Can Not Delete Entities In Additional Check"
                }
            };

        DataSeederTempData.Instance.SaveIntIdKeys<TDst>(Repo.GetAll<TDst>().ToDictionary(k => k.Key, v => v.Id));
        return null;
    }

    private static (List<TDst>, List<TDst>, List<TDst>) CompareLists(IReadOnlyCollection<TDst> existing,
        IReadOnlyCollection<TDst> mustBe)
    {
        if (mustBe == null)
            return (null, null, null);

        var duplicateExistingKeys = existing.GroupBy(x => x.Key)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key).ToList();

        if (duplicateExistingKeys.Count != 0)
        {
            var strKeyList = string.Join(", ", duplicateExistingKeys);
            Console.WriteLine("existing contains duplicate keys {0}", strKeyList);
            throw new Exception("existing contains duplicate keys");
        }


        var duplicateMustBeKeys = mustBe.GroupBy(x => x.Key)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key).ToList();

        if (duplicateMustBeKeys.Count != 0)
        {
            var strKeyList = string.Join(", ", duplicateMustBeKeys);
            Console.WriteLine("mustBe contains duplicate keys {0}", strKeyList);
            throw new Exception("mustBe contains duplicate keys");
        }

        var forAdd = new List<TDst>();
        var forUpdate = new List<TDst>();
        var forDelete = new List<TDst>();
        var allKeys = existing.Select(s => s.Key).Union(mustBe.Select(s => s.Key)).Distinct().ToList();
        var couples = allKeys.Select(s => new Tuple<TDst, TDst>(existing.SingleOrDefault(sod => sod.Key == s),
            mustBe.SingleOrDefault(sod => sod.Key == s))).ToList();
        foreach (var couple in couples)
            if (couple.Item2 is not null && couple.Item1 is null)
            {
                forAdd.Add(couple.Item2);
            }
            else if (couple.Item1 is not null && couple.Item2 is null)
            {
                forDelete.Add(couple.Item1);
            }
            else if (couple.Item1 is not null && couple.Item2 is not null && !couple.Item1.EqualsTo(couple.Item2))
            {
                couple.Item1.UpdateTo(couple.Item2);
                forUpdate.Add(couple.Item1);
            }

        return (forAdd, forUpdate, forDelete);
    }
}