using System.Collections.Generic;

namespace BackendCarcass.LibCrud.Models;

public record TableRowsData(int AllRowsCount, int Offset, List<dynamic> Rows);
