using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSneaking.Domain
{
    public class Edge
    {
        public Vector3 position;

        public Edge(Vector3 position)
        {
            this.position = position;
        }

        public void Translate(Vector3 deltaMovement)
        {
            position += deltaMovement;
        }
    }

    public interface IEdges
    {
        List<Vector3> GetPositions();
        void Translate(Vector3 deltaMovement);
        void Translate(float deltaX, float deltaY, float deltaZ);
    }

    public class Edges : IEnumerable, IEdges
    {
        private Edge[] _edges;

        public Edges(Edge[] pArray)
        {
            _edges = new Edge[pArray.Length];
            for (int i = 0; i < pArray.Length; i++)
            {
                _edges[i] = pArray[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) GetEnumerator();
        }

        public List<Vector3> GetPositions()
        {
            List<Vector3> positions = new List<Vector3>();
            for(int i = 0; i < _edges.Length; i++)
            {
                positions.Add(_edges[i].position);
            }
            return positions;
        }

        public EdgeEnum GetEnumerator()
        {
            return new EdgeEnum(_edges);
        }

        public void Translate(Vector3 deltaMovement)
        {
            for (int i = 0; i < _edges.Length; i++) {
                _edges[i].Translate(deltaMovement);
            }
        }

        public void Translate(float deltaX, float deltaY, float deltaZ)
        {
            Translate(new Vector3(deltaX, deltaY, deltaZ));
        }
    }

    public class EdgeEnum : IEnumerator
    {
        public Edge[] _edges;

        int position = -1;

        public EdgeEnum(Edge[] list)
        {
            _edges = list;
        }

        public bool MoveNext()
        {
            position++;
            return (position < _edges.Length);
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public Edge Current
        {
            get
            {
                try
                {
                    return _edges[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
