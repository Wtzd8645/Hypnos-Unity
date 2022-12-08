using System;
using UnityEngine;

namespace Morpheus.GameState
{
    public class GameStateConfig : ScriptableObject
    {
        [NonSerialized] public GameStateBase[] gameStates;
        public int startGameState;
    }
}