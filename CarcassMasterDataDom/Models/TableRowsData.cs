using System.Collections.Generic;

namespace CarcassMasterDataDom.Models;

public record TableRowsData(int AllRowsCount, int Offset, List<dynamic> Rows);