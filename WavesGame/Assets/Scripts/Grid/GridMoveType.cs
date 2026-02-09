/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 *
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System;

namespace Grid
{
    public enum GridMoveType
    {
        Cross, Area, Diagonal,
        Up, Down, Left, Right
        
        //TODO add Self
    }

    public static class GridMoveTypeExtensions
    {
        public static string GridMovementSymbol(GridMoveType type)
        {
            return type switch
            {
                GridMoveType.Cross => "~+",
                GridMoveType.Area => "~*",
                GridMoveType.Diagonal => "~X",
                GridMoveType.Up => "~^",
                GridMoveType.Down => "~V",
                GridMoveType.Left => "~<",
                GridMoveType.Right => "~>",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}