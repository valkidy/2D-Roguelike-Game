using System.Collections;
using UnityEngine;

public class Enemy : CellObject
{
    public int Health = 3;
    public float MoveSpeed = 5.0f; // Speed at which the enemy moves
    private int m_CurrentHealth;
    private Animator m_Animator;
    private bool m_IsMoving = false;
    private Vector3 m_MoveTarget; // Target position for smooth movement

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        GameManager.Instance.TurnManager.OnTick += TurnHappened;
    }

    private void OnDestroy()
    {
        GameManager.Instance.TurnManager.OnTick -= TurnHappened;
    }

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        m_CurrentHealth = Health;
    }

    public override bool PlayerWantsToEnter()
    {
        m_CurrentHealth -= 1;

        if (m_CurrentHealth <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            // Play damage animation when hit
            m_Animator.SetTrigger("Damage");
        }

        return false;
    }

    private bool MoveTo(Vector2Int coord)
    {
        var board = GameManager.Instance.BoardManager;
        var targetCell = board.GetCellData(coord);

        if (targetCell == null || !targetCell.Passable || targetCell.ContainedObject != null)
        {
            return false;
        }

        // Remove enemy from current cell
        var currentCell = board.GetCellData(m_Cell);
        currentCell.ContainedObject = null;

        // Add it to the next cell
        targetCell.ContainedObject = this;
        m_Cell = coord;
        m_MoveTarget = board.CellToWorld(coord); // Set the target position
        m_IsMoving = true; // Start moving
        m_Animator.SetBool("Moving", m_IsMoving); // Update animator

        return true;
    }

    private void Update()
    {
        if (m_IsMoving)
        {
            // Smoothly move towards the target position
            transform.position = Vector3.MoveTowards(transform.position, m_MoveTarget, MoveSpeed * Time.deltaTime);
            
            if (Vector3.Distance(transform.position, m_MoveTarget) < 0.01f)
            {
                transform.position = m_MoveTarget; // Snap to target position
                m_IsMoving = false; // Stop moving
                m_Animator.SetBool("Moving", m_IsMoving); // Update animator
            }
        }
    }

    private void TurnHappened()
    {
        if (m_IsMoving) return; // If the enemy is moving, wait for the next turn

        var playerCell = GameManager.Instance.PlayerController.Cell;
        int xDist = playerCell.x - m_Cell.x;
        int yDist = playerCell.y - m_Cell.y;

        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);

        if ((xDist == 0 && absYDist == 1) || (yDist == 0 && absXDist == 1))
        {
            // We are adjacent to the player, attack!
            m_Animator.SetTrigger("Attack");
            GameManager.Instance.ChangeFood(3);

            // Optionally, play a damage animation on the player
            GameManager.Instance.PlayerController.Damage();
        }
        else
        {
            if (absXDist > absYDist)
            {
                if (!TryMoveInX(xDist))
                {
                    TryMoveInY(yDist);
                }
            }
            else
            {
                if (!TryMoveInY(yDist))
                {
                    TryMoveInX(xDist);
                }
            }
        }
    }

    private bool TryMoveInX(int xDist)
    {
        if (xDist > 0) // Player is to the right
        {
            return MoveTo(m_Cell + Vector2Int.right);
        }
        return MoveTo(m_Cell + Vector2Int.left); // Player is to the left
    }

    private bool TryMoveInY(int yDist)
    {
        if (yDist > 0) // Player is above
        {
            return MoveTo(m_Cell + Vector2Int.up);
        }
        return MoveTo(m_Cell + Vector2Int.down); // Player is below
    }
}
