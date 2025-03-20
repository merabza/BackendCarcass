using System;
using System.Collections.Generic;
using System.Linq;
using CarcassDb;
using CarcassDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace CarcassDataSeeding;

public /*open*/ class DataSeederRepository : IDataSeederRepository
{
    private readonly CarcassDbContext _context;
    private readonly ILogger<DataSeederRepository> _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
    protected DataSeederRepository(CarcassDbContext ctx, ILogger<DataSeederRepository> logger)
    {
        _context = ctx;
        _logger = logger;
    }

    public List<T> GetAll<T>() where T : class
    {
        //IMdLoaderCreator creator = MasterDataRepoManager.Instance.GetLoaderCreator(typeof(T));
        //ICustomMdLoader loader = creator?.Create(_context);

        //if (loader != null && typeof(T) is IDataType)
        //  return loader.GetEntity().Cast<T>();

        return [.. _context.Set<T>()];
    }

    public List<ManyToManyJoin> GetManyToManyJoins(int parentDataTypeId, int childDataTypeId)
    {
        return [.. _context.ManyToManyJoins.Where(w => w.PtId == parentDataTypeId && w.CtId == childDataTypeId)];
    }

    public bool HaveAnyRecord<T>() where T : class
    {
        return _context.Set<T>().Any();
    }

    public string GetTableName<T>()
    {
        var entType = _context.Model.GetEntityTypes().SingleOrDefault(s => s.ClrType == typeof(T));
        return entType?.GetTableName();
    }

    public bool CreateEntities<T>(List<T> entities)
    {
        if (entities == null || entities.Count == 0)
            return true;

        try
        {
            foreach (var entity in entities)
                _context.Add(entity);
            return SaveChanges();
        }
        catch (Exception e)
        {
            StShared.WriteException(e, $"Error when creating CreateEntities type: {typeof(T)}", true, _logger, false);
            return false;
        }
    }

    public bool DeleteEntities<T>(List<T> entities)
    {
        if (entities == null || entities.Count == 0)
            return true;

        try
        {
            foreach (var entity in entities)
                _context.Remove(entity);
            return SaveChanges();
        }
        catch (Exception e)
        {
            StShared.WriteException(e, $"Error when creating CreateEntities type: {typeof(T)}", true, _logger, false);
            return false;
        }
    }

    public bool SaveChanges()
    {
        try
        {
            _context.SaveChanges();
            return true;
        }
        catch (Exception e)
        {
            StShared.WriteException(e, "Error when saving changes", true, _logger, false);
            return false;
        }
    }

    public bool SetDtParentDataTypes(Tuple<int, int>[] dtdt)
    {
        try
        {
            foreach (var tdt in dtdt)
            {
                var dt = _context.DataTypes.SingleOrDefault(s => s.DtId == tdt.Item1);
                if (dt != null)
                    dt.DtParentDataTypeId = tdt.Item2;
            }

            return SaveChanges();
        }
        catch (Exception e)
        {
            StShared.WriteException(e, "Error when SetDtParentDataTypes", true, _logger, false);
            return false;
        }
    }

    public bool SetManyToManyJoinParentChildDataTypes(Tuple<int, int, int>[] dtdtdt)
    {
        try
        {
            foreach (var tdt in dtdtdt)
            {
                var dt = _context.DataTypes.SingleOrDefault(s => s.DtId == tdt.Item1);
                if (dt == null)
                    continue;

                dt.DtManyToManyJoinParentDataTypeId = tdt.Item2;
                dt.DtManyToManyJoinChildDataTypeId = tdt.Item3;
            }

            return SaveChanges();
        }
        catch (Exception e)
        {
            StShared.WriteException(e, "Error when SetDtParentDataTypes", true, _logger, false);
            return false;
        }
    }

    public bool SetUpdates<T>(List<T> forUpdate)
    {
        if (forUpdate == null || forUpdate.Count == 0)
            return true;
        try
        {
            foreach (var rec in forUpdate)
                _context.Update(rec);
            return SaveChanges();
        }
        catch (Exception e)
        {
            StShared.WriteException(e, $"Error when SetUpdates type: {typeof(T)}", true, _logger, false);
            return false;
        }
    }

    public bool RemoveRedundantDataTypesByTableNames(string[] toRemoveTableNames)
    {
        try
        {
            _context.DataTypes.RemoveRange(_context.DataTypes.Where(w => toRemoveTableNames.Contains(w.DtTable)));
            return SaveChanges();
        }
        catch (Exception e)
        {
            StShared.WriteException(e, "Error when RemoveRedundantDataTypesByTableNames", true, _logger, false);
            return false;
        }
    }

    public bool RemoveNeedlessRecords<TDst>(List<TDst> needLessList) where TDst : class
    {
        if (needLessList == null || needLessList.Count == 0)
            return true;

        try
        {
            _context.RemoveRange(needLessList);
            return SaveChanges();
        }
        catch (Exception e)
        {
            StShared.WriteException(e, $"Error when RemoveNeedlessRecords type: {typeof(TDst)}", true, _logger, false);
            return false;
        }
    }
}