using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UndoRedoSystem
{
    private static UndoRedoSystem Instance;
    private List<Pair<Action, Action>> actions = new List<Pair<Action, Action>>();
    private int index = 0;

    public UndoRedoSystem GetInstance()
    {
        if( Instance == null )
            Instance = new UndoRedoSystem();
        return Instance;
    }

    public void ExecuteAction( Action action, Action undo )
    {
        var newAction = new Pair<Action, Action>{ First = action, Second = undo };
        actions.Add( newAction );
        ++index;
        action();
    }

    public bool UndoAction()
    {
        if( actions.IsEmpty() )
            return false;

        if( index == 0 )
            return false;

        --index;
        actions[index].Second();
        return true;
    }

    public bool RedoAction()
    {
        if( actions.IsEmpty() )
            return false;

        if( index == actions.Count )
            return false;

        actions[index].First();
        ++index;
        actions.Resize( index );
        return true;
    }
}