using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BettencourtCamden.Lab6
{

    public class AIBehavior : MonoBehaviour
    {
        [SerializeField] private GameObject charToMove;
        [SerializeField] private GameObject playerChar; // agent can see player and react to it on a delay
        [SerializeField] private MovesHandler agentMoveHandler;
        [SerializeField] private InputManager playerInputManager;
        public MovesHandler.NextMoveCommand nextMove = MovesHandler.NextMoveCommand.Move;
        private int advance = 1; // moves forward when advance is 1, backward when advance is -1

        private enum AgentBehavior
        {
            MoveOnly, //only pursues the player
            Random, //takes random actions
            Smart, //chooses based on decision tree algorithm
            Weighted //adds a little randomness, "fairest" algorithm
        }
        [SerializeField] AgentBehavior agentBehavior;

        private void DetermineMove()
        {
            switch (agentBehavior)
            {
                case AgentBehavior.MoveOnly:
                    DetermineMoveOnly();
                    break;

                case AgentBehavior.Random:
                    DetermineMoveRandom();
                    break;

                case AgentBehavior.Smart:
                    DetermineMoveSmart();
                    break;

                case AgentBehavior.Weighted:
                    DetermineMoveWeighted();
                    break;
            }
        }
        private void DetermineMoveOnly()
        {
            nextMove = MovesHandler.NextMoveCommand.Move;
        }

        private void DetermineMoveRandom()
        {
            nextMove = (MovesHandler.NextMoveCommand)(Random.value * 4.99f);
        }

        private void DetermineMoveSmart()
        {
            // decision tree model:
            // if my position is within 1 unit of the player, check if they are planning on attacking or blocking
            if(Vector3.Distance(charToMove.transform.position, playerChar.transform.position) < 2f)
            {
                // if they are planning on attacking, set my state to blocking
                if(playerInputManager.nextMove == MovesHandler.NextMoveCommand.Push)
                {
                    nextMove = MovesHandler.NextMoveCommand.Shine;
                }
                // if they are planning to block, move away briefly
                else if(playerInputManager.nextMove == MovesHandler.NextMoveCommand.Shine)
                {
                    nextMove = MovesHandler.NextMoveCommand.Move;
                    advance = -1;
                }
                // if they are jumping or moving next, press the attack
                else
                {
                    nextMove = MovesHandler.NextMoveCommand.Push;
                    advance = 1;
                }
            }
            else
            {
                // base case: move towards player
                nextMove = MovesHandler.NextMoveCommand.Move;
                advance = 1;
            }
        }

        private void DetermineMoveWeighted()
        {
            float randSwitch = Random.value;
            // decision tree model:
            // if my position is within 1 unit of the player, check if they are planning on attacking or blocking
            if (Vector3.Distance(charToMove.transform.position, playerChar.transform.position) < 2f)
            {
                // if they are planning on attacking, set my state to blocking with a chance to retreat
                if (playerInputManager.nextMove == MovesHandler.NextMoveCommand.Push)
                {
                    nextMove = MovesHandler.NextMoveCommand.Shine;
                    if(randSwitch > .8)
                    {
                        nextMove = MovesHandler.NextMoveCommand.Move;
                        advance = -1;
                    }
                }
                // if they are planning to block, move away briefly or also block
                else if (playerInputManager.nextMove == MovesHandler.NextMoveCommand.Shine)
                {
                    nextMove = MovesHandler.NextMoveCommand.Move;
                    advance = -1;
                    if (randSwitch > .8)
                    {
                        nextMove = MovesHandler.NextMoveCommand.Shine;
                        advance = 1;
                    }
                }
                // if they are jumping or moving next, press the attack, with a chance to jump instead
                else
                {
                    nextMove = MovesHandler.NextMoveCommand.Push;
                    advance = 1;
                    if (randSwitch > .8)
                    {
                        nextMove = MovesHandler.NextMoveCommand.Jump;
                    }
                }
            }
            else
            {
                // base case: move towards player
                nextMove = MovesHandler.NextMoveCommand.Move;
                advance = 1;
                // don't tamper with this one since it happens so often
            }
        }

        private void FixedUpdate()
        {
            agentMoveHandler.DecrementLag();
            var moveVal = new Vector2(charToMove.transform.position.z - playerChar.transform.position.z, playerChar.transform.position.x - charToMove.transform.position.x);
            DetermineMove();
            bool moveSucceeded = agentMoveHandler.ProcessNextMove(nextMove, moveVal * advance);
            if (moveSucceeded) nextMove = MovesHandler.NextMoveCommand.Move;
        }
    }
}
