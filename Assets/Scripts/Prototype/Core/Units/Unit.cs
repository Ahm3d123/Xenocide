using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MaterialSetter))]
[RequireComponent(typeof(IObjectTweener))]
public abstract class Unit : MonoBehaviour
{
    // Dependencies
    private MaterialSetter materialSetter;
    private IObjectTweener tweener;

    [Header("Properties (read-only)")]
    // Only for display in Unity Editor. DO NOT USE.
    [SerializeField] private Vector2Int SF_unitPosition;
    [SerializeField] private PlayerTeam SF_unitTeam;
    [SerializeField] private int SF_availableMoveCount = 0;
    [SerializeField] private int SF_availableAttackCount = 0;
    

    [Header("Stats")]
    // To edit the stats, edit the prefab of the unit
    // These are default values
    [SerializeField] private int id = 0;
    [SerializeField] public int HP = 1;
    [SerializeField] public int maxHP = 1;
    [SerializeField] public int ATK = 1;
    [SerializeField] public int DEF = 1;

    // Important properties
    public Field Field { get; set; }
    public Vector2Int OccupiedSquare { get; set; }
    public PlayerTeam Team { get; set; }
    public bool HasMoved { get; private set; }

    // Available move set
    public HashSet<Vector2Int> availableMoves;
    // Available attack set
    public HashSet<Vector2Int> availableAttacks;

    // Generate available move set
    public abstract HashSet<Vector2Int> GenerateAvailableMoves();
    // Generate available attack set
    public abstract HashSet<Vector2Int> GenerateAvailableAttacks();

    #region ID system
    private static int nextID = 1;
    protected int InitializeID()
    {
        if (id > 0)
            return id;
        else
            return (id = nextID++);
    }
    #endregion

    #region Initialize and Update
    private void Awake()
    {
        // Set dependencies
        materialSetter = GetComponent<MaterialSetter>();
        tweener = GetComponent<IObjectTweener>();

        // Initialize properties
        HasMoved = false;
        availableMoves = new HashSet<Vector2Int>();
        availableAttacks = new HashSet<Vector2Int>();

        // Initialize stats
        HP = maxHP;
    }

    private void Update()
    {
        // Update display values to internal values
        // Only for debugging
        SF_unitPosition = OccupiedSquare;
        SF_unitTeam = Team;
        SF_availableMoveCount = availableMoves.Count;
        SF_availableAttackCount = availableAttacks.Count;
    }

    // Set data after instantitation
    public void SetData(Vector2Int coords, PlayerTeam team, Field field)
    {
        // ID system
        InitializeID();

        // Important properties
        this.OccupiedSquare = coords;
        this.Team = team;
        this.Field = field;

        // GameObject position
        transform.position = field.CalculatePositionFromCoords(coords);
    }
    #endregion

    #region Logic
    public bool IsFromSameTeam(Unit unit)
    {
        return Team == unit.Team;
    }

    public bool CanMoveTo(Vector2Int coords)
    {
        return availableMoves.Contains(coords);
    }
    #endregion

    #region Movement
    public virtual void MoveUnit(Vector2Int coords)
    {
        Vector3 targetPosition = Field.CalculatePositionFromCoords(coords);
        OccupiedSquare = coords;
        HasMoved = true;

        tweener.MoveTo(transform, targetPosition);
    }

    protected void ClearMoves()
    {
        availableMoves.Clear();
    }

    protected void AddMove(Vector2Int coords)
    {
        availableMoves.Add(coords);
    }

    protected void AddMoves(ICollection<Vector2Int> coordsCollection)
    {
        availableMoves.UnionWith(coordsCollection);
    }

    protected void RemoveMove(Vector2Int coords)
    {
        availableMoves.Remove(coords);
    }

    protected void RemoveMoves(ICollection<Vector2Int> coordsCollection)
    {
        availableMoves.ExceptWith(coordsCollection);
    }

    #endregion

    #region Attack
    public virtual void AttackAt(Vector2Int coords)
    {
        throw new NotImplementedException();
    }

    protected void ClearAttacks()
    {
        availableAttacks.Clear();
    }

    protected void AddAttack(Vector2Int coords)
    {
        availableAttacks.Add(coords);
    }

    protected void AddAttacks(ICollection<Vector2Int> coordsCollection)
    {
        availableAttacks.UnionWith(coordsCollection);
    }

    protected void RemoveAttack(Vector2Int coords)
    {
        availableAttacks.Remove(coords);
    }

    protected void RemoveAttacks(ICollection<Vector2Int> coordsCollection)
    {
        availableAttacks.ExceptWith(coordsCollection);
    }
    #endregion

    #region Utility
    public void SetMaterial(Material material)
    {
        if (materialSetter == null)
            materialSetter = GetComponent<MaterialSetter>();
        materialSetter.SetSingleMaterial(material);
    }
    #endregion

    // TODO: Remove in future commit after method reference = 0
    #region Legacy
    public abstract HashSet<Vector2Int> SelectAvailableSquares();
    public bool IsAttackingUnitOfType<T>() where T : Unit
    {
        foreach (var square in availableMoves)
        {
            if (Field.GetUnitOnSquare(square) is T)
                return true;
        }
        return false;
    }
    protected Unit GetUnitInDirection<T>(PlayerTeam team, Vector2Int direction) where T : Unit
    {
        for (int i = 1; i <= Field.FIELD_SIZE; i++)
        {
            Vector2Int nextCoords = OccupiedSquare + direction * i;
            Unit unit = Field.GetUnitOnSquare(nextCoords);

            if (!Field.CheckIfCoordsAreOnField(nextCoords))
                return null;
            if (unit != null)
            {
                if (unit.Team != team || !(unit is T))
                    return null;
                else if (unit.Team == team && unit is T)
                    return unit;
            }
        }
        return null;
    }
    public void LineMovement(Vector2Int[] directions, float range)
    {
        foreach (var direction in directions)
        {
            for (int i = 1; i <= range; i++)
            {
                Vector2Int nextCoords = OccupiedSquare + direction * i;
                Unit unit = Field.GetUnitOnSquare(nextCoords);

                if (!Field.CheckIfCoordsAreOnField(nextCoords))
                    break;

                if (unit == null)
                    AddMove(nextCoords);
                else if (!unit.IsFromSameTeam(this))
                {
                    AddMove(nextCoords);
                    break;
                }
                else if (unit.IsFromSameTeam(this))
                    break;
            }
        }
    }
    #endregion

}