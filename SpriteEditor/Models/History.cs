using System.Collections.Generic;

namespace SpriteEditor.Models
{
    public class History
    {
        private int Index { get; set; }
        private List<HistoryState> States { get; set; }

        public bool CanUndo { get { return Index > 0; } }
        public bool CanRedo { get { return Index < States.Count - 1; } }

        public HistoryState GetPreviousState()
        {
            if (!CanUndo)
                return default;
            Index--;
            System.Console.WriteLine(States[Index].StateName);
            return States[Index];

        }

        public HistoryState GetNextState()
        {
            if (!CanRedo)
                return default;
            Index++;
            System.Console.WriteLine(States[Index].StateName);
            return States[Index];
        }

        public void AddState(HistoryState state)
        {
            if(CanRedo)
            {
                var newStates = new List<HistoryState>(Index + 1);
                for(int i = 0; i < Index + 1; i++)
                {
                    newStates.Add(States[i]);
                }
                States = newStates;
            }
            System.Console.WriteLine(state.StateName);
            States.Add(state);
            Index = States.Count - 1;
        }

        public History()
        {
            States = new List<HistoryState>();
            Index = 0;
        }
    }
}
