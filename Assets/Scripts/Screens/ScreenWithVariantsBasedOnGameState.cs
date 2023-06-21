using System;
using System.Collections.Generic;
using IntentCheckers;
using UnityEngine;

namespace Screens
{
    /// <summary>
    /// Screen with variants (of enabled View and IntentChecker-s)
    /// todo NOTE: this implementation was chosen for declarative enable/disable logic
    /// todo NOTE: (state-machine-like-'transitions', but not exactly)
    /// todo NOTE: (alternative implementation: multiple Screens, with/without inheritance, with similar structure and logic)
    /// --
    /// todo NOTE: Normal-GameState has no special View but has Pause-IntentChecker
    /// </summary>
    public class ScreenWithVariantsBasedOnGameState : MonoBehaviour
    {
        public GameObject ViewForGamePaused;
        public GameObject ViewForGameLoosed;
        public GameObject ViewForLevelWin;

        private IntentCheckerThroughKeyCode _pauseIntentChecker;
        private IntentCheckerThroughKeyCode _unpauseIntentChecker;
        private IntentCheckerThroughKeyCode _quitIntentChecker;
        private IntentCheckerThroughKeyCode _restartIntentChecker;
        private IntentCheckerThroughKeyCode _proceedToNextLevelIntentChecker;

        private readonly List<IntentCheckerThroughKeyCode> _allIntentCheckers = new List<IntentCheckerThroughKeyCode>();

        public void Init(Action onWantPause, Action onWantUnpause, Action onWantQuit,
            Action onWantRestart, Action onWantProceedToNextLevel)
        {
            _pauseIntentChecker = new IntentCheckerThroughKeyCode(onWantPause, KeyCode.Escape);
            _unpauseIntentChecker = new IntentCheckerThroughKeyCode(onWantUnpause, KeyCode.Escape);
            _quitIntentChecker = new IntentCheckerThroughKeyCode(onWantQuit, KeyCode.Q);
            _restartIntentChecker = new IntentCheckerThroughKeyCode(onWantRestart, KeyCode.R);
            _proceedToNextLevelIntentChecker = new IntentCheckerThroughKeyCode(onWantProceedToNextLevel, KeyCode.E);

            _allIntentCheckers.Add(_pauseIntentChecker);
            _allIntentCheckers.Add(_unpauseIntentChecker);
            _allIntentCheckers.Add(_quitIntentChecker);
            _allIntentCheckers.Add(_restartIntentChecker);
            _allIntentCheckers.Add(_proceedToNextLevelIntentChecker);
        }

        void Update()
        {
            foreach (var intentChecker in _allIntentCheckers)
            {
                bool isEventDetected = intentChecker.Update();

                // todo NOTE: we break loop if any event is detected (otherwise there will be bug
                // todo NOTE: with multiple events in same frame due to the same KeyCode
                // todo NOTE: (Pause-Unpause with the same KeyCode.Escape currently))
                if (isEventDetected) break;
            }
        }

        public void OnChangedGameState(GameState gameState)
        {
            RecalculateVisibilityOfViewsBasedOnGameState(gameState);
            RecalculateIntentsEnabledBasedOnGameState(gameState);
        }

        private void RecalculateVisibilityOfViewsBasedOnGameState(GameState gameState)
        {
            ViewForGamePaused.SetActive(gameState == GameState.Paused);
            ViewForGameLoosed.SetActive(gameState == GameState.Loosed);
            ViewForLevelWin.SetActive(gameState == GameState.LevelWin);
        }

        private void RecalculateIntentsEnabledBasedOnGameState(GameState gameState)
        {
            _pauseIntentChecker.IsEnabled = (gameState == GameState.Normal);
            _unpauseIntentChecker.IsEnabled = (gameState == GameState.Paused);
            _quitIntentChecker.IsEnabled = (gameState == GameState.Paused || gameState == GameState.Loosed || gameState == GameState.LevelWin);
            _restartIntentChecker.IsEnabled = (gameState == GameState.Paused || gameState == GameState.Loosed);
            _proceedToNextLevelIntentChecker.IsEnabled = (gameState == GameState.LevelWin);
        }
    }
}