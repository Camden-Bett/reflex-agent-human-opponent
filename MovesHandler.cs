using System.Collections;
using UnityEngine;

namespace BettencourtCamden.Lab6
{
    public class MovesHandler : MonoBehaviour
    {
        [SerializeField] private GameObject charToMove;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform shine;
        [SerializeField] private Material blockMat;
        [SerializeField] private float knockbackShine = 120f;
        [SerializeField] private float knockbackPunch = 400f;
        [SerializeField] private float speed = 5f;
        [SerializeField] private float jumpHeight = 1000f;
        [SerializeField] private Respawner respawner;

        public bool blocking = false;
        public bool attacking = false;

        public enum NextMoveCommand //buffers incoming move inputs until lag is depleted
        {
            Move,
            Jump,
            Push,
            Shine
        };
        [SerializeField] NextMoveCommand nextMove;

        private Rigidbody rb;
        
        [SerializeField] public float remainingLag = 0f; // lag is applied during a move and decremented continuously

        private void Start ()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void DecrementLag()
        {
            remainingLag -= Time.fixedDeltaTime;
            if (remainingLag <= 0f ) { remainingLag = 0f; }
        }

        // returns true if the move succeeded
        public bool ProcessNextMove(NextMoveCommand nextCommand, Vector2 moveVal)
        {
            SetRotation(moveVal);
            switch (nextCommand)
            {
                case NextMoveCommand.Move:
                    Move(moveVal);
                    return true;

                case NextMoveCommand.Jump:
                    return Jump();

                case NextMoveCommand.Push:
                    return Push();

                case NextMoveCommand.Shine:
                    return Shine();

                default:
                    return true;
            }
        }

        private void SetRotation(Vector2 moveVal)
        {
            var lookAnglePotential = Mathf.Atan2(moveVal.x, moveVal.y) * Mathf.Rad2Deg;
            var lookAngle = lookAnglePotential == 0f ? Vector3.SignedAngle(Vector3.right, charToMove.transform.right, Vector3.up) : lookAnglePotential;
            charToMove.transform.rotation = Quaternion.Euler(0f, lookAngle, 0f);
            //Quaternion.Rotate(charToMove.transform.rotation, Quaternion.Euler(new Vector3(0, Mathf.Atan2(moveVal.x, moveVal.y) * 180f / Mathf.PI, 0)), ROTATE_STRENGTH);
        }

        private void Move(Vector2 moveVal)
        {
            // if there is no lag and the input is nonzero
            if (remainingLag <= 0f && moveVal.magnitude > 0.1f)
            {
                // convert 2D movement vector to a 3D movement
                var moveVector3 = new Vector3(moveVal.y, 0, moveVal.x * -1);
                moveVector3.Normalize();
                rb.MovePosition(new Vector3(transform.position.x, 1.5f, transform.position.z) + Time.fixedDeltaTime * speed * moveVector3);
            }
        }

        private bool Jump()
        {
            if (remainingLag <= 0f)
            {
                // apply input lag, then jump
                //release rigidbody constraints for a moment??
                remainingLag += 1.1f;
                rb.AddForce(jumpHeight * rb.mass * Vector3.up, ForceMode.Impulse);
                return true;
            }
            return false; // jump failed; try again next update
        }

        private bool Push()
        {
            if (remainingLag <= 0f)
            {
                // apply input lag, then attack
                remainingLag += 0.5f;
                leftArm.gameObject.SetActive(true);
                rightArm.gameObject.SetActive(true);
                attacking = true;
                StartCoroutine(RetractArmsDelay());
                return true;
            }
            return false; // push failed; try again next update
        }

        IEnumerator RetractArmsDelay()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            leftArm.gameObject.SetActive(false);
            rightArm.gameObject.SetActive(false);
            attacking = false;
        }

        private bool Shine()
        {
            if (remainingLag <= 0f)
            {
                // apply input lag, then block
                remainingLag += 1f;
                shine.gameObject.SetActive(true);
                blocking = true;
                StartCoroutine(RetractShineDelay());
                return true;
            }
            return false; // shine failed; try again next update
        }

        IEnumerator RetractShineDelay()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            shine.gameObject.SetActive(false);
            blocking = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            // three collision types we need to check for:
            // another player's arms (medium force back, short stun)
            // if opponent is using Shine, sends self back twice as hard
            if (collision.gameObject.name.Contains("Arm"))
            {
                // I collided with arms; if I am blocking, do not apply knockback or stun
                // add stun and apply knockback if not blocking
                if (!blocking)
                {
                    remainingLag += 0.5f;
                    Vector3 direction = collision.gameObject.transform.up * -1 + Vector3.up;
                    rb.AddForce(knockbackPunch * Time.fixedDeltaTime * direction, ForceMode.Impulse);
                }

            }

            // another player's shine (stronger force back, longer stun, as long as opponent is attacking)
            if (collision.gameObject.name.Equals("Shine"))
            {
                // I collided with a shine; if I am attacking, I get blown back hard. if I am not, it is a small hit
                // add high stun and knockback to self if I am attacking, low otherwise
                Vector3 direction = gameObject.transform.position - collision.gameObject.transform.position + Vector3.up;
                if (attacking)
                {
                    remainingLag += 1f;
                    rb.AddForce(knockbackPunch * Time.fixedDeltaTime * 4 * direction, ForceMode.Impulse);
                } else
                {
                    remainingLag += 0.2f;
                    rb.AddForce(knockbackShine * Time.fixedDeltaTime * direction, ForceMode.Impulse);
                }

            }

            if (collision.gameObject.name.Contains("Beans"))
            {
                // I merely bumped the opponent; flat but small knockback
                remainingLag += 0.4f;
                Vector3 direction = collision.gameObject.transform.right + Vector3.up;
                rb.AddForce(knockbackShine * Time.fixedDeltaTime * direction, ForceMode.Impulse);

            }

            // edge of stage (lose state)
            if (collision.gameObject.name.Equals("Wall"))
            {
                if(respawner != null) respawner.Respawn();
            }
        }
    }
}
