using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Forward,
    Back,
    Right,
    Left,
    Up,
    Down
}

public static class DirectionExtensions
{
    /// <summary>
    /// Converts Direction to Vector3Int.
    /// </summary>
    public static Vector3Int GetVector(this Direction direction)
    {
        return direction switch
        {
            Direction.Back => Vector3Int.back,
            Direction.Forward => Vector3Int.forward,
            Direction.Up => Vector3Int.up,
            Direction.Down => Vector3Int.down,
            Direction.Left => Vector3Int.left,
            Direction.Right => Vector3Int.right,
            _ => throw new Exception("Invalid direction")
        };
    }

    /// <summary>
    /// Lists all directions.
    /// </summary>
    public static Direction[] ListDirections()
    {
        Direction[] directions =
        {
            Direction.Back,
            Direction.Down,
            Direction.Forward,
            Direction.Left,
            Direction.Right,
            Direction.Up
        };
        return directions;
    }
}
