using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEditorRename : IGameEvent
{

    string oldName, newName;

    public GameEventExecutionResult res;

    public GameEventExecutionResult Result { get { return res; } private set { res = value; } }

    GameEditor ed;

    GameEditor.GameEditorState State;

    public GameEditorRename(GameEditor ed, string newName)
    {

        this.ed = ed;

        State = ed.EditorState.State;

        this.newName = newName;

        oldName = ed.LevelData.LevelName;

        res = GameEventExecutionResult.Failed;

        Execute();

    }

    public void Execute()
    {

        if (string.IsNullOrEmpty(newName) || string.IsNullOrWhiteSpace(newName))
        {
            Scene.main.ShowMessageBox("Level name cannot be empty", "Editor message");
            res = GameEventExecutionResult.Failed;
            return;
        }

        ed.LevelData.LevelName = newName;
        ed.levelNameEditorField.text = newName;
        res = GameEventExecutionResult.Success;
    }

    public void Revert()
    {
        ed.LevelData.LevelName = oldName;
        ed.levelNameEditorField.text = oldName;

        if (ed.EditorState.State != State)
        {
            ed.EditorState.State = State;
        }
    }
}





public class GameEditorAddCellToGroup : IGameEvent
{

    int GroupIndex;

    int CellIndex;

    GameEventExecutionResult res;

    GameEditor ed;

    GameEditor.GameEditorState State;

    public GameEventExecutionResult Result { get { return res; } }

    public GameEditorAddCellToGroup(GameEditor ed, int groupIndex, int cellIndex)
    {
        res = GameEventExecutionResult.Failed;
        this.ed = ed;
        State = ed.EditorState.State;
        GroupIndex = groupIndex;
        CellIndex = cellIndex;

        Execute();
    }

    public void Execute()
    {
        if (GroupIndex >= 0 && !ed.CellGroupsLists[GroupIndex].Contains(CellIndex))
        {
            //Debug.Log("Add cell " + CellIndex + " to " + GroupIndex);
            ed.CellGroupsLists[GroupIndex].Add(CellIndex);
            //Debug.Log(ed.GroupStatesString());
            

            ed.EditorState.State = GameEditor.GameEditorState.ConfiguringCellGroups;
            ed.currentCellGroup = GroupIndex;


            //print(GroupIndex + " {" + string.Join(", ", ed.CellGroupsLists[GroupIndex]) + "}");

            ed.ReDrawCellGroupControls(false, GroupIndex, true);

            res = GameEventExecutionResult.Success;
        }
        else
        {
            res = GameEventExecutionResult.Failed;
        }
    }

    public void Revert()
    {
        //Debug.Log("Reverse Add cell " + CellIndex + " from " + GroupIndex);
        ed.CellGroupsLists[GroupIndex].Remove(CellIndex);
        //Debug.Log(ed.GroupStatesString());

        ed.EditorState.State = State;
        ed.currentCellGroup = GroupIndex;

        ed.ReDrawCellGroupControls(false, GroupIndex, true);

    }
}

public class GameEditorAddCellGroup : IGameEvent
{

    int GroupID;

    GameEditor.GameEditorState State;

    GameEditor ed;

    GameEventExecutionResult res = GameEventExecutionResult.Failed;

    public GameEditorAddCellGroup(GameEditor ed)
    {
        this.ed = ed;
        State = ed.EditorState.State;
        GroupID = ed.CellGroupsLists.Count;

        Execute();
    }

    public GameEventExecutionResult Result { get { return res; } }

    public void Execute()
    {
        //Debug.Log("Add cell group " + ed.CellGroupsLists.Count);
        ed.CellGroupsLists.Add(new List<int>());
        //Debug.Log(ed.GroupStatesString());

        ed.EditorState.State = GameEditor.GameEditorState.ConfiguringCellGroups;
        GroupID = ed.CellGroupsLists.Count - 1;
        ed.currentCellGroup = GroupID;

        ed.ReDrawCellGroupControls(true);
        res = GameEventExecutionResult.Success;
    }

    public void Revert()
    {
        //Debug.Log("Reverse Add cell group " + GroupID);
        ed.CellGroupsLists.RemoveAt(GroupID);
        //Debug.Log(ed.GroupStatesString());

        ed.EditorState.State = GameEditor.GameEditorState.Idle;

        ed.ReDrawCellGroupControls(true);
    }
}

public class GameEditorRemoveCellFromGroup : IGameEvent
{

    GameEventExecutionResult res = GameEventExecutionResult.Failed;

    int GroupIndex;

    int CellIndex;

    GameEditor.GameEditorState State;

    GameEditor ed;

    public GameEventExecutionResult Result { get { return res; } }

    public GameEditorRemoveCellFromGroup(GameEditor ed, int GroupID, int CellID)
    {
        res = GameEventExecutionResult.Failed;
        this.ed = ed;
        State = ed.EditorState.State;
        GroupIndex = GroupID;
        CellIndex = CellID;

        Execute();
    }

    public void Execute()
    {
        if (GroupIndex >= 0 && GroupIndex < ed.CellGroupsLists.Count)
        {
            if (ed.CellGroupsLists[GroupIndex].Contains(CellIndex))
            {
                //Debug.Log("Remove cell " + CellIndex + " from " + GroupIndex);
                ed.CellGroupsLists[GroupIndex].Remove(CellIndex);
                //Debug.Log(ed.GroupStatesString());

                ed.EditorState.State = GameEditor.GameEditorState.ConfiguringCellGroups;
                ed.currentCellGroup = GroupIndex;

                ed.ReDrawCellGroupControls(false, GroupIndex, true);

                res = GameEventExecutionResult.Success;

            }
        }
    }

    public void Revert()
    {
        //Debug.Log("Revert Remove cell " + CellIndex + " from " + GroupIndex);
        ed.CellGroupsLists[GroupIndex].Add(CellIndex);
        //Debug.Log(ed.GroupStatesString());

        ed.EditorState.State = GameEditor.GameEditorState.ConfiguringCellGroups;
        ed.currentCellGroup = GroupIndex;

        ed.ReDrawCellGroupControls(false, GroupIndex, true);
    }
}

public class GameEditorRemoveCellGroup : IGameEvent
{

    int GroupID;

    List<int> Group;

    GameEventExecutionResult res = GameEventExecutionResult.Failed;

    GameEditor.GameEditorState State;

    GameEditor ed;

    public GameEventExecutionResult Result { get { return res; } }

    public GameEditorRemoveCellGroup(GameEditor ed, int groupID)
    {

        this.ed = ed;
        GroupID = groupID;
        State = ed.EditorState.State;

        Execute();
    }

    public void Execute()
    {

        if (ed.CellGroupsLists.Count < GroupID || GroupID < 0)
            return;

        Group = ed.CellGroupsLists[GroupID];
        //Debug.Log("Remove cell group " + GroupID + " data: {" + string.Join(", ", Group) + "}");
        ed.CellGroupsLists.RemoveAt(GroupID);
        //Debug.Log(ed.GroupStatesString());

        ed.EditorState.State = GameEditor.GameEditorState.Idle;

        ed.ReDrawCellGroupControls(true);     

        res = GameEventExecutionResult.Success;

    }

    public void Revert()
    {
        //Debug.Log("Reverse Remove cell group " + GroupID + " data: {" + string.Join(", ", Group) + "}");
        ed.CellGroupsLists.Insert(GroupID, Group);
        //Debug.Log(ed.GroupStatesString());

        ed.EditorState.State = State;

        ed.ReDrawCellGroupControls(true, GroupID);

    }
}