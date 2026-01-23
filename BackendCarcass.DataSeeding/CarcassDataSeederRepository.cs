using System;
using System.Collections.Generic;
using System.Linq;
using BackendCarcass.Database;
using BackendCarcass.Database.Models;
using Microsoft.Extensions.Logging;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.DataSeeding;

public /*open*/ class CarcassDataSeederRepository : ICarcassDataSeederRepository
{
    private readonly CarcassDbContext _context;
    private readonly ILogger<CarcassDataSeederRepository> _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CarcassDataSeederRepository(CarcassDbContext ctx, ILogger<CarcassDataSeederRepository> logger)
    {
        _context = ctx;
        _logger = logger;
    }

    public List<ManyToManyJoin> GetManyToManyJoins(int parentDataTypeId, int childDataTypeId)
    {
        return [.. _context.ManyToManyJoins.Where(w => w.PtId == parentDataTypeId && w.CtId == childDataTypeId)];
    }

    public bool SetDtParentDataTypes(Tuple<int, int>[] dtdt)
    {
        try
        {
            foreach (var tdt in dtdt)
            {
                var dt = _context.DataTypes.SingleOrDefault(s => s.DtId == tdt.Item1);
                dt?.DtParentDataTypeId = tdt.Item2;
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
                dt?.DtManyToManyJoinParentDataTypeId = tdt.Item2;
                dt?.DtManyToManyJoinChildDataTypeId = tdt.Item3;
            }

            return SaveChanges();
        }
        catch (Exception e)
        {
            StShared.WriteException(e, "Error when SetDtParentDataTypes", true, _logger, false);
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

    private bool SaveChanges()
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
}
