using UnityEngine;
using UnityEngine.InputSystem;

namespace BettencourtCamden.Lab6
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private MovesHandler playerMoveHandler;
        [SerializeField] private Respawner respawner;
        private PlayerInput inputScheme;
        InputAction jump;
        InputAction shine;
        InputAction headbutt;
        InputAction move;
        public MovesHandler.NextMoveCommand nextMove = MovesHandler.NextMoveCommand.Move;

        private void Awake()
        {
            inputScheme = new PlayerInput();
            _ = new ResetHandler(inputScheme.Player.Reset, respawner);
            _ = new QuitHandler(inputScheme.Player.Quit);

        }

        private void Start()
        {
            jump = inputScheme.Player.Jump;
            jump.performed += Jump_performed;
            jump.Enable();
            headbutt = inputScheme.Player.Headbutt;
            headbutt.performed += Headbutt_performed;
            headbutt.Enable();
            shine = inputScheme.Player.Shine;
            shine.performed += Shine_performed;
            shine.Enable();
            move = inputScheme.Player.Move;
            move.Enable();
        }

        private void Jump_performed(InputAction.CallbackContext obj)
        {
            if (playerMoveHandler.remainingLag <= 0f) nextMove = MovesHandler.NextMoveCommand.Jump;
        }

        private void Headbutt_performed(InputAction.CallbackContext obj)
        {
            if (playerMoveHandler.remainingLag <= 0f) nextMove = MovesHandler.NextMoveCommand.Push;
        }

        private void Shine_performed(InputAction.CallbackContext obj)
        {
            if (playerMoveHandler.remainingLag <= 0f) nextMove = MovesHandler.NextMoveCommand.Shine;
        }


        private void FixedUpdate()
        {
            playerMoveHandler.DecrementLag();
            var moveVal = move.ReadValue<Vector2>();
            bool moveSucceeded = playerMoveHandler.ProcessNextMove(nextMove, moveVal);
            if (moveSucceeded) nextMove = MovesHandler.NextMoveCommand.Move;
        }
    }
}
