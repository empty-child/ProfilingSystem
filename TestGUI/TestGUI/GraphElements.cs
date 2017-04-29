﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGUI
{
    public class Node<T>
    {
        // Private member-variables
        private T data;
        private NodeList<T> neighbors = null;
        private Dictionary<T, object> additionalData;

        public Node() { }
        public Node(T data) : this(data, null) { }
        public Node(T data, NodeList<T> neighbors)
        {
            this.data = data;
            this.neighbors = neighbors;
        }


        public Node(T data, NodeList<T> neighbors, Dictionary<T, object> additionalData)
        {
            this.data = data;
            this.neighbors = neighbors;
            this.additionalData = additionalData;
        }

        public T Value
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }

        public NodeList<T> Neighbors
        {
            get
            {
                return neighbors;
            }
            set
            {
                neighbors = value;
            }
        }

        public Dictionary<T, object> ParametersData
        {
            get
            {
                return additionalData;
            }
            set
            {
                additionalData = value;
            }
        }
    }

    public class NodeList<T> : Collection<Node<T>>
    {
        public NodeList() : base() { }

        public NodeList(int initialSize)
        {
            // Add the specified number of items
            for (int i = 0; i < initialSize; i++)
                base.Items.Add(default(Node<T>));
        }

        public Node<T> FindByValue(T value)
        {
            // search the list for the value
            foreach (Node<T> node in Items)
                if (node.Value.Equals(value))
                    return node;

            // if we reached here, we didn't find a matching node
            return null;
        }
    }

    public class GraphNode<T> : Node<T>
    {
        private List<int> costs;

        public GraphNode() : base() { }
        public GraphNode(T value) : base(value) { }
        public GraphNode(T value, NodeList<T> neighbors) : base(value, neighbors) { }
        public GraphNode(T value, NodeList<T> neighbors, Dictionary<T, object> additionalData) : base(value, neighbors, additionalData) { }

        new public NodeList<T> Neighbors
        {
            get
            {
                if (base.Neighbors == null)
                    base.Neighbors = new NodeList<T>();

                return base.Neighbors;
            }
        }

        public List<int> Costs
        {
            get
            {
                if (costs == null)
                    costs = new List<int>();

                return costs;
            }
        }
    }

    public class Graph<T> : IEnumerable<T>
    {
        private NodeList<T> nodeSet;

        public Graph() : this(null) { }
        public Graph(NodeList<T> nodeSet)
        {
            if (nodeSet == null)
                this.nodeSet = new NodeList<T>();
            else
                this.nodeSet = nodeSet;
        }

        public void AddNode(GraphNode<T> node)
        {
            // adds a node to the graph
            nodeSet.Add(node);
        }

        public void AddNode(T value)
        {
            // adds a node to the graph
            nodeSet.Add(new GraphNode<T>(value));
        }

        public void AddNode(T value, Dictionary<T, object> parameters)
        {
            // adds a node with additional data to the graph
            nodeSet.Add(new GraphNode<T>(value, null, parameters));
        }

        public void AddDirectedEdge(T inputFrom, T inputTo, int cost)
        {
            GraphNode<T> from = (GraphNode<T>)nodeSet.FindByValue(inputFrom);
            GraphNode<T> to = (GraphNode<T>)nodeSet.FindByValue(inputTo);
            from.Neighbors.Add(to);
            from.Costs.Add(cost);
        }

        public void AddUndirectedEdge(T inputFrom, T inputTo, int cost)
        {
            GraphNode<T> from = (GraphNode<T>)nodeSet.FindByValue(inputFrom);
            GraphNode<T> to = (GraphNode<T>)nodeSet.FindByValue(inputTo);
            from.Neighbors.Add(to);
            from.Costs.Add(cost);

            to.Neighbors.Add(from);
            to.Costs.Add(cost);
        }

        public Dictionary<Node<T>, int> Distance(T from)
        {
            Node<T> inputFrom = new Node<T>();
            GraphNode<T> temp = (GraphNode<T>)nodeSet.FindByValue(from);
            if (temp != null) { inputFrom = (Node<T>)temp; };
            //GraphNode<T> to = (GraphNode<T>)nodeSet.FindByValue(inputTo);
            //if (from == null || to == null) return -1;
            //if (from.Neighbors.Contains(to))
            //{
            //    return 1;
            //}
            //else
            //{
            //Queue<Node<string>> NodeQuene = new Queue<Node<string>>();
            //NodeQuene.Enqueue(from);


            bool flag = false;
            int count = 0;
            Node<T> currentNode = inputFrom;
            List<Node<T>> inputList = new List<Node<T>>();
            List<Node<T>> outputList = new List<Node<T>>();
            inputList.Add(inputFrom);
            Dictionary<Node<T>, int> commonCost = new Dictionary<Node<T>, int>();
            //commonCost.Add(currentNode, 0);

            while (flag == false)
            {
                foreach (var node in inputList)
                {
                    if(!commonCost.ContainsKey(node)) commonCost.Add(node, count);
                    foreach (var innode in node.Neighbors)
                    {
                        if (!commonCost.ContainsKey(innode)) outputList.Add(innode);
                    }

                }
                if (outputList == null || outputList.Count == 0)
                {
                    flag = true;
                }
                else
                {
                    inputList.Clear();
                    inputList = outputList.ToList();
                    outputList.Clear();
                    count++;
                }
            }

            //}
            return commonCost;
        }

        public bool Contains(T value)
        {
            return nodeSet.FindByValue(value) != null;
        }

        public bool Remove(T value)
        {
            // first remove the node from the nodeset
            GraphNode<T> nodeToRemove = (GraphNode<T>)nodeSet.FindByValue(value);
            if (nodeToRemove == null)
                // node wasn't found
                return false;

            // otherwise, the node was found
            nodeSet.Remove(nodeToRemove);

            // enumerate through each node in the nodeSet, removing edges to this node
            foreach (GraphNode<T> gnode in nodeSet)
            {
                int index = gnode.Neighbors.IndexOf(nodeToRemove);
                if (index != -1)
                {
                    // remove the reference to the node and associated cost
                    gnode.Neighbors.RemoveAt(index);
                    gnode.Costs.RemoveAt(index);
                }
            }

            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }

        public NodeList<T> Nodes
        {
            get
            {
                return nodeSet;
            }
        }

        public int Count
        {
            get { return nodeSet.Count; }
        }
    }
}
