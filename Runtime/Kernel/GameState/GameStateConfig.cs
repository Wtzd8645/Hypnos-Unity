using System;
using UnityEngine;

namespace Hypnos.GameState
{
    public class GameStateConfig : ScriptableObject
    {
        [NonSerialized] public GameStateBase[] gameStates;
        public int startGameState;
    }
}