using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 절차적 맵 생성을 위한 그래프 생성기
/// - BFS 기반 노드 연결
/// </summary>
public class RoomGraph
{
    private List<RoomNode> nodes;
    private Queue<RoomNode> queue;
    private int maxNodeCount;
    private int currentNodeCount;

    public RoomGraph(int maxNodeCount)
    {
        this.maxNodeCount = maxNodeCount;
        this.currentNodeCount = 0;
        nodes = new List<RoomNode>();
        queue = new Queue<RoomNode>();
    }

    public List<RoomNode> Generate(Vector2Int startPosition)
    {
        RoomNode startNode = new RoomNode(startPosition);
        AddNode(startNode);
        queue.Enqueue(startNode);
        startNode.Tag = RoomType.Start;

        while (queue.Count > 0 && currentNodeCount < maxNodeCount)
        {
            RoomNode currentNode = queue.Dequeue();
            AddNeighbor(currentNode);

            foreach (RoomNode neighbor in currentNode.Neighbors)
            {
                if (!neighbor.IsVisited)
                {
                    neighbor.IsVisited = true;
                    queue.Enqueue(neighbor);
                }
            }
        }

        nodes[currentNodeCount - 1].Tag = RoomType.Exit; // 마지막 노드를 Exit으로 설정
        return nodes;
    }

    //1. 자기 주변 대각선 4방향에 대해 노드가 존재하는지 확인
    //2. 대각선에 노드가 존재한다면 해당 방향의 노드는 생성할 수 없음(ex, (1, 1) 방향에 노드가 존재한다면 (1, 0), (0, 1) 방향은 생성할 수 없음)
    //3. 생성하는 방향의 두칸 앞에 노드가 존재한다면 해당 방향은 생성할 수 없음
    private void AddNeighbor(RoomNode node)
    {
        int randomNode = Random.Range(1, 16);
        int bitMask = node.getBitMask() | SearchDiagonalNeighbors(node) | SearchJumpNeighbors(node);
        while ((randomNode & ~bitMask) == 0 && bitMask != 15)
        {
            randomNode = Random.Range(1, 16);
        }
        randomNode &= ~bitMask; //A AND NOT B
        if (randomNode == 0)
        {
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            if ((randomNode & (1 << i)) != 0)
            {
                DirectionType direction = (DirectionType)i;
                Vector2Int neighborPosition = node.Position + direction.ToVector();
                RoomNode neighborNode = GetNode(neighborPosition);

                if (neighborNode == null)
                {
                    neighborNode = new RoomNode(neighborPosition);
                    AddNode(neighborNode);
                    neighborNode.Tag = RoomType.Normal;
                    neighborNode.OpenDoor(direction.Opposite());
                    node.Neighbors.Add(neighborNode);
                    node.OpenDoor(direction);
                }
            }
        }
    }

    //생성할 수 없는 방향의 비트마스크 반환
    private int SearchDiagonalNeighbors(RoomNode node)
    {
        Vector2Int[] directions = { new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1) };
        int bitMask = 0;
        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int direction = directions[i];


            // 대각선 방향에 노드가 존재하는지 확인
            Vector2Int neighborPosition = node.Position + direction;
            RoomNode neighborNode = GetNode(neighborPosition);

            if (neighborNode != null)
            {
                switch (i)
                {
                    case 0: // 오른쪽 위 대각선
                        bitMask |= (int)DoorFlag.North;
                        bitMask |= (int)DoorFlag.East;
                        break;
                    case 1: // 왼쪽 위 대각선
                        bitMask |= (int)DoorFlag.North;
                        bitMask |= (int)DoorFlag.West;
                        break;
                    case 2: // 오른쪽 아래 대각선
                        bitMask |= (int)DoorFlag.East;
                        bitMask |= (int)DoorFlag.South;
                        break;
                    case 3: // 왼쪽 아래 대각선
                        bitMask |= (int)DoorFlag.West;
                        bitMask |= (int)DoorFlag.South;
                        break;
                }
            }

        }
        return bitMask;
    }

    private int SearchJumpNeighbors(RoomNode node)
    {
        Vector2Int[] directions = { new Vector2Int(2, 0), new Vector2Int(-2, 0), new Vector2Int(0, 2), new Vector2Int(0, -2) };
        int bitMask = 0;
        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int direction = directions[i];
            Vector2Int neighborPosition = node.Position + direction;
            RoomNode neighborNode = GetNode(neighborPosition);

            if (neighborNode != null)
            {
                switch (i)
                {
                    case 0: // 오른쪽
                        bitMask |= (int)DoorFlag.East;
                        break;
                    case 1: // 왼쪽
                        bitMask |= (int)DoorFlag.West;
                        break;
                    case 2: // 위쪽
                        bitMask |= (int)DoorFlag.North;
                        break;
                    case 3: // 아래쪽
                        bitMask |= (int)DoorFlag.South;
                        break;
                }
            }
        }
        return bitMask;
    }
    private void AddNode(RoomNode newNode)
    {
        nodes.Add(newNode);
        currentNodeCount++;
    }

    private RoomNode GetNode(Vector2Int position)
    {
        return nodes.Find(node => node.Position == position);
    }
}