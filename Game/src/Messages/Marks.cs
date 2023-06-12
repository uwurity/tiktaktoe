using System;
using System.Collections.Generic;

namespace tiktaktoe.Messages;

[Serializable]
public class Marks : Dictionary<string, MarkMetadata>
{
}