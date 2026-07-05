using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SimplifyVW_Heap 
{
    class Node
    {
        public Vector2 point;
        public float area;
        public Node prev;
        public Node next;
        public bool removed;
    }

    public static IEnumerator Simplify(List<Vector2> input, float minArea, System.Action<List<Vector2>> onComplete)
    {
        if (input == null || input.Count < 3)
        {
            onComplete?.Invoke(input);
            yield break;
        }

        // Crear nodos
        List<Node> nodes = new List<Node>();
        for (int i = 0; i < input.Count; i++)
        {
            nodes.Add(new Node { point = input[i] });
        }

        // Conectar lista doble
        for (int i = 0; i < nodes.Count; i++)
        {
            if (i > 0) nodes[i].prev = nodes[i - 1];
            if (i < nodes.Count - 1) nodes[i].next = nodes[i + 1];
        }

        // Heap correcto
        var heap = new MinHeap<Node>();

        // Inicializar áreas
        for (int i = 1; i < nodes.Count - 1; i++)
        {
            nodes[i].area = Area(nodes[i - 1], nodes[i], nodes[i + 1]);
            heap.Enqueue(nodes[i], nodes[i].area);
        }

        int iterations = 0;

        // Loop principal
        while (heap.Count > 0)
        {
            Node n = heap.Dequeue();

            if (n.removed) continue;
            if (n.area > minArea) break;

            n.removed = true;

            // reconectar
            if (n.prev != null) n.prev.next = n.next;
            if (n.next != null) n.next.prev = n.prev;

            // actualizar vecino anterior
            if (n.prev != null && n.prev.prev != null && n.prev.next != null)
            {
                n.prev.area = Area(n.prev.prev, n.prev, n.prev.next);
                heap.Enqueue(n.prev, n.prev.area);
            }

            // actualizar vecino siguiente
            if (n.next != null && n.next.next != null && n.next.prev != null)
            {
                n.next.area = Area(n.next.prev, n.next, n.next.next);
                heap.Enqueue(n.next, n.next.area);
            }

            // evitar freeze
            iterations++;
            if (iterations % 100 == 0)
                yield return null;
        }

        // reconstruir resultado
        List<Vector2> result = new List<Vector2>();
        Node current = nodes[0];

        while (current != null)
        {
            if (!current.removed)
                result.Add(current.point);

            current = current.next;
        }

        onComplete?.Invoke(result);
    }

    static float Area(Node a, Node b, Node c)
    {
        return Mathf.Abs(
            (a.point.x * (b.point.y - c.point.y) +
             b.point.x * (c.point.y - a.point.y) +
             c.point.x * (a.point.y - b.point.y)) * 0.5f
        );
    }

    class MinHeap<T>
    {
        private List<(T item, float priority)> heap = new List<(T, float)>();

        public int Count => heap.Count;

        public void Enqueue(T item, float priority)
        {
            heap.Add((item, priority));
            HeapifyUp(heap.Count - 1);
        }

        public T Dequeue()
        {
            var root = heap[0].item;

            heap[0] = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);

            HeapifyDown(0);

            return root;
        }

        void HeapifyUp(int i)
        {
            while (i > 0)
            {
                int parent = (i - 1) / 2;

                if (heap[i].priority >= heap[parent].priority)
                    break;

                Swap(i, parent);
                i = parent;
            }
        }

        void HeapifyDown(int i)
        {
            int last = heap.Count - 1;

            while (true)
            {
                int left = i * 2 + 1;
                int right = i * 2 + 2;
                int smallest = i;

                if (left <= last && heap[left].priority < heap[smallest].priority)
                    smallest = left;

                if (right <= last && heap[right].priority < heap[smallest].priority)
                    smallest = right;

                if (smallest == i)
                    break;

                Swap(i, smallest);
                i = smallest;
            }
        }

        void Swap(int a, int b)
        {
            var temp = heap[a];
            heap[a] = heap[b];
            heap[b] = temp;
        }
    }
}
