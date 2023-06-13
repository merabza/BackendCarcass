using System;
using System.Collections.Generic;
using System.Linq;
using CarcassDb;
using CarcassMasterDataDom;

namespace CarcassDataSeeding;

public /*open*/ class AdvancedDataSeeder<TDst> : DataSeeder<TDst> where TDst : class, IDataType, IMyEquatable
{
    protected AdvancedDataSeeder(string dataSeedFolder, IDataSeederRepository repo) : base(dataSeedFolder, repo)
    {
    }

    public void CreateTempData()
    {
        List<TDst> dataList = Repo.GetAll<TDst>();
        DataSeederTempData.Instance.SaveIntIdKeys<TDst>(dataList.ToDictionary(k => k.Key, v => v.Id));
    }

    protected override bool AdditionalCheck()
    {
        (List<TDst> forAdd, List<TDst> forUpdate) = CompareLists(Repo.GetAll<TDst>(), CreateMustList());


        if (!Repo.CreateEntities(forAdd) || !Repo.SetUpdates(forUpdate))
            return false;
        DataSeederTempData.Instance.SaveIntIdKeys<TDst>(Repo.GetAll<TDst>().ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    private (List<TDst>, List<TDst>) CompareLists(List<TDst> existing, List<TDst> mustBe)
    {
        if (mustBe == null)
            return (null, null);

        List<TDst> forAdd = new List<TDst>();
        List<TDst> forUpdate = new List<TDst>();
        List<string> allKeys = existing.Select(s => s.Key).Union(mustBe.Select(s => s.Key))
            .Distinct().ToList();
        List<Tuple<TDst, TDst>> couples = allKeys.Select(s =>
            new Tuple<TDst, TDst>(existing.SingleOrDefault(sod => sod.Key == s),
                mustBe.SingleOrDefault(sod => sod.Key == s))).ToList();
        foreach (Tuple<TDst, TDst> couple in couples)
        {
            if (couple.Item1 == null)
            {
                forAdd.Add(couple.Item2);
            }
            else if (couple.Item2 != null && !couple.Item1.EqualsTo(couple.Item2))
            {
                couple.Item1.UpdateTo(couple.Item2);
                forUpdate.Add(couple.Item1);
            }
        }

        return (forAdd, forUpdate);
    }
}