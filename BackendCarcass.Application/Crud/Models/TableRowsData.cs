using System.Collections.Generic;

namespace BackendCarcass.Application.Crud.Models;

public record TableRowsData(int AllRowsCount, int Offset, List<dynamic> Rows);
