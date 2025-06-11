using Characters.CombatSystems;
using Characters.InputSystems;
using Characters.MovementSystems;
using Cysharp.Threading.Tasks;
using Dan.Main;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Controllers
{
    /// <summary>
    /// Main controller for the player character.
    /// Manages player-specific systems such as input, movement, and combat.
    /// Also implements singleton access for global player references.
    /// </summary>
    public class PlayerController : BaseController<PlayerInputSystem>
    {
        #region Inspectors & Variables
        
        [PropertyTooltip("Handles grid-based movement logic and interaction with the world.")]
        [SerializeField] private GridMovementSystem gridMovementSystem;

        [PropertyTooltip("Handles protection and invincible")]
        [SerializeField] private GuardSystem guardSystem;

        [PropertyTooltip("Handles score conter")]
        [SerializeField] private ScoreSystem scoreSystem;
        
        /// <summary>
        /// Gets the grid-based movement system used by the player.
        /// </summary>
        public GridMovementSystem GridMovementSystem => gridMovementSystem;
        
        /// <summary>
        /// Singleton reference to the player controller instance in the scene.
        /// </summary>
        public static PlayerController Instance { get; private set; }

        private static string playerName;

        private static int highestScore;

        public static string PlayerName => playerName;

        public static int HighestScore => highestScore;

        #endregion

        #region Unity Methods

        /// <summary>
        /// Initializes player systems and assigns singleton instance.
        /// Destroys duplicate instances to ensure only one player exists.
        /// </summary>
        protected override void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            gridMovementSystem?.Initialize(this);
            guardSystem?.Initialize(this);
            HealthSystem?.OnDead.AddListener(() =>
            {
                SetHighestScore(scoreSystem.Score);
            });
            base.Awake();
        }

        /// <summary>
        /// Subscribes player input events to their corresponding system actions.
        /// </summary>
        protected override void OnEnable()
        {
            if (!inputSystem) return;

            inputSystem.OnMoveInputPerform += GridMovementSystem.TryMoveAction;
            inputSystem.OnGuardInputPerform += guardSystem.Guard;
            gridMovementSystem.OnLandingAfterJump += scoreSystem.AddScore;
            base.OnEnable();
        }

        /// <summary>
        /// Unsubscribes input events when the object is disabled.
        /// </summary>
        protected override void OnDisable()
        {
            if (!inputSystem) return;
            
            inputSystem.OnMoveInputPerform -= GridMovementSystem.TryMoveAction;
            inputSystem.OnGuardInputPerform -= guardSystem.Guard;
            gridMovementSystem.OnLandingAfterJump -= scoreSystem.AddScore;
            base.OnDisable();
        }

        #endregion
        
        public static void LoadData(string name, int lastedHighScore)
        {
            if (name.Length <= 0) return;
            playerName = name;
            highestScore = lastedHighScore;
        }
        
        private void SetHighestScore(int score)
        {
            if (playerName.Length <= 0)
            {
                Debug.Log("empty name, leaderboard will not save");
                return;
            }

            if (highestScore >= score)
            {
                Debug.Log("score is less than high score, leaderboard will not save");
                return;
            }
            
            // upload to leader board
            highestScore = score;
            Leaderboards.ProjectHopper.UploadNewEntry(playerName, score);
        }
    } 
}
