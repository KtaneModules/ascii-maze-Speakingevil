using System.Collections.Generic;
using System.Linq;
using System;

namespace AsciiMaze
{
    public class MazeSolver
    {
        private readonly string[][] _maze;

        public MazeSolver(string[][] maze)
        {
            _maze = maze;
        }

        public char[] SolveMaze(int[] current, int[] exit)
        {
            var startPos = new Coordinate(current);
            var ending = new Coordinate(exit);

            var solveMoves = new List<SolveMove>();
            var intersections = new LinkedList<Intersection>();
            var visited = new List<Coordinate>();
            var neighbors = GetNeighbors(startPos);

            intersections.AddFirst(new Intersection(startPos,
                neighbors.Select(x => new Pair<Movement, bool>(new Movement(x.Coordinate, x.Direction), false))
                    .ToList(), 0));
            visited.Add(new Coordinate(intersections.First().Position));

            var currentPos = intersections.First().Paths.First(x => !x.Item2).Item1;
            solveMoves.Add(new SolveMove(currentPos.Direction, intersections.First().Index));
            intersections.First().Paths.First().Item2 = true;

            while (intersections.Any())
            {
                visited.Add(new Coordinate(currentPos.Coordinate));
                if (currentPos.Coordinate == ending)
                {
                    //The current position is the ending, so go to the end of the code.
                    return solveMoves.Select(x => x.Instruction).ToArray();
                }

                neighbors = GetNeighbors(currentPos.Coordinate);
                if (neighbors.Count > 2)
                {
                    var pos = currentPos;
                    if (intersections.All(x => x.Position != pos.Coordinate))
                    {
                        //This is a new intersection
                        intersections.AddFirst(new Intersection(currentPos.Coordinate,
                            neighbors.Select(x =>
                                    new Pair<Movement, bool>(new Movement(x.Coordinate, x.Direction),
                                        visited.Any(y => y == x.Coordinate)))
                                .ToList(), intersections.First().Index + 1));
                    }
                    else
                    {
                        //We've already been to this intersection
                        intersections.First().Paths.ForEach(x => x.Item2 = visited.Any(y => y == x.Item1.Coordinate));
                        //Remove all of the moves that we've previously made from this intersection
                        solveMoves.RemoveAll(x => x.IntSection == intersections.First().Index);
                        if (intersections.First().Paths.All(x => x.Item2))
                        {
                            //We've already used all possible paths, in this intersection. Which means that we disregard this one and go to the previous one
                            intersections.RemoveFirst();
                            currentPos.Coordinate = intersections.First().Position;
                            if (intersections.First().Index == 0)
                            {
                                //If this is the start just clear everything
                                solveMoves.Clear();
                            }

                            continue;
                        }
                    }

                    //Move to the first path in the intersection that we didn't use yet
                    currentPos = intersections.First().Paths.First(x => !x.Item2).Item1;
                    solveMoves.Add(new SolveMove(currentPos.Direction, intersections.First().Index));
                    intersections.First().Paths.First().Item2 = true;
                    continue;
                }

                if (neighbors.Count == 2)
                {
                    //This is a hallway with only one possible way to take
                    var chosenNeighbor = neighbors.Single(x => visited.All(y => y != x.Coordinate));
                    currentPos.Coordinate = chosenNeighbor.Coordinate;
                    solveMoves.Add(new SolveMove(chosenNeighbor.Direction, intersections.First().Index));
                    continue;
                }

                //This is a dead end so go back to the previous intersection
                currentPos.Coordinate = intersections.First().Position;
                if (intersections.First().Index == 0)
                {
                    //It might be that the previous intersection is the start, and if that only has 2 possible paths we have to make an exception that it always clears the moves.
                    solveMoves.Clear();
                }
            }

            //We've explored all of the intersections and none of them lead to the exit, which means that the solver is broken.
            throw new InvalidOperationException(
                "There is an error in the Twitch Plays solver! Please send a bug report with the logfile attached!");
        }

        private List<Movement> GetNeighbors(Coordinate position)
        {
            var positions = new List<Movement>();
            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0: //left
                        if (position.X != 0 && _maze[position.Y * 2][position.X * 2 - 1] != "\x25a0")
                            positions.Add(new Movement(new Coordinate(position.Y, position.X - 1), 'l'));
                        break;
                    case 1: // up
                        if (position.Y != 0 && _maze[position.Y * 2 - 1][position.X * 2] != "\x25a0")
                            positions.Add(new Movement(new Coordinate(position.Y - 1, position.X), 'u'));
                        break;
                    case 2: // right
                        if (position.X != 6 && _maze[position.Y * 2][position.X * 2 + 1] != "\x25a0")
                            positions.Add(new Movement(new Coordinate(position.Y, position.X + 1), 'r'));
                        break;
                    case 3: // down
                        if (position.Y != 6 && _maze[position.Y * 2 + 1][position.X * 2] != "\x25a0")
                            positions.Add(new Movement(new Coordinate(position.Y + 1, position.X), 'd'));
                        break;
                }
            }

            return positions;
        }
    }

    public class Pair<T1, T2>
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; set; }

        public Pair(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

    public class SolveMove
    {
        public char Instruction { get; private set; }
        public int IntSection { get; private set; }

        public SolveMove(char instruction, int intersection)
        {
            Instruction = instruction;
            IntSection = intersection;
        }
    }

    public class Coordinate
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public static bool operator ==(Coordinate coor1, Coordinate coor2)
        {
            return !ReferenceEquals(coor1, null) && coor1.Equals(coor2);
        }

        public static bool operator !=(Coordinate coor1, Coordinate coo2)
        {
            return !ReferenceEquals(coor1, null) && !(coor1.Equals(coo2));
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", Y, X);
        }

        public override bool Equals(object obj)
        {
            var c = obj as Coordinate;
            return c != null && this.X == c.X && this.Y == c.Y;
        }

        public Coordinate(int y, int x)
        {
            X = x;
            Y = y;
        }

        public Coordinate(int[] coordinate) : this(coordinate[0], coordinate[1])
        {
        }

        public Coordinate(Coordinate coordinate) : this(coordinate.Y, coordinate.X)
        {
        }
    }

    public class Movement
    {
        public Coordinate Coordinate { get; set; }
        public char Direction { get; private set; }

        public Movement(Coordinate coordinate, char direction)
        {
            Coordinate = coordinate;
            Direction = direction;
        }
    }

    public class Intersection
    {
        public Coordinate Position { get; private set; }
        public List<Pair<Movement, bool>> Paths { get; private set; }
        public int Index { get; private set; }

        public Intersection(Coordinate position, List<Pair<Movement, bool>> paths, int index)
        {
            Position = position;
            Paths = paths;
            Index = index;
        }
    }
}