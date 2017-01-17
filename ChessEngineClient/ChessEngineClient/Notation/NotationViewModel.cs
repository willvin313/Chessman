﻿using ChessEngine;
using Framework.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Data;

namespace ChessEngineClient.ViewModel
{
    public class NotationViewModel : ViewModelBase
    {
        private IBoardService analysisBoardService = null;
        private ObservableCollection<MoveDataGroup> groupedMoves = new ObservableCollection<MoveDataGroup>();
        private MoveData currentMove = null;

        #region Properties

        public ObservableCollection<MoveDataGroup> GroupedMoves
        {
            get { return groupedMoves; }
            set
            {
                if (value != groupedMoves)
                {
                    groupedMoves = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public MoveData CurrentMove
        {
            get { return currentMove; }
            set
            {
                if (currentMove != value)
                {
                    currentMove = value;
                    if (currentMove != null)
                    {
                        analysisBoardService.GoToMove(currentMove.Index);
                        NotifyPropertyChanged();
                    }
                    Messenger.Default.Send(new GenericMessage<MoveData>(currentMove), NotificationMessages.CurrentMoveChanged);
                }
            }
        }

        #endregion

        public NotationViewModel(IBoardService analysisBoardService)
        {
            this.analysisBoardService = analysisBoardService;
            Messenger.Default.Register<MessageBase>(this, NotificationMessages.MoveExecuted, OnMoveExecutedMessage);
        }

        private void OnMoveExecutedMessage(MessageBase message)
        {
            // check if message is intended for current board service
            if (message.Target == analysisBoardService)
                LoadExecutedMove();
        }

        private void LoadExecutedMove()
        {
            var currentMoveData = analysisBoardService.GetCurrentMove();
            int groupIndex = currentMoveData.Index / 2;
            bool isWhiteMove = currentMoveData.Index % 2 == 0;

            if (GroupedMoves.Count == groupIndex)
            {
                GroupedMoves.Add(new MoveDataGroup(groupIndex + 1) { WhiteMove = currentMoveData });
            }
            else
            {
                bool isSameAsDisplayedMove = isWhiteMove ? 
                    GroupedMoves[groupIndex].WhiteMove.PgnMove == currentMoveData.PgnMove :
                    GroupedMoves[groupIndex].BlackMove?.PgnMove == currentMoveData.PgnMove;

                if (!isSameAsDisplayedMove)
                {
                    //remove the entire group since ethe group moves collection is not observable
                    var overridingGroup = GroupedMoves[groupIndex];

                    //white move can never be null in a group
                    if (isWhiteMove)
                    {
                        overridingGroup.WhiteMove = currentMoveData;
                        overridingGroup.BlackMove = null;
                    }
                    else
                    {
                        overridingGroup.BlackMove = currentMoveData;
                    }

                    GroupedMoves[groupIndex] = overridingGroup;

                    // remove all other moves
                    while (groupIndex + 1 < GroupedMoves.Count)
                        GroupedMoves.RemoveAt(GroupedMoves.Count - 1);
                }
            }

            CurrentMove = isWhiteMove ? GroupedMoves[groupIndex].WhiteMove : 
                GroupedMoves[groupIndex].BlackMove;
        }

        public void ReloadMoves()
        {
            int groupIndex = 1;
            List<MoveDataGroup> newGroupedMoves = new List<MoveDataGroup>();
            var moves = analysisBoardService.GetMoves(false);

            bool startedAsBlack = analysisBoardService.WasBlackFirstToMove();

            using (var movesEnumerator = moves.GetEnumerator())
            {
                while (movesEnumerator.MoveNext())
                {
                    MoveDataGroup moveGroup = new MoveDataGroup(groupIndex);
                    if (startedAsBlack && groupIndex == 1)
                        moveGroup.WhiteMove = MoveData.CreateEmptyMove();
                    else
                        moveGroup.WhiteMove = movesEnumerator.Current;

                    if (moveGroup.WhiteMove.Index == -1 || movesEnumerator.MoveNext())
                        moveGroup.BlackMove = movesEnumerator.Current;

                    newGroupedMoves.Add(moveGroup);
                    groupIndex++;
                }
            }

            GroupedMoves.Clear();
            newGroupedMoves.ForEach(GroupedMoves.Add);

            MoveData currentMove = moves.FirstOrDefault(m => m.IsCurrent);
            if (currentMove != null)
                CurrentMove = currentMove;
            // select the first ... item by default
            else if (startedAsBlack && GroupedMoves.Count > 0)
                CurrentMove = GroupedMoves[0].WhiteMove;
        }
    }
}
