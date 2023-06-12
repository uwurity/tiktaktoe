using System;
using System.Collections.Generic;

namespace tiktaktoe.Messages;

[Serializable]
public class Board : List<List<Mark?>>
{
}