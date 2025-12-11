using System;
using System.Collections.Generic;
using DamasChinas_Server.Common;

namespace DamasChinas_Server.Game
{
    public struct HexCoordinate : IEquatable<HexCoordinate>
    {
        private const int DistanceDivisor = 2;
        private const int HashSeed = 17;
        private const int HashFactor = 31;
        private const int HalfDivisor = 2;

        public static readonly IReadOnlyList<HexCoordinate> Directions = new[]
        {
            new HexCoordinate(1, -1, 0),
            new HexCoordinate(1, 0, -1),
            new HexCoordinate(0, 1, -1),
            new HexCoordinate(-1, 1, 0),
            new HexCoordinate(-1, 0, 1),
            new HexCoordinate(0, -1, 1)
        };

        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public HexCoordinate(int x, int y, int z)
        {
            if (x + y + z != 0)
            {
                throw new ArgumentException(MessageCode.InvalidCubeCoordinate.ToString(), nameof(z));
            }

            X = x;
            Y = y;
            Z = z;
        }

        public static HexCoordinate operator +(HexCoordinate left, HexCoordinate right)
        {
            return new HexCoordinate(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        public static HexCoordinate operator -(HexCoordinate left, HexCoordinate right)
        {
            return new HexCoordinate(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        public static bool operator ==(HexCoordinate left, HexCoordinate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HexCoordinate left, HexCoordinate right)
        {
            return !left.Equals(right);
        }

        public int DistanceTo(HexCoordinate other)
        {
            return (Math.Abs(X - other.X) + Math.Abs(Y - other.Y) + Math.Abs(Z - other.Z)) / DistanceDivisor;
        }

        public bool Equals(HexCoordinate other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return obj is HexCoordinate coordinate && Equals(coordinate);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = HashSeed;
                hash = hash * HashFactor + X;
                hash = hash * HashFactor + Y;
                hash = hash * HashFactor + Z;
                return hash;
            }
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        public HexCoordinate Half()
        {
            if ((X % HalfDivisor) != 0 || (Y % HalfDivisor) != 0 || (Z % HalfDivisor) != 0)
            {
                throw new InvalidOperationException(MessageCode.InvalidHalfCoordinate.ToString());
            }

            return new HexCoordinate(X / HalfDivisor, Y / HalfDivisor, Z / HalfDivisor);
        }
    }
}
