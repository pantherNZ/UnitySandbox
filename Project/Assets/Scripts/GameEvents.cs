using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ToolBoundEventArgs : EventArgs
{
    public PlayerController player;
    public IBasePlayerTool newTool;
    public Int32 toolIndex;
}

public class ToolSelectedEventArgs : EventArgs
{
    public PlayerController player;
    public Int32 oldToolIndex;
    public Int32 newToolIndex;
}