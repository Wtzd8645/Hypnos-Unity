using System;
using UnityEngine;

namespace Blanketmen.Hypnos
{
    public class GameStateConfig : ScriptableObject
    {
        [NonSerialized] public GameStateBase[] gameStates;
        public int startGameState;
    }
}