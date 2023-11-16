using System.Collections.Generic;

namespace LibCrud.Models;

public record TableRowsData(int AllRowsCount, int Offset, List<dynamic> Rows);